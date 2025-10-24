using UnityEngine;

[CreateAssetMenu(fileName = "OreSO", menuName = "ScriptableObjects/WorldGenerator/Ore")]
public class EverlastingEdenOreSO : ScriptableObject
{
    [Header("Copper-----------------------------------------------------------------------------")] 
    public bool enableCopper;
    [Header("Large Vein")] 
    public int CopperLargeVeinQuantity;
    public int CopperLargeVeinMaxTileQuantity;
    public float CopperLargeVeinSpreadChance;

    [Header("Small Vein")] 
    public int CopperSmallVeinQuantity;
    public int CopperSmallVeinMaxTileQuantity;
    public float CopperSmallVeinSpreadChance;
    
    [Header("Iron")] 
    public bool enableIron;
    public int IronQuantity;
}
