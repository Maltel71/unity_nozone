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

    private GameObject _carriedVersion;
    private GameObject _worldPrefab;

    public bool IsCarrying => _carriedVersion != null;
    public bool CanRun => !IsCarrying || !disableRunWhileCarrying;
    public bool CanJump => !IsCarrying || !disableJumpWhileCarrying;

    private void Awake()
    {
        _controller = GetComponent<CharacterController2D>();
        _hotbar = GetComponent<Hotbar>();
    }

    private void Update()
    {
        if (InteractPressed())
        {
            if (debugMode) Debug.Log($"[PickupSystem] >>> E pressed | IsCarrying: {IsCarrying} | _carriedVersion is null: {_carriedVersion == null}");

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
        if (pad != null && pad.buttonWest.wasPressedThisFrame) return true; // Xbox X / PS4 Square

        return false;
    }

    private void TryPickup()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);

        if (debugMode)
        {
            Debug.Log($"[PickupSystem] OverlapCircle at {transform.position} r={pickupRange} | hits: {hits.Length}");
            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject) continue;
                bool isLarge = largeObjectTags.Contains(hit.tag);
                bool isSmall = smallItemTags.Contains(hit.tag);
                Debug.Log($"[PickupSystem]   Collider: '{hit.gameObject.name}' tag='{hit.tag}' isLarge={isLarge} isSmall={isSmall}");
            }
        }

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
            if (debugMode) Debug.Log("[PickupSystem] No valid pickup found. Check tags match the Large/Small tag lists.");
            return;
        }

        if (debugMode) Debug.Log($"[PickupSystem] Best pickup: '{best.gameObject.name}' isLarge={bestIsLarge}");

        if (bestIsLarge)
            PickupLarge(best);
        else
            PickupSmall(best);
    }

    private void PickupLarge(Collider2D col)
    {
        if (debugMode) Debug.Log($"[PickupSystem] PickupLarge called on '{col.gameObject.name}'");

        var carriable = col.GetComponent<CarriableObject>();
        if (carriable == null)
        {
            if (debugMode) Debug.LogWarning($"[PickupSystem] FAIL — no CarriableObject on '{col.gameObject.name}'");
            return;
        }

        if (debugMode) Debug.Log($"[PickupSystem] CarriableObject found | carriedVersion={(carriable.carriedVersion != null ? carriable.carriedVersion.name : "NULL")} | worldPrefab={(carriable.worldPrefab != null ? carriable.worldPrefab.name : "NULL — assign it in the Inspector on CarriableObject!")}");

        if (carriable.carriedVersion == null)
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

        _carriedVersion.SetActive(true);

        if (debugMode) Debug.Log($"[PickupSystem] World object destroyed | carriedVersion active={_carriedVersion.activeSelf} | parent='{_carriedVersion.transform.parent?.name ?? "none"}'");

        float effective = walkSpeedMultiplier * speedMultiplier;
        _controller.SetSpeedMultiplier(effective);

        if (debugMode) Debug.Log($"[PickupSystem] Pickup complete | IsCarrying={IsCarrying} | speedMultiplier={effective}");
    }

    private void PickupSmall(Collider2D col)
    {
        if (_hotbar == null)
        {
            if (debugMode) Debug.LogWarning("[PickupSystem] No Hotbar found on player.");
            return;
        }

        var worldItem = col.GetComponent<WorldItem>();
        if (worldItem == null)
        {
            if (debugMode) Debug.LogWarning($"[PickupSystem] No WorldItem component on '{col.gameObject.name}'.");
            return;
        }
        if (worldItem.itemData == null)
        {
            if (debugMode) Debug.LogWarning($"[PickupSystem] WorldItem on '{col.gameObject.name}' has no ItemData assigned.");
            return;
        }

        for (int i = 0; i < _hotbar.SlotCount; i++)
        {
            if (_hotbar.GetItem(i) != null) continue;
            _hotbar.SetItem(i, worldItem.itemData);
            Destroy(col.gameObject);
            if (debugMode) Debug.Log($"[PickupSystem] Small item '{worldItem.itemData.itemName}' added to hotbar slot {i}.");
            return;
        }

        if (debugMode) Debug.Log("[PickupSystem] Hotbar is full.");
    }

    private void Drop()
    {
        if (debugMode) Debug.Log($"[PickupSystem] Drop called | _carriedVersion='{(_carriedVersion != null ? _carriedVersion.name : "NULL")}' | _worldPrefab='{(_worldPrefab != null ? _worldPrefab.name : "NULL")}'");

        if (_carriedVersion == null) return;

        _carriedVersion.SetActive(false);

        if (_worldPrefab != null)
        {
            Vector2 dropPos = (Vector2)transform.position + holdOffset;
            GameObject spawned = Instantiate(_worldPrefab, dropPos, Quaternion.identity);
            spawned.SetActive(true);

            if (spawned.GetComponent<Rigidbody2D>() == null)
                spawned.AddComponent<Rigidbody2D>();

            if (debugMode) Debug.Log($"[PickupSystem] Spawned '{spawned.name}' at {dropPos}.");
        }
        else
        {
            if (debugMode) Debug.LogWarning("[PickupSystem] No worldPrefab assigned — assign it in the Inspector on the CarriableObject component of your world pickup object.");
        }

        _carriedVersion = null;
        _worldPrefab = null;
        _controller.SetSpeedMultiplier(1f);

        if (debugMode) Debug.Log($"[PickupSystem] Drop complete | IsCarrying={IsCarrying}");
    }

    private void UpdateHeldPosition()
    {
        if (_carriedVersion == null) return;

        if (mouseAim && Mouse.current != null && Camera.main != null)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 dir = (mouseWorld - (Vector2)transform.position).normalized;
            _carriedVersion.transform.localPosition = dir * holdOffset.magnitude;
        }
        else
        {
            _carriedVersion.transform.localPosition = holdOffset;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsCarrying ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}