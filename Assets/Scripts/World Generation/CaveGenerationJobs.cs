
using System.Diagnostics;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

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
    
    [BurstCompile(CompileSynchronously = true)]
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
    
    [BurstCompile(CompileSynchronously = true)]
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
}