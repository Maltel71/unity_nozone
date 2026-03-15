using UnityEngine;

public class SpeedrunTimer : MonoBehaviour
{
    public static SpeedrunTimer Instance { get; private set; }

    [SerializeField] private float _elapsed;
    private bool _running;

    public float Elapsed => _elapsed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _running = true;
    }

    private void Update()
    {
        if (!_running) return;
        _elapsed += Time.deltaTime;
    }

    public void Stop()
    {
        _running = false;
    }

    public static string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60);
        int s = (int)(seconds % 60);
        int ms = (int)((seconds * 100) % 100);
        return $"{m:00}:{s:00}.{ms:00}";
    }
}