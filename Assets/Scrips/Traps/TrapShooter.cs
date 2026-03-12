using System.Collections;
using UnityEngine;

public class TrapShooter : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootForce = 10f;

    [Header("Burst")]
    [SerializeField] private int shootCount = 1;
    [SerializeField] private float shootDelay = 0f;
    [SerializeField] private float timeBetweenShots = 0.2f;

    public void Activate()
    {
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        if (shootDelay > 0f)
            yield return new WaitForSeconds(shootDelay);

        for (int i = 0; i < shootCount; i++)
        {
            FireProjectile();

            if (i < shootCount - 1 && timeBetweenShots > 0f)
                yield return new WaitForSeconds(timeBetweenShots);
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || shootPoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = proj.AddComponent<Rigidbody2D>();

        rb.AddForce((Vector2)shootPoint.right * shootForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        if (shootPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(shootPoint.position, 0.1f);
        Gizmos.DrawRay(shootPoint.position, shootPoint.right * 2f);
    }
}