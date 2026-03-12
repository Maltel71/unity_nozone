using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Run Settings")]
    [SerializeField] private bool keyboardRunIsToggle = false;
    [SerializeField] private bool controllerRunIsToggle = true;

    [Header("Crouch Settings")]
    [SerializeField] private bool keyboardCrouchIsToggle = false;
    [SerializeField] private bool controllerCrouchIsToggle = false;

    private CharacterController2D _controller;
    private StaminaSystem _stamina;
    private PickupSystem _pickup;

    private bool _kbRunToggle;
    private bool _padRunToggle;
    private bool _kbCrouchToggle;
    private bool _padCrouchToggle;

    private bool _isRunning;
    private bool _isMoving;
    private bool _inputDisabled;

    private void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
        _stamina = GetComponent<StaminaSystem>();
        _pickup = GetComponent<PickupSystem>();
        _controller.OnJumped += () => _stamina?.UseJumpStamina();
    }

    private void Start()
    {
        _ = Keyboard.current;
        _ = Gamepad.current;
    }

    private void Update()
    {
        if (_inputDisabled) return;
        if (Time.timeScale == 0f) return;

        HandleMovement();
        HandleJump();
        HandleRun();
        HandleCrouch();
        HandleStaminaTick();
    }

    /// <summary>
    /// Disables all player input and stops movement. Called on death.
    /// </summary>
    public void DisableInput()
    {
        _inputDisabled = true;
        _isRunning = false;
        _isMoving = false;

        _controller.SetMoveInput(0f);
        _controller.StopRun();
        _controller.SetCrouch(false);
    }

    private void HandleMovement()
    {
        float move = 0f;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move += 1f;
        }

        var pad = Gamepad.current;
        if (pad != null)
        {
            float stickX = pad.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.1f) move = stickX;
        }

        _isMoving = Mathf.Abs(move) > 0.1f;
        _controller.SetMoveInput(move);
    }

    private void HandleJump()
    {
        bool pressed = false;

        var kb = Keyboard.current;
        if (kb != null)
            pressed = kb.spaceKey.wasPressedThisFrame
                   || kb.wKey.wasPressedThisFrame
                   || kb.upArrowKey.wasPressedThisFrame;

        var pad = Gamepad.current;
        if (pad != null) pressed |= pad.buttonSouth.wasPressedThisFrame;

        bool pickupAllowsJump = _pickup == null || _pickup.CanJump;

        if (pressed && pickupAllowsJump)
            _controller.Jump();
    }

    private void HandleRun()
    {
        var pad = Gamepad.current;
        bool usingPad = pad != null && pad.leftStick.ReadValue().magnitude > 0.1f;
        bool shouldRun = false;

        if (usingPad)
        {
            if (controllerRunIsToggle)
            {
                if (pad.leftStickButton.wasPressedThisFrame) _padRunToggle = !_padRunToggle;
                if (pad.leftStick.ReadValue().magnitude < 0.1f) _padRunToggle = false;
                shouldRun = _padRunToggle;
            }
            else
            {
                shouldRun = pad.leftStickButton.isPressed;
            }
        }
        else
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (keyboardRunIsToggle)
                {
                    if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame)
                        _kbRunToggle = !_kbRunToggle;
                    shouldRun = _kbRunToggle;
                }
                else
                {
                    shouldRun = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
                }
            }
        }

        bool pickupAllowsRun = _pickup == null || _pickup.CanRun;
        _isRunning = shouldRun && pickupAllowsRun && (_stamina == null || _stamina.CanRun());

        if (_isRunning)
            _controller.StartRun();
        else
            _controller.StopRun();
    }

    private void HandleStaminaTick()
    {
        if (_stamina == null) return;

        if (_isRunning && _isMoving)
            _stamina.DrainRunStamina(Time.deltaTime);
        else if (_isMoving)
            _stamina.DrainWalkStamina(Time.deltaTime);
        else
            _stamina.RegenerateStamina(Time.deltaTime);
    }

    private void HandleCrouch()
    {
        var kb = Keyboard.current;
        var pad = Gamepad.current;
        bool usingPad = pad != null;
        bool shouldCrouch = false;

        if (usingPad && controllerCrouchIsToggle)
        {
            if (pad.rightStickButton.wasPressedThisFrame) _padCrouchToggle = !_padCrouchToggle;
            shouldCrouch = _padCrouchToggle;
        }
        else if (usingPad)
        {
            shouldCrouch = pad.rightStickButton.isPressed;
        }
        else if (kb != null && keyboardCrouchIsToggle)
        {
            if (kb.leftCtrlKey.wasPressedThisFrame || kb.rightCtrlKey.wasPressedThisFrame)
                _kbCrouchToggle = !_kbCrouchToggle;
            shouldCrouch = _kbCrouchToggle;
        }
        else if (kb != null)
        {
            shouldCrouch = kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed;
        }

        _controller.SetCrouch(shouldCrouch);
    }
}