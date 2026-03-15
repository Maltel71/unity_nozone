using UnityEngine;

public class DamageZoneRollingBoulder : MonoBehaviour
{
    [Header("Speed Threshold")]
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 10f;

    [Header("Damage")]
    [SerializeField] private float minDamage = 5f;
    [SerializeField] private float maxDamage = 40f;
    [SerializeField] private float damageInterval = 0.3f;

    [Header("Debug")]
    [SerializeField] private float _currentSpeed;
    [SerializeField] private float _currentDamage;

    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D _rb;
    private float _damageTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _currentSpeed = _rb != null ? _rb.linearVelocity.magnitude : 0f;
        _currentDamage = _currentSpeed >= minSpeed
            ? Mathf.Lerp(minDamage, maxDamage, Mathf.InverseLerp(minSpeed, maxSpeed, _currentSpeed))
            : 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_currentSpeed < minSpeed) return;
        ApplyDamage(other);
        _damageTimer = damageInterval;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_currentSpeed < minSpeed) return;

        _damageTimer -= Time.fixedDeltaTime;
        if (_damageTimer <= 0f)
        {
            ApplyDamage(other);
            _damageTimer = damageInterval;
        }
    }

    private void ApplyDamage(Collider2D other)
    {
        var health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        health.TakeDamage(_currentDamage);

        if (!health.IsDead)
            CameraShake2D.Instance?.TriggerShake();
    }
}