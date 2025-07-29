using UnityEngine;

[CreateAssetMenu(fileName = "EverlastingEdenOreSO", menuName = "WorldGenerator/EverlastingEden/Ore")]
public class EverlastingEdenOreSO : ScriptableObject
{
    [Header("Copper")] 
    public bool EnableCopper;
    public int CopperLargeVeinQuantity;
    public int CopperSmallVeinQuantity;
    public float CopperLargeVeinSpreadChance;
    public float CopperSmallVeinSpreadChance;

    [Header("Iron")] 
    public bool _enableIron;
    public int IronQuantity;
}
