using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using EverlastingEdenGenerationJobs;

[BurstCompile]
public class EverlastingEdenWorldGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap _tileMap;
    
    private Dictionary<Vector3Int, Tile> _blocks;
    public Tile stone, blue, orange, grass, dirt;
    private int _totalBlocks;
    private float _tempTimer = 0;
    
    [Header("Size")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;

    [Header("Stone World Generator")] 
    [SerializeField] private EverlastingEdenStoneSO _mainWorldStoneSO; 

    private void Awake()
    {
        _blocks = new Dictionary<Vector3Int, Tile>();  
    }

    public void GenerateWorldParallel()
    {
        _totalBlocks = _width * _height;
        
        //stuff to do before
        var stopwatch = Stopwatch.StartNew();
        _tileMap.ClearAllTiles();
        
        NativeArray<int> tileTypeMap = new NativeArray<int>(_totalBlocks, Allocator.TempJob, NativeArrayOptions.ClearMemory);
        
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

            Vector3Int tilePos = new Vector3Int(-(_width / 2) + x , y);
            
            switch (tile)
            {
                case 0:
                    _tileMap.SetTile(tilePos, null);
                    break;
                case 1:
                    _tileMap.SetTile(tilePos, stone);
                    break;
                case 2: 
                    _tileMap.SetTile(tilePos, blue);
                    break;
            }
        }
    }

    private void GenerateStoneLayer(NativeArray<int> tileTypeMap)
    {
        // stone base structure
        if (_mainWorldStoneSO._enableStoneBase)
        {
            var stoneBaseJob = new EverlastingEdenGenerationJobs.StoneBaseGenerationJob()
            {
                width = _width,
                frequency = _mainWorldStoneSO._stoneBaseFrequency,
                persistance = _mainWorldStoneSO._stoneBasePersistance,
                octaves = _mainWorldStoneSO._stoneBaseOctaves,
                amplitude = _mainWorldStoneSO._stoneBaseAmplitude,
                stoneChance = _mainWorldStoneSO._stoneBaseChance,
                tileTypeMap = tileTypeMap
            };

            JobHandle stoneBaseJobHandler = default;
            stoneBaseJobHandler = stoneBaseJob.ScheduleParallelByRef(_totalBlocks, 128, stoneBaseJobHandler);
            stoneBaseJobHandler.Complete();
        }

        //stone detail layer
        if (_mainWorldStoneSO._enableStoneDetail)
        {
            var stoneDetailJob = new EverlastingEdenGenerationJobs.StoneDetailGenerationJob()
            {
                width = _width,
                frequency = _mainWorldStoneSO._stoneDetailFrequency,
                persistance = _mainWorldStoneSO._stoneDetailPersistance,
                octaves =  _mainWorldStoneSO._stoneDetailOctaves,
                amplitude = _mainWorldStoneSO._stoneDetailAmplitude,
                stoneChance = _mainWorldStoneSO._stoneDetailChance,
                tileTypeMap = tileTypeMap
            };
            
            JobHandle stoneDetailJobHandler = default;
            stoneDetailJobHandler = stoneDetailJob.ScheduleParallelByRef(_totalBlocks, 120, stoneDetailJobHandler);
            stoneDetailJobHandler.Complete();
        }
        
        if (_mainWorldStoneSO._enableStoneTunnel)
        {
            var stoneTunnelJob = new EverlastingEdenGenerationJobs.StoneTunnelGenerationJob()
            {
                width = _width,
                frequency = _mainWorldStoneSO._stoneTunnelFrequency,
                persistance = _mainWorldStoneSO._stoneTunnelPersistance,
                octaves = _mainWorldStoneSO._stoneTunnelOctaves,
                amplitude = _mainWorldStoneSO._stoneTunnelAmplitude,
                stoneChance = _mainWorldStoneSO._stoneTunnelChance,
                tileTypeMap = tileTypeMap
            };

            JobHandle stoneTunnelJobHandler = default;
            stoneTunnelJobHandler = stoneTunnelJob.ScheduleParallelByRef(_totalBlocks, 120, stoneTunnelJobHandler);
            stoneTunnelJobHandler.Complete();
        }
    }
};