using UnityEngine;

/// <summary>
/// Rotates the player's arm sprites toward the carried object when holding one.
/// When free, arms swing during walk/run and angle forward when airborne.
/// On death, arms smoothly drop downward.
/// </summary>
public class PlayerArms : MonoBehaviour
{
    [Header("Arm Transforms")]
    [Tooltip("Pivot transform of the left arm sprite.")]
    [SerializeField] private Transform leftArm;
    [Tooltip("Pivot transform of the right arm sprite.")]
    [SerializeField] private Transform rightArm;

    [Header("Rest Angles (local Z)")]
    [SerializeField] private float leftArmRestAngle = 0f;
    [SerializeField] private float rightArmRestAngle = 0f;

    [Header("Hold Angles (local Z offset applied on top of look direction)")]
    [SerializeField] private float leftArmHoldOffset = 0f;
    [SerializeField] private float rightArmHoldOffset = 0f;

    [Header("Walk Swing")]
    [SerializeField] private float walkSwingMin = -25f;
    [SerializeField] private float walkSwingMax = 25f;
    [SerializeField] private float walkSwingSpeed = 6f;

    [Header("Run Swing")]
    [SerializeField] private float runSwingMin = -45f;
    [SerializeField] private float runSwingMax = 45f;
    [SerializeField] private float runSwingSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpLeftArmAngle = 35f;
    [SerializeField] private float jumpRightArmAngle = -35f;

    [Header("Smoothing")]
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Death Drop")]
    [SerializeField] private float deathDropAmount = 0.4f;
    [SerializeField] private float deathDropSpeed = 4f;

    private PickupSystem _pickup;
    private CharacterController2D _controller;
    private Rigidbody2D _rb;
    private PlayerHealth _playerHealth;

    private float _swingTime;
    private bool _isDead;
    private float _leftArmBaseY;
    private float _rightArmBaseY;
    private float _currentDropOffset;

    private void Awake()
    {
        _pickup = GetComponent<PickupSystem>();
        _controller = GetComponent<CharacterController2D>();
        _rb = GetComponent<Rigidbody2D>();
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        if (leftArm != null) _leftArmBaseY = leftArm.localPosition.y;
        if (rightArm != null) _rightArmBaseY = rightArm.localPosition.y;
    }

    private void OnEnable()
    {
        if (_playerHealth != null)
            _playerHealth.OnDied += OnDied;
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
            _playerHealth.OnDied -= OnDied;
    }

    private void OnDied()
    {
        _isDead = true;
    }

    private void LateUpdate()
    {
        if (_isDead)
        {
            _currentDropOffset = Mathf.MoveTowards(_currentDropOffset, deathDropAmount, deathDropSpeed * Time.deltaTime);
            ApplyDropOffset();
            return;
        }

        AdvanceSwing();

        if (leftArm != null)
            UpdateArm(leftArm, leftArmRestAngle, leftArmHoldOffset, leftArmJumpAngle: jumpLeftArmAngle, swingPhase: 0f);

        if (rightArm != null)
            UpdateArm(rightArm, rightArmRestAngle, rightArmHoldOffset, leftArmJumpAngle: jumpRightArmAngle, swingPhase: Mathf.PI);
    }

    private void ApplyDropOffset()
    {
        if (leftArm != null)
        {
            Vector3 pos = leftArm.localPosition;
            pos.y = _leftArmBaseY - _currentDropOffset;
            leftArm.localPosition = pos;
        }

        if (rightArm != null)
        {
            Vector3 pos = rightArm.localPosition;
            pos.y = _rightArmBaseY - _currentDropOffset;
            rightArm.localPosition = pos;
        }
    }

    private void AdvanceSwing()
    {
        bool grounded = _controller != null && _controller.IsGrounded;
        bool moving = _rb != null && Mathf.Abs(_rb.linearVelocity.x) > 0.05f;

        if (grounded && moving)
        {
            bool running = _controller != null && _controller.IsRunning;
            float speed = running ? runSwingSpeed : walkSwingSpeed;
            _swingTime += Time.deltaTime * speed;
        }
        else if (grounded)
        {
            _swingTime = 0f;
        }
    }

    private void UpdateArm(Transform arm, float restAngle, float holdOffset, float leftArmJumpAngle, float swingPhase)
    {
        Transform carried = _pickup != null ? _pickup.CarriedObjectTransform : null;

        if (carried != null)
        {
            Vector2 dir = (Vector2)carried.position - (Vector2)arm.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + holdOffset;
            arm.rotation = Quaternion.Lerp(arm.rotation, Quaternion.Euler(0f, 0f, angle), rotationSpeed * Time.deltaTime);
            return;
        }

        bool grounded = _controller != null && _controller.IsGrounded;
        bool running = _controller != null && _controller.IsRunning;
        bool moving = _rb != null && Mathf.Abs(_rb.linearVelocity.x) > 0.05f;

        float targetAngle;

        if (!grounded)
        {
            targetAngle = restAngle + leftArmJumpAngle;
        }
        else if (moving)
        {
            float min = running ? runSwingMin : walkSwingMin;
            float max = running ? runSwingMax : walkSwingMax;
            float t = Mathf.Sin(_swingTime + swingPhase) * 0.5f + 0.5f;
            targetAngle = restAngle + Mathf.Lerp(min, max, t);
        }
        else
        {
            targetAngle = restAngle;
        }

        arm.localRotation = Quaternion.Lerp(arm.localRotation, Quaternion.Euler(0f, 0f, targetAngle), rotationSpeed * Time.deltaTime);
    }
}