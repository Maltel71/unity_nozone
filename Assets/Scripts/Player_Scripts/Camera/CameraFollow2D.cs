using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerTag = "Player";

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindTarget();
        SnapToTarget();
    }

    private void FindTarget()
    {
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null)
        {
            target = player.transform;
            _controller = target.GetComponent<CharacterController2D>();
            _targetRb = target.GetComponent<Rigidbody2D>();
        }

        _lookAheadOffset = Vector2.zero;
        _velocity = Vector2.zero;
    }

    private void SnapToTarget()
    {
        if (target == null) return;
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        UpdateLookAhead();

        Vector2 desiredPos = (Vector2)target.position + _lookAheadOffset;
        Vector2 currentPos = transform.position;

        Vector2 delta = desiredPos - currentPos;
        if (delta.magnitude > maxDistance)
            desiredPos = currentPos + delta.normalized * maxDistance;

        Vector2 smoothed = Vector2.SmoothDamp(currentPos, desiredPos, ref _velocity, smoothTime, followSpeed);

        Vector2 shakeOffset = CameraShake2D.Instance != null ? CameraShake2D.Instance.ShakeOffset : Vector2.zero;

        transform.position = new Vector3(
            smoothed.x + shakeOffset.x,
            smoothed.y + shakeOffset.y,
            transform.position.z
        );
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