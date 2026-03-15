using UnityEngine;

public enum CrabState { Idle, Patrol, Shooting }

[RequireComponent(typeof(Rigidbody2D))]
public class ShellCrabController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Raycasts")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float edgeCheckDistance = 0.8f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Raycast Offsets")]
    [SerializeField] private Vector2 frontWallOffset = new Vector2(0.4f, 0f);
    [SerializeField] private Vector2 backWallOffset = new Vector2(0.4f, 0f);
    [SerializeField] private Vector2 frontEdgeOffset = new Vector2(0.4f, -0.3f);
    [SerializeField] private Vector2 backEdgeOffset = new Vector2(0.4f, -0.3f);

    [Header("Edge Turn Cooldown")]
    [SerializeField] private float edgeTurnCooldown = 0.5f;

    [Header("Day/Night")]
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private float sleepDelay = 0f;
    [SerializeField] private float wakeDelay = 0f;

    [Header("Sleep")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite sleepSprite;
    [SerializeField] private Sprite awakeSprite;
    [SerializeField] private ParticleSystem sleepParticles;
    [SerializeField] private float kinematicDelay = 0.5f;

    [Header("Colliders")]
    [SerializeField] private Collider2D sleepCollider;
    [SerializeField] private Collider2D awakeCollider;

    [Header("Damage Trigger")]
    [SerializeField] private Collider2D damageTriggerCollider;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D _rb;
    private CrabState _state = CrabState.Patrol;
    private int _facingDir = 1;
    private float _edgeTurnCooldownTimer;
    private bool _isSleeping;

    public CrabState State => _state;
    public bool IsSleeping => _isSleeping;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (dayNightCycle == null)
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
    }

    private void OnEnable()
    {
        if (dayNightCycle == null) return;
        dayNightCycle.OnSunsetBegin += OnSunsetBegin;
        dayNightCycle.OnSunriseBegin += OnSunriseBegin;
    }

    private void OnDisable()
    {
        if (dayNightCycle == null) return;
        dayNightCycle.OnSunsetBegin -= OnSunsetBegin;
        dayNightCycle.OnSunriseBegin -= OnSunriseBegin;
    }

    private void OnSunsetBegin()
    {
        StopAllCoroutines();
        if (wakeDelay > 0f)
            StartCoroutine(DelayedCall(ExitSleepState, wakeDelay));
        else
            ExitSleepState();
    }

    private void OnSunriseBegin()
    {
        StopAllCoroutines();
        if (sleepDelay > 0f)
            StartCoroutine(DelayedCall(EnterSleepState, sleepDelay));
        else
            EnterSleepState();
    }

    private System.Collections.IEnumerator DelayedCall(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    private System.Collections.IEnumerator DelayedKinematic()
    {
        yield return new WaitForSeconds(kinematicDelay);
        if (_isSleeping)
            _rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Start()
    {
        bool isDay = dayNightCycle != null &&
                     (dayNightCycle.State == DayNightCycle.CycleState.Day ||
                      dayNightCycle.State == DayNightCycle.CycleState.Sunset);

        if (isDay)
            EnterSleepState();
        else
            ExitSleepState();
    }

    private void FixedUpdate()
    {
        if (_isSleeping)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }

        switch (_state)
        {
            case CrabState.Patrol: Patrol(); break;
            case CrabState.Shooting:
            case CrabState.Idle:
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                break;
        }

        if (_edgeTurnCooldownTimer > 0f)
            _edgeTurnCooldownTimer -= Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isSleeping) return;
        if (!other.CompareTag(playerTag)) return;

        other.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage, DamageSource.Default, transform.position);
    }

    // Blocked while sleeping so ShellCrabShooter cannot override the idle state
    public void SetState(CrabState state)
    {
        if (_isSleeping) return;
        _state = state;
    }

    private void EnterSleepState()
    {
        _isSleeping = true;
        _state = CrabState.Idle;

        StartCoroutine(DelayedKinematic());

        if (spriteRenderer != null && sleepSprite != null)
            spriteRenderer.sprite = sleepSprite;

        if (awakeCollider != null) awakeCollider.enabled = false;
        if (sleepCollider != null) sleepCollider.enabled = true;
        if (damageTriggerCollider != null) damageTriggerCollider.enabled = false;
        if (sleepParticles != null) sleepParticles.Play();
    }

    private void ExitSleepState()
    {
        _isSleeping = false;
        _state = CrabState.Patrol;
        _rb.bodyType = RigidbodyType2D.Dynamic;

        if (spriteRenderer != null && awakeSprite != null)
            spriteRenderer.sprite = awakeSprite;

        if (sleepCollider != null) sleepCollider.enabled = false;
        if (awakeCollider != null) awakeCollider.enabled = true;
        if (damageTriggerCollider != null) damageTriggerCollider.enabled = true;
        if (sleepParticles != null) sleepParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void Patrol()
    {
        CheckTurn();
        _rb.linearVelocity = new Vector2(_facingDir * patrolSpeed, _rb.linearVelocity.y);
    }

    private void CheckTurn()
    {
        if (HitsWall())
        {
            Flip();
            return;
        }

        if (_edgeTurnCooldownTimer > 0f) return;

        if (DetectsEdge())
            Flip();
    }

    private bool HitsWall()
    {
        Vector2 origin = transform.position;

        Vector2 frontWallOrigin = origin + new Vector2(frontWallOffset.x * _facingDir, frontWallOffset.y);
        bool frontWall = Physics2D.Raycast(frontWallOrigin, Vector2.right * _facingDir, wallCheckDistance, groundLayer);

        Vector2 backWallOrigin = origin + new Vector2(-backWallOffset.x * _facingDir, backWallOffset.y);
        bool backWall = Physics2D.Raycast(backWallOrigin, Vector2.right * -_facingDir, wallCheckDistance, groundLayer);

        return frontWall || backWall;
    }

    private bool DetectsEdge()
    {
        Vector2 origin = transform.position;

        Vector2 frontEdgeOrigin = origin + new Vector2(frontEdgeOffset.x * _facingDir, frontEdgeOffset.y);
        bool noFrontGround = !Physics2D.Raycast(frontEdgeOrigin, Vector2.down, edgeCheckDistance, groundLayer);

        Vector2 backEdgeOrigin = origin + new Vector2(-backEdgeOffset.x * _facingDir, backEdgeOffset.y);
        bool noBackGround = !Physics2D.Raycast(backEdgeOrigin, Vector2.down, edgeCheckDistance, groundLayer);

        bool grounded = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

        return noFrontGround || (!grounded && noBackGround);
    }

    private void Flip()
    {
        _facingDir *= -1;
        transform.localScale = new Vector3(_facingDir, 1f, 1f);
        _edgeTurnCooldownTimer = edgeTurnCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;
        int dir = Application.isPlaying ? _facingDir : 1;

        Vector2 frontWallOrigin = origin + new Vector2(frontWallOffset.x * dir, frontWallOffset.y);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(frontWallOrigin, Vector2.right * dir * wallCheckDistance);

        Vector2 backWallOrigin = origin + new Vector2(-backWallOffset.x * dir, backWallOffset.y);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(backWallOrigin, Vector2.right * -dir * wallCheckDistance);

        Vector2 frontEdgeOrigin = origin + new Vector2(frontEdgeOffset.x * dir, frontEdgeOffset.y);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(frontEdgeOrigin, Vector2.down * edgeCheckDistance);

        Vector2 backEdgeOrigin = origin + new Vector2(-backEdgeOffset.x * dir, backEdgeOffset.y);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(backEdgeOrigin, Vector2.down * edgeCheckDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, Vector2.down * groundCheckDistance);
    }
}