using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)] public float parallaxAmount = 0.5f;
    public bool affectX = true;
    public bool affectY = false;

    private Transform _cam;
    private Vector3 _lastCamPos;

    void Start()
    {
        _cam = Camera.main.transform;
        _lastCamPos = _cam.position;
    }

    void LateUpdate()
    {
        Vector3 delta = _cam.position - _lastCamPos;

        float moveX = affectX ? delta.x * (1f - parallaxAmount) : 0f;
        float moveY = affectY ? delta.y * (1f - parallaxAmount) : 0f;

        transform.position += new Vector3(moveX, moveY, 0f);
        _lastCamPos = _cam.position;
    }
}