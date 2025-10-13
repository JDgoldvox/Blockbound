using UnityEngine;

public static class TilemapConverter
{
    public static Vector2Int IndexToCoord(int index, int width, int height)
    {
        //coord:
        //x = i % width
        //y = index/width
        //offset x by subtracting half width
        //offset y by subtracting half height
        
        int halfWidth = width/2;
        int halfHeight = height/2;
        
        Vector2Int coord = Vector2Int.zero;

        coord.x = (index % width) - halfWidth; //- width to remove offset
        coord.y = (index / width) - halfHeight;

        return coord;
    }

    public static int CoordToIndex(int x, int y, int width, int height)
    {
        return CoordToIndex(new Vector2Int(x,y), width, height);
    }
    
    public static int CoordToIndex(Vector2Int coord, int width, int height)
    {
        // index = (y×width)+x
        // offset x only adding a half width
        
        int halfWidth = width/2;
        int halfHeight = height/2;
        
        // 1. Convert centered X to 0-based X (x_1D)
        int xConverted = coord.x + halfWidth;
        
        // 2. Convert centered Y to 0-based Y (y_1D)
        int yConverted = coord.y + halfHeight; 
        
        // 3. Final Index = (y_1D * FULL WIDTH) + x_1D
        return (yConverted * width) + xConverted; 
    }
}
