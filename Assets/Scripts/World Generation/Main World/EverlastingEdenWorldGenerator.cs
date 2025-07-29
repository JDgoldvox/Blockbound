using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using EverlastingEdenGenerationJobs;
using Unity.Jobs.LowLevel.Unsafe;

[BurstCompile]
public class EverlastingEdenWorldGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap _tileMap;
    
    private Dictionary<Vector3Int, Tile> _blocks;
    public Tile debugBlock, stone, copper, iron, grass, dirt;
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
        
        NativeArray<int> tileTypeMap = new NativeArray<int>(_totalBlocks, Allocator.TempJob);
        
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
                case -1:
                    _tileMap.SetTile(tilePos, debugBlock);
                    break;
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
                Width = _width,
                Frequency = _everlastingEdenStoneSO.StoneBaseFrequency,
                Persistance = _everlastingEdenStoneSO.StoneBasePersistance,
                Octaves = _everlastingEdenStoneSO.StoneBaseOctaves,
                Amplitude = _everlastingEdenStoneSO.StoneBaseAmplitude,
                StoneChance = _everlastingEdenStoneSO.StoneBaseChance,
                TileTypeMap = tileTypeMap
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
                Width = _width,
                Frequency = _everlastingEdenStoneSO.StoneDetailFrequency,
                Persistance = _everlastingEdenStoneSO.StoneDetailPersistance,
                Octaves =  _everlastingEdenStoneSO.StoneDetailOctaves,
                Amplitude = _everlastingEdenStoneSO.StoneDetailAmplitude,
                StoneChance = _everlastingEdenStoneSO.StoneDetailChance,
                TileTypeMap = tileTypeMap
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
                Frequency = _everlastingEdenStoneSO.StoneTunnelFrequency,
                Persistance = _everlastingEdenStoneSO.StoneTunnelPersistance,
                Octaves = _everlastingEdenStoneSO.StoneTunnelOctaves,
                Amplitude = _everlastingEdenStoneSO.StoneTunnelAmplitude,
                StoneChance = _everlastingEdenStoneSO.StoneTunnelChance,
                TileTypeMap = tileTypeMap
            };

            JobHandle stoneTunnelJobHandler = default;
            stoneTunnelJobHandler = stoneTunnelJob.ScheduleParallelByRef(_totalBlocks, 120, stoneTunnelJobHandler);
            stoneTunnelJobHandler.Complete();
        }
    }

    private void GenerateOreLayer(NativeArray<int> tileTypeMap)
    {
        System.Random baseSeedGenerator = new System.Random();

        if (_everlastingEdenOreSO.EnableCopper)
        {
            List<NativeList<Vector2Int>> threadedOreResults = new List<NativeList<Vector2Int>>();
            int numJobs = JobsUtility.JobWorkerCount + 1;
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(numJobs,  Allocator.TempJob);
            
            for (int i = 0; i < numJobs; i++)
            {
                threadedOreResults.Add(new NativeList<Vector2Int>(Allocator.TempJob));

                var copperJob = new EverlastingEdenGenerationJobs.CopperLargeGenerationJob()
                {
                    BaseSeed = (uint)baseSeedGenerator.Next(1, int.MaxValue) | 1,
                    Width = _width,
                    Height = _height,
                    SpreadChance = _everlastingEdenOreSO.CopperLargeVeinSpreadChance,
                    TileTypeMap = tileTypeMap, //This is for reference for surroundings
                    NewOre = threadedOreResults[i]
                };

                handles[i] = copperJob.ScheduleParallel(1, 1, default);
            }
            
            JobHandle.CompleteAll(handles);
            
            
            
            
            //dispose handles
            //dispose native arrays
        }
    }
};