using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[CreateAssetMenu(fileName = "BlockRegistry", menuName = "ScriptableObjects/Registry/BlockRegistry")]
public class BlockRegistry: ScriptableObject
{
    public AYellowpaper.SerializedCollections.SerializedDictionary<int, BlockSO> blocks;
}
