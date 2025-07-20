using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap _tileMap;
    public Tile stone, blue, orange, grass, dirt;
    private int _truePositiveOffset = 10000;
    
    [Header("Size")]
    [SerializeField] private int width;
    [SerializeField] private int height;
        
    [Header("Stone")]
    [SerializeField] private float _frequency = 1; //0.01f - 0.1f
    [SerializeField] private float _persistance = 1; //0.01 - 1f
    [SerializeField] private int _octaves = 1; //1-8

    private float _tempTimer = 0;
    // Start is called once before the fi   rst execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        GenerateWorld();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateWorld()
    {
        var stopwatch = Stopwatch.StartNew();
        
        _tileMap.ClearAllTiles();
        
        for (int x = -width; x < width; x++)
        {
            for (int y = -height; y < height; y++)
            {
                SpawnWorldTile(new Vector3Int(x, y));
            }
        }
        
        stopwatch.Stop();
        UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + "ms");
    }

    private void SpawnWorldTile(Vector3Int pos)
    {
        float chance = OctavePerlinNoise(pos.x, pos.y, _octaves, _persistance, _frequency);
        //Debug.Log(chance);
        if (chance < 0.5f)
        {
            _tileMap.SetTile(pos, stone);
        }
    }

    private float OctavePerlinNoise(float x, float y, int octaves, float persistence, float frequency)
    {
        float total = 0;
        float maxValue = 0;
        float amplitude = 1;
        
        for (int i = 0 ; i < octaves; i++) {
            total += Mathf.PerlinNoise((x + _truePositiveOffset) * frequency, (y + _truePositiveOffset) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }
        
        return total/maxValue;
    }
}
