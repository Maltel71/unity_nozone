using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Coyote Jump")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Fall Gravity")]
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float jumpGravityMultiplier = 1f;
    [SerializeField] private float baseGravityScale = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D _rb;
    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isRunning;
    private bool _isCrouching;
    private float _moveInput;
    private float _speedMultiplier = 1f;

    private float _coyoteTimer;
    private float _jumpBufferTimer;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void Update()
    {
        CheckGrounded();
        UpdateCoyoteTime();
        UpdateJumpBuffer();
        UpdateFallGravity();
    }

    private void FixedUpdate()
    {
        float speed = (_isRunning && !_isCrouching ? runSpeed : walkSpeed) * _speedMultiplier;
        _rb.linearVelocity = new Vector2(_moveInput * speed, _rb.linearVelocity.y);

        if (_moveInput != 0f)
            transform.localScale = new Vector3(Mathf.Sign(_moveInput), 1f, 1f);

        // Consume buffered jump in FixedUpdate so physics is applied correctly
        if (_jumpBufferTimer > 0f && CanCoyoteJump())
        {
            ExecuteJump();
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
        }
    }

    private void CheckGrounded()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void UpdateCoyoteTime()
    {
        if (_wasGrounded && !_isGrounded)
            _coyoteTimer = coyoteTime;
        else if (_isGrounded)
            _coyoteTimer = 0f;
        else
            _coyoteTimer -= Time.deltaTime;
    }

    private void UpdateJumpBuffer()
    {
        if (_jumpBufferTimer > 0f)
            _jumpBufferTimer -= Time.deltaTime;
    }

    private void UpdateFallGravity()
    {
        if (_rb.linearVelocity.y < 0f)
            _rb.gravityScale = baseGravityScale * fallGravityMultiplier;
        else if (_rb.linearVelocity.y > 0f)
            _rb.gravityScale = baseGravityScale * jumpGravityMultiplier;
        else
            _rb.gravityScale = baseGravityScale;
    }

    private bool CanCoyoteJump() => _isGrounded || _coyoteTimer > 0f;

    private void ExecuteJump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
    }

    public void SetMoveInput(float value) => _moveInput = value;
    public void SetSpeedMultiplier(float value) => _speedMultiplier = Mathf.Max(value, 0f);

    public void Jump()
    {
        _jumpBufferTimer = jumpBufferTime;
    }

    public void StartRun() => _isRunning = true;
    public void StopRun() => _isRunning = false;
    public void SetCrouch(bool value) => _isCrouching = value;
    public void Interact() => Debug.Log("Interact");

    public bool IsRunning => _isRunning;
    public bool IsCrouching => _isCrouching;
    public bool IsGrounded => _isGrounded;
}