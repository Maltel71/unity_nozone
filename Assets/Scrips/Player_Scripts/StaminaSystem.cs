using UnityEngine;
using UnityEngine.UI;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float runDrainPerSecond = 15f;
    [SerializeField] private float walkDrainPerSecond = 0f;
    [SerializeField] private float regenPerSecond = 10f;
    [SerializeField] private float jumpCost = 10f;

    [Header("Regen")]
    [SerializeField] private float regenDelay = 1.5f;
    [SerializeField] private float regenAcceleration = 5f; // rate/sec² the regen speeds up

    [Header("Low Stamina")]
    [SerializeField] private float reducedJumpMultiplier = 0.5f;

    [Header("UI")]
    [SerializeField] private Slider staminaBar;

    private float _currentStamina;
    private float _regenDelayTimer;
    private float _currentRegenRate;

    public float CurrentStamina => _currentStamina;
    public float MaxStamina => maxStamina;
    public float JumpMultiplier => _currentStamina > 0f ? 1f : reducedJumpMultiplier;

    private void Start()
    {
        _currentStamina = maxStamina;
        UpdateBar();
    }

    public bool CanRun() => _currentStamina > 0f;
    public bool CanJump() => _currentStamina >= jumpCost;

    public void UseJumpStamina()
    {
        _currentStamina = Mathf.Max(_currentStamina - jumpCost, 0f);
        ResetRegen();
        UpdateBar();
    }

    public void DrainRunStamina(float deltaTime)
    {
        _currentStamina = Mathf.Max(_currentStamina - runDrainPerSecond * deltaTime, 0f);
        ResetRegen();
        UpdateBar();
    }

    public void DrainWalkStamina(float deltaTime)
    {
        _currentStamina = Mathf.Max(_currentStamina - walkDrainPerSecond * deltaTime, 0f);
        ResetRegen();
        UpdateBar();
    }

    public void RegenerateStamina(float deltaTime)
    {
        if (_regenDelayTimer > 0f)
        {
            _regenDelayTimer -= deltaTime;
            return;
        }

        _currentRegenRate = Mathf.Min(_currentRegenRate + regenAcceleration * deltaTime, regenPerSecond);
        _currentStamina = Mathf.Min(_currentStamina + _currentRegenRate * deltaTime, maxStamina);
        UpdateBar();
    }

    private void ResetRegen()
    {
        _regenDelayTimer = regenDelay;
        _currentRegenRate = 0f;
    }

    private void UpdateBar()
    {
        if (staminaBar == null) return;
        float normalized = _currentStamina / maxStamina;
        if (!Mathf.Approximately(staminaBar.value, normalized))
            staminaBar.value = normalized;
    }
}