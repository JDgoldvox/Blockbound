using UnityEngine;

[CreateAssetMenu(fileName = "CaveSO", menuName = "WorldGenerator/Cave")]
public class EverlastingEdenCaveSO : ScriptableObject
{
    [Header("Debug")] 
    public bool FillWithMaterial;
    
    [Header("Cave Base")] 
    [Tooltip("Generate standard cave system")]
    public bool EnableCaveBase;
    //public bool _stoneBaseAsAir;
    public float CaveBaseFrequency; //0.01f - 0.1f
    public float CaveBasePersistance; //0.01 - 1f
    public int CaveBaseOctaves; //1-4
    public float CaveBaseAmplitude; //0.5-3
    public float CaveBaseMaterialFillChance;//0-1
    
    [Header("Small Cave")]
    public bool EnableSmallCave; 
    public float SmallCaveFrequency; 
    public float SmallCavePersistance; 
    public int SmallCaveOctaves; 
    public float SmallCaveAmplitude;
    public float SmallCaveMaterialFillChance;
    
    [Header("Cavern")]
    public bool _enableStoneTunnel; 
    public float StoneTunnelFrequency; 
    public float StoneTunnelPersistance; 
    public int StoneTunnelOctaves ;
    public float StoneTunnelAmplitude;
    public float StoneTunnelChance;
}
