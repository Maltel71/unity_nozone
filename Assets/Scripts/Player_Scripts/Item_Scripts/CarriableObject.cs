using UnityEngine;

public class CarriableObject : MonoBehaviour
{
    [Tooltip("Index into the PickupSystem's carried object list.")]
    public int carriedIndex;

    [Tooltip("Persisted health transferred between world and carried states.")]
    public float? storedHealth;

    [Header("Outline")]
    [SerializeField] private GameObject outlineObject;
    [SerializeField] private float outlineRange = 2f;
    [SerializeField] private string playerTag = "Player";

    private Transform _player;

    private void Start()
    {
        GameObject p = GameObject.FindWithTag(playerTag);
        if (p != null) _player = p.transform;

        if (outlineObject != null) outlineObject.SetActive(false);
    }

    private void Update()
    {
        if (outlineObject == null || _player == null) return;

        bool inRange = Vector2.Distance(transform.position, _player.position) <= outlineRange;
        if (outlineObject.activeSelf != inRange)
            outlineObject.SetActive(inRange);
    }
}