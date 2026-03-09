using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("UI")]
    [SerializeField] private Slider healthBar;

    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        UpdateBar();

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (healthBar != null)
            healthBar.value = currentHealth / maxHealth;
    }

    private void Die()
    {
        Debug.Log("Player died.");
        // Hook death logic here
    }
}