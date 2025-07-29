using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace EverlastingEdenGenerationJobs
{
    #region Stone

    [BurstCompile]
    public struct StoneBaseGenerationJob : IJobFor
    {
        [ReadOnly] public float Frequency;
        [ReadOnly] public float Persistance;
        [ReadOnly] public int Octaves;
        [ReadOnly] public int Width;
        [ReadOnly] public float Amplitude;
        [ReadOnly] public float StoneChance;
        
        public NativeArray<int> TileTypeMap;
        
        public void Execute(int index)
        {
            int x =  index % Width;
            int y =  index / Width;
            
            float rng = NoiseUtils.OctaveSimplexNoise(x, y, Octaves, Persistance, Frequency, Amplitude);

            if (rng < StoneChance)
            {
                TileTypeMap[index] = 1;
            }
        }
    }
    
    [BurstCompile]
    public struct StoneDetailGenerationJob : IJobFor
    {
        [ReadOnly] public float Frequency;
        [ReadOnly] public float Persistance;
        [ReadOnly] public int Octaves;
        [ReadOnly] public int Width;
        [ReadOnly] public float Amplitude;
        [ReadOnly] public float StoneChance;
        
        public NativeArray<int> TileTypeMap;
        
        public void Execute(int index)
        {
            int x =  index % Width;
            int y =  index / Width;
            
            float rng = NoiseUtils.OctavePerlinNoise(x, y, Octaves, Persistance, Frequency, Amplitude);

            if (rng < StoneChance)
            {
                TileTypeMap[index] = 0;
            }
        }
    }
    
    [BurstCompile]
    public struct StoneTunnelGenerationJob : IJobFor
    {
        [ReadOnly] public float Frequency;
        [ReadOnly] public float Persistance;
        [ReadOnly] public int Octaves;
        [ReadOnly] public int width;
        [ReadOnly] public float Amplitude;
        [ReadOnly] public float StoneChance;
        
        public NativeArray<int> TileTypeMap;
        
        public void Execute(int index)
        {
            int x =  index % width;
            int y =  index / width;
            
            float chance = NoiseUtils.OctaveWorleyBoundryNoise(x, y, Octaves, Persistance, Frequency, Amplitude);

            if (chance < StoneChance)
            {
                TileTypeMap[index] = 0;
            }
        }
    }

    #endregion Stone
    
   #region Ore
   
   // public struct BBBBBBCopperGenerationJob : IJobFor
   // {
   //     [ReadOnly] public float frequency;
   //     [ReadOnly] public float persistance;
   //     [ReadOnly] public int octaves;
   //     [ReadOnly] public int width;
   //     [ReadOnly] public float amplitude;
   //     [ReadOnly] public float chance;
   //     [ReadOnly] public bool debugMode;
   //      
   //     public NativeArray<int> tileTypeMap;
   //      
   //     public void Execute(int index)
   //     {
   //         int x =  index % width;
   //         int y =  index / width;
   //          
   //         float rng = NoiseUtils.OctavePerlinNoise(x, y, octaves, persistance, frequency, amplitude);
   //
   //         if (debugMode && rng < chance)
   //         {
   //             tileTypeMap[index] = -1;
   //         }
   //         else if (rng < chance && tileTypeMap[index] == 1)
   //         {
   //             tileTypeMap[index] = 2;
   //         }
   //
   //     }
   // }
   
   [BurstCompile]
   public struct CopperLargeGenerationJob : IJobFor
   {
   
       //[ReadOnly] public NativeArray<Vector2Int> RandomCoordinates;
       [ReadOnly] public uint BaseSeed;
       [ReadOnly] public int Width;
       [ReadOnly] public int Height;
       [ReadOnly] public float SpreadChance;
       [ReadOnly] public bool DebugMode;
       [ReadOnly] public NativeArray<int> TileTypeMap;
       public NativeList<Vector2Int> NewOre;
       
       public void Execute(int index)
       {
           uint seed = BaseSeed + (uint)index * 747796405u;
           var rng = new Unity.Mathematics.Random(seed);


           // //generate random coordinates where ore could spawn
           // List<Vector2Int> randomCoords = new List<Vector2Int>();
           // for (int i = 0; i < _everlastingEdenOreSO.CopperQuantity; i++)
           // {
           //     int rngX = rng.NextInt(-halfWidth, halfWidth);
           //     int rngY = rng.NextInt(-halfWidth, halfWidth);
           //     randomCoords.Add(new Vector2Int(rngX, rngY));
           // }



           // if (DebugMode && rng < Chance)
           // {
           //     tileTypeMap[index] = -1;
           // }
           // else if (rng < Chance && tileTypeMap[index] == 1)
           // {
           //     tileTypeMap[index] = 2;
           // }
       }
   }
   
   #endregion
}