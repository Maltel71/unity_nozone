using System.Collections;
using UnityEngine;

public class TrapShooter : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float shootForce = 10f;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private string playerTag = "Player";

    [Header("Sprites")]
    [SerializeField] private GameObject armedSprite;
    [SerializeField] private GameObject emptySprite;
    [SerializeField] private float rearmVisualLeadTime = 1.5f;

    private bool _onCooldown;
    private Transform ShootOrigin => spawnPoint != null ? spawnPoint : transform;

    private void Start()
    {
        SetArmed(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_onCooldown || !other.CompareTag(playerTag)) return;
        StartCoroutine(ShootAndCooldown());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_onCooldown || !other.CompareTag(playerTag)) return;
        StartCoroutine(ShootAndCooldown());
    }

    public void Activate()
    {
        if (_onCooldown) return;
        StartCoroutine(ShootAndCooldown());
    }

    private IEnumerator ShootAndCooldown()
    {
        _onCooldown = true;
        SetArmed(false);

        Transform origin = ShootOrigin;
        GameObject proj = Instantiate(projectilePrefab, origin.position, origin.rotation);
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>() ?? proj.AddComponent<Rigidbody2D>();
        rb.AddForce(origin.up * shootForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(cooldown - rearmVisualLeadTime);
        SetArmed(true);

        yield return new WaitForSeconds(rearmVisualLeadTime);
        _onCooldown = false;
    }

    private void SetArmed(bool armed)
    {
        if (armedSprite != null) armedSprite.SetActive(armed);
        if (emptySprite != null) emptySprite.SetActive(!armed);
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = ShootOrigin;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin.position, 0.1f);
        Gizmos.DrawRay(origin.position, origin.up * 2f);
    }
}