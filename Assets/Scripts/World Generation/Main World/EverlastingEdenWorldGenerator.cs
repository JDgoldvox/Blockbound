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
    public Tile stone, copper, iron, grass, dirt;
    private int _totalBlocks;
    private float _tempTimer = 0;
    
    [Header("Size")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;

    [Header("Stone World Generator")] 
    [SerializeField] private EverlastingEdenStoneSO _everlastingEdenStoneSO; 
    [SerializeField] private EverlastingEdenOreSO _everlastingEdenOreSO; 

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
        
        //Generate Ore
        GenerateOreLayer(tileTypeMap);
        
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
                    _tileMap.SetTile(tilePos, copper);
                    break;
            }
        }
    }

    private void GenerateStoneLayer(NativeArray<int> tileTypeMap)
    {
        // stone base structure
        if (_everlastingEdenStoneSO._enableStoneBase)
        {
            var stoneBaseJob = new EverlastingEdenGenerationJobs.StoneBaseGenerationJob()
            {
                width = _width,
                frequency = _everlastingEdenStoneSO._stoneBaseFrequency,
                persistance = _everlastingEdenStoneSO._stoneBasePersistance,
                octaves = _everlastingEdenStoneSO._stoneBaseOctaves,
                amplitude = _everlastingEdenStoneSO._stoneBaseAmplitude,
                stoneChance = _everlastingEdenStoneSO._stoneBaseChance,
                tileTypeMap = tileTypeMap
            };

            JobHandle stoneBaseJobHandler = default;
            stoneBaseJobHandler = stoneBaseJob.ScheduleParallelByRef(_totalBlocks, 128, stoneBaseJobHandler);
            stoneBaseJobHandler.Complete();
        }

        //stone detail layer
        if (_everlastingEdenStoneSO._enableStoneDetail)
        {
            var stoneDetailJob = new EverlastingEdenGenerationJobs.StoneDetailGenerationJob()
            {
                width = _width,
                frequency = _everlastingEdenStoneSO._stoneDetailFrequency,
                persistance = _everlastingEdenStoneSO._stoneDetailPersistance,
                octaves =  _everlastingEdenStoneSO._stoneDetailOctaves,
                amplitude = _everlastingEdenStoneSO._stoneDetailAmplitude,
                stoneChance = _everlastingEdenStoneSO._stoneDetailChance,
                tileTypeMap = tileTypeMap
            };
            
            JobHandle stoneDetailJobHandler = default;
            stoneDetailJobHandler = stoneDetailJob.ScheduleParallelByRef(_totalBlocks, 120, stoneDetailJobHandler);
            stoneDetailJobHandler.Complete();
        }
        
        if (_everlastingEdenStoneSO._enableStoneTunnel)
        {
            var stoneTunnelJob = new EverlastingEdenGenerationJobs.StoneTunnelGenerationJob()
            {
                width = _width,
                frequency = _everlastingEdenStoneSO._stoneTunnelFrequency,
                persistance = _everlastingEdenStoneSO._stoneTunnelPersistance,
                octaves = _everlastingEdenStoneSO._stoneTunnelOctaves,
                amplitude = _everlastingEdenStoneSO._stoneTunnelAmplitude,
                stoneChance = _everlastingEdenStoneSO._stoneTunnelChance,
                tileTypeMap = tileTypeMap
            };

            JobHandle stoneTunnelJobHandler = default;
            stoneTunnelJobHandler = stoneTunnelJob.ScheduleParallelByRef(_totalBlocks, 120, stoneTunnelJobHandler);
            stoneTunnelJobHandler.Complete();
        }
    }

    private void GenerateOreLayer(NativeArray<int> tileTypeMap)
    {
        if (_everlastingEdenOreSO._enableCopper)
        {
            var copperJob = new EverlastingEdenGenerationJobs.CopperGenerationJob()
            {
                width = _width,
                frequency = _everlastingEdenStoneSO._stoneBaseFrequency,
                persistance = _everlastingEdenStoneSO._stoneBasePersistance,
                octaves = _everlastingEdenStoneSO._stoneBaseOctaves,
                amplitude = _everlastingEdenStoneSO._stoneBaseAmplitude,
                chance = _everlastingEdenStoneSO._stoneBaseChance,
                tileTypeMap = tileTypeMap
            };

            JobHandle copperJobHandler = default;
            copperJobHandler = copperJob.ScheduleParallelByRef(_totalBlocks, 128, copperJobHandler);
            copperJobHandler.Complete();
        }
    }
};