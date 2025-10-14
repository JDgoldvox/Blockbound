using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapChunkManager : MonoBehaviour
{
    private const int CHUNK_SIZE = 64;
    public Dictionary<Vector3Int, Tilemap> Chunks = new  Dictionary<Vector3Int, Tilemap>();
    
    
    //Chunks will be 64 x 64
    
    /// <summary>
    /// Function will be key to create tile maps
    /// store bottom left position of each chunk
    /// </summary>
    public void InitChunks(int width, int height, int bottomRowYPosition, Transform chunkParent)
    {
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

    public Tilemap ReturnChunkAtPosition(Vector3Int position, int offsetY)
    {
        //find how far away from the bottom left of the nearest chunk is
        int remainderX = position.x % CHUNK_SIZE;
        int remainderY = position.y % CHUNK_SIZE;
        
        Vector3Int chunkPosition =  new Vector3Int(position.x - remainderX, (position.y - remainderY), 0);

        if (Chunks.ContainsKey(chunkPosition))
        {
            return Chunks[chunkPosition];
        }
        
        Debug.LogError($"[Chunk Error] Key {chunkPosition} was calculated but not found in the dictionary.");
        Debug.LogError($"[Chunk Info] Position that caused error: {position}.");
        return null;
    }
}
