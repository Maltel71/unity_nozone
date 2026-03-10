using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D _rb;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _isCrouching;
    private float _moveInput;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void Update() => CheckGrounded();

    private void FixedUpdate()
    {
        float speed = _isRunning && !_isCrouching ? runSpeed : walkSpeed;
        _rb.linearVelocity = new Vector2(_moveInput * speed, _rb.linearVelocity.y);
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void SetMoveInput(float value) => _moveInput = value;
    public void Jump()
    {
        if (_isGrounded)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
    }
    public void StartRun() => _isRunning = true;
    public void StopRun() => _isRunning = false;
    public void SetCrouch(bool value) => _isCrouching = value;
    public void Interact() => Debug.Log("Interact");

    public bool IsRunning => _isRunning;
    public bool IsCrouching => _isCrouching;
    public bool IsGrounded => _isGrounded;
}