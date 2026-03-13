using System.Collections;
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

    [Header("Death")]
    [SerializeField] private string deathAnimationTrigger = "Death";
    [SerializeField] private float gameOverDelay = 2f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;

    public event System.Action OnDamaged;
    public event System.Action OnDied;

    private float _currentHealth;
    private float _regenDelayTimer;
    private float _currentRegenRate;
    private bool _isDead;

    private Animator _animator;
    private PlayerInputHandler _inputHandler;
    private CharacterController2D _controller;

    public bool IsDead => _isDead;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _inputHandler = GetComponent<PlayerInputHandler>();
        _controller = GetComponent<CharacterController2D>();
    }

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
        if (_isDead) return;

        _currentHealth = Mathf.Max(_currentHealth - amount, 0f);
        ResetRegen();
        UpdateBar();

        OnDamaged?.Invoke();

        if (_currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (_isDead) return;

        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        UpdateBar();
    }

    private void HandleRegen()
    {
        if (_isDead) return;
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
        _isDead = true;

        if (_inputHandler != null)
            _inputHandler.DisableInput();

        if (_controller != null)
            _controller.SetMoveInput(0f);

        if (_animator != null && !string.IsNullOrEmpty(deathAnimationTrigger))
            _animator.SetTrigger(deathAnimationTrigger);

        OnDied?.Invoke();

        StartCoroutine(ShowGameOverAfterDelay());
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.ShowGameOver();
        else
            Debug.Log("[PlayerHealth] Player died — no GameOverManager found in scene.");
    }
}