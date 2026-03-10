using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SunlightBurn : MonoBehaviour
{
    [Header("Burn Settings")]
    [SerializeField] private float burnDamagePerTick = 5f;
    [SerializeField] private float burnDelay = 1f;
    [SerializeField] private float burnTickRate = 0.5f;

    [Header("Light Detection")]
    [SerializeField] private Light2D[] sunLights;
    [SerializeField] private LayerMask shadowCasterLayers;

    private PlayerHealth _playerHealth;
    private Coroutine _burnCoroutine;
    private bool _isInSunlight;

    private void Awake()
    {
        _playerHealth = GetComponentInParent<PlayerHealth>();
    }

    private void Update()
    {
        bool exposed = CheckSunlightExposure();

        if (exposed && !_isInSunlight)
        {
            _isInSunlight = true;
            _burnCoroutine = StartCoroutine(BurnRoutine());
        }
        else if (!exposed && _isInSunlight)
        {
            _isInSunlight = false;
            if (_burnCoroutine != null)
            {
                StopCoroutine(_burnCoroutine);
                _burnCoroutine = null;
            }
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

        while (_isInSunlight)
        {
            _playerHealth.TakeDamage(burnDamagePerTick);
            yield return new WaitForSeconds(burnTickRate);
        }
    }
}