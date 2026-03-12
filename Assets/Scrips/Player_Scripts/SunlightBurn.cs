using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SunlightBurn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayNightCycle dayNightCycle;

    [Header("Burn Settings")]
    [SerializeField] private float burnDamagePerTick = 5f;
    [SerializeField] private float burnDelay = 1f;
    [SerializeField] private float burnTickRate = 0.5f;

    [Header("Accelerando")]
    [SerializeField] private bool useAccelerando = true;
    [SerializeField] private float accelerandoStartRate = 2f;
    [SerializeField] private float accelerandoMinRate = 0.1f;
    [SerializeField] private float accelerandoSpeed = 0.5f;

    [Header("Light Detection")]
    [SerializeField] private Light2D[] sunLights;
    [SerializeField] private LayerMask shadowCasterLayers;

    [Header("Camera Shake")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeSpeed = 15f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem smokingParticles;
    [SerializeField] private float smokeDelay = 0.5f;
    [SerializeField] private ParticleSystem burningParticles;

    private PlayerHealth _playerHealth;
    private bool _exposed;
    private Coroutine _burnCoroutine;

    private void Awake()
    {
        _playerHealth = GetComponentInParent<PlayerHealth>();
    }

    private void Update()
    {
        bool burnAllowed = dayNightCycle == null || dayNightCycle.BurnEnabled;
        bool wasExposed = _exposed;
        _exposed = burnAllowed && CheckSunlightExposure();

        if (_exposed && !wasExposed)
        {
            if (_burnCoroutine != null) StopCoroutine(_burnCoroutine);
            _burnCoroutine = StartCoroutine(BurnSequence());
        }
        else if (!_exposed && wasExposed)
        {
            if (_burnCoroutine != null)
            {
                StopCoroutine(_burnCoroutine);
                _burnCoroutine = null;
            }
            smokingParticles?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            burningParticles?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private IEnumerator BurnSequence()
    {
        if (smokeDelay > 0f)
            yield return new WaitForSeconds(smokeDelay);

        smokingParticles?.Play();

        float remainingBurnDelay = Mathf.Max(0f, burnDelay - smokeDelay);
        if (remainingBurnDelay > 0f)
            yield return new WaitForSeconds(remainingBurnDelay);

        burningParticles?.Play();

        float currentRate = useAccelerando ? accelerandoStartRate : burnTickRate;

        while (true)
        {
            _playerHealth?.TakeDamage(burnDamagePerTick);
            CameraShake2D.Instance?.TriggerShake(shakeIntensity, shakeDuration, shakeSpeed);

            yield return new WaitForSeconds(currentRate);

            if (useAccelerando)
                currentRate = Mathf.Max(currentRate - accelerandoSpeed * currentRate, accelerandoMinRate);
        }
    }

    private bool CheckSunlightExposure()
    {
        foreach (Light2D light in sunLights)
        {
            if (light == null || !light.enabled) continue;
            Vector2 dir = (Vector2)light.transform.position - (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, shadowCasterLayers);
            if (!hit) return true;
        }
        return false;
    }
}