using UnityEngine;

// Auto-added at runtime by FallingFloor. Do not add manually.
public class CrumbleFade : MonoBehaviour
{
    private float _lifetime;
    private float _fadeDuration;
    private float _timer;
    private SpriteRenderer _sr;

    public void Init(float lifetime, float fadeDuration)
    {
        _lifetime = lifetime;
        _fadeDuration = fadeDuration;
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        float fadeStart = _lifetime - _fadeDuration;
        if (_timer >= fadeStart)
        {
            float t = Mathf.Clamp01((_timer - fadeStart) / _fadeDuration);
            if (_sr != null)
            {
                Color c = _sr.color;
                c.a = 1f - t;
                _sr.color = c;
            }
        }

        if (_timer >= _lifetime)
            Destroy(gameObject);
    }
}