using UnityEngine;

public enum ItemType { Consumable }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    public Sprite icon;

    [Header("Stacking")]
    public bool isStackable = true;
    public int maxStack = 10;

    [Header("Usage")]
    public ItemType itemType = ItemType.Consumable;
    public float healAmount = 0f;
    public bool useInstantly = false;
}