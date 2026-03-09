using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Run Settings")]
    public bool keyboardRunIsToggle = false;
    public bool controllerRunIsToggle = true;

    [Header("Crouch Settings")]
    public bool keyboardCrouchIsToggle = false;
    public bool controllerCrouchIsToggle = false;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D _rb;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _isCrouching;
    private float _moveInput;

    private bool _keyboardRunToggleState;
    private bool _controllerRunToggleState;
    private bool _keyboardCrouchToggleState;
    private bool _controllerCrouchToggleState;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckGrounded();
        HandleMovementInput();
        HandleJumpInput();
        HandleRunInput();
        HandleCrouchInput();
        HandleInteractInput();
    }

    private void FixedUpdate()
    {
        float speed = _isRunning && !_isCrouching ? runSpeed : walkSpeed;
        _rb.linearVelocity = new Vector2(_moveInput * speed, _rb.linearVelocity.y);
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleMovementInput()
    {
        _moveInput = 0f;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) _moveInput -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) _moveInput += 1f;
        }

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            float stickX = gamepad.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.1f)
                _moveInput = stickX;

            if (controllerRunIsToggle && _controllerRunToggleState && Mathf.Abs(_moveInput) < 0.1f)
                _controllerRunToggleState = false;
        }
    }

    private void HandleJumpInput()
    {
        if (!_isGrounded) return;

        bool jumpPressed = false;

        var kb = Keyboard.current;
        if (kb != null)
            jumpPressed = kb.spaceKey.wasPressedThisFrame
                       || kb.wKey.wasPressedThisFrame
                       || kb.upArrowKey.wasPressedThisFrame;

        var gamepad = Gamepad.current;
        if (gamepad != null)
            jumpPressed |= gamepad.buttonSouth.wasPressedThisFrame;

        if (jumpPressed)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
    }

    private void HandleRunInput()
    {
        var gamepad = Gamepad.current;
        bool usingController = gamepad != null && gamepad.leftStick.ReadValue().magnitude > 0.1f;

        if (usingController)
        {
            if (controllerRunIsToggle)
            {
                if (gamepad.leftStickButton.wasPressedThisFrame)
                    _controllerRunToggleState = !_controllerRunToggleState;
                _isRunning = _controllerRunToggleState;
            }
            else
            {
                _isRunning = gamepad.leftStickButton.isPressed;
            }
        }
        else
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (keyboardRunIsToggle)
            {
                if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame)
                    _keyboardRunToggleState = !_keyboardRunToggleState;
                _isRunning = _keyboardRunToggleState;
            }
            else
            {
                _isRunning = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            }
        }
    }

    private void HandleCrouchInput()
    {
        var kb = Keyboard.current;
        var gamepad = Gamepad.current;
        bool usingController = gamepad != null;

        if (usingController && gamepad.rightStickButton.wasPressedThisFrame && controllerCrouchIsToggle)
            _controllerCrouchToggleState = !_controllerCrouchToggleState;

        if (kb != null && (kb.leftCtrlKey.wasPressedThisFrame || kb.rightCtrlKey.wasPressedThisFrame) && keyboardCrouchIsToggle)
            _keyboardCrouchToggleState = !_keyboardCrouchToggleState;

        if (usingController && controllerCrouchIsToggle)
            _isCrouching = _controllerCrouchToggleState;
        else if (usingController)
            _isCrouching = gamepad.rightStickButton.isPressed;
        else if (kb != null && keyboardCrouchIsToggle)
            _isCrouching = _keyboardCrouchToggleState;
        else if (kb != null)
            _isCrouching = kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed;
    }

    private void HandleInteractInput()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame)
            OnInteract();

        // Q reserved for consumable
    }

    private void OnInteract()
    {
        Debug.Log("Interact");
    }

    public bool IsRunning => _isRunning;
    public bool IsCrouching => _isCrouching;
    public bool IsGrounded => _isGrounded;
}