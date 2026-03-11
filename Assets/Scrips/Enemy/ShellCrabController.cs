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
    [SerializeField] private Vector2 frontEdgeOffset = new Vector2(0.4f, -0.3f);
    [SerializeField] private Vector2 backEdgeOffset = new Vector2(0.4f, -0.3f);

    private Rigidbody2D _rb;
    private CrabState _state = CrabState.Patrol;
    private int _facingDir = 1;

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
    }

    public void SetState(CrabState state) => _state = state;

    private void Patrol()
    {
        if (ShouldTurn())
            Flip();

        _rb.linearVelocity = new Vector2(_facingDir * patrolSpeed, _rb.linearVelocity.y);
    }

    private bool ShouldTurn()
    {
        Vector2 origin = transform.position;

        // Front wall check
        Vector2 wallOrigin = origin + new Vector2(frontWallOffset.x * _facingDir, frontWallOffset.y);
        bool wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * _facingDir, wallCheckDistance, groundLayer);

        // Front edge check — no ground ahead means ledge
        Vector2 edgeOrigin = origin + new Vector2(frontEdgeOffset.x * _facingDir, frontEdgeOffset.y);
        bool noFrontGround = !Physics2D.Raycast(edgeOrigin, Vector2.down, edgeCheckDistance, groundLayer);

        // Back edge check — stability fallback
        Vector2 backOrigin = origin + new Vector2(-backEdgeOffset.x * _facingDir, backEdgeOffset.y);
        bool noBackGround = !Physics2D.Raycast(backOrigin, Vector2.down, edgeCheckDistance, groundLayer);

        // Ground check — ensure we're standing on something
        bool grounded = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

        return wallHit || noFrontGround || (!grounded && noBackGround);
    }

    private void Flip()
    {
        _facingDir *= -1;
        transform.localScale = new Vector3(_facingDir, 1f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;
        int dir = Application.isPlaying ? _facingDir : 1;

        // Front wall
        Vector2 wallOrigin = origin + new Vector2(frontWallOffset.x * dir, frontWallOffset.y);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(wallOrigin, Vector2.right * dir * wallCheckDistance);

        // Front edge
        Vector2 edgeOrigin = origin + new Vector2(frontEdgeOffset.x * dir, frontEdgeOffset.y);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(edgeOrigin, Vector2.down * edgeCheckDistance);

        // Back edge
        Vector2 backOrigin = origin + new Vector2(-backEdgeOffset.x * dir, backEdgeOffset.y);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(backOrigin, Vector2.down * edgeCheckDistance);

        // Ground
        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, Vector2.down * groundCheckDistance);
    }
}