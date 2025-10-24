
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
    
    [BurstCompile(CompileSynchronously = true)]
    public struct OreFloodFillJob : IJobFor
    {
        [ReadOnly] public NativeArray<int> randomX;
        [ReadOnly] public NativeArray<int> randomY;
        [ReadOnly] public int maxQuantityPerVein;
        [ReadOnly] public int spawnMaterialID;
        [ReadOnly] public float spreadChance;
        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public Unity.Mathematics.Random rng;
        [NativeDisableParallelForRestriction] public NativeList<Vector2Int>.ParallelWriter newOreTilePositions;
        [NativeDisableParallelForRestriction] public NativeArray<int> tileTypeMap;
        public void Execute(int index)
        {
            Vector2Int startPosition = new Vector2Int(randomX[index], randomY[index]);
            
            //set up data containers
            NativeList<Vector2Int> tileCoordExplorationList = new NativeList<Vector2Int>(Allocator.Temp);
            NativeHashSet<Vector2Int> visitedTiles = new NativeHashSet<Vector2Int>(50, Allocator.Temp);
            int tileCount = 0;
            
            // Add the starting element
            tileCoordExplorationList.Add(startPosition);
            
            while (tileCoordExplorationList.Length != 0 && tileCount < maxQuantityPerVein) 
            {
                 //Randomise a coordinate from the list
                 int nextRandomIndex = rng.NextInt(0, tileCoordExplorationList.Length);
                 Vector2Int tilePos = tileCoordExplorationList[nextRandomIndex];
                 int tilePosIndex = tileCoordExplorationList.IndexOf(tilePos);
                 tileCoordExplorationList.RemoveAtSwapBack(tilePosIndex);
                
                 //if this position is not a spawnable block
                 if (!TilemapUtils.IsSpecificTile(tilePos, spawnMaterialID, tileTypeMap, width, height))
                 {
                     continue;
                 }
                         
                 //if reached max tiles for this ore vein, return
                 tileCount++;
                 visitedTiles.Add(tilePos);
                 newOreTilePositions.AddNoResize(tilePos);
                 
                if (tileCount > maxQuantityPerVein) {
                     break;
                }
                
                //queue adjacent tiles that have spwanable tile
                var adjacentStoneTiles = TilemapUtils.ReturnSpecificAdjacentTilesParallel(
                    tilePos,
                    spawnMaterialID,
                    tileTypeMap,
                    height,
                    width
                    );
                
                //queue adjacent tiles, if not already in queue
                foreach (var newTilePos in adjacentStoneTiles)
                {
                    //rng if this tile should exist
                    float chance = rng.NextFloat(0, 1);
                    
                    //ignore if chance missed
                    if (chance > spreadChance)
                    {
                        continue;
                    }
                    
                    //ignore if already visited
                    if (visitedTiles.Contains(newTilePos))
                    {
                        continue;
                    }
                    
                    //add this coordinate to exploration queue
                    tileCoordExplorationList.Add(newTilePos);
                }
                
                adjacentStoneTiles.Dispose();
            }

            tileCoordExplorationList.Dispose();
            visitedTiles.Dispose();
        }
    }
    
    
}