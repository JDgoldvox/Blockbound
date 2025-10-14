using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using System.Linq;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using EverlastingEdenGenerationJobs;
using Unity.Jobs.LowLevel.Unsafe;
using Debug = UnityEngine.Debug;

[BurstCompile]
public class EverlastingEdenWorldGenerator : MonoBehaviour
{
    //[SerializeField] private Tilemap _tileMap;
    
    private Dictionary<Vector3Int, Tile> _blocks;
    public Tile debugBlock, stone, copper, iron, grass, dirt;
    private int _totalBlocks;
    private float _tempTimer = 0;
    
    [Header("Size")]
    [SerializeField] private int _halfWidth; 
    [SerializeField] private int _halfHeight;
    private int _width = 0;
    private int _height = 0;
    public int BottomRowYPosition = 0;
    [HideInInspector]public int YOffset = 0;

    [Header("Stone World Generator")] 
    [SerializeField] private EverlastingEdenStoneSO _everlastingEdenStoneSO; 
    [SerializeField] private EverlastingEdenOreSO _everlastingEdenOreSO; 
    
    //tilemap Chunks
    TilemapChunkManager _chunkManager;
    [SerializeField] private Transform _tilemapParent;
    
    private void Awake()
    {
        _blocks = new Dictionary<Vector3Int, Tile>();  
        _chunkManager = new TilemapChunkManager();
    }

    public void GenerateWorld()
    {
        //random struct
        System.Random baseSeedGenerator = new System.Random();
        uint seed = (uint)baseSeedGenerator.Next(1, int.MaxValue) | 1;
        Unity.Mathematics.Random rng = new Unity.Mathematics.Random(seed);

        _width = _halfWidth * 2;
        _height = _halfHeight * 2;
        _totalBlocks = _width * _height;
        YOffset = _halfHeight + BottomRowYPosition;
        
        //init chunks
        _chunkManager.InitChunks(_width, _height, BottomRowYPosition, _tilemapParent);
        
        //stuff to do before
        NativeArray<int> tileTypeMap = new NativeArray<int>(_totalBlocks, Allocator.TempJob);
        
        //Generate stone
        GenerateStoneLayer(tileTypeMap);
        
        //Generate Ore
        GenerateOreLayer(tileTypeMap, ref rng);
        
        //main thread map build
        BuildTileMap(tileTypeMap);
        
        //clean up
        tileTypeMap.Dispose();
    }

    private void BuildTileMap(NativeArray<int> tileTypeMap)
    {
        //main thread map build
        for (int i = 0; i < tileTypeMap.Length; i++)
        {
            Vector2Int coord = TilemapConverter.IndexToCoord(i, _width, _height);
            int tile = tileTypeMap[i];
            Vector3Int tilePos = new Vector3Int(coord.x, coord.y + YOffset, 0);
            
            //get tilemap from spacially quantized world
            Tilemap _tilemap = _chunkManager.ReturnChunkAtPosition(tilePos, YOffset);
            
            switch (tile)
            {
                case -1:
                    _tilemap.SetTile(tilePos, debugBlock);
                    break;
                case 0:
                    _tilemap.SetTile(tilePos, null);
                    break;
                case 1:
                    _tilemap.SetTile(tilePos, stone);
                    break;
                case 2:
                    _tilemap.SetTile(tilePos, copper);
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
                Height = _height,
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
                Height = _height,
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
                Width = _width,
                Height = _height,
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
    
    private void GenerateOreLayer(NativeArray<int> tileTypeMap, ref Unity.Mathematics.Random rng)
    {
        if (_everlastingEdenOreSO.EnableCopper)
        {
            for (int i = 0; i < _everlastingEdenOreSO.CopperLargeVeinQuantity; i++)
            {
                //generate random start position within the layer
                int rngX = rng.NextInt(-_halfWidth, _halfWidth);
                int rngY = rng.NextInt(-_halfHeight, _halfHeight);
                Vector2Int startPosition = new Vector2Int(rngX, rngY);
                
                FloodFill(
                    startPosition,
                    tileTypeMap,
                    _everlastingEdenOreSO.CopperLargeVeinMaxTileQuantity,
                    _everlastingEdenOreSO.CopperLargeVeinSpreadChance,
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