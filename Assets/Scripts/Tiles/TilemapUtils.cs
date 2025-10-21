using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class TilemapUtils
{
    public static bool IsOutOfBounds(Vector2Int position, int width, int height)
    {
        int halfWidth = width / 2;
        int halfHeight = height / 2;
        
        bool xOutOfBounds = (position.x < -halfWidth) || (position.x >= halfWidth);
        bool yOutOfBounds = (position.y < -halfHeight) || (position.y >= halfHeight);
        return xOutOfBounds || yOutOfBounds;
    }
    
    public static bool IsSpecificTile(Vector2Int position, int tileID, NativeArray<int> TileTypeMap, int width, int height)
    {
        
        if (TileTypeMap[TilemapConverter.CoordToIndex(position, width, height)] == tileID)
        {
            return true;
        }
        
        return false;
    }
    
    public static bool IsSpecificTile(int index, int tileID, NativeArray<int> TileTypeMap)
    {
        if (TileTypeMap[index] == tileID)
        {
            return true;
        }
        
        return false;
    }

    public static List<Vector2Int> ReturnSpecificAdjacentTiles(Vector2Int centreTilePosition, int tileID, NativeArray<int> tileTypeMap, int height, int width)
    {
        List<Vector2Int> adjacentTiles = new List<Vector2Int>();
        int halfWidth = width / 2;
        
        //top left
        Vector2Int newTilePos = new Vector2Int(centreTilePosition.x - 1, centreTilePosition.y + 1);
        if (!IsOutOfBounds(newTilePos, width, height))
        {
            if (IsSpecificTile(newTilePos, tileID, tileTypeMap, width, height))
            {
                adjacentTiles.Add(newTilePos);
            }
        }
        
        //top middle
        newTilePos = new Vector2Int(centreTilePosition.x, centreTilePosition.y + 1);
        if (!IsOutOfBounds(newTilePos, width, height))
        {
            if (IsSpecificTile(newTilePos, tileID, tileTypeMap, width, height))
            {
                adjacentTiles.Add(newTilePos);
            }
        }
        
        //top right
        newTilePos = new Vector2Int(centreTilePosition.x + 1, centreTilePosition.y + 1);
        if (!IsOutOfBounds(newTilePos, width, height))
        {
            if (IsSpecificTile(newTilePos, tileID, tileTypeMap, width, height))
            {
                adjacentTiles.Add(newTilePos);
            }
        }
        
        //middle left
        newTilePos = new Vector2Int(centreTilePosition.x - 1, centreTilePosition.y);
        if (!IsOutOfBounds(newTilePos, width, height))
        {
            if (IsSpecificTile(newTilePos, tileID, tileTypeMap, width, height))
            {
                adjacentTiles.Add(newTilePos);
            }
        }
        
        //middle right
        newTilePos = new Vector2Int(centreTilePosition.x + 1, centreTilePosition.y);
        if (!IsOutOfBounds(newTilePos, width, height))
        {
            if (IsSpecificTile(newTilePos, tileID, tileTypeMap, width, height))
            {
                adjacentTiles.Add(newTilePos);
            }
        }
        
        //bottom left
        newTilePos = new Vector2Int(centreTilePosition.x - 1, centreTilePosition.y - 1);
        if (!IsOutOfBounds(newTilePos, width, height))
        {
            if (IsSpecificTile(newTilePos, tileID, tileTypeMap, width, height))
            {
                adjacentTiles.Add(newTilePos);
            }
        }
        
        //bottom middle
        newTilePos = new Vector2Int(centreTilePosition.x, centreTilePosition.y - 1);
        if (!IsOutOfBounds(newTilePos, width, height))
        {
            if (IsSpecificTile(newTilePos, tileID, tileTypeMap, width, height))
            {
                adjacentTiles.Add(newTilePos);
            }
        }
        
        //bottom right
            newTilePos = new Vector2Int(centreTilePosition.x + 1, centreTilePosition.y - 1);
        if (!IsOutOfBounds(newTilePos, width, height))
        {
            if (IsSpecificTile(newTilePos, tileID, tileTypeMap, width, height))
            {
                adjacentTiles.Add(newTilePos);
            }
        }
        
        return adjacentTiles;
    }
    
    
}
