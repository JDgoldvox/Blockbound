using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace WorldGenerationJobs
{
    [BurstCompile]
    public struct DebugMaterialFillJob : IJobFor //Fills all tiles with a tile
    {
        public int MaterialID;
        public NativeArray<int> TileTypeMap;

        public void Execute(int index)
        {
            TileTypeMap[index] = MaterialID;
        }
    }
    
    [BurstCompile]
    public struct SimplexJob : IJobFor
    {
        [ReadOnly] public float Frequency;
        [ReadOnly] public float Persistance;
        [ReadOnly] public int Octaves;
        [ReadOnly] public int Width;
        [ReadOnly] public int Height;
        [ReadOnly] public float Amplitude;
        [ReadOnly] public float FillChance;
        [ReadOnly] public int MaterialID;
        
        public NativeArray<int> TileTypeMap;
        
        public void Execute(int index)
        {
            Vector2Int coord = TilemapConverter.IndexToCoord(index, Width, Height);
            float rng = NoiseUtils.OctaveSimplexNoise(coord.x,coord.y, Octaves, Persistance, Frequency, Amplitude);

            if (rng < FillChance)
            {
                TileTypeMap[index] = MaterialID;
            }
        }
    }
    
    [BurstCompile]
    public struct WorleyJob : IJobFor
    {
        [ReadOnly] public float Frequency;
        [ReadOnly] public float Persistance;
        [ReadOnly] public int Octaves;
        [ReadOnly] public int Width;
        [ReadOnly] public int Height;
        [ReadOnly] public float Amplitude;
        [ReadOnly] public float fillChance;
        [ReadOnly] public int MaterialNum;
        
        public NativeArray<int> TileTypeMap;
        
        public void Execute(int index)
        {
            Vector2Int coord = TilemapConverter.IndexToCoord(index, Width, Height);
            
            float chance = NoiseUtils.OctaveWorleyNoise(coord.x, coord.y, Octaves, Persistance, Frequency, Amplitude);

            if (chance > fillChance)
            {
                TileTypeMap[index] = MaterialNum;
            }
        }
    }
    
    
    
   #region Ore
   
   [BurstCompile]
   public struct CopperGenerationJob : IJob 
   {
       [ReadOnly] public uint BaseSeed;
       [ReadOnly] public int Width;
       [ReadOnly] public int Height;
       [ReadOnly] public float SpreadChance;
       [ReadOnly] public NativeArray<int> TileTypeMap;
       public NativeList<Vector2Int> NewOre;

       private int halfWidth;
       
       public void Execute()
       {
           // uint seed = BaseSeed * 747796405u; //+ (uint)index
           // var rng = new Unity.Mathematics.Random(seed);
           // halfWidth = Width / 2;
           //
           // //generate random coordinates where ore could spawn
           // int rngX = rng.NextInt(-halfWidth, halfWidth);
           // int rngY = rng.NextInt(0, Height);
           // Vector2Int start = new Vector2Int(rngX, rngY);
           //
           // //if original centre tile is not stone, return
           // if (TileTypeMap[TilemapConverter.CoordToIndex(start, Width)] == 0)
           // {
           //     return;
           // }
           //
           // //start flood fill
           // NativeQueue<Vector2Int> tileQueue = new NativeQueue<Vector2Int>(Allocator.Temp);
           //  
           // tileQueue.Enqueue(start);
           //
           // while (tileQueue.Count > 0)
           // {
           //     Vector2Int tilePos = tileQueue.Dequeue();
           //     
           //     //check if this tile is a stone tile
           //     if (!IsStoneTile(tilePos))
           //     {
           //         continue;
           //     }
           //     
           //     //rng if this tile should exist
           //     if (!IsTileExist(rng))
           //     {
           //         continue;
           //     }
           //      
           //     //if Is a stone tile and does exist, 
           //     //add to tiletype vector
           //     NewOre.Add(tilePos);
           //     
           //     //add surrounding tile to queue
           //     AddAdjacentTilesToQueue(tilePos, ref tileQueue);
           // }
           //
           // tileQueue.Dispose();
       }
   }
   
   #endregion
}