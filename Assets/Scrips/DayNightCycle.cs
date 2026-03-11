using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public enum CycleState { Day, TransitionToNight, NightUnderground, TransitionToDay }

    [Header("References")]
    [SerializeField] private Light2D sunLight;

    [Header("Durations (seconds)")]
    [SerializeField] private float dayDuration = 240f;
    [SerializeField] private float transitionToNightDuration = 8f;
    [SerializeField] private float undergroundDuration = 4f;
    [SerializeField] private float transitionToDayDuration = 8f;

    [Header("Light Intensity")]
    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private float nightIntensity = 0.15f;

    [Header("Light Color")]
    [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.8f);
    [SerializeField] private Color nightColor = new Color(0.2f, 0.25f, 0.5f);

    [Header("Orbit")]
    [Tooltip("The world point the sun orbits around.")]
    [SerializeField] private Vector2 orbitCenter = Vector2.zero;
    [Tooltip("Distance from the orbit center to the sun.")]
    [SerializeField] private float orbitRadius = 20f;
    [Tooltip("Angle on the circle where the sun rises (left side). Default: 90")]
    [SerializeField] private float sunriseAngle = 90f;
    [Tooltip("Angle on the circle where the sun sets (right side). Default: -90")]
    [SerializeField] private float sunsetAngle = -90f;

    [Header("Gameplay")]
    [SerializeField] private bool burnEnabledAtNight = false;

    private CycleState _state;
    private float _stateTimer;
    private float _stateDuration;

    // Underground arc: continues clockwise from sunsetAngle by another 180 degrees
    private float UndergroundArrivalAngle => sunsetAngle - 180f;

    public bool BurnEnabled => _state == CycleState.Day ||
                               (_state == CycleState.TransitionToNight && burnEnabledAtNight);
    public bool IsDay => _state == CycleState.Day;
    public CycleState State => _state;

    private void Start()
    {
        // Start at noon: t = 0.5 places sun at midpoint angle
        _state = CycleState.Day;
        _stateDuration = dayDuration;
        _stateTimer = dayDuration * 0.5f;

        float noonAngle = Mathf.Lerp(sunriseAngle, sunsetAngle, 0.5f);
        PlaceSun(noonAngle);
        ApplyLight(dayIntensity, dayColor);
    }

    private void Update()
    {
        _stateTimer -= Time.deltaTime;

        float t = 1f - Mathf.Clamp01(_stateTimer / _stateDuration);

        switch (_state)
        {
            case CycleState.Day:
                PlaceSun(Mathf.Lerp(sunriseAngle, sunsetAngle, t));

                if (_stateTimer <= 0f)
                    EnterState(CycleState.TransitionToNight, transitionToNightDuration);
                break;

            case CycleState.TransitionToNight:
                ApplyLight(
                    Mathf.Lerp(dayIntensity, 0f, t),
                    Color.Lerp(dayColor, nightColor, t)
                );

                if (_stateTimer <= 0f)
                    EnterState(CycleState.NightUnderground, undergroundDuration);
                break;

            case CycleState.NightUnderground:
                PlaceSun(Mathf.Lerp(sunsetAngle, UndergroundArrivalAngle, t));
                ApplyLight(
                    Mathf.Lerp(0f, nightIntensity, t),
                    nightColor
                );

                if (_stateTimer <= 0f)
                {
                    PlaceSun(sunriseAngle);
                    EnterState(CycleState.TransitionToDay, transitionToDayDuration);
                }
                break;

            case CycleState.TransitionToDay:
                ApplyLight(
                    Mathf.Lerp(nightIntensity, dayIntensity, t),
                    Color.Lerp(nightColor, dayColor, t)
                );

                if (_stateTimer <= 0f)
                    EnterState(CycleState.Day, dayDuration);
                break;
        }
    }

    // Places the sun on the orbit circle at the given angle and points it toward the center
    private void PlaceSun(float angleDeg)
    {
        if (sunLight == null) return;

        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 pos = new Vector3(
            orbitCenter.x + Mathf.Cos(rad) * orbitRadius,
            orbitCenter.y + Mathf.Sin(rad) * orbitRadius,
            sunLight.transform.position.z
        );

        sunLight.transform.position = pos;

        // Rotate the spotlight to face the orbit center so shadows cast inward
        Vector2 toCenter = orbitCenter - (Vector2)pos;
        float faceAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
        sunLight.transform.rotation = Quaternion.Euler(0f, 0f, faceAngle);
    }

    private void EnterState(CycleState state, float duration)
    {
        _state = state;
        _stateDuration = duration;
        _stateTimer = duration;
    }

    private void ApplyLight(float intensity, Color color)
    {
        if (sunLight == null) return;
        sunLight.intensity = intensity;
        sunLight.color = color;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw the full orbit circle
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.3f);
        DrawCircle(orbitCenter, orbitRadius, 48);

        // Center point
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(orbitCenter, 0.25f);

        // Key positions
        DrawOrbitPoint(sunriseAngle, Color.cyan, 0.4f);
        DrawOrbitPoint(Mathf.Lerp(sunriseAngle, sunsetAngle, 0.5f), Color.white, 0.4f);
        DrawOrbitPoint(sunsetAngle, Color.red, 0.4f);
    }

    private void DrawOrbitPoint(float angleDeg, Color color, float size)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 pos = new Vector3(
            orbitCenter.x + Mathf.Cos(rad) * orbitRadius,
            orbitCenter.y + Mathf.Sin(rad) * orbitRadius,
            0f
        );
        Gizmos.color = color;
        Gizmos.DrawWireSphere(pos, size);
    }

    private void DrawCircle(Vector2 center, float radius, int segments)
    {
        float step = 360f / segments;
        float rad0 = 0f;
        Vector3 prev = new Vector3(center.x + Mathf.Cos(rad0) * radius, center.y + Mathf.Sin(rad0) * radius, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float rad = i * step * Mathf.Deg2Rad;
            Vector3 next = new Vector3(center.x + Mathf.Cos(rad) * radius, center.y + Mathf.Sin(rad) * radius, 0f);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}