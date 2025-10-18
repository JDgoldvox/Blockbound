using UnityEngine;

/// <summary>
/// This script is the base class to ANY item and includes all basic stats
/// </summary>
public class Item : ScriptableObject
{
    [Header("Item General Properties")]
    public ItemType type;
    public Sprite sprite;
    
    public enum ItemType
    {
        Block,
        Equipment,
        Consumable
    }
}
