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
using Debug = UnityEngine.Debug;

[BurstCompile]
public class EverlastingEdenWorldGenerator : WorldGenerator
{
    public Tile debugBlock, stone, copper, iron, grass, dirt;
    
    [Header("Stone World Generator")] 
    [SerializeField] private CaveSO caveSO; 
    [SerializeField] private EverlastingEdenOreSO _oreSO; 
    
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
        //GenerateOreLayer(tileTypeMap, ref rng);
        
        //main thread map build
        BuildTileMap(tileTypeMap);
        
        //clean up
        tileTypeMap.Dispose();
    }

    

    private void GenerateCaveLayer(NativeArray<int> tileTypeMap)
    {
        //Debug Caves
        if (caveSO.DebugFillWithMaterial)
        {
            var debugMaterialFillJob = new DebugMaterialFillJob()
            {
                TileTypeMap = tileTypeMap
            };

            JobHandle debugMaterialFillHandle = default;
            debugMaterialFillHandle = debugMaterialFillJob.ScheduleParallel(_totalBlocks, 128, debugMaterialFillHandle);
            debugMaterialFillHandle.Complete();
        }
        
        // stone base structure
        if (caveSO.EnableCaveBase)
        {
            var caveBaseJob = new WorldGenerationJobs.SimplexJob()
            {
                Width = _width,
                Height = _height,
                Frequency = caveSO.CaveBaseFrequency,
                Persistance = caveSO.CaveBasePersistance,
                Octaves = caveSO.CaveBaseOctaves,
                Amplitude = caveSO.CaveBaseAmplitude,
                FillChance = caveSO.CaveBaseMaterialFillChance,
                MaterialNum = 1,
                TileTypeMap = tileTypeMap
            };

            JobHandle caveBaseJobHandler = default;
            caveBaseJobHandler = caveBaseJob.ScheduleParallelByRef(_totalBlocks, 128, caveBaseJobHandler);
            caveBaseJobHandler.Complete();
        }

        //stone detail layer
        if (caveSO.EnableSmallCave)
        {
            var smallCaveJob = new WorldGenerationJobs.SimplexJob()
            {
                Width = _width,
                Height = _height,
                Frequency = caveSO.SmallCaveFrequency,
                Persistance = caveSO.SmallCavePersistance,
                Octaves =  caveSO.SmallCaveOctaves,
                Amplitude = caveSO.SmallCaveAmplitude,
                FillChance = caveSO.SmallCaveFillChance,
                MaterialNum = 0,
                TileTypeMap = tileTypeMap
            };
            
            JobHandle smallCaveJobHandler = default;
            smallCaveJobHandler = smallCaveJob.ScheduleParallelByRef(_totalBlocks, 120, smallCaveJobHandler);
            smallCaveJobHandler.Complete();
        }

        if (caveSO.EnableSmallCaveFill)
        {
            var smallCaveFillJob = new WorldGenerationJobs.SimplexJob()
            {
                Width = _width,
                Height = _height,
                Frequency = caveSO.SmallCaveFillFrequency,
                Persistance = caveSO.SmallCaveFillPersistance,
                Octaves = caveSO.SmallCaveFillOctaves,
                Amplitude = caveSO.SmallCaveFillAmplitude,
                FillChance = caveSO.SmallCaveFillMaterialFillChance,
                MaterialNum = 1,
                TileTypeMap = tileTypeMap
            };

            JobHandle smallCaveFillJobHandler = default;
            smallCaveFillJobHandler = smallCaveFillJob.ScheduleParallelByRef(_totalBlocks, 120, smallCaveFillJobHandler);
            smallCaveFillJobHandler.Complete();
        }
        
        if (caveSO.EnableLargeCaveFill)
        {
            var LargeCaveFillJob = new WorldGenerationJobs.SimplexJob()
            {
                Width = _width,
                Height = _height,
                Frequency = caveSO.LargeCaveFillFrequency,
                Persistance = caveSO.LargeCaveFillPersistance,
                Octaves = caveSO.LargeCaveFillOctaves,
                Amplitude = caveSO.LargeCaveFillAmplitude,
                FillChance = caveSO.LargeCaveFillMaterialFillChance,
                MaterialNum = 1,
                TileTypeMap = tileTypeMap
            };
        
            JobHandle LargeCaveFillJobHandler = default;
            LargeCaveFillJobHandler = LargeCaveFillJob.ScheduleParallelByRef(_totalBlocks, 120, LargeCaveFillJobHandler);
            LargeCaveFillJobHandler.Complete();
        }
        
        if (caveSO.EnableCavern)
        {
            var cavernJob = new WorldGenerationJobs.WorleyJob()
            {
                Width = _width,
                Height = _height,
                Frequency = caveSO.CavernFrequency,
                Persistance = caveSO.CavernPersistance,
                Octaves = caveSO.CavernOctaves,
                Amplitude = caveSO.CavernAmplitude,
                fillChance = caveSO.CavernChance,
                MaterialNum = 0,
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
                
                FloodFill(
                    startPosition,
                    tileTypeMap,
                    _oreSO.CopperLargeVeinMaxTileQuantity,
                    _oreSO.CopperLargeVeinSpreadChance,
                    ref rng);
            }
        }
    }

    private void FloodFill(
        Vector2Int startPositionCoord,
        NativeArray<int> tileTypeMap,
        int maxQuantityPerVein,
        float spreadChance,
        ref Unity.Mathematics.Random rng)
    {
        List<Vector2Int> tileCoordExplorationList = new List<Vector2Int>();
        HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();
        
        tileCoordExplorationList.Add(startPositionCoord);
        int tileCount = 0;
        
        while (tileCoordExplorationList.Count != 0) 
        {
            //Randomise a coordinate from the list
            int nextRandomIndex = rng.NextInt(0, tileCoordExplorationList.Count - 1);
            Vector2Int tilePos = tileCoordExplorationList[nextRandomIndex];
            tileCoordExplorationList.Remove(tileCoordExplorationList[nextRandomIndex]);
            
            //if this position is not stone, continue, skip
            if (!TilemapUtils.IsSpecificTile(tilePos, 1, tileTypeMap, _width, _height))
            {
                continue;
            }
                    
            //if reached max tiles for this ore vein, return
            tileCount++;
            visitedTiles.Add(tilePos);
            tileTypeMap[TilemapConverter.CoordToIndex(tilePos, _width, _height)] = 2; //TEMP set this to 2 which is the ore

            if (tileCount == maxQuantityPerVein)
            {
                break;
            }
                    
            //queue adjacent tiles that are stone
            var adjacentStoneTiles = TilemapUtils.ReturnSpecificAdjacentTiles(
                tilePos,
                1,
                tileTypeMap,
                _height,
                _width
                );

            //queue adjacent tiles, if not already in queue
            foreach (var newTilePos in adjacentStoneTiles)
            {
                //rng if this tile should exist
                float chance = rng.NextFloat(0, 1);
                
                //ignore if chance missed
                if (chance > spreadChance)
                {
                    continue;
                }
                
                //ignore if already visited
                if (visitedTiles.Contains(newTilePos))
                {
                    continue;
                }
                
                //add this coordinate to exploration queue
                tileCoordExplorationList.Add(newTilePos);
            }
        }
    }
};