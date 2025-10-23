using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Stores the bottom left of the chunk
/// </summary>
public class TilemapChunkManager : MonoBehaviour
{
    private const int CHUNK_SIZE = 64;
    public Dictionary<Vector3Int, Tilemap> chunks;

    private void Awake()
    {
        chunks = new Dictionary<Vector3Int, Tilemap>(); //bottom left of chunk
    }

    /// <summary>
    /// Function will be key to create tile maps
    /// store bottom left position of each chunk
    /// </summary>
    public void InitChunks(int width, int height, int bottomRowYPosition, Transform chunkParent)
    {
        //Check if we already created tilemaps before
        foreach (Transform child in chunkParent)
        {
            Destroy(child.gameObject);
        }
            
        //extra info, height and width are always even
        //world will always be multiple of 64
        //chunk parent will always have grid component
        
        //1. calulate how many chunks we will need for each row and column
        int chunkNum = (width * height) / (CHUNK_SIZE * CHUNK_SIZE);
        //2. quantize this space
        //x will always be the same from width
        //y will change depending on the yOffset    
        
        int halfWidth = width / 2;
        int firstYOutOfBounds = height + bottomRowYPosition;
        
        //Starting position of quantisation
        //start bottom left
        int xLeft = -halfWidth;
        
        //loop over all chunks, keeping bottom left tile as the reference
         for (int col = xLeft; col < halfWidth; col += CHUNK_SIZE)
         {
             for (int row = bottomRowYPosition; row < firstYOutOfBounds; row += CHUNK_SIZE)
             {
                 GameObject newTilemapObject = new GameObject();
                 newTilemapObject.transform.parent = chunkParent;   
                 Tilemap newTilemapComponent = newTilemapObject.AddComponent<Tilemap>();
                 newTilemapObject.AddComponent<TilemapRenderer>();
                 
                 //store chunk
                 chunks[new Vector3Int(col, row)] = newTilemapComponent;
             }
         }
    }

    public Tilemap ReturnChunkAtPosition(Vector3Int position, int bottomRowPosition)
    {
        //find how far away from the bottom left of the nearest chunk is
        //divide, floor and then multiply, cant use mod because negative numbers always rounds the opposite direction
        
        int chunkXIndex = position.x / CHUNK_SIZE;

        if (position.x < 0 && position.x % CHUNK_SIZE != 0)
        {
            chunkXIndex--;
        }
    
        // --- Y-Axis Calculation --- take away to align with 0
        int chunkYIndex = (position.y - bottomRowPosition) / CHUNK_SIZE;
    
        if (position.y < 0 && position.y % CHUNK_SIZE != 0)
        {
            chunkYIndex--;
        }

        // Multiply back to get the world coordinate of the key (the bottom-left corner)
        int chunkKeyX = chunkXIndex * CHUNK_SIZE;
        int chunkKeyY = (chunkYIndex * CHUNK_SIZE) + bottomRowPosition; //add bottom row position back to reflect real position
        
        Vector3Int chunkPositionIndex =  new Vector3Int(chunkKeyX,chunkKeyY, 0);

        if (chunks.ContainsKey(chunkPositionIndex))
        {
            return chunks[chunkPositionIndex];
        }
        
        Debug.LogError($"[Chunk Error] Key {chunkPositionIndex} was calculated but not found in the dictionary.");
        Debug.LogError($"[Chunk Info] Position that caused error: {position}.");
        return null;
    }
    
    //Add a ring around the outside the tilemap 1 block in length to make sure rule tiling works
    //should be called only once
    public void WriteAllChunkOuterLayers(int width, int height)
    {
        foreach (var currentChunk in chunks)
        {
            WriteChunkOuterLayer(width, height, currentChunk);
        }
    }

    public void WriteChunkOuterLayer(int width, int height, KeyValuePair<Vector3Int, Tilemap> currentChunk)
    {
        //get tilemap at hand
        Tilemap currentTileMap = currentChunk.Value;
            
        // new tile chunk key
        // starting Tile Position
        // loop limit length
        // direction to increment
        var chunkChecks = new (Vector3Int newChunkKey, Vector3Int startingChunkPosition, int loopLimit, Vector3Int step)[]
            {
                //left side
                (
                    newChunkKey: new Vector3Int(currentChunk.Key.x - CONSTANTS.CHUNK_LENGTH, currentChunk.Key.y), //new chunk key
                    startingChunkPosition: new Vector3Int(currentChunk.Key.x - 1, currentChunk.Key.y), //starting loop position
                    loopLimit: CONSTANTS.CHUNK_LENGTH, //loop limit
                    step: new Vector3Int(0,1,0) //increment
                ),
                
                //top side
                (
                    newChunkKey: new Vector3Int(currentChunk.Key.x, currentChunk.Key.y + CONSTANTS.CHUNK_LENGTH),
                    startingChunkPosition: new Vector3Int(currentChunk.Key.x, currentChunk.Key.y +  CONSTANTS.CHUNK_LENGTH),
                    loopLimit: CONSTANTS.CHUNK_LENGTH,
                    step: new Vector3Int(1,0,0)
                ),
                
                //right side
                (
                    newChunkKey: new Vector3Int(currentChunk.Key.x + CONSTANTS.CHUNK_LENGTH, currentChunk.Key.y),
                    startingChunkPosition: new Vector3Int(currentChunk.Key.x + CONSTANTS.CHUNK_LENGTH, currentChunk.Key.y),
                    loopLimit: CONSTANTS.CHUNK_LENGTH,
                    step: new Vector3Int(0,1,0)
                ),
                
                //bottom side
                (
                    newChunkKey: new Vector3Int(currentChunk.Key.x, currentChunk.Key.y - CONSTANTS.CHUNK_LENGTH),
                    startingChunkPosition: new Vector3Int(currentChunk.Key.x, currentChunk.Key.y - 1),
                    loopLimit: CONSTANTS.CHUNK_LENGTH,
                    step: new Vector3Int(1,0,0)
                ),
                    
                //Top left
                (
                    newChunkKey: new Vector3Int(currentChunk.Key.x - CONSTANTS.CHUNK_LENGTH, currentChunk.Key.y + CONSTANTS.CHUNK_LENGTH),
                    startingChunkPosition: new Vector3Int(currentChunk.Key.x - 1, currentChunk.Key.y + CONSTANTS.CHUNK_LENGTH),
                    loopLimit: 1,
                    step: new Vector3Int(0,0,0)
                ),
                    
                //Top Right
                (
                    newChunkKey: new Vector3Int(currentChunk.Key.x + CONSTANTS.CHUNK_LENGTH, currentChunk.Key.y + CONSTANTS.CHUNK_LENGTH),
                    startingChunkPosition: new Vector3Int(currentChunk.Key.x + CONSTANTS.CHUNK_LENGTH, currentChunk.Key.y + CONSTANTS.CHUNK_LENGTH),
                    loopLimit: 1,
                    step: new Vector3Int(0,0,0)
                ),
                
                //Bottom Left
                (
                    newChunkKey: new Vector3Int(currentChunk.Key.x - CONSTANTS.CHUNK_LENGTH, currentChunk.Key.y - CONSTANTS.CHUNK_LENGTH),
                    startingChunkPosition: new Vector3Int(currentChunk.Key.x - 1, currentChunk.Key.y - 1),
                    loopLimit: 1,
                    step: new Vector3Int(0,0,0)
                ),
                
                //Bottom Right
                (
                    newChunkKey: new Vector3Int(currentChunk.Key.x + CONSTANTS.CHUNK_LENGTH, currentChunk.Key.y - CONSTANTS.CHUNK_LENGTH),
                    startingChunkPosition: new Vector3Int(currentChunk.Key.x + CONSTANTS.CHUNK_LENGTH,  currentChunk.Key.y - 1),
                    loopLimit: 1,
                    step: new Vector3Int(0,0,0)
                ),
                 
            };

        //go through all checks, Top, bottom, left, right, TL, TR, BL, BR
        foreach (var check in chunkChecks)
        {
            //check whether this outer chunk exists

            if (chunks.TryGetValue(check.newChunkKey, out Tilemap newTilemap))
            {
                //loop through all tiles on this side
                for (int i = 0; i < check.loopLimit; i++)
                {
                    Vector3Int positionToSetNewTile = check.startingChunkPosition + (check.step * i);
                    TileBase newTileBase = newTilemap.GetTile(positionToSetNewTile);
                    
                    //set tile
                    currentTileMap.SetTile(positionToSetNewTile, newTileBase);
                    
                    //turn invisable
                    currentTileMap.SetColor(positionToSetNewTile, new Color(0,0,0,0));
                }
            }
        }
    }

}
