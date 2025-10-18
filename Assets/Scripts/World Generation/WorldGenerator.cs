using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;

[BurstCompile]
public class WorldGenerator : MonoBehaviour
{
    protected int _totalBlocks;
    
    [Header("Size")]
    [SerializeField] protected int _halfWidth; 
    [SerializeField] protected int _halfHeight;
    protected int _width = 0;
    protected int _height = 0;
    public int bottomRowYPosition = 0;
    [HideInInspector] public int yOffset = 0;
    
    //tilemap Chunks
    protected TilemapChunkManager _chunkManager;
    [SerializeField] protected Transform _tilemapParent;
    
    private void Awake()
    {
        _chunkManager = _tilemapParent.AddComponent<TilemapChunkManager>();
    }
    
    public void BuildTileMap(NativeArray<int> tileTypeMap)
    {
        //main thread map build
        for (int i = 0; i < tileTypeMap.Length; i++)
        {
            Vector2Int coord = TilemapConverter.IndexToCoord(i, _width, _height);
            int tileID = tileTypeMap[i];
            Vector3Int tilePos = new Vector3Int(coord.x, coord.y + yOffset, 0);
            
            //get tilemap from spacially quantized world
            Tilemap _tilemap = _chunkManager.ReturnChunkAtPosition(tilePos, bottomRowYPosition);
            
            ItemSO item = ItemFinder.instance.IDToItem(tileID);
            BlockItemSO blockItem = item as BlockItemSO;
            _tilemap.SetTile(tilePos, blockItem.tileBase);
        }
    }
    
    protected void OreFloodFill(
        Vector2Int startPositionCoord,
        NativeArray<int> tileTypeMap,
        int maxQuantityPerVein,
        float spreadChance,
        int spawnInsideMaterialID,
        int oreMaterialID,
        ref Unity.Mathematics.Random rng
        )
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
            
            //if this position is not a spawnable block
            if (!TilemapUtils.IsSpecificTile(tilePos, spawnInsideMaterialID, tileTypeMap, _width, _height))
            {
                continue;
            }
                    
            //if reached max tiles for this ore vein, return
            tileCount++;
            visitedTiles.Add(tilePos);
            tileTypeMap[TilemapConverter.CoordToIndex(tilePos, _width, _height)] = oreMaterialID; 

            if (tileCount == maxQuantityPerVein)
            {
                break;
            }
                    
            //queue adjacent tiles that have spwanable tile
            var adjacentStoneTiles = TilemapUtils.ReturnSpecificAdjacentTiles(
                tilePos,
                spawnInsideMaterialID,
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
}
