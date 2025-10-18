using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using System.Linq;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using WorldGenerationJobs;
using Unity.Jobs.LowLevel.Unsafe;

[BurstCompile]
public class EverlastingEdenWorldGenerator : WorldGenerator
{
    [SerializeField] private CaveSO _caveSO; 
    [SerializeField] private EverlastingEdenOreSO _oreSO;

    [Header("Blocks")] 
    [SerializeField] private BlockItemSO _debugBlock;
    [SerializeField] private BlockItemSO _airBlock;
    [SerializeField] private BlockItemSO _dirtBlock;
    [SerializeField] private BlockItemSO _stoneBlock;
    [SerializeField] private BlockItemSO _copperOreBlock;
    
    //[Header("Background")]
    
    public void GenerateWorld()
    {
        //random struct
        System.Random baseSeedGenerator = new System.Random();
        uint seed = (uint)baseSeedGenerator.Next(1, int.MaxValue) | 1;
        Unity.Mathematics.Random rng = new Unity.Mathematics.Random(seed);

        _width = _halfWidth * 2;
        _height = _halfHeight * 2;
        _totalBlocks = _width * _height;
        yOffset = _halfHeight + bottomRowYPosition;
        
        //init chunks
        _chunkManager.InitChunks(_width, _height, bottomRowYPosition, _tilemapParent);
        
        //stuff to do before
        NativeArray<int> tileTypeMap = new NativeArray<int>(_totalBlocks, Allocator.TempJob);
        
        //Generate stone
        GenerateCaveLayer(tileTypeMap);
        
        //Generate Ore
        GenerateOreLayer(tileTypeMap, ref rng);
        
        //main thread map build
        BuildTileMap(tileTypeMap);
        
        //clean up
        tileTypeMap.Dispose();
    }

    

    private void GenerateCaveLayer(NativeArray<int> tileTypeMap)
    {
        //Debug Caves
        if (_caveSO.DebugFillWithMaterial)
        {
            var debugMaterialFillJob = new DebugMaterialFillJob()
            {
                MaterialID = _stoneBlock.ID,
                TileTypeMap = tileTypeMap
            };

            JobHandle debugMaterialFillHandle = default;
            debugMaterialFillHandle = debugMaterialFillJob.ScheduleParallel(_totalBlocks, 128, debugMaterialFillHandle);
            debugMaterialFillHandle.Complete();
        }
        
        // Base Stone Caves
        if (_caveSO.EnableCaveBase)
        {
            var caveBaseJob = new WorldGenerationJobs.SimplexJob()
            {
                Width = _width,
                Height = _height,
                Frequency = _caveSO.CaveBaseFrequency,
                Persistance = _caveSO.CaveBasePersistance,
                Octaves = _caveSO.CaveBaseOctaves,
                Amplitude = _caveSO.CaveBaseAmplitude,
                FillChance = _caveSO.CaveBaseMaterialFillChance,
                MaterialID = _stoneBlock.ID,
                TileTypeMap = tileTypeMap
            };

            JobHandle caveBaseJobHandler = default;
            caveBaseJobHandler = caveBaseJob.ScheduleParallelByRef(_totalBlocks, 128, caveBaseJobHandler);
            caveBaseJobHandler.Complete();
        }

        //Small Caves
        if (_caveSO.EnableSmallCave)
        {
            var smallCaveJob = new WorldGenerationJobs.SimplexJob()
            {
                Width = _width,
                Height = _height,
                Frequency = _caveSO.SmallCaveFrequency,
                Persistance = _caveSO.SmallCavePersistance,
                Octaves =  _caveSO.SmallCaveOctaves,
                Amplitude = _caveSO.SmallCaveAmplitude,
                FillChance = _caveSO.SmallCaveFillChance,
                MaterialID = _airBlock.ID,
                TileTypeMap = tileTypeMap
            };
            
            JobHandle smallCaveJobHandler = default;
            smallCaveJobHandler = smallCaveJob.ScheduleParallelByRef(_totalBlocks, 120, smallCaveJobHandler);
            smallCaveJobHandler.Complete();
        }

        if (_caveSO.EnableSmallCaveFill)
        {
            var smallCaveFillJob = new WorldGenerationJobs.SimplexJob()
            {
                Width = _width,
                Height = _height,
                Frequency = _caveSO.SmallCaveFillFrequency,
                Persistance = _caveSO.SmallCaveFillPersistance,
                Octaves = _caveSO.SmallCaveFillOctaves,
                Amplitude = _caveSO.SmallCaveFillAmplitude,
                FillChance = _caveSO.SmallCaveFillMaterialFillChance,
                MaterialID = _stoneBlock.ID,
                TileTypeMap = tileTypeMap
            };

            JobHandle smallCaveFillJobHandler = default;
            smallCaveFillJobHandler = smallCaveFillJob.ScheduleParallelByRef(_totalBlocks, 120, smallCaveFillJobHandler);
            smallCaveFillJobHandler.Complete();
        }
        
        if (_caveSO.EnableLargeCaveFill)
        {
            var LargeCaveFillJob = new WorldGenerationJobs.SimplexJob()
            {
                Width = _width,
                Height = _height,
                Frequency = _caveSO.LargeCaveFillFrequency,
                Persistance = _caveSO.LargeCaveFillPersistance,
                Octaves = _caveSO.LargeCaveFillOctaves,
                Amplitude = _caveSO.LargeCaveFillAmplitude,
                FillChance = _caveSO.LargeCaveFillMaterialFillChance,
                MaterialID = _stoneBlock.ID,
                TileTypeMap = tileTypeMap
            };
        
            JobHandle LargeCaveFillJobHandler = default;
            LargeCaveFillJobHandler = LargeCaveFillJob.ScheduleParallelByRef(_totalBlocks, 120, LargeCaveFillJobHandler);
            LargeCaveFillJobHandler.Complete();
        }
        
        if (_caveSO.EnableCavern)
        {
            var cavernJob = new WorldGenerationJobs.WorleyJob()
            {
                Width = _width,
                Height = _height,
                Frequency = _caveSO.CavernFrequency,
                Persistance = _caveSO.CavernPersistance,
                Octaves = _caveSO.CavernOctaves,
                Amplitude = _caveSO.CavernAmplitude,
                fillChance = _caveSO.CavernChance,
                MaterialNum = _airBlock.ID,
                TileTypeMap = tileTypeMap
            };

            JobHandle cavernJobHandler = default;
            cavernJobHandler = cavernJob.ScheduleParallelByRef(_totalBlocks, 120, cavernJobHandler);
            cavernJobHandler.Complete();
        }
    }
    
    private void GenerateOreLayer(NativeArray<int> tileTypeMap, ref Unity.Mathematics.Random rng)
    {
        if (_oreSO.EnableCopper)
        {
            for (int i = 0; i < _oreSO.CopperLargeVeinQuantity; i++)
            {
                //generate random start position within the layer
                int rngX = rng.NextInt(-_halfWidth, _halfWidth);
                int rngY = rng.NextInt(-_halfHeight, _halfHeight);
                Vector2Int startPosition = new Vector2Int(rngX, rngY);
                
                OreFloodFill(
                    startPosition,
                    tileTypeMap,
                    _oreSO.CopperLargeVeinMaxTileQuantity,
                    _oreSO.CopperLargeVeinSpreadChance,
                    _stoneBlock.ID,
                    _copperOreBlock.ID,
                    ref rng
                    );
            }
        }
    }

    
};