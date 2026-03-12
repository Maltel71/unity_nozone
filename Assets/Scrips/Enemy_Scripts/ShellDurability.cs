using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShellDurability : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField, HideInInspector] private float _currentHealth;

    [Header("Sun Damage")]
    [SerializeField] private Light2D[] sunLights;
    [SerializeField] private LayerMask shadowCasterLayers;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private float damagePerSecond = 10f;

    [Header("Damage Sprites")]
    [SerializeField] private Sprite intactSprite;
    [SerializeField] private Sprite damagedSprite1;
    [SerializeField] private Sprite damagedSprite2;
    [SerializeField] private Sprite damagedSprite3;

    [Header("Death")]
    [SerializeField] private GameObject[] crumbPrefabs;
    [SerializeField] private int crumbCount = 4;
    [SerializeField] private float crumbSpawnRadius = 0.3f;
    [SerializeField] private float crumbForce = 3f;

    private SpriteRenderer _sr;
    private bool _isDead;

    // Shown in inspector via custom property
    public float CurrentHealth => _currentHealth;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
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
        if (_sr == null) return;

        float t = _currentHealth / maxHealth;

        if (t > 0.75f)
            _sr.sprite = intactSprite;
        else if (t > 0.5f)
            _sr.sprite = damagedSprite1;
        else if (t > 0.25f)
            _sr.sprite = damagedSprite2;
        else
            _sr.sprite = damagedSprite3;
    }

    private void Die()
    {
        _isDead = true;

        if (_sr != null) _sr.enabled = false;

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        _currentHealth = maxHealth;
    }
#endif
}