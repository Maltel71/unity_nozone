using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerTag = "Player";

    [Header("Follow X")]
    [SerializeField] private float followSpeedX = 5f;
    [SerializeField] private float smoothTimeX = 0.12f;
    [SerializeField] private float maxDistanceX = 3f;

    [Header("Follow Y")]
    [SerializeField] private float followSpeedY = 5f;
    [SerializeField] private float smoothTimeY = 0.12f;
    [SerializeField] private float maxDistanceY = 3f;

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 3f;

    private Vector2 _lookAheadOffset;
    private float _velocityX;
    private float _velocityY;
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
        _velocityX = 0f;
        _velocityY = 0f;
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

        float deltaX = desiredPos.x - currentPos.x;
        float deltaY = desiredPos.y - currentPos.y;

        if (Mathf.Abs(deltaX) > maxDistanceX)
            desiredPos.x = currentPos.x + Mathf.Sign(deltaX) * maxDistanceX;
        if (Mathf.Abs(deltaY) > maxDistanceY)
            desiredPos.y = currentPos.y + Mathf.Sign(deltaY) * maxDistanceY;

        float smoothX = Mathf.SmoothDamp(currentPos.x, desiredPos.x, ref _velocityX, smoothTimeX, followSpeedX);
        float smoothY = Mathf.SmoothDamp(currentPos.y, desiredPos.y, ref _velocityY, smoothTimeY, followSpeedY);

        Vector2 shakeOffset = CameraShake2D.Instance != null ? CameraShake2D.Instance.ShakeOffset : Vector2.zero;

        transform.position = new Vector3(
            smoothX + shakeOffset.x,
            smoothY + shakeOffset.y,
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