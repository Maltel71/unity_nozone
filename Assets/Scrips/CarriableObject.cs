using UnityEngine;

public class CarriableObject : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Pre-existing child of the player, disabled by default.")]
    public GameObject carriedVersion;
    [Tooltip("Prefab instantiated in the world when dropped.")]
    public GameObject worldPrefab;

    [Header("Carry Settings")]
    [Range(0f, 1f)]
    [Tooltip("Multiplies the player's movement speed while carrying this object.")]
    public float speedMultiplier = 0.8f;
}