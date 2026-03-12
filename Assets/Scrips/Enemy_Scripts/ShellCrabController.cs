using UnityEngine;

public enum CrabState { Idle, Patrol, Shooting }

[RequireComponent(typeof(Rigidbody2D))]
public class ShellCrabController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Raycasts")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float edgeCheckDistance = 0.8f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Raycast Offsets")]
    [SerializeField] private Vector2 frontWallOffset = new Vector2(0.4f, 0f);
    [SerializeField] private Vector2 backWallOffset = new Vector2(0.4f, 0f);
    [SerializeField] private Vector2 frontEdgeOffset = new Vector2(0.4f, -0.3f);
    [SerializeField] private Vector2 backEdgeOffset = new Vector2(0.4f, -0.3f);

    [Header("Edge Turn Cooldown")]
    [SerializeField] private float edgeTurnCooldown = 0.5f;

    private Rigidbody2D _rb;
    private CrabState _state = CrabState.Patrol;
    private int _facingDir = 1;
    private float _edgeTurnCooldownTimer;

    public CrabState State => _state;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        switch (_state)
        {
            case CrabState.Patrol: Patrol(); break;
            case CrabState.Shooting:
            case CrabState.Idle:
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                break;
        }

        if (_edgeTurnCooldownTimer > 0f)
            _edgeTurnCooldownTimer -= Time.fixedDeltaTime;
    }

    public void SetState(CrabState state) => _state = state;

    private void Patrol()
    {
        CheckTurn();
        _rb.linearVelocity = new Vector2(_facingDir * patrolSpeed, _rb.linearVelocity.y);
    }

    private void CheckTurn()
    {
        if (HitsWall())
        {
            Flip();
            return;
        }

        if (_edgeTurnCooldownTimer > 0f) return;

        if (DetectsEdge())
            Flip();
    }

    private bool HitsWall()
    {
        Vector2 origin = transform.position;

        Vector2 frontWallOrigin = origin + new Vector2(frontWallOffset.x * _facingDir, frontWallOffset.y);
        bool frontWall = Physics2D.Raycast(frontWallOrigin, Vector2.right * _facingDir, wallCheckDistance, groundLayer);

        Vector2 backWallOrigin = origin + new Vector2(-backWallOffset.x * _facingDir, backWallOffset.y);
        bool backWall = Physics2D.Raycast(backWallOrigin, Vector2.right * -_facingDir, wallCheckDistance, groundLayer);

        return frontWall || backWall;
    }

    private bool DetectsEdge()
    {
        Vector2 origin = transform.position;

        Vector2 frontEdgeOrigin = origin + new Vector2(frontEdgeOffset.x * _facingDir, frontEdgeOffset.y);
        bool noFrontGround = !Physics2D.Raycast(frontEdgeOrigin, Vector2.down, edgeCheckDistance, groundLayer);

        Vector2 backEdgeOrigin = origin + new Vector2(-backEdgeOffset.x * _facingDir, backEdgeOffset.y);
        bool noBackGround = !Physics2D.Raycast(backEdgeOrigin, Vector2.down, edgeCheckDistance, groundLayer);

        bool grounded = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

        return noFrontGround || (!grounded && noBackGround);
    }

    private void Flip()
    {
        _facingDir *= -1;
        transform.localScale = new Vector3(_facingDir, 1f, 1f);
        _edgeTurnCooldownTimer = edgeTurnCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;
        int dir = Application.isPlaying ? _facingDir : 1;

        // Front wall
        Vector2 frontWallOrigin = origin + new Vector2(frontWallOffset.x * dir, frontWallOffset.y);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(frontWallOrigin, Vector2.right * dir * wallCheckDistance);

        // Back wall
        Vector2 backWallOrigin = origin + new Vector2(-backWallOffset.x * dir, backWallOffset.y);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(backWallOrigin, Vector2.right * -dir * wallCheckDistance);

        // Front edge
        Vector2 frontEdgeOrigin = origin + new Vector2(frontEdgeOffset.x * dir, frontEdgeOffset.y);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(frontEdgeOrigin, Vector2.down * edgeCheckDistance);

        // Back edge
        Vector2 backEdgeOrigin = origin + new Vector2(-backEdgeOffset.x * dir, backEdgeOffset.y);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(backEdgeOrigin, Vector2.down * edgeCheckDistance);

        // Ground
        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, Vector2.down * groundCheckDistance);
    }
}