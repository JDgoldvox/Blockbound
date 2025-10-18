using UnityEngine;

[CreateAssetMenu(fileName = "CaveSO", menuName = "ScriptableObjects/WorldGenerator/Cave")]
public class CaveSO : ScriptableObject
{
    [Header("Debug")] 
    public bool DebugFillWithMaterial;
    
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
    public float SmallCaveFillChance;
    
    [Header("Cavern")]
    public bool EnableCavern; 
    public float CavernFrequency; 
    public float CavernPersistance; 
    public int CavernOctaves ;
    public float CavernAmplitude;
    public float CavernChance;
    
    [Header("Small Cave Fill")]
    public bool EnableSmallCaveFill; 
    public float SmallCaveFillFrequency; 
    public float SmallCaveFillPersistance; 
    public int SmallCaveFillOctaves; 
    public float SmallCaveFillAmplitude;
    public float SmallCaveFillMaterialFillChance;
    
    [Header("Large Cave Fill")]
    public bool EnableLargeCaveFill; 
    public float LargeCaveFillFrequency; 
    public float LargeCaveFillPersistance; 
    public int LargeCaveFillOctaves; 
    public float LargeCaveFillAmplitude;
    public float LargeCaveFillMaterialFillChance;
}
