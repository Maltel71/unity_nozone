using UnityEngine;

public class Hotbar : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int slotCount = 5;

    [Header("State")]
    [SerializeField] private int selectedSlot = 0;

    private ItemData[] slots;

    public int SelectedSlot => selectedSlot;
    public int SlotCount => slotCount;

    private void Awake()
    {
        slots = new ItemData[slotCount];
    }

    private void Update()
    {
        HandleSlotSelection();
    }

    private void HandleSlotSelection()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                break;
            }
        }
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;
        selectedSlot = index;
        Debug.Log($"Selected slot {index}: {(slots[index] != null ? slots[index].itemName : "Empty")}");
    }

    public void SetItem(int index, ItemData item)
    {
        if (index < 0 || index >= slotCount) return;
        slots[index] = item;
    }

    public ItemData GetItem(int index)
    {
        if (index < 0 || index >= slotCount) return null;
        return slots[index];
    }

    public ItemData GetSelectedItem() => GetItem(selectedSlot);
}