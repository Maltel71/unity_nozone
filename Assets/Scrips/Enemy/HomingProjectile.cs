using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private string playerTag = "Player";

    [Header("Arc")]
    [SerializeField] private float arcDuration = 0.3f;
    [SerializeField] private float arcUpwardSpeed = 6f;

    private ProjectilePool _pool;
    private Transform _target;
    private float _speed;
    private float _homingStrength;
    private float _lifetimeTimer;
    private float _arcTimer;
    private Vector2 _velocity;
    private bool _hit;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private ParticleSystem _particles;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _particles = GetComponentInChildren<ParticleSystem>();

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (_particles != null)
        {
            var main = _particles.main;
            main.loop = false;
            main.playOnAwake = false;
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void Initialize(ProjectilePool pool)
    {
        _pool = pool;
    }

    public void Launch(Transform target, float speed, float homingStrength)
    {
        _target = target;
        _speed = speed;
        _homingStrength = homingStrength;
        _lifetimeTimer = lifetime;
        _arcTimer = arcDuration;
        _hit = false;

        // Start moving straight up during the arc phase
        _velocity = Vector2.up * arcUpwardSpeed;

        if (_spriteRenderer != null) _spriteRenderer.enabled = true;
        if (_collider != null) _collider.enabled = true;
    }

    private void FixedUpdate()
    {
        if (_hit) return;

        _lifetimeTimer -= Time.fixedDeltaTime;
        if (_lifetimeTimer <= 0f)
        {
            ReturnToPool();
            return;
        }

        if (_arcTimer > 0f)
        {
            // Arc phase: fly upward, no homing yet
            _arcTimer -= Time.fixedDeltaTime;
        }
        else
        {
            // Homing phase: steer toward the player
            ApplyHoming();
        }

        _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
        RotateToVelocity();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hit) return;
        _hit = true;

        if (other.CompareTag(playerTag))
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        if (_spriteRenderer != null) _spriteRenderer.enabled = false;
        if (_collider != null) _collider.enabled = false;
        _rb.linearVelocity = Vector2.zero;

        if (_particles != null)
            StartCoroutine(PlayParticleThenReturn());
        else
            ReturnToPool();
    }

    private IEnumerator PlayParticleThenReturn()
    {
        _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _particles.Play();
        yield return null;
        yield return new WaitWhile(() => _particles.IsAlive(true));
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        _hit = false;
        _pool?.ReturnProjectile(this);
    }

    private void ApplyHoming()
    {
        if (_target == null) return;
        Vector2 toTarget = ((Vector2)(_target.position - transform.position)).normalized;
        Vector2 steered = Vector2.Lerp(_velocity.normalized, toTarget, _homingStrength * Time.fixedDeltaTime);
        _velocity = steered.normalized * _speed;
    }

    private void RotateToVelocity()
    {
        if (_velocity.sqrMagnitude < 0.001f) return;
        float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
        _rb.MoveRotation(angle);
    }
}