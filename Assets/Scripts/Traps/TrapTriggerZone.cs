using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnEntry
{
    public GameObject prefab;
    public Transform spawnPoint;

    public void Spawn()
    {
        if (prefab == null || spawnPoint == null) return;
        Object.Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}

public class TrapTriggerZone : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private LayerMask lineOfSightMask;

    [Header("Trigger Settings")]
    [SerializeField] private float activationDelay = 0f;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float resetDelay = 3f;

    [Header("Random Delay")]
    [SerializeField] private bool useRandomDelay = false;
    [SerializeField] private float minDelay = 0f;
    [SerializeField] private float maxDelay = 1f;

    [Header("Arrow Shooters")]
    [SerializeField] private List<TrapShooter> arrowShooters = new();

    [Header("Drop Objects")]
    [SerializeField] private List<Rigidbody2D> dropObjects = new();
    [SerializeField] private float dropForce = 2f;
    [SerializeField] private float dropDelay = 0f;

    [Header("Spawn Objects")]
    [SerializeField] private List<SpawnEntry> spawnObjects = new();

    private bool _triggered;
    private bool _resetting;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || _resetting) return;
        if (!other.CompareTag(playerTag)) return;
        if (requireLineOfSight && !HasLineOfSight(other.transform)) return;

        _triggered = true;
        StartCoroutine(ActivationRoutine());
    }

    private IEnumerator ActivationRoutine()
    {
        float delay = useRandomDelay ? Random.Range(minDelay, maxDelay) : activationDelay;
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        ActivateAll();

        if (!triggerOnce)
        {
            _resetting = true;
            yield return new WaitForSeconds(resetDelay);
            _triggered = false;
            _resetting = false;
        }
    }

    private void ActivateAll()
    {
        foreach (TrapShooter shooter in arrowShooters)
            shooter?.Activate();

        if (dropObjects.Count > 0)
            StartCoroutine(DropRoutine());

        foreach (SpawnEntry entry in spawnObjects)
            entry?.Spawn();
    }

    private IEnumerator DropRoutine()
    {
        if (dropDelay > 0f)
            yield return new WaitForSeconds(dropDelay);

        foreach (Rigidbody2D rb in dropObjects)
        {
            if (rb == null) continue;
            rb.bodyType = RigidbodyType2D.Dynamic;
            if (dropForce > 0f)
                rb.AddForce(Vector2.down * dropForce, ForceMode2D.Impulse);
        }
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector2 origin = transform.position;
        Vector2 dir = (Vector2)target.position - origin;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, dir.magnitude, lineOfSightMask);
        return !hit;
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}