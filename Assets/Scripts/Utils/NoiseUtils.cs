using System;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
    
[BurstCompile]
public static class NoiseUtils
{
    private static int _truePositiveOffset = 10000;
    
    public static float OctavePerlinNoise(float x, float y, int octaves, float persistence, float frequency)
    {
        float total = 0;
        float maxValue = 0;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            float2 coordinate = new float2((x + _truePositiveOffset) * frequency, (y + _truePositiveOffset) * frequency);
            total += Unity.Mathematics.noise.pnoise(coordinate , new float2(100000f,100000)) * amplitude;
            
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }
        
        return total / maxValue;
    }
}
