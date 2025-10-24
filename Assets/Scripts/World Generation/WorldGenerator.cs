using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using Unity.Jobs;
using WorldGenerationJobs;

//HOW TO USE CLASS
/// <summary>
///  1. variable all the tiles being used
///  2. PreGenerationTasks()
///  3. Do jobs to generate terrain
///  4. CleanUp()
/// </summary>

[BurstCompile]
public class WorldGenerator : MonoBehaviour
{
    [Header("World Size")] 
    [SerializeField] protected int _chunkHalfWidth; //total half width of chunks
    [SerializeField] protected int _chunkHalfHeight;
    protected int _totalBlocks;
    protected int _halfWidth = 0; //num of tiles 
    protected int _halfHeight = 0;
    protected int _width = 0;
    protected int _height = 0;
    public int bottomRowYPosition = 0;
    [HideInInspector] public int yOffset = 0;
    
    //tilemap chunks
    protected TilemapChunkManager _chunkManager;
    [SerializeField] protected Transform _tilemapParent;
    protected NativeArray<int> _tileTypeMap;
    protected Unity.Mathematics.Random rng;
    
    //tilemap lookup
    protected Dictionary<int, BlockItemSO> _localTilebaseLookUp;
    
    private void Awake()
    {
        _localTilebaseLookUp = new Dictionary<int, BlockItemSO>();
        _chunkManager = _tilemapParent.AddComponent<TilemapChunkManager>();
    }

    private void Start()
    {
        System.Random baseSeedGenerator = new System.Random();
        uint seed = (uint)baseSeedGenerator.Next(1, int.MaxValue) | 1;
        rng = new Unity.Mathematics.Random(seed);
    }

    protected void PreGenerationTasks()
    {
        _halfWidth = CONSTANTS.CHUNK_LENGTH * _chunkHalfWidth;
        _halfHeight = CONSTANTS.CHUNK_LENGTH * _chunkHalfHeight;
        _width = _halfWidth * 2;
        _height = _halfHeight * 2;
        _totalBlocks = _width * _height;
        yOffset = _halfHeight + bottomRowYPosition;
        
        //init chunks
        _chunkManager.InitChunks(_width, _height, bottomRowYPosition, _tilemapParent);
        
        //init tile map
        _tileTypeMap = new NativeArray<int>(_totalBlocks, Allocator.TempJob);
    }

    protected void CleanUp()
    {
        //fix edges of rule tile tile chunks 
        _chunkManager.WriteAllChunkOuterLayers(_width, _height);
        
        _tileTypeMap.Dispose();
    }
    
    public void BuildTileMap()
    {
        //main thread map build
        for (int i = 0; i < _tileTypeMap.Length; i++)
        {
            Vector2Int coord = TilemapConverter.IndexToCoord(i, _width, _height);
            int tileID = _tileTypeMap[i];
            Vector3Int tilePos = new Vector3Int(coord.x, coord.y + yOffset, 0);
            Tilemap _tilemap = _chunkManager.ReturnChunkAtPosition(tilePos, bottomRowYPosition);
            _tilemap.SetTile(tilePos, _localTilebaseLookUp[tileID].tileBase);
        }
    }
    
    protected void OreFloodFill(
        int maxQuantityPerVein,
        float spreadChance,
        int spawnInsideMaterialID,
        int oreMaterialID
        )
    {
        //generate random start position within the layer
        int rngX = rng.NextInt(-_halfWidth, _halfWidth);
        int rngY = rng.NextInt(-_halfHeight, _halfHeight);
        Vector2Int startPosition = new Vector2Int(rngX, rngY);
        
        //set up data containers
        List<Vector2Int> tileCoordExplorationList = new List<Vector2Int> { startPosition };
        HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();
        int tileCount = 0;
        
        while (tileCoordExplorationList.Count != 0 && tileCount < maxQuantityPerVein) 
        {
            //Randomise a coordinate from the list
            int nextRandomIndex = rng.NextInt(0, tileCoordExplorationList.Count);
            Vector2Int tilePos = tileCoordExplorationList[nextRandomIndex];
            tileCoordExplorationList.Remove(tileCoordExplorationList[nextRandomIndex]);
            
            //if this position is not a spawnable block
            if (!TilemapUtils.IsSpecificTile(tilePos, spawnInsideMaterialID, _tileTypeMap, _width, _height))
            {
                continue;
            }
                    
            //if reached max tiles for this ore vein, return
            tileCount++;
            visitedTiles.Add(tilePos);
            _tileTypeMap[TilemapConverter.CoordToIndex(tilePos, _width, _height)] = oreMaterialID; 

            //queue adjacent tiles that have spwanable tile
            var adjacentStoneTiles = TilemapUtils.ReturnSpecificAdjacentTiles(
                tilePos,
                spawnInsideMaterialID,
                _tileTypeMap,
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
}
