using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[BurstCompile]
public class WorldGenerator : MonoBehaviour
{
    protected int _totalBlocks;
    protected float _tempTimer = 0;
    
    [Header("Size")]
    [SerializeField] protected int _halfWidth; 
    [SerializeField] protected int _halfHeight;
    protected int _width = 0;
    protected int _height = 0;
    public int bottomRowYPosition = 0;
    [HideInInspector]public int yOffset = 0;
    
    //tilemap Chunks
    protected TilemapChunkManager _chunkManager;
    [SerializeField] protected Transform _tilemapParent;
    
    private void Awake()
    {
        _chunkManager = new TilemapChunkManager();
    }
    
    public void BuildTileMap(NativeArray<int> tileTypeMap)
    {
        //main thread map build
        for (int i = 0; i < tileTypeMap.Length; i++)
        {
            Vector2Int coord = TilemapConverter.IndexToCoord(i, _width, _height);
            int tile = tileTypeMap[i];
            Vector3Int tilePos = new Vector3Int(coord.x, coord.y + yOffset, 0);
            
            //get tilemap from spacially quantized world
            Tilemap _tilemap = _chunkManager.ReturnChunkAtPosition(tilePos, bottomRowYPosition);
            
            // switch (tile)
            // {
            //     case -1:
            //         _tilemap.SetTile(tilePos, debugBlock);
            //         break;
            //     case 0:
            //         _tilemap.SetTile(tilePos, null);
            //         break;
            //     case 1:
            //         _tilemap.SetTile(tilePos, stone);
            //         break;
            //     case 2:
            //         _tilemap.SetTile(tilePos, copper);
            //         break;
            // }
        }
    }
}
