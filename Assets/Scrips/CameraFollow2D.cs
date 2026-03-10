using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private float maxDistance = 3f;

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 3f;

    private Vector2 _lookAheadOffset;
    private Vector2 _velocity;
    private CharacterController2D _controller;
    private Rigidbody2D _targetRb;

    private void Awake()
    {
        if (target == null) return;
        _controller = target.GetComponent<CharacterController2D>();
        _targetRb = target.GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        UpdateLookAhead();

        Vector2 desiredPos = (Vector2)target.position + _lookAheadOffset;
        Vector2 currentPos = transform.position;

        // Clamp desired position to max distance
        Vector2 delta = desiredPos - currentPos;
        if (delta.magnitude > maxDistance)
            desiredPos = currentPos + delta.normalized * maxDistance;

        Vector2 smoothed = Vector2.SmoothDamp(currentPos, desiredPos, ref _velocity, smoothTime, followSpeed);
        transform.position = new Vector3(smoothed.x, smoothed.y, transform.position.z);
    }

    private void UpdateLookAhead()
    {
        if (_targetRb == null) return;

        float velocityX = _targetRb.linearVelocity.x;
        float runMultiplier = (_controller != null && _controller.IsRunning) ? 1.5f : 1f;
        float targetX = Mathf.Abs(velocityX) > 0.1f
            ? Mathf.Sign(velocityX) * lookAheadDistance * runMultiplier
            : 0f;

        _lookAheadOffset.x = Mathf.Lerp(_lookAheadOffset.x, targetX, lookAheadSpeed * Time.deltaTime);
    }
}