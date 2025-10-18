using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BlockItemSO",  menuName = "ScriptableObjects/Items/BlockItemSO")]
public class BlockItemSO : ItemSO
{
    [Header("Block Properties")]
    public TileBase tileBase;
    
}
