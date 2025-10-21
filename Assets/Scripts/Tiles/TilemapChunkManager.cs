using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapChunkManager : MonoBehaviour
{
    private const int CHUNK_SIZE = 64;
    public Dictionary<Vector3Int, Tilemap> Chunks;

    private void Awake()
    {
        Chunks = new Dictionary<Vector3Int, Tilemap>();
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
                 Chunks[new Vector3Int(col, row)] = newTilemapComponent;
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

        if (Chunks.ContainsKey(chunkPositionIndex))
        {
            return Chunks[chunkPositionIndex];
        }
        
        Debug.LogError($"[Chunk Error] Key {chunkPositionIndex} was calculated but not found in the dictionary.");
        Debug.LogError($"[Chunk Info] Position that caused error: {position}.");
        return null;
    }
}
