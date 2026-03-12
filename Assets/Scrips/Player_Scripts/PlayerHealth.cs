using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Regeneration")]
    [SerializeField] private bool regenEnabled = true;
    [SerializeField] private float regenDelay = 3f;
    [SerializeField] private float regenPerSecond = 5f;

    [Header("Regen Accelerando")]
    [SerializeField] private bool useAccelerando = true;
    [SerializeField] private float accelerandoStartRate = 1f;
    [SerializeField] private float accelerandoAcceleration = 3f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;

    private float _currentHealth;
    private float _regenDelayTimer;
    private float _currentRegenRate;

    private void Start()
    {
        _currentHealth = maxHealth;
        UpdateBar();
    }

    private void Update()
    {
        HandleRegen();
    }

    public void TakeDamage(float amount)
    {
        _currentHealth = Mathf.Max(_currentHealth - amount, 0f);
        ResetRegen();
        UpdateBar();

        if (_currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        UpdateBar();
    }

    private void HandleRegen()
    {
        if (!regenEnabled || Mathf.Approximately(_currentHealth, maxHealth)) return;

        if (_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= Time.deltaTime;
            return;
        }

        if (useAccelerando)
            _currentRegenRate = Mathf.Min(_currentRegenRate + accelerandoAcceleration * Time.deltaTime, regenPerSecond);
        else
            _currentRegenRate = regenPerSecond;

        _currentHealth = Mathf.Min(_currentHealth + _currentRegenRate * Time.deltaTime, maxHealth);
        UpdateBar();
    }

    private void ResetRegen()
    {
        _regenDelayTimer = regenDelay;
        _currentRegenRate = useAccelerando ? accelerandoStartRate : regenPerSecond;
    }

    private void UpdateBar()
    {
        if (healthBar == null) return;
        float normalized = _currentHealth / maxHealth;
        if (!Mathf.Approximately(healthBar.value, normalized))
            healthBar.value = normalized;
    }

    private void Die()
    {
        Debug.Log("Player died.");
        // Hook death logic here
    }
}