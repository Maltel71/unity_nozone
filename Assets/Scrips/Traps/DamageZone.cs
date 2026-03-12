using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private string playerTag = "Player";

    private float _damageTimer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        ApplyDamage(other);
        _damageTimer = damageInterval;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        _damageTimer -= Time.fixedDeltaTime;
        if (_damageTimer <= 0f)
        {
            ApplyDamage(other);
            _damageTimer = damageInterval;
        }
    }

    private void ApplyDamage(Collider2D other)
    {
        other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
    }
}