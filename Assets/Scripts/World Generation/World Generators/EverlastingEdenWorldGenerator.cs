using System.Timers;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using WorldGenerationJobs;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

[BurstCompile]
public class EverlastingEdenWorldGenerator : WorldGenerator
{
    [SerializeField] private CaveSO _caveSO; 
    [SerializeField] private EverlastingEdenOreSO _oreSO;
    
    [Header("Blocks")] 
    [SerializeField] private BlockItemSO _debugBlock;
    [SerializeField] private BlockItemSO _airBlock;
    [SerializeField] private BlockItemSO _dirtBlock;
    [SerializeField] private BlockItemSO _stoneBlock;
    [SerializeField] private BlockItemSO _copperOreBlock;
    
    //[Header("Background")]
    
    public void GenerateWorld()
    {
        Stopwatch stopwatch = new Stopwatch(); 
        
        //TEMP *******************************************
        stopwatch.Start();
        
        //fill lookup
        _localTilebaseLookUp.Clear();
        _localTilebaseLookUp.Add(_debugBlock.ID, _debugBlock);
        _localTilebaseLookUp.Add(_airBlock.ID, _airBlock);
        _localTilebaseLookUp.Add(_dirtBlock.ID, _dirtBlock);
        _localTilebaseLookUp.Add(_stoneBlock.ID, _stoneBlock);
        _localTilebaseLookUp.Add(_copperOreBlock.ID, _copperOreBlock);
        
        PreGenerationTasks();
        
        stopwatch.Stop();
        Debug.Log("PREP: " + stopwatch.Elapsed.TotalSeconds + " seconds");
        //TEMP *******************************************
        stopwatch.Reset();
        stopwatch.Start();
        
        GenerateCaveLayer();
        
        stopwatch.Stop();
        Debug.Log("CAVE GEN: " + stopwatch.Elapsed.TotalSeconds + " seconds");
        //TEMP *******************************************
        stopwatch.Reset();
        stopwatch.Start();
        
        GenerateOreLayer();
        
        stopwatch.Stop();
        Debug.Log("ORE GEN: " + stopwatch.Elapsed.TotalSeconds + " seconds");
        //TEMP *******************************************
        
        stopwatch.Reset();
        stopwatch.Start();
        
        BuildTileMap();
        
        stopwatch.Stop();
        Debug.Log("Build time: " + stopwatch.Elapsed.TotalSeconds + " seconds");
        
        //TEMP *******************************************
        stopwatch.Reset();
        stopwatch.Start();
        
        CleanUp();
        stopwatch.Stop();
        Debug.Log("cleanup: " + stopwatch.Elapsed.TotalSeconds + " seconds");
    }

    private void GenerateCaveLayer()
    {
        //Debug Caves
        var debugMaterialFillJob = new DebugMaterialFillJob()
        {
            MaterialID = _stoneBlock.ID,
            TileTypeMap = _tileTypeMap
        };
        JobUtils.ScheduleJobAndExecuteParallelFor(debugMaterialFillJob, _totalBlocks, _caveSO.DebugFillWithMaterial);
        
        // Base Stone Caves
        var caveBaseJob = new WorldGenerationJobs.SimplexJob()
        {
            Width = _width,
            Height = _height,
            Frequency = _caveSO.CaveBaseFrequency,
            Persistance = _caveSO.CaveBasePersistance,
            Octaves = _caveSO.CaveBaseOctaves,
            Amplitude = _caveSO.CaveBaseAmplitude,
            FillChance = _caveSO.CaveBaseMaterialFillChance,
            MaterialID = _stoneBlock.ID,
            TileTypeMap = _tileTypeMap
        };
        JobUtils.ScheduleJobAndExecuteParallelFor(caveBaseJob, _totalBlocks, _caveSO.EnableCaveBase);
        
        //Small Caves
        var smallCaveJob = new WorldGenerationJobs.SimplexJob()
        {
            Width = _width,
            Height = _height,
            Frequency = _caveSO.SmallCaveFrequency,
            Persistance = _caveSO.SmallCavePersistance,
            Octaves =  _caveSO.SmallCaveOctaves,
            Amplitude = _caveSO.SmallCaveAmplitude,
            FillChance = _caveSO.SmallCaveFillChance,
            MaterialID = _airBlock.ID,
            TileTypeMap = _tileTypeMap
        };
        JobUtils.ScheduleJobAndExecuteParallelFor(smallCaveJob, _totalBlocks, _caveSO.EnableCaveBase);
        
        // Small Cave fill
        var smallCaveFillJob = new WorldGenerationJobs.SimplexJob()
        {
            Width = _width,
            Height = _height,
            Frequency = _caveSO.SmallCaveFillFrequency,
            Persistance = _caveSO.SmallCaveFillPersistance,
            Octaves = _caveSO.SmallCaveFillOctaves,
            Amplitude = _caveSO.SmallCaveFillAmplitude,
            FillChance = _caveSO.SmallCaveFillMaterialFillChance,
            MaterialID = _stoneBlock.ID,
            TileTypeMap = _tileTypeMap
        };
        JobUtils.ScheduleJobAndExecuteParallelFor(smallCaveFillJob, _totalBlocks, _caveSO.EnableCaveBase);
        
        // Large Cave fill 
        var LargeCaveFillJob = new WorldGenerationJobs.SimplexJob()
        {
            Width = _width,
            Height = _height,
            Frequency = _caveSO.LargeCaveFillFrequency,
            Persistance = _caveSO.LargeCaveFillPersistance,
            Octaves = _caveSO.LargeCaveFillOctaves,
            Amplitude = _caveSO.LargeCaveFillAmplitude,
            FillChance = _caveSO.LargeCaveFillMaterialFillChance,
            MaterialID = _stoneBlock.ID,
            TileTypeMap = _tileTypeMap
        };
        JobUtils.ScheduleJobAndExecuteParallelFor(LargeCaveFillJob, _totalBlocks, _caveSO.EnableCaveBase);
        
        //Caverns
        var cavernJob = new WorldGenerationJobs.WorleyJob()
        {
            Width = _width,
            Height = _height,
            Frequency = _caveSO.CavernFrequency,
            Persistance = _caveSO.CavernPersistance,
            Octaves = _caveSO.CavernOctaves,
            Amplitude = _caveSO.CavernAmplitude,
            fillChance = _caveSO.CavernChance,
            MaterialNum = _airBlock.ID,
            TileTypeMap = _tileTypeMap
        };
        JobUtils.ScheduleJobAndExecuteParallelFor(cavernJob, _totalBlocks, _caveSO.EnableCaveBase);
    }
    
    private void GenerateOreLayer()
    {

        
        if (_oreSO.enableCopper)
        {
            for (int i = 0; i < _oreSO.CopperLargeVeinQuantity; i++)
            {
                OreFloodFill(
                    _oreSO.CopperLargeVeinMaxTileQuantity,
                    _oreSO.CopperLargeVeinSpreadChance,
                    _stoneBlock.ID,
                    _copperOreBlock.ID
                ); 
            }
        }
 
    }
};