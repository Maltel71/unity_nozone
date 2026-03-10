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

    private bool _kbRunToggle, _padRunToggle, _kbCrouchToggle, _padCrouchToggle;
    private float _move;
    private bool _jump, _run, _crouch, _interact, _isMoving, _isRunning;

    private void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
        _stamina = GetComponent<StaminaSystem>();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        var pad = Gamepad.current;
        bool usingPad = pad != null && pad.leftStick.ReadValue().magnitude > 0.1f;

        // Movement
        _move = 0f;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) _move -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) _move += 1f;
        }
        if (pad != null && Mathf.Abs(pad.leftStick.x.ReadValue()) > 0.1f)
            _move = pad.leftStick.x.ReadValue();

        // Jump
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)) _jump = true;
        if (pad != null && pad.buttonSouth.wasPressedThisFrame) _jump = true;

        // Run
        if (usingPad)
        {
            if (controllerRunIsToggle)
            {
                if (pad.leftStickButton.wasPressedThisFrame) _padRunToggle = !_padRunToggle;
                if (pad.leftStick.ReadValue().magnitude < 0.1f) _padRunToggle = false;
                _run = _padRunToggle;
            }
            else _run = pad.leftStickButton.isPressed;
        }
        else if (kb != null)
        {
            if (keyboardRunIsToggle)
            {
                if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame) _kbRunToggle = !_kbRunToggle;
                _run = _kbRunToggle;
            }
            else _run = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
        }

        // Crouch
        if (usingPad && controllerCrouchIsToggle)
        {
            if (pad.rightStickButton.wasPressedThisFrame) _padCrouchToggle = !_padCrouchToggle;
            _crouch = _padCrouchToggle;
        }
        else if (usingPad) _crouch = pad.rightStickButton.isPressed;
        else if (kb != null && keyboardCrouchIsToggle)
        {
            if (kb.leftCtrlKey.wasPressedThisFrame || kb.rightCtrlKey.wasPressedThisFrame) _kbCrouchToggle = !_kbCrouchToggle;
            _crouch = _kbCrouchToggle;
        }
        else if (kb != null) _crouch = kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed;

        // Interact
        if (kb != null && kb.eKey.wasPressedThisFrame) _interact = true;
    }

    private void FixedUpdate()
    {
        _isMoving = Mathf.Abs(_move) > 0.1f;
        _controller.SetMoveInput(_move);

        if (_jump && _controller.IsGrounded && (_stamina == null || _stamina.CanJump()))
        {
            _controller.Jump();
            _stamina?.UseJumpStamina();
        }

        _isRunning = _run && (_stamina == null || _stamina.CanRun());
        if (_isRunning) _controller.StartRun(); else _controller.StopRun();

        _controller.SetCrouch(_crouch);

        if (_interact) _controller.Interact();

        if (_stamina == null)
        {
        }
        else
        {
            if (_isRunning && _isMoving) _stamina.DrainRunStamina(Time.fixedDeltaTime);
            else if (_isMoving) _stamina.DrainWalkStamina(Time.fixedDeltaTime);
            else _stamina.RegenerateStamina(Time.fixedDeltaTime);
        }

        _jump = false;
        _interact = false;
    }
}