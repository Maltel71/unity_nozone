using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShellDurability : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float _currentHealth;

    [Header("Sun Damage")]
    [SerializeField] private LayerMask shadowCasterLayers;
    [SerializeField] private float damagePerSecond = 10f;

    private DayNightCycle _dayNightCycle;
    private Light2D _sunLight;

    [Header("Damage Stages (child GameObjects)")]
    [SerializeField] private GameObject damageSprite1; // 100% - 75%
    [SerializeField] private GameObject damageSprite2; // 75%  - 50%
    [SerializeField] private GameObject damageSprite3; // 50%  - 25%
    [SerializeField] private GameObject damageSprite4; // 25%  - 0%

    [Header("Death")]
    [SerializeField] private GameObject[] crumbs;
    [SerializeField] private float crumbForce = 3f;

    private bool _isDead;
    private bool _healthInitialized;

    public float CurrentHealth => _currentHealth;

    public void SetHealth(float health)
    {
        _currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        _healthInitialized = true;
        UpdateSprite();
    }

    private void Awake()
    {
        _dayNightCycle = FindFirstObjectByType<DayNightCycle>();
        _sunLight = _dayNightCycle != null ? _dayNightCycle.SunLight : null;
        if (!_healthInitialized) _currentHealth = maxHealth;
        UpdateSprite();
    }

    private void Update()
    {
        if (_isDead) return;
        if (_dayNightCycle != null && !_dayNightCycle.BurnEnabled) return;
        if (!IsInSunlight()) return;

        _currentHealth -= damagePerSecond * Time.deltaTime;
        _currentHealth = Mathf.Max(_currentHealth, 0f);

        UpdateSprite();

        if (_currentHealth <= 0f)
            Die();
    }

    private bool IsInSunlight()
    {
        if (_sunLight == null || !_sunLight.enabled) return false;

        Vector2 origin = (Vector2)transform.position;
        Vector2 dir = (Vector2)_sunLight.transform.position - origin;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir.normalized, dir.magnitude, shadowCasterLayers);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;
            return false;
        }
        return true;
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

        PickupSystem pickup = FindFirstObjectByType<PickupSystem>();
        if (pickup != null && pickup.CarriedObjectTransform != null &&
            pickup.CarriedObjectTransform.IsChildOf(transform))
            pickup.ForceDropCarried();

        if (crumbs != null)
        {
            foreach (GameObject crumb in crumbs)
            {
                if (crumb == null) continue;

                crumb.SetActive(true);
                crumb.transform.SetParent(null);

                Rigidbody2D rb = crumb.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.AddForce(Random.insideUnitCircle.normalized * crumbForce, ForceMode2D.Impulse);
                }
            }
        }

        Destroy(gameObject);
    }
}