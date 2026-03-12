using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireFlicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light2D fireLight;

    [Header("Intensity")]
    [SerializeField] private float baseIntensity = 1f;
    [SerializeField] private float flickerAmount = 0.3f;
    [SerializeField] private float flickerSpeed = 8f;

    [Header("Radius")]
    [SerializeField] private float radiusFlickerAmount = 0f;

    private float _baseOuterRadius;
    private float _targetIntensity;
    private float _targetRadius;

    private void Start()
    {
        if (fireLight == null) return;

        _baseOuterRadius = fireLight.pointLightOuterRadius;
        PickNewTargets();
    }

    private void Update()
    {
        if (fireLight == null) return;

        fireLight.intensity = Mathf.Lerp(fireLight.intensity, _targetIntensity, flickerSpeed * Time.deltaTime);

        if (radiusFlickerAmount > 0f)
            fireLight.pointLightOuterRadius = Mathf.Lerp(fireLight.pointLightOuterRadius, _targetRadius, flickerSpeed * Time.deltaTime);

        if (Mathf.Abs(fireLight.intensity - _targetIntensity) < 0.01f)
            PickNewTargets();
    }

    private void PickNewTargets()
    {
        _targetIntensity = baseIntensity + Random.Range(-flickerAmount, flickerAmount);

        if (radiusFlickerAmount > 0f)
            _targetRadius = _baseOuterRadius + Random.Range(-radiusFlickerAmount, radiusFlickerAmount);
    }
}