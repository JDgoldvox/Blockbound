using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
//using Unity.Mathematics;

[BurstCompile]
public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap _tileMap;
    
    private Dictionary<Vector3Int, Tile> _blocks;
    public Tile stone, blue, orange, grass, dirt;
    private int _totalBlocks;
    
    [Header("Size")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
        
    [Header("Stone")]
    [SerializeField] private float _frequency = 1; //0.01f - 0.1f
    [SerializeField] private float _persistance = 1; //0.01 - 1f
    [SerializeField] private int _octaves = 1; //1-8
    
    private float _tempTimer = 0;
    
    [BurstCompile]
    private struct GenerateWorldJob : IJobFor
    {
        [ReadOnly] public float freq;
        [ReadOnly] public float persist;
        [ReadOnly] public int oct;
        [ReadOnly] public int width;
        
        public NativeArray<int> tileTypeMap;
        
        public void Execute(int index)
        {
            int x =  index % width;
            int y =  index / width;
            
            float chance = NoiseUtils.OctavePerlinNoise(x, y, oct, persist, freq);

            if (chance < 0.5f)
            {
                tileTypeMap[index] = 1;
            }
        }
    }

    private void Awake()
    {
        _blocks = new Dictionary<Vector3Int, Tile>();  
        _totalBlocks = _width * _height;
    }

    void Start()
    {
        
    }

    public void GenerateWorldParallel()
    {
        //stuff to do before
        var stopwatch = Stopwatch.StartNew();
        _tileMap.ClearAllTiles();
        
        NativeArray<int> _tileTypeMap = new NativeArray<int>(_totalBlocks, Allocator.TempJob);
        
        // Initialize the job data
        var job = new GenerateWorldJob()
        {
            freq = _frequency,
            persist = _persistance,
            oct =  _octaves,
            width = _width,
            tileTypeMap = _tileTypeMap
        };
        
        JobHandle jobHandler = default;
        jobHandler = job.ScheduleParallelByRef(_totalBlocks, 64, jobHandler);
        jobHandler.Complete();
        
        //main thread map build
        for (int i = 0; i < _tileTypeMap.Length; i++)
        {
            int x = i % _width;
            int y = i / _width;
            int tile = _tileTypeMap[i];

            Vector3Int tilePos = new Vector3Int(-(_width / 2) + x , -(_height / 2) + y);
            
            switch (tile)
            {
                case 0:
                    break;
                case 1:
                    _tileMap.SetTile(tilePos, stone);
                    break;
            }
        }
        
        //clean up
        _tileTypeMap.Dispose();
        
        stopwatch.Stop();
        UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + "ms");
    }
    
    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public void GenerateWorld()
    {
        var stopwatch = Stopwatch.StartNew();
        
        _tileMap.ClearAllTiles();
        
        int halfWidth = _width / 2;
        int halfHeight = _height / 2;
        
        for (int x = -halfWidth; x < halfWidth; x++) 
        {
            for (int y = -halfHeight; y < halfHeight; y++)
            {
                SpawnWorldTile(new Vector3Int(x, y));
            }
        }
        
        stopwatch.Stop();
        UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + "ms");
    }
    
    private void SpawnWorldTile(Vector3Int pos)
    {
        float chance = NoiseUtils.OctavePerlinNoise(pos.x, pos.y, _octaves, _persistance, _frequency);
        
        if (chance < 0.5f)
        {
            _tileMap.SetTile(pos, stone);
        }
    }
};