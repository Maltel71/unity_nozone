using UnityEngine;

public class CarriableObject : MonoBehaviour
{
    [Tooltip("Index into the PickupSystem's carried object list.")]
    public int carriedIndex;

    [Tooltip("Persisted health transferred between world and carried states.")]
    public float? storedHealth;
}