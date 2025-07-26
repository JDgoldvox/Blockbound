using UnityEngine;

[CreateAssetMenu(fileName = "EverlastingEdenOreSO", menuName = "WorldGenerator/EverlastingEden/Ore")]
public class EverlastingEdenOreSO : ScriptableObject
{
    [Header("Copper")] public bool _enableCopper;
    public float _copperFrequency = 1; //0.01f - 0.1f
    public float _copperPersistance = 1; //0.01 - 1f
    public int _copperOctaves = 1; //1-8
    public float _copperAmplitude = 2;
    public float _copperChance;

    [Header("Iron")] public bool _enableIron;
    public float _ironFrequency = 1; //0.01f - 0.1f
    public float _ironPersistance = 1; //0.01 - 1f
    public int _ironOctaves = 1; //1-8
    public float _ironAmplitude = 2;
    public float _ironChance;
}
