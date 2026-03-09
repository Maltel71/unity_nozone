using System.Collections;
using UnityEngine;

public class SunlightBurn : MonoBehaviour
{
    [Header("Burn Settings")]
    [SerializeField] private float burnDamagePerTick = 5f;
    [SerializeField] private float burnDelay = 1f;
    [SerializeField] private float burnTickRate = 0.5f;

    private PlayerHealth playerHealth;
    private Coroutine burnCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            burnCoroutine = StartCoroutine(BurnRoutine());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
            burnCoroutine = null;
        }

        playerHealth = null;
    }

    private IEnumerator BurnRoutine()
    {
        yield return new WaitForSeconds(burnDelay);

        while (true)
        {
            playerHealth.TakeDamage(burnDamagePerTick);
            yield return new WaitForSeconds(burnTickRate);
        }
    }
}