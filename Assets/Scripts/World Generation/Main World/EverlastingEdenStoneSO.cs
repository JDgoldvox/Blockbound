using UnityEngine;

[CreateAssetMenu(fileName = "EverlastingEdenStoneSO", menuName = "WorldGenerator/EverlastingEden/Stone")]
public class EverlastingEdenStoneSO : ScriptableObject
{
    [Header("Stone Base")] 
    public bool _enableStoneBase;
    //public bool _stoneBaseAsAir;
    public float _stoneBaseFrequency = 1; //0.01f - 0.1f
    public float _stoneBasePersistance = 1; //0.01 - 1f
    public int _stoneBaseOctaves = 1; //1-8
    public float _stoneBaseAmplitude = 2;
    public float _stoneBaseChance;
    
    [Header("Stone Detail")]
    public bool _enableStoneDetail; 
    public float _stoneDetailFrequency = 1; 
    public float _stoneDetailPersistance = 1; 
    public int _stoneDetailOctaves = 1; 
    public float _stoneDetailAmplitude = 2;
    public float _stoneDetailChance;
    
    [Header("Stone Tunnel")]
    public bool _enableStoneTunnel; 
    public float _stoneTunnelFrequency = 1; 
    public float _stoneTunnelPersistance = 1; 
    public int _stoneTunnelOctaves = 1;
    public float _stoneTunnelAmplitude = 2;
    public float _stoneTunnelChance;
}
