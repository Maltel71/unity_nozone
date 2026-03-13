using UnityEngine;

/// <summary>
/// Fully code-driven animation controller.
/// Directly calls Animator.Play — no Animator transitions required.
/// Animator states must be named identically to their AnimationClip names.
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

    [Header("Movement")]
    [SerializeField] private AnimationClip idleAnimation;
    [SerializeField] private AnimationClip walkAnimation;
    [SerializeField] private AnimationClip runAnimation;

    [Header("Jump")]
    [SerializeField] private AnimationClip jumpAnimation;
    [SerializeField] private AnimationClip fallAnimation;
    [SerializeField] private AnimationClip landAnimation;

    [Header("Carry")]
    [SerializeField] private AnimationClip pickupAnimation;
    [SerializeField] private AnimationClip carryIdleAnimation;
    [SerializeField] private AnimationClip carryWalkAnimation;
    [SerializeField] private AnimationClip dropAnimation;

    [Header("Combat")]
    [SerializeField] private AnimationClip damageAnimation;
    [SerializeField] private AnimationClip deathAnimation;

    [Header("Debug")]
    [SerializeField] private string currentAnimationName;

    private string _currentClipName;
    private bool _isDead;
    private bool _isJumping;
    private bool _isOneShotPlaying;
    private float _oneShotEndTime;
    private bool _wasGrounded;
    private bool _wasCarrying;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (movementController == null) movementController = GetComponent<CharacterController2D>();
        if (pickupSystem == null) pickupSystem = GetComponent<PickupSystem>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        _wasGrounded = movementController != null && movementController.IsGrounded;
        _wasCarrying = pickupSystem != null && pickupSystem.IsCarrying;
        Play(idleAnimation);
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
        if (_isDead) return;

        // Release one-shot lock once the clip duration has elapsed
        if (_isOneShotPlaying && Time.time >= _oneShotEndTime)
            _isOneShotPlaying = false;

        if (_isOneShotPlaying) return;

        bool grounded = movementController != null && movementController.IsGrounded;
        bool carrying = pickupSystem != null && pickupSystem.IsCarrying;

        // Landing — any transition from airborne to grounded
        if (grounded && !_wasGrounded)
        {
            _isJumping = false;
            _wasGrounded = grounded;
            _wasCarrying = carrying;
            PlayOneShot(landAnimation);
            return;
        }
        _wasGrounded = grounded;

        // Pickup / drop transitions
        if (carrying != _wasCarrying)
        {
            AnimationClip transitionClip = carrying ? pickupAnimation : dropAnimation;
            _wasCarrying = carrying;

            if (transitionClip != null)
            {
                PlayOneShot(transitionClip);
                return;
            }
        }

        SelectLocomotion(grounded, carrying);
    }

    // ─── Locomotion Selection ─────────────────────────────────────────────────

    private void SelectLocomotion(bool grounded, bool carrying)
    {
        float speedX = rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
        float speedY = rb != null ? rb.linearVelocity.y : 0f;
        bool moving = speedX > 0.05f;
        bool running = movementController != null && movementController.IsRunning && moving;

        // Airborne
        if (!grounded)
        {
            // Switch from jump to fall once we begin descending
            if (_isJumping && speedY < 0f)
                _isJumping = false;

            Play(_isJumping ? jumpAnimation : fallAnimation);
            return;
        }

        // Grounded + carrying
        if (carrying)
        {
            Play(moving
                ? carryWalkAnimation != null ? carryWalkAnimation : walkAnimation
                : carryIdleAnimation != null ? carryIdleAnimation : idleAnimation);
            return;
        }

        // Grounded locomotion
        if (running) Play(runAnimation);
        else if (moving) Play(walkAnimation);
        else Play(idleAnimation);
    }

    // ─── Event Callbacks ──────────────────────────────────────────────────────

    private void OnJumped()
    {
        _isJumping = true;
        _isOneShotPlaying = false; // Jump always interrupts other one-shots
        Play(jumpAnimation);
    }

    private void OnDamaged()
    {
        if (damageAnimation == null) return;
        PlayOneShot(damageAnimation);
    }

    private void OnDied()
    {
        _isDead = true;
        _isOneShotPlaying = false;
        Play(deathAnimation);
    }

    // ─── Playback Helpers ─────────────────────────────────────────────────────

    /// <summary>Plays a looping or persistent animation. Skips if already playing.</summary>
    private void Play(AnimationClip clip)
    {
        if (clip == null || _currentClipName == clip.name) return;

        _currentClipName = clip.name;
        currentAnimationName = clip.name;
        animator.Play(clip.name);
    }

    /// <summary>Plays a one-shot animation and locks locomotion for its full duration.</summary>
    private void PlayOneShot(AnimationClip clip)
    {
        if (clip == null) return;

        _isOneShotPlaying = true;
        _oneShotEndTime = Time.time + clip.length;
        _currentClipName = clip.name;
        currentAnimationName = clip.name;
        animator.Play(clip.name);
    }
}