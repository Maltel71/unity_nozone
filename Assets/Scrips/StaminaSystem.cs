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

    [Header("Low Stamina")]
    [SerializeField] private float reducedJumpMultiplier = 0.5f;

    [Header("UI")]
    [SerializeField] private Slider staminaBar;

    private float _currentStamina;

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
        UpdateBar();
    }

    public void DrainRunStamina(float deltaTime)
    {
        _currentStamina = Mathf.Max(_currentStamina - runDrainPerSecond * deltaTime, 0f);
        UpdateBar();
    }

    public void DrainWalkStamina(float deltaTime)
    {
        _currentStamina = Mathf.Max(_currentStamina - walkDrainPerSecond * deltaTime, 0f);
        UpdateBar();
    }

    public void RegenerateStamina(float deltaTime)
    {
        _currentStamina = Mathf.Min(_currentStamina + regenPerSecond * deltaTime, maxStamina);
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (staminaBar != null)
            staminaBar.value = _currentStamina / maxStamina;
    }
}