using UnityEngine;

/// <summary>
/// This script is the base class to ANY item and includes all basic stats
/// </summary>
public class ItemSO : ScriptableObject
{
    [Header("ItemSO General Properties")]
    public int ID;
    public Sprite sprite;
}
