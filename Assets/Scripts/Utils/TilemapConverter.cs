using UnityEngine;

public static class TilemapConverter
{
    public static Vector2Int IndexToCoord(int index, int width)
    {
        Vector2Int coord = Vector2Int.zero;

        coord.x = (index % width) - (width / 2); //- width to remove offset
        coord.y = index / width;

        return coord;
    }

    public static int CoordToIndex(int x, int y, int width)
    {
        return (y * width) + (x + (width/2)); // x + width for offset
    }
    
    public static int CoordToIndex(Vector2Int coord, int width)
    {
        return (coord.y * width) + (coord.x + (width / 2)); // x + width for offset
    }
}
