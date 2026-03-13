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

    [Header("VFX")]
    [SerializeField] private ParticleSystem burningParticle;

    private PlayerHealth _playerHealth;
    private Coroutine _burnCoroutine;
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
            if (burningParticle != null) burningParticle.Play();
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
        if (burningParticle != null) burningParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (_burnCoroutine != null)
        {
            StopCoroutine(_burnCoroutine);
            _burnCoroutine = null;
        }
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

        yield return new WaitForSeconds(burnDelay);

        float currentRate = !useAccelerando ? burnTickRate : accelerandoStartRate;

        while (_isInSunlight)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Reaction/Burning");
            _playerHealth.TakeDamage(burnDamagePerTick, DamageSource.Sunlight);
            yield return new WaitForSeconds(currentRate);

            if (!useAccelerando)
                continue;
            currentRate = Mathf.Max(currentRate - accelerandoSpeed * currentRate * Time.deltaTime, accelerandoMinRate);
        }
    }
}