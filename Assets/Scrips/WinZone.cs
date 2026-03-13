using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string victorySceneName = "Victory";
    [SerializeField] private float delay = 0.5f;

    private bool _triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;
        StartCoroutine(LoadVictoryScene());
    }

    private IEnumerator LoadVictoryScene()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(victorySceneName);
    }
}