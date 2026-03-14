using System.Collections;
using UnityEngine;

public class TrapShooter : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootForce = 10f;

    [Header("Burst")]
    [SerializeField] private int shootCount = 1;
    [SerializeField] private float timeBetweenShots = 0.2f;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float activationDelay = 0f;
    [SerializeField] private float resetDelay = 3f;

    private bool _triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || !other.CompareTag(playerTag)) return;
        _triggered = true;
        StartCoroutine(ActivationRoutine());
    }

    private IEnumerator ActivationRoutine()
    {
        if (activationDelay > 0f)
            yield return new WaitForSeconds(activationDelay);

        StartCoroutine(ShootRoutine());

        if (!triggerOnce)
        {
            yield return new WaitForSeconds(resetDelay);
            _triggered = false;
        }
    }

    private IEnumerator ShootRoutine()
    {
        for (int i = 0; i < shootCount; i++)
        {
            FireProjectile();
            if (i < shootCount - 1)
                yield return new WaitForSeconds(timeBetweenShots);
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, transform.position, transform.rotation);
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>() ?? proj.AddComponent<Rigidbody2D>();
        rb.AddForce(transform.right * shootForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * 2f);
    }
}