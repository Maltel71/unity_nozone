using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShellDurability : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float _currentHealth;

    [Header("Sun Damage")]
    [SerializeField] private Light2D[] sunLights;
    [SerializeField] private LayerMask shadowCasterLayers;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private float damagePerSecond = 10f;

    [Header("Damage Stages (child GameObjects)")]
    [SerializeField] private GameObject damageSprite1; // 100% - 75%
    [SerializeField] private GameObject damageSprite2; // 75%  - 50%
    [SerializeField] private GameObject damageSprite3; // 50%  - 25%
    [SerializeField] private GameObject damageSprite4; // 25%  - 0%

    [Header("Death")]
    [SerializeField] private GameObject[] crumbPrefabs;
    [SerializeField] private int crumbCount = 4;
    [SerializeField] private float crumbSpawnRadius = 0.3f;
    [SerializeField] private float crumbForce = 3f;

    private bool _isDead;

    private void Awake()
    {
        _currentHealth = maxHealth;
        UpdateSprite();
    }

    private void Update()
    {
        if (_isDead) return;
        if (dayNightCycle != null && !dayNightCycle.BurnEnabled) return;
        if (!IsInSunlight()) return;

        _currentHealth -= damagePerSecond * Time.deltaTime;
        _currentHealth = Mathf.Max(_currentHealth, 0f);

        UpdateSprite();

        if (_currentHealth <= 0f)
            Die();
    }

    private bool IsInSunlight()
    {
        foreach (Light2D light in sunLights)
        {
            if (light == null || !light.enabled) continue;

            Vector2 dir = (Vector2)light.transform.position - (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, shadowCasterLayers);

            if (!hit) return true;
        }
        return false;
    }

    private void UpdateSprite()
    {
        float t = _currentHealth / maxHealth;

        SetActive(damageSprite1, t > 0.75f);
        SetActive(damageSprite2, t <= 0.75f && t > 0.5f);
        SetActive(damageSprite3, t <= 0.5f && t > 0.25f);
        SetActive(damageSprite4, t <= 0.25f);
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }

    private void Die()
    {
        _isDead = true;

        if (crumbPrefabs != null && crumbPrefabs.Length > 0)
        {
            for (int i = 0; i < crumbCount; i++)
            {
                GameObject prefab = crumbPrefabs[Random.Range(0, crumbPrefabs.Length)];
                if (prefab == null) continue;

                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * crumbSpawnRadius;
                GameObject crumb = Instantiate(prefab, spawnPos, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));

                Rigidbody2D rb = crumb.GetComponent<Rigidbody2D>();
                if (rb == null) rb = crumb.AddComponent<Rigidbody2D>();

                rb.AddForce(Random.insideUnitCircle.normalized * crumbForce, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }
}