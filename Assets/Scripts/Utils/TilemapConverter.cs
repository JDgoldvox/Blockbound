using UnityEngine;

public static class TilemapConverter
{
    public static Vector2Int IndexToCoord(int index, int width)
    {
        Vector2Int coord = Vector2Int.zero;

        coord.x = index % width;
        coord.y = index / width;

        return coord;
    }

    public static int CoordToIndex(int x, int y, int width)
    {
        return (y * width) + x;
    }
}
