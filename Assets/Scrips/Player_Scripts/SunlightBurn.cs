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
    [SerializeField] private float smokeLingerDuration = 1.5f;
    [SerializeField] private ParticleSystem burningParticles;

    private PlayerHealth _playerHealth;
    private Coroutine _burnCoroutine;
    private Coroutine _smokeLingerCoroutine;
    private bool _isInSunlight;

    private void Awake()
    {
        _playerHealth = GetComponentInParent<PlayerHealth>();
    }

    private void Update()
    {
        if (dayNightCycle != null && !dayNightCycle.BurnEnabled)
        {
            if (_isInSunlight)
                StopBurn();
            return;
        }

        bool exposed = CheckSunlightExposure();

        if (exposed && !_isInSunlight)
        {
            _isInSunlight = true;
            _burnCoroutine = StartCoroutine(BurnRoutine());
        }
        else if (!exposed && _isInSunlight)
        {
            StopBurn();
        }
    }

    private void StopBurn()
    {
        _isInSunlight = false;

        if (_burnCoroutine != null)
        {
            StopCoroutine(_burnCoroutine);
            _burnCoroutine = null;
        }

        burningParticles?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (_smokeLingerCoroutine != null)
            StopCoroutine(_smokeLingerCoroutine);
        _smokeLingerCoroutine = StartCoroutine(SmokeLingerRoutine());
    }

    private IEnumerator SmokeLingerRoutine()
    {
        yield return new WaitForSeconds(smokeLingerDuration);
        smokingParticles?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        _smokeLingerCoroutine = null;
    }

    private bool CheckSunlightExposure()
    {
        foreach (Light2D light in sunLights)
        {
            if (light == null || !light.enabled) continue;

            Vector2 direction = (Vector2)light.transform.position - (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, direction.magnitude, shadowCasterLayers);

            if (!hit) return true;
        }

        return false;
    }

    private IEnumerator BurnRoutine()
    {
        if (_playerHealth == null)
        {
            Debug.LogError("SunlightBurn: PlayerHealth not found.", this);
            yield break;
        }

        // Cancel any linger still running from a previous exposure
        if (_smokeLingerCoroutine != null)
        {
            StopCoroutine(_smokeLingerCoroutine);
            _smokeLingerCoroutine = null;
        }

        // Wait before smoke starts
        yield return new WaitForSeconds(smokeDelay);

        if (!_isInSunlight) yield break;

        if (smokingParticles != null && !smokingParticles.isPlaying)
            smokingParticles.Play();

        float remainingBurnDelay = Mathf.Max(0f, burnDelay - smokeDelay);
        yield return new WaitForSeconds(remainingBurnDelay);

        if (!_isInSunlight) yield break;

        // Damage phase begins — start burning particles
        if (burningParticles != null && !burningParticles.isPlaying)
            burningParticles.Play();

        float currentRate = !useAccelerando ? burnTickRate : accelerandoStartRate;

        while (_isInSunlight)
        {
            _playerHealth.TakeDamage(burnDamagePerTick);
            CameraShake2D.Instance?.TriggerShake(shakeIntensity, shakeDuration, shakeSpeed);

            yield return new WaitForSeconds(currentRate);

            if (!useAccelerando)
                continue;
            currentRate = Mathf.Max(currentRate - accelerandoSpeed * currentRate * Time.deltaTime, accelerandoMinRate);
        }
    }
}