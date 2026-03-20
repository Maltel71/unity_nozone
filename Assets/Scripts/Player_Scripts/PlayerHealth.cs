using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum DamageSource { Default, Sunlight }

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

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("Death")]
    [SerializeField] private float gameOverDelay = 2f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;

    [Header("VFX")]
    [SerializeField] private ParticleSystem hitParticles;

    [Header("FMOD")]
    [SerializeField] private string fmodHealthParameter = "PlayerHealthNormalized";
    

    public event System.Action<DamageSource> OnDamaged;
    public event System.Action OnDied;

    private float _currentHealth;
    private float _regenDelayTimer;
    private float _currentRegenRate;
    private float _knockbackTimer;
    private bool _isDead;

    private bool _canRunAudio = true;

    private PlayerInputHandler _inputHandler;
    private CharacterController2D _controller;
    private Rigidbody2D _rb;

    public bool IsDead => _isDead;
    public bool IsKnockedBack => _knockbackTimer > 0f;

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _controller = GetComponent<CharacterController2D>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _currentHealth = maxHealth;
        UpdateBar();
    }

    private void Update()
    {
        HandleRegen();
        if (_knockbackTimer > 0f) _knockbackTimer -= Time.deltaTime;
    }

    public void TakeDamage(float amount, DamageSource source = DamageSource.Default, Vector2? sourcePosition = null)
    {
        if (_isDead) return;

        _currentHealth = Mathf.Max(_currentHealth - amount, 0f);
        ResetRegen();
        UpdateBar();

        OnDamaged?.Invoke(source);

        if (source != DamageSource.Default)
        {
            _canRunAudio = false;
            // FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Reaction/CatHurt");
            _canRunAudio = true;
        }

        if (source != DamageSource.Sunlight && _canRunAudio)
        {
            _canRunAudio = false;
            // FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Reaction/Burning");
        }

        if (source != DamageSource.Sunlight)
            hitParticles?.Play();
        

        if (sourcePosition.HasValue && source != DamageSource.Sunlight)
            ApplyKnockback(sourcePosition.Value);

        if (_currentHealth <= 0f)
            Die();
           // FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Reaction/CatHurt");
    }


    private void ApplyKnockback(Vector2 sourcePosition)
    {
        Vector2 dir = ((Vector2)transform.position - sourcePosition).normalized;
        _rb.linearVelocity = dir * knockbackForce;
        _knockbackTimer = knockbackDuration;
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
        float normalized = _currentHealth / maxHealth;

        if (healthBar != null && !Mathf.Approximately(healthBar.value, normalized))
            healthBar.value = normalized;

        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(fmodHealthParameter, normalized);
    }

    private void Die()
    {
        _isDead = true;

        if (_inputHandler != null)
            _inputHandler.DisableInput();

        if (_controller != null)
            _controller.SetMoveInput(0f);

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