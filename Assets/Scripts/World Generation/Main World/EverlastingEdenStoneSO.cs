using UnityEngine;

[CreateAssetMenu(fileName = "EverlastingEdenStoneSO", menuName = "WorldGenerator/EverlastingEden/Stone")]
public class EverlastingEdenStoneSO : ScriptableObject
{
    [Header("Stone Base")] 
    public bool _enableStoneBase;
    //public bool _stoneBaseAsAir;
    public float StoneBaseFrequency = 1; //0.01f - 0.1f
    public float StoneBasePersistance = 1; //0.01 - 1f
    public int StoneBaseOctaves = 1; //1-8
    public float StoneBaseAmplitude = 2;
    public float StoneBaseChance;
    
    [Header("Stone Detail")]
    public bool _enableStoneDetail; 
    public float StoneDetailFrequency = 1; 
    public float StoneDetailPersistance = 1; 
    public int StoneDetailOctaves = 1; 
    public float StoneDetailAmplitude = 2;
    public float StoneDetailChance;
    
    [Header("Stone Tunnel")]
    public bool _enableStoneTunnel; 
    public float StoneTunnelFrequency = 1; 
    public float StoneTunnelPersistance = 1; 
    public int StoneTunnelOctaves = 1;
    public float StoneTunnelAmplitude = 2;
    public float StoneTunnelChance;
}
