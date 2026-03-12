using UnityEngine;

public class CameraShake2D : MonoBehaviour
{
    public static CameraShake2D Instance { get; private set; }

    [Header("Defaults")]
    [SerializeField] private float defaultIntensity = 0.2f;
    [SerializeField] private float defaultDuration = 0.3f;
    [SerializeField] private float defaultSpeed = 25f;

    [Header("Current Shake")]
    [SerializeField] private float shakeIntensity;
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeSpeed;

    [Header("Limits")]
    [SerializeField] private float maxShakeDistance = 1f;
    [SerializeField] private bool allowStacking = false;

    private float _shakeTimer;

    /// <summary>
    /// The current shake offset to be applied by CameraFollow2D when writing the final position.
    /// </summary>
    public Vector2 ShakeOffset { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (_shakeTimer <= 0f)
        {
            ShakeOffset = Vector2.zero;
            return;
        }

        _shakeTimer -= Time.deltaTime;

        if (_shakeTimer <= 0f)
        {
            ShakeOffset = Vector2.zero;
            return;
        }

        float clampedIntensity = Mathf.Min(shakeIntensity, maxShakeDistance);
        ShakeOffset = Random.insideUnitCircle * clampedIntensity;
    }

    /// <summary>
    /// Triggers a shake with custom intensity, duration, and speed.
    /// If a shake is already active, the stronger one takes priority unless AllowStacking is enabled.
    /// </summary>
    public void TriggerShake(float intensity, float duration, float speed)
    {
        if (_shakeTimer > 0f && !allowStacking)
        {
            if (intensity <= shakeIntensity) return;
        }

        shakeIntensity = intensity;
        shakeDuration = duration;
        shakeSpeed = speed;
        _shakeTimer = duration;
    }

    /// <summary>
    /// Triggers a shake using the default inspector values.
    /// </summary>
    public void TriggerShake()
    {
        TriggerShake(defaultIntensity, defaultDuration, defaultSpeed);
    }
}