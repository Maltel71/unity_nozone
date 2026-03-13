using UnityEngine;
using UnityEngine.InputSystem;

public class Hotbar : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int slotCount = 5;

    [Header("State")]
    [SerializeField] private int selectedSlot = 0;

    private ItemData[] _slots;
    private int[] _stacks;

    private PlayerHealth _playerHealth;

    public int SelectedSlot => selectedSlot;
    public int SlotCount => slotCount;

    private void Awake()
    {
        _slots = new ItemData[slotCount];
        _stacks = new int[slotCount];
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        HandleSlotSelection();
        HandleUse();
    }

    private void HandleSlotSelection()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) SelectSlot(0);
        else if (kb.digit2Key.wasPressedThisFrame) SelectSlot(1);
        else if (kb.digit3Key.wasPressedThisFrame) SelectSlot(2);
        else if (kb.digit4Key.wasPressedThisFrame) SelectSlot(3);
        else if (kb.digit5Key.wasPressedThisFrame) SelectSlot(4);
    }

    private void HandleUse()
    {
        var kb = Keyboard.current;
        var pad = Gamepad.current;

        bool pressed = (kb != null && kb.fKey.wasPressedThisFrame) ||
                       (pad != null && pad.buttonNorth.wasPressedThisFrame);

        if (pressed)
            UseSelectedItem();
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;
        selectedSlot = index;
    }

    public void UseSelectedItem()
    {
        ItemData item = _slots[selectedSlot];
        if (item == null) return;

        switch (item.itemType)
        {
            case ItemType.Consumable:
                if (item.healAmount > 0f && _playerHealth != null)
                    _playerHealth.Heal(item.healAmount);
                break;
        }

        Debug.Log($"Used '{item.itemName}' from slot {selectedSlot}.");
        ConsumeItem(selectedSlot);
    }

    private void ConsumeItem(int index)
    {
        _stacks[index]--;
        if (_stacks[index] <= 0)
        {
            _slots[index] = null;
            _stacks[index] = 0;
        }
    }

    // Returns true if item was added, stacked, or consumed instantly
    public bool TryAddItem(ItemData item)
    {
        // Instant-use items apply their effect immediately and never occupy a slot
        if (item.useInstantly)
        {
            ApplyItemEffect(item);
            Debug.Log($"Used '{item.itemName}' instantly on pickup.");
            return true;
        }

        // Try to stack first
        if (item.isStackable)
        {
            for (int i = 0; i < slotCount; i++)
            {
                if (_slots[i] == item && _stacks[i] < item.maxStack)
                {
                    _stacks[i]++;
                    Debug.Log($"Stacked '{item.itemName}' in slot {i} ({_stacks[i]}/{item.maxStack}).");
                    return true;
                }
            }
        }

        // Find empty slot
        for (int i = 0; i < slotCount; i++)
        {
            if (_slots[i] != null) continue;
            _slots[i] = item;
            _stacks[i] = 1;
            Debug.Log($"'{item.itemName}' added to slot {i}.");
            return true;
        }

        Debug.Log("Hotbar is full.");
        return false;
    }

    private void ApplyItemEffect(ItemData item)
    {
        switch (item.itemType)
        {
            case ItemType.Consumable:
                if (item.healAmount > 0f && _playerHealth != null)
                    _playerHealth.Heal(item.healAmount);
                break;
        }
    }

    public void SetItem(int index, ItemData item)
    {
        if (index < 0 || index >= slotCount) return;
        _slots[index] = item;
        _stacks[index] = item != null ? 1 : 0;
    }

    public ItemData GetItem(int index)
    {
        if (index < 0 || index >= slotCount) return null;
        return _slots[index];
    }

    public int GetStack(int index)
    {
        if (index < 0 || index >= slotCount) return 0;
        return _stacks[index];
    }

    public ItemData GetSelectedItem() => GetItem(selectedSlot);
}