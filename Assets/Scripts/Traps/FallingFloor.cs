using UnityEngine;

public class FallingFloor : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float startHealth = 100f;
    [SerializeField] private float drainPerSecond = 25f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField, HideInInspector] private float _currentHealth;

    [Header("Debug")]
    [SerializeField] private float currentHealthDebug;

    [Header("Damage Stages")]
    [Tooltip("Index 0 = full health, last index = nearly dead.")]
    [SerializeField] private GameObject[] damageStages;

    [Header("Crumble")]
    [SerializeField] private GameObject[] crumblePieces;
    [SerializeField] private float crumbleForce = 4f;
    [SerializeField] private float crumbleLifetime = 3f;
    [SerializeField] private float crumbleFadeDuration = 1f;

    [Header("Collider")]
    [SerializeField] private Collider2D platformCollider;
    [SerializeField] private Collider2D triggerCollider;

    private bool _isDead;

    private void Reset()
    {
        _currentHealth = maxHealth;
    }

    private void Start()
    {
        _currentHealth = Mathf.Clamp(startHealth, 0f, maxHealth);
        currentHealthDebug = _currentHealth;
        UpdateDamageStage();

        if (crumblePieces != null)
            foreach (GameObject piece in crumblePieces)
                if (piece != null) piece.SetActive(false);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_isDead || !other.CompareTag(playerTag)) return;
        _currentHealth = Mathf.Max(_currentHealth - drainPerSecond * Time.deltaTime, 0f);
        currentHealthDebug = _currentHealth;
        UpdateDamageStage();
        if (_currentHealth <= 0f)
            Crumble();
    }

    private void UpdateDamageStage()
    {
        if (damageStages == null || damageStages.Length == 0) return;

        float t = _currentHealth / maxHealth;
        int count = damageStages.Length;
        int index = Mathf.Clamp(count - 1 - Mathf.FloorToInt(t * count), 0, count - 1);

        for (int i = 0; i < count; i++)
            if (damageStages[i] != null)
                damageStages[i].SetActive(i == index);
    }

    private void Crumble()
    {
        _isDead = true;

        if (platformCollider != null) platformCollider.enabled = false;
        if (triggerCollider != null) triggerCollider.enabled = false;

        if (damageStages != null)
            foreach (GameObject stage in damageStages)
                if (stage != null) stage.SetActive(false);

        if (crumblePieces != null)
        {
            foreach (GameObject piece in crumblePieces)
            {
                if (piece == null) continue;
                piece.SetActive(true);
                piece.transform.SetParent(null);

                Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.AddForce(Random.insideUnitCircle.normalized * crumbleForce, ForceMode2D.Impulse);
                }

                var fade = piece.AddComponent<CrumbleFade>();
                fade.Init(crumbleLifetime, crumbleFadeDuration);
            }
        }

        Destroy(gameObject);
    }
}