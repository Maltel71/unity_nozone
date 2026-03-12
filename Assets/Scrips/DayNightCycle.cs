using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public enum CycleState { Day, Sunset, Night, Sunrise }

    [Header("Sun Light (2D Spot Light)")]
    [SerializeField] private Light2D sunLight;

    [Header("Day Settings")]
    [SerializeField] private float dayDuration = 240f;
    [SerializeField] private float sunStartAngle = 180f;
    [SerializeField] private float sunEndAngle = 0f;
    [SerializeField] private float sunDayIntensity = 1f;

    [Header("Sunset Settings")]
    [SerializeField] private float sunFadeOutDuration = 8f;
    [SerializeField] private float lightSwitchDelay = 0.5f;

    [Header("Night Light (Global Light 2D)")]
    [SerializeField] private Light2D nightLight;

    [Header("Night Settings")]
    [SerializeField] private float nightDuration = 150f;
    [SerializeField] private float nightLightIntensity = 0.4f;
    [SerializeField] private float nightFadeInDuration = 5f;
    [SerializeField] private float nightFadeOutDuration = 5f;

    [Header("Sunrise Settings")]
    [SerializeField] private float sunFadeInDuration = 8f;

    [Header("Night Overlay Sprite")]
    [SerializeField] private SpriteRenderer nightOverlay;

    [Header("Orbit")]
    [SerializeField] private Vector2 orbitCenter = Vector2.zero;
    [SerializeField] private float orbitRadius = 20f;

    [Header("Gameplay")]
    [SerializeField] private bool burnEnabledAtNight = false;

    private CycleState _state;
    private float _currentSunAngle;

    public bool BurnEnabled => _state == CycleState.Day ||
                               (_state == CycleState.Sunset && burnEnabledAtNight);
    public bool IsDay => _state == CycleState.Day;
    public CycleState State => _state;

    private void Start()
    {
        if (nightLight != null)
        {
            nightLight.intensity = 0f;
            nightLight.enabled = false;
        }

        if (nightOverlay != null)
            SetOverlayAlpha(0f);

        _currentSunAngle = sunStartAngle;

        if (sunLight != null)
        {
            sunLight.intensity = sunDayIntensity;
            sunLight.enabled = true;
        }

        PlaceSun(_currentSunAngle);

        _state = CycleState.Day;
        StartCoroutine(DayRoutine());
    }

    // ─── Day ──────────────────────────────────────────────────────────────────

    private IEnumerator DayRoutine()
    {
        _state = CycleState.Day;

        float elapsed = 0f;
        while (elapsed < dayDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dayDuration);
            _currentSunAngle = Mathf.Lerp(sunStartAngle, sunEndAngle, t);
            PlaceSun(_currentSunAngle);
            yield return null;
        }

        StartCoroutine(SunsetRoutine());
    }

    // ─── Sunset ───────────────────────────────────────────────────────────────

    private IEnumerator SunsetRoutine()
    {
        _state = CycleState.Sunset;

        float rotationSpeed = Mathf.Abs(sunEndAngle - sunStartAngle) / dayDuration;
        float direction = Mathf.Sign(sunEndAngle - sunStartAngle);
        float elapsed = 0f;

        while (elapsed < sunFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sunFadeOutDuration);

            _currentSunAngle = sunEndAngle + rotationSpeed * elapsed * direction;
            PlaceSun(_currentSunAngle);

            if (sunLight != null)
                sunLight.intensity = Mathf.Lerp(sunDayIntensity, 0f, t);

            SetOverlayAlpha(t);

            yield return null;
        }

        if (sunLight != null) sunLight.intensity = 0f;
        SetOverlayAlpha(1f);

        yield return new WaitForSeconds(lightSwitchDelay);

        if (sunLight != null) sunLight.enabled = false;

        StartCoroutine(NightRoutine());
    }

    // ─── Night ────────────────────────────────────────────────────────────────

    private IEnumerator NightRoutine()
    {
        _state = CycleState.Night;

        // Snap sun to start angle silently while disabled
        _currentSunAngle = sunStartAngle;
        PlaceSun(_currentSunAngle);

        if (nightLight != null)
        {
            nightLight.intensity = 0f;
            nightLight.enabled = true;
        }

        yield return StartCoroutine(FadeLight(nightLight, 0f, nightLightIntensity, nightFadeInDuration));

        yield return new WaitForSeconds(nightDuration);

        yield return StartCoroutine(FadeLight(nightLight, nightLightIntensity, 0f, nightFadeOutDuration));

        if (nightLight != null) nightLight.enabled = false;

        yield return new WaitForSeconds(lightSwitchDelay);

        StartCoroutine(SunriseRoutine());
    }

    // ─── Sunrise ──────────────────────────────────────────────────────────────

    private IEnumerator SunriseRoutine()
    {
        _state = CycleState.Sunrise;

        _currentSunAngle = sunStartAngle;
        PlaceSun(_currentSunAngle);

        if (sunLight != null)
        {
            sunLight.intensity = 0f;
            sunLight.enabled = true;
        }

        float rotationSpeed = Mathf.Abs(sunEndAngle - sunStartAngle) / dayDuration;
        float direction = Mathf.Sign(sunEndAngle - sunStartAngle);
        float elapsed = 0f;

        while (elapsed < sunFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sunFadeInDuration);

            _currentSunAngle = sunStartAngle + rotationSpeed * elapsed * direction;
            PlaceSun(_currentSunAngle);

            if (sunLight != null)
                sunLight.intensity = Mathf.Lerp(0f, sunDayIntensity, t);

            SetOverlayAlpha(1f - t);

            yield return null;
        }

        if (sunLight != null) sunLight.intensity = sunDayIntensity;
        SetOverlayAlpha(0f);

        // Continue day from where sunrise left off
        float dayElapsed = sunFadeInDuration;
        _state = CycleState.Day;

        while (dayElapsed < dayDuration)
        {
            dayElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(dayElapsed / dayDuration);
            _currentSunAngle = Mathf.Lerp(sunStartAngle, sunEndAngle, t);
            PlaceSun(_currentSunAngle);
            yield return null;
        }

        StartCoroutine(SunsetRoutine());
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void SetOverlayAlpha(float alpha)
    {
        if (nightOverlay == null) return;
        Color c = nightOverlay.color;
        c.a = alpha;
        nightOverlay.color = c;
    }

    private IEnumerator FadeLight(Light2D light, float from, float to, float duration)
    {
        if (light == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            light.intensity = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        light.intensity = to;
    }

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

        Vector2 toCenter = orbitCenter - (Vector2)pos;
        float faceAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
        sunLight.transform.rotation = Quaternion.Euler(0f, 0f, faceAngle);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.3f);
        DrawCircle(orbitCenter, orbitRadius, 48);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(orbitCenter, 0.25f);

        DrawOrbitPoint(sunStartAngle, Color.cyan, 0.4f);
        DrawOrbitPoint(sunEndAngle, Color.red, 0.4f);
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
        Vector3 prev = new Vector3(center.x + Mathf.Cos(0f) * radius, center.y + Mathf.Sin(0f) * radius, 0f);
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