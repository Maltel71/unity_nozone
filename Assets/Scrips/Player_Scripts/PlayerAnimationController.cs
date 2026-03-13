using UnityEngine;

/// <summary>
/// Reads player state from existing systems and drives all Animator parameters
/// from a single centralized script. Contains no gameplay logic.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CharacterController2D movementController;
    [SerializeField] private PickupSystem pickupSystem;
    [SerializeField] private PlayerHealth playerHealth;

    // ── Animator Parameters ────────────────────────────────────────────────────
    // Bools
    // IsMoving     → blends Idle / Walk / Run
    // IsRunning    → selects Run over Walk
    // IsGrounded   → used for jump/fall transitions
    // IsJumping    → drives Player_Jump_InAir_Anim loop
    // IsFalling    → optional fall blend
    // IsDead       → locks into death idle after death anim finishes
    //
    // Triggers
    // Jump         → Player_Jump_Anim
    // Land         → Player_Jump_Landing_Anim
    // Damage       → Player_TakeDamage_Anim
    // Death        → Player_Death_Anim → Player_Death_Idle_Anim
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Bool Parameters")]
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string isRunningParam = "IsRunning";
    [SerializeField] private string isGroundedParam = "IsGrounded";
    [SerializeField] private string isJumpingParam = "IsJumping";
    [SerializeField] private string isFallingParam = "IsFalling";
    [SerializeField] private string isDeadParam = "IsDead";

    [Header("Trigger Parameters")]
    [SerializeField] private string jumpTrigger = "Jump";
    [SerializeField] private string landTrigger = "Land";
    [SerializeField] private string damageTrigger = "Damage";
    [SerializeField] private string deathTrigger = "Death";

    private bool _wasGrounded;
    private bool _isJumping;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (movementController == null) movementController = GetComponent<CharacterController2D>();
        if (pickupSystem == null) pickupSystem = GetComponent<PickupSystem>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (movementController != null)
            movementController.OnJumped += OnJumped;

        if (playerHealth != null)
        {
            playerHealth.OnDamaged += OnDamaged;
            playerHealth.OnDied += OnDied;
        }
    }

    private void OnDisable()
    {
        if (movementController != null)
            movementController.OnJumped -= OnJumped;

        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= OnDamaged;
            playerHealth.OnDied -= OnDied;
        }
    }

    private void Update()
    {
        UpdateMovement();
        UpdateGrounded();
        UpdateFalling();
    }

    // ─── Movement ─────────────────────────────────────────────────────────────

    private void UpdateMovement()
    {
        if (movementController == null || rb == null) return;

        float speed = Mathf.Abs(rb.linearVelocity.x);
        bool isMoving = speed > 0.05f;
        bool isRunning = movementController.IsRunning && isMoving;

        animator.SetBool(isMovingParam, isMoving);
        animator.SetBool(isRunningParam, isRunning);
    }

    // ─── Jump / Ground ────────────────────────────────────────────────────────

    private void OnJumped()
    {
        _isJumping = true;
        animator.SetBool(isJumpingParam, true);
        animator.SetBool(isGroundedParam, false);
        animator.SetTrigger(jumpTrigger);
    }

    private void UpdateGrounded()
    {
        if (movementController == null) return;

        bool grounded = movementController.IsGrounded;
        animator.SetBool(isGroundedParam, grounded);

        if (grounded && !_wasGrounded && _isJumping)
        {
            _isJumping = false;
            animator.SetBool(isJumpingParam, false);
            animator.SetBool(isFallingParam, false);
            animator.SetTrigger(landTrigger);
        }

        _wasGrounded = grounded;
    }

    private void UpdateFalling()
    {
        if (rb == null || movementController == null) return;

        bool falling = rb.linearVelocity.y < -0.1f && !movementController.IsGrounded;
        animator.SetBool(isFallingParam, falling);
    }

    // ─── Combat ───────────────────────────────────────────────────────────────

    private void OnDamaged()
    {
        animator.SetTrigger(damageTrigger);
    }

    private void OnDied()
    {
        animator.SetBool(isDeadParam, true);
        animator.SetTrigger(deathTrigger);
    }
}