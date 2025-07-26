using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace EverlastingEdenGenerationJobs
{
    #region MyRegion

     [BurstCompile]
    public struct StoneBaseGenerationJob : IJobFor
    {
        [ReadOnly] public float frequency;
        [ReadOnly] public float persistance;
        [ReadOnly] public int octaves;
        [ReadOnly] public int width;
        [ReadOnly] public float amplitude;
        [ReadOnly] public float stoneChance;
        
        public NativeArray<int> tileTypeMap;
        
        public void Execute(int index)
        {
            int x =  index % width;
            int y =  index / width;
            
            float rng = NoiseUtils.OctaveSimplexNoise(x, y, octaves, persistance, frequency, amplitude);

            if (rng < stoneChance)
            {
                tileTypeMap[index] = 1;
            }
        }
    }
    
    [BurstCompile]
    public struct StoneDetailGenerationJob : IJobFor
    {
        [ReadOnly] public float frequency;
        [ReadOnly] public float persistance;
        [ReadOnly] public int octaves;
        [ReadOnly] public int width;
        [ReadOnly] public float amplitude;
        [ReadOnly] public float stoneChance;
        
        public NativeArray<int> tileTypeMap;
        
        public void Execute(int index)
        {
            int x =  index % width;
            int y =  index / width;
            
            float rng = NoiseUtils.OctavePerlinNoise(x, y, octaves, persistance, frequency, amplitude);

            if (rng < stoneChance)
            {
                tileTypeMap[index] = 0;
            }
        }
    }
    
    [BurstCompile]
    public struct StoneTunnelGenerationJob : IJobFor
    {
        [ReadOnly] public float frequency;
        [ReadOnly] public float persistance;
        [ReadOnly] public int octaves;
        [ReadOnly] public int width;
        [ReadOnly] public float amplitude;
        [ReadOnly] public float stoneChance;
        
        public NativeArray<int> tileTypeMap;
        
        public void Execute(int index)
        {
            int x =  index % width;
            int y =  index / width;
            
            float chance = NoiseUtils.OctaveWorleyBoundryNoise(x, y, octaves, persistance, frequency, amplitude);

            if (chance < stoneChance)
            {
                tileTypeMap[index] = 0;
            }
        }
    }

    #endregion Stone
    
   
}