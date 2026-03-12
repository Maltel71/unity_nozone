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

    [Header("Wiggle")]
    [SerializeField] private float minWiggleAngle = -10f;
    [SerializeField] private float maxWiggleAngle = 10f;
    [SerializeField] private float minWiggleInterval = 0.1f;
    [SerializeField] private float maxWiggleInterval = 0.3f;

    [Header("Rotation")]
    [SerializeField] private float spriteRotationOffset = -90f;

    [Header("Pre-Launch")]
    [SerializeField] private float preLaunchHeight = 0.5f;
    [SerializeField] private float preLaunchUpTime = 0.25f;
    [SerializeField] private float preLaunchDownTime = 0.2f;
    [SerializeField] private float preLaunchPauseTime = 0.1f;
    [SerializeField] private AnimationCurve preLaunchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private ProjectilePool _pool;
    private Transform _target;
    private float _speed;
    private float _homingStrength;
    private float _lifetimeTimer;
    private float _arcTimer;
    private Vector2 _velocity;
    private bool _hit;
    private bool _isPreparing;
    private ParticleSystem _muzzleFlash;

    private float _wiggleAngle;
    private float _wiggleTimer;
    private float _wiggleInterval;

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

    public bool IsPreparing => _isPreparing;

    public void Initialize(ProjectilePool pool)
    {
        _pool = pool;
    }

    /// <summary>
    /// Starts the pre-launch animation then fires. Call this instead of Launch directly.
    /// The projectile must already be active and parented to the ProjectileHoldPoint.
    /// </summary>
    public void Prepare(Transform target, float speed, float homingStrength, ParticleSystem muzzleFlash = null)
    {
        _target = target;
        _speed = speed;
        _homingStrength = homingStrength;
        _hit = false;
        _isPreparing = true;
        _muzzleFlash = muzzleFlash;

        if (_spriteRenderer != null) _spriteRenderer.enabled = false;
        if (_collider != null) _collider.enabled = false;

        StartCoroutine(PreLaunchRoutine());
    }

    private IEnumerator PreLaunchRoutine()
    {
        Vector3 restLocal = Vector3.zero;
        Vector3 upLocal = new Vector3(0f, preLaunchHeight, 0f);

        // Become visible
        if (_spriteRenderer != null) _spriteRenderer.enabled = true;

        // Move upward
        float t = 0f;
        while (t < preLaunchUpTime)
        {
            t += Time.deltaTime;
            float curved = preLaunchCurve.Evaluate(Mathf.Clamp01(t / preLaunchUpTime));
            transform.localPosition = Vector3.Lerp(restLocal, upLocal, curved);
            yield return null;
        }

        // Move back down
        t = 0f;
        while (t < preLaunchDownTime)
        {
            t += Time.deltaTime;
            float curved = preLaunchCurve.Evaluate(Mathf.Clamp01(t / preLaunchDownTime));
            transform.localPosition = Vector3.Lerp(upLocal, restLocal, curved);
            yield return null;
        }

        transform.localPosition = restLocal;

        // Pause at rest position
        if (preLaunchPauseTime > 0f)
            yield return new WaitForSeconds(preLaunchPauseTime);

        Launch();
    }

    private void Launch()
    {
        _isPreparing = false;
        _lifetimeTimer = lifetime;
        _arcTimer = arcDuration;

        _velocity = Vector2.up * arcUpwardSpeed;

        PickNewWiggle();

        if (_collider != null) _collider.enabled = true;

        _muzzleFlash?.Play();

        // Detach from hold point so it moves freely in world space
        transform.SetParent(null);
    }

    private void FixedUpdate()
    {
        if (_hit || _isPreparing) return;

        _lifetimeTimer -= Time.fixedDeltaTime;
        if (_lifetimeTimer <= 0f)
        {
            Explode();
            return;
        }

        if (_arcTimer > 0f)
        {
            _arcTimer -= Time.fixedDeltaTime;
        }
        else
        {
            ApplyHoming();
            ApplyWiggle();
        }

        _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
        RotateToVelocity();
    }

    private void ApplyHoming()
    {
        if (_target == null) return;
        Vector2 toTarget = ((Vector2)(_target.position - transform.position)).normalized;
        Vector2 steered = Vector2.Lerp(_velocity.normalized, toTarget, _homingStrength * Time.fixedDeltaTime);
        _velocity = steered.normalized * _speed;
    }

    private void ApplyWiggle()
    {
        _wiggleTimer -= Time.fixedDeltaTime;
        if (_wiggleTimer <= 0f)
            PickNewWiggle();

        float rad = _wiggleAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        Vector2 dir = _velocity.normalized;
        Vector2 wiggled = new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos);
        _velocity = wiggled * _speed;
    }

    private void PickNewWiggle()
    {
        _wiggleAngle = Random.Range(minWiggleAngle, maxWiggleAngle);
        _wiggleInterval = Random.Range(minWiggleInterval, maxWiggleInterval);
        _wiggleTimer = _wiggleInterval;
    }

    private void RotateToVelocity()
    {
        if (_velocity.sqrMagnitude < 0.001f) return;
        float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg + spriteRotationOffset;
        _rb.MoveRotation(angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hit) return;

        if (other.CompareTag(playerTag))
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        Explode();
    }

    private void Explode()
    {
        if (_hit) return;
        _hit = true;

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
        _isPreparing = false;
        _pool?.ReturnProjectile(this);
    }
}