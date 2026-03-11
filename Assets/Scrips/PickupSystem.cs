using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController2D))]
public class PickupSystem : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private List<string> largeObjectTags = new();
    [SerializeField] private List<string> smallItemTags = new();

    [Header("Hold Position")]
    [SerializeField] private Vector2 holdOffset = new Vector2(0f, 1.5f);
    [SerializeField] private bool mouseAim = false;

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

    private void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
        _hotbar = GetComponent<Hotbar>();

        // Ensure all carried objects start disabled
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
            if (debugMode) Debug.LogWarning($"[PickupSystem] FAIL — no CarriableObject on '{col.gameObject.name}'");
            return;
        }

        if (debugMode) Debug.Log($"[PickupSystem] CarriableObject found | carriedVersion={(carriable.carriedVersion != null ? carriable.carriedVersion.name : "NULL")} | worldPrefab={(carriable.worldPrefab != null ? carriable.worldPrefab.name : "NULL — assign it in the Inspector on CarriableObject!")}");

        CarriedObjectEntry entry = carriedObjects[index];
        if (entry.carriedObject == null)
        {
            if (debugMode) Debug.LogWarning("[PickupSystem] FAIL — carriedVersion is not assigned on CarriableObject.");
            return;
        }

        // Cache before Destroy — Destroy removes the component, nulling any reference to it
        _carriedVersion = carriable.carriedVersion;
        _worldPrefab = carriable.worldPrefab;
        float speedMultiplier = carriable.speedMultiplier;

        if (debugMode) Debug.Log($"[PickupSystem] Cached | _carriedVersion='{_carriedVersion.name}' | _worldPrefab={(_worldPrefab != null ? _worldPrefab.name : "NULL")}");

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

        for (int i = 0; i < _hotbar.SlotCount; i++)
        {
            if (_hotbar.GetItem(i) != null) continue;
            _hotbar.SetItem(i, worldItem.itemData);
            Destroy(col.gameObject);
            if (debugMode) Debug.Log($"[PickupSystem] '{worldItem.itemData.itemName}' added to hotbar slot {i}.");
            return;
        }

        if (debugMode) Debug.Log("[PickupSystem] Hotbar is full.");
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
            if (debugMode) Debug.LogWarning("[PickupSystem] No worldPrefab assigned — assign it in the Inspector on the CarriableObject component of your world pickup object.");
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
            carried.transform.localPosition = dir * holdOffset.magnitude;
        }
        else
        {
            carried.transform.localPosition = holdOffset;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsCarrying ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}