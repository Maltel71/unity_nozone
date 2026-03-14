using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject arrowVisual;
    [SerializeField] private ParticleSystem hitParticles;

    private Rigidbody2D _rb;
    private Collider2D _collider;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (_rb.linearVelocity.sqrMagnitude < 0.01f) return;
        float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        if (_collider != null) _collider.enabled = false;
        if (arrowVisual != null) arrowVisual.SetActive(false);
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Static;

        if (hitParticles != null)
        {
            hitParticles.Play();
            yield return new WaitForSeconds(2f);
        }

        Destroy(gameObject);
    }
}