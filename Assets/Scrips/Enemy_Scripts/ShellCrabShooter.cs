using UnityEngine;

public class ShellCrabShooter : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private string playerTag = "Player";

    [Header("Shooting")]
    [SerializeField] private float fireRate = 1f;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float homingStrength = 3f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlash;

    private ShellCrabController _controller;
    private ProjectilePool _pool;
    private Transform _player;

    private float _fireTimer;
    private HomingProjectile _activeProjectile;

    private void Awake()
    {
        _controller = GetComponent<ShellCrabController>();
        _pool = GetComponentInChildren<ProjectilePool>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogWarning("[ShellCrabShooter] No GameObject found with tag: " + playerTag);
    }

    private void Update()
    {
        if (_player == null) return;

        float dist = Vector2.Distance(transform.position, _player.position);
        bool playerInRange = dist <= detectionRange;

        if (playerInRange)
        {
            if (_controller.State != CrabState.Idle)
                _controller.SetState(CrabState.Idle);

            // Don't advance the timer while a projectile is still in its pre-launch animation
            bool preparing = _activeProjectile != null && _activeProjectile.IsPreparing;
            if (!preparing)
                _fireTimer += Time.deltaTime;

            if (_fireTimer >= 1f / fireRate)
            {
                _fireTimer = 0f;
                Shoot();
            }
        }
        else
        {
            _fireTimer = 0f;

            if (_controller.State == CrabState.Idle)
                _controller.SetState(CrabState.Patrol);
        }
    }

    private void Shoot()
    {
        if (_pool == null) return;

        HomingProjectile proj = _pool.GetProjectile();
        if (proj == null)
        {
            Debug.LogWarning("[ShellCrabShooter] No available projectile in pool.");
            return;
        }

        proj.gameObject.SetActive(true);
        _activeProjectile = proj;
        proj.Prepare(_player, projectileSpeed, homingStrength, muzzleFlash);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}