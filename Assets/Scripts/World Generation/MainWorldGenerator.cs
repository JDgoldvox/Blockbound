using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using MainWorldGenerationJobs;

[BurstCompile]
public class MainWorldGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap _tileMap;
    
    private Dictionary<Vector3Int, Tile> _blocks;
    public Tile stone, blue, orange, grass, dirt;
    private int _totalBlocks;
    
    [Header("Size")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
        
    [Header("Stone Base")]
    [SerializeField] private float _frequency = 1; //0.01f - 0.1f
    [SerializeField] private float _persistance = 1; //0.01 - 1f
    [SerializeField] private int _octaves = 1; //1-8
    [SerializeField] private float _amplitude = 2;
    
    private float _tempTimer = 0;

    private void Awake()
    {
        _blocks = new Dictionary<Vector3Int, Tile>();  
    }

    void Start()
    {
        
    }

    public void GenerateWorldParallel()
    {
        _totalBlocks = _width * _height;
        
        //stuff to do before
        var stopwatch = Stopwatch.StartNew();
        _tileMap.ClearAllTiles();
        
        NativeArray<int> tileTypeMap = new NativeArray<int>(_totalBlocks, Allocator.TempJob);
        
        //Generate stone
        GenerateStoneLayer(tileTypeMap);
        
        //main thread map build
        BuildTileMap(tileTypeMap);
        
        //clean up
        tileTypeMap.Dispose();
        
        stopwatch.Stop();
        UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + "ms");
    }

    private void BuildTileMap(NativeArray<int> tileTypeMap)
    {
        //main thread map build
        for (int i = 0; i < tileTypeMap.Length; i++)
        {
            int x = i % _width;
            int y = i / _width;
            int tile = tileTypeMap[i];

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
    }

    private void GenerateStoneLayer(NativeArray<int> tileTypeMap)
    {
        //Base stone layer
        var stoneBaseJob = new MainWorldGenerationJobs.StoneBaseGenerationJob()
        {
            frequency = _frequency,
            persistance = _persistance,
            octaves =  _octaves,
            width = _width,
            amplitude = _amplitude,
            tileTypeMap = tileTypeMap
        };
        
        JobHandle stoneBaseJobHandler = default;
        stoneBaseJobHandler = stoneBaseJob.ScheduleParallelByRef(_totalBlocks, 128, stoneBaseJobHandler);
        
        //stoneBaseJobHandler.Complete();
        
        //stone detail layer
        
        var stoneDetailJob = new MainWorldGenerationJobs.StoneDetailGenerationJob()
        {
            frequency = _frequency,
            persistance = _persistance,
            octaves =  _octaves,
            width = _width,
            amplitude = _amplitude,
            tileTypeMap = tileTypeMap
        };
        
        JobHandle stoneDetailJobHandler = stoneDetailJob.ScheduleParallelByRef(_totalBlocks, 64, stoneBaseJobHandler);
        
        //waits for job to complete //dont need it anymore
        stoneDetailJobHandler.Complete();
        
    }
};