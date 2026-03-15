using UnityEngine;
using TMPro;

public class VictoryTimerDisplay : MonoBehaviour
{
    private void Start()
    {
        var tmp = GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;

        if (SpeedrunTimer.Instance != null)
        {
            SpeedrunTimer.Instance.Stop();
            tmp.text = SpeedrunTimer.FormatTime(SpeedrunTimer.Instance.Elapsed);
            Destroy(SpeedrunTimer.Instance.gameObject);
        }
        else
        {
            tmp.text = "No time recorded";
        }
    }
}