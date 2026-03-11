using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class CarriedObjectEntry
{
    [Tooltip("Disabled child GameObject on the player prefab.")]
    public GameObject carriedObject;
    [Tooltip("Prefab spawned in the world when dropped.")]
    public GameObject worldPrefab;
}

[RequireComponent(typeof(CharacterController2D))]
public class PickupSystem : MonoBehaviour
{
    [Header("Carried Objects")]
    [SerializeField] private List<CarriedObjectEntry> carriedObjects = new();

    [Header("Pickup")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private List<string> largeObjectTags = new();
    [SerializeField] private List<string> smallItemTags = new();

    [Header("Drop")]
    [SerializeField] private Vector2 dropOffset = new Vector2(0f, 1.5f);

    [Header("Hold Position")]
    [SerializeField] private Vector2 holdOffset = new Vector2(0f, 1.5f);
    [SerializeField] private bool mouseAim = false;

    [Header("Mouse Aim Angle Limits")]
    [SerializeField] private float minAimAngle = -30f;
    [SerializeField] private float maxAimAngle = 90f;

    [Header("Movement Effects")]
    [SerializeField] private float walkSpeedMultiplier = 0.8f;
    [SerializeField] private bool disableRunWhileCarrying = true;
    [SerializeField] private bool disableJumpWhileCarrying = false;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private CharacterController2D _controller;
    private Hotbar _hotbar;

    private int _activeIndex = -1;

    public bool IsCarrying => _activeIndex >= 0;
    public bool CanRun => !IsCarrying || !disableRunWhileCarrying;
    public bool CanJump => !IsCarrying || !disableJumpWhileCarrying;

    private bool IsFacingRight => transform.localScale.x >= 0f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
        _hotbar = GetComponent<Hotbar>();

        foreach (var entry in carriedObjects)
            if (entry.carriedObject != null)
                entry.carriedObject.SetActive(false);
    }

    private void Update()
    {
        if (InteractPressed())
        {
            if (IsCarrying)
                Drop();
            else
                TryPickup();
        }

        if (IsCarrying)
            UpdateHeldPosition();
    }

    private bool InteractPressed()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame) return true;

        var pad = Gamepad.current;
        if (pad != null && pad.buttonWest.wasPressedThisFrame) return true;

        return false;
    }

    private void TryPickup()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);

        float nearestDist = float.MaxValue;
        Collider2D best = null;
        bool bestIsLarge = false;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            bool isLarge = largeObjectTags.Contains(hit.tag);
            bool isSmall = !isLarge && smallItemTags.Contains(hit.tag);
            if (!isLarge && !isSmall) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                best = hit;
                bestIsLarge = isLarge;
            }
        }

        if (best == null)
        {
            if (debugMode) Debug.Log("[PickupSystem] No valid pickup found.");
            return;
        }

        if (bestIsLarge)
            PickupLarge(best);
        else
            PickupSmall(best);
    }

    private void PickupLarge(Collider2D col)
    {
        var carriable = col.GetComponent<CarriableObject>();
        if (carriable == null)
        {
            if (debugMode) Debug.LogWarning($"[PickupSystem] No CarriableObject on '{col.gameObject.name}'.");
            return;
        }

        int index = carriable.carriedIndex;
        if (index < 0 || index >= carriedObjects.Count)
        {
            if (debugMode) Debug.LogWarning($"[PickupSystem] carriedIndex {index} is out of range.");
            return;
        }

        CarriedObjectEntry entry = carriedObjects[index];
        if (entry.carriedObject == null)
        {
            if (debugMode) Debug.LogWarning($"[PickupSystem] carriedObject at index {index} is not assigned.");
            return;
        }

        Destroy(col.gameObject);

        _activeIndex = index;
        entry.carriedObject.SetActive(true);

        _controller.SetSpeedMultiplier(walkSpeedMultiplier);

        if (debugMode) Debug.Log($"[PickupSystem] Picked up index {index} '{entry.carriedObject.name}'.");
    }

    private void PickupSmall(Collider2D col)
    {
        if (_hotbar == null)
        {
            if (debugMode) Debug.LogWarning("[PickupSystem] No Hotbar found on player.");
            return;
        }

        var worldItem = col.GetComponent<WorldItem>();
        if (worldItem == null || worldItem.itemData == null)
        {
            if (debugMode) Debug.LogWarning($"[PickupSystem] Missing WorldItem or ItemData on '{col.gameObject.name}'.");
            return;
        }

        if (_hotbar.TryAddItem(worldItem.itemData))
            Destroy(col.gameObject);
    }

    private void Drop()
    {
        if (_activeIndex < 0) return;

        CarriedObjectEntry entry = carriedObjects[_activeIndex];

        entry.carriedObject.SetActive(false);

        if (entry.worldPrefab != null)
        {
            Vector2 spawnPos = (Vector2)transform.position + dropOffset;
            GameObject spawned = Instantiate(entry.worldPrefab, spawnPos, Quaternion.identity);
            if (spawned.GetComponent<Rigidbody2D>() == null)
                spawned.AddComponent<Rigidbody2D>();

            if (debugMode) Debug.Log($"[PickupSystem] Dropped '{spawned.name}' at {spawnPos}.");
        }
        else
        {
            if (debugMode) Debug.LogWarning($"[PickupSystem] No worldPrefab assigned at index {_activeIndex}.");
        }

        _activeIndex = -1;
        _controller.SetSpeedMultiplier(1f);
    }

    private void UpdateHeldPosition()
    {
        if (_activeIndex < 0) return;
        GameObject carried = carriedObjects[_activeIndex].carriedObject;
        if (carried == null) return;

        if (mouseAim && Mouse.current != null && Camera.main != null)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 dir = (mouseWorld - (Vector2)transform.position).normalized;

            if (!IsFacingRight)
                dir.x = -dir.x;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float clampedAngle = Mathf.Clamp(angle, minAimAngle, maxAimAngle);
            float rad = clampedAngle * Mathf.Deg2Rad;
            Vector2 clampedDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            carried.transform.localPosition = clampedDir * holdOffset.magnitude;

            float worldAngle = IsFacingRight ? clampedAngle - 90f : 90f - clampedAngle;
            carried.transform.rotation = Quaternion.Euler(0f, 0f, worldAngle);
        }
        else
        {
            carried.transform.localPosition = holdOffset;
            carried.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsCarrying ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}