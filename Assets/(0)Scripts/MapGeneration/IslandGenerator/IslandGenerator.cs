using System.Collections.Generic;
using System.Linq;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using VisualDebugging;

using Random = Unity.Mathematics.Random;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.SpatialPartitionUtils;

namespace KaizerWaldCode.MapGeneration
{
    public static class IslandGenerator
    {
        //GENERATE RANDOM SAMPLES
        //==============================================================================================================
        public static float3[] GenerateRandomPoints(GeneralMapSettings gSettings, MapSettings mSettings, int numCellPerAxis)
        {
            SamplesSettings samplesSettings = new SamplesSettings(mSettings, numCellPerAxis);
            Debug.Log($"num samples : {samplesSettings.numCellPerAxis}");
            Debug.Log($"samples Size : {samplesSettings.cellSize}");
            using NativeArray<Bounds> bounds = GetCellsBounds(mSettings.mapSize, numCellPerAxis);
            using NativeArray<float3> samplesPosition = AllocNtvAry<float3>(samplesSettings.totalNumCells);
            
            RandomPointsJob job = new RandomPointsJob(gSettings, mSettings, samplesSettings, samplesPosition, bounds);
            JobHandle jobHandle = job.ScheduleParallel(samplesSettings.totalNumCells, JobsUtility.JobWorkerCount - 1, default);
            jobHandle.Complete();
            
            return samplesPosition.ToArray();
        }
        
        //RADIALS
        //==============================================================================================================
        public static int[] GetCoastLine(float3[] samplesPos, GeneralMapSettings gSettings, MapSettings mSettings)
        {
            using NativeArray<int> samplesNtvArr = AllocNtvAry<int>(samplesPos.Length);
            using NativeArray<float3> samplesPosition = samplesPos.ToNativeArray();
            
            IslandCoastJob islandCoastJob = new IslandCoastJob(gSettings.seed, mSettings.mapSize, samplesPosition, samplesNtvArr);
            JobHandle jobHandle = islandCoastJob.ScheduleParallel(samplesPos.Length, JobsUtility.JobWorkerCount - 1, default);
            jobHandle.Complete();
            
            HashSet<int> test = new HashSet<int>(samplesNtvArr.ToArray());
            test.Remove(-1);
            
            return test.ToArray();
        }
        
        //VERTICES CLOSEST CELL ID
        //==============================================================================================================
        public static int[] GetCellsClosestVertices(SamplesSettings sSettings, float3[] verticesPos, float3[] samplesPos)
        {
            using NativeArray<float3> verticesPosition = verticesPos.ToNativeArray();
            using NativeArray<float3> samplesPosition = samplesPos.ToNativeArray();
            using NativeArray<int> verticesCellsId = AllocNtvAry<int>(verticesPos.Length);
            
            VerticesClosestCellIdJob job = new VerticesClosestCellIdJob(sSettings, verticesPosition, samplesPosition, verticesCellsId);
            
            JobHandle jobHandle = job.ScheduleParallel(verticesPos.Length, JobsUtility.JobWorkerCount - 1, default);
            jobHandle.Complete();

            return verticesCellsId.ToArray();
        }

        //ISLAND TEXTURE (limited to 2 colors for now)
        //==============================================================================================================
        public static Texture2D SetTextureOnIsland(MapSettings mapSettings, int[] verticesCellId, int[] islandId)
        {
            using NativeArray<Color> colorsMap = AllocNtvAry<Color>(verticesCellId.Length);
            using NativeArray<int> vCellIds = verticesCellId.ToNativeArray();
            using NativeArray<int> islandIds = islandId.ToNativeArray();
            
            TextureIslandJob job = new TextureIslandJob(colorsMap, vCellIds, islandIds);
            job.ScheduleParallel(verticesCellId.Length, JobsUtility.JobWorkerCount - 1, default).Complete();
            
            Texture2D tex = TextureGenerator.TextureFromColourMap(colorsMap.ToArray(), mapSettings.mapPointPerAxis, mapSettings.mapPointPerAxis);
            return tex;
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesClosestCellIdJob : IJobFor
    {
        [ReadOnly] private int jNumCellMap;
        [ReadOnly] private float jCellSize;
        [ReadOnly] private NativeArray<float3> jSamplesPos;
        [ReadOnly] private NativeArray<float3> jVertices;
        [NativeDisableParallelForRestriction]
        [WriteOnly]private NativeArray<int> jVerticesCellId;

        public VerticesClosestCellIdJob(SamplesSettings sSettings, NativeArray<float3> vertices, NativeArray<float3> samplesPos, NativeArray<int> verticesCellId)
        {
            jNumCellMap = sSettings.numCellPerAxis;
            jCellSize = sSettings.cellSize;
            jSamplesPos = samplesPos;
            jVertices = vertices;
            jVerticesCellId = verticesCellId;
        }
        
        public void Execute(int index)
        {
            int cellId = KwGrid.Get2DCellID(jVertices[index], jNumCellMap, jCellSize, jVertices[0]);
            (int numCell, int2 xRange, int2 yRange) = KwGrid.CellGridRanges(in cellId, jNumCellMap);
            
            //Check Cells around
            //==========================================================================================================
            NativeArray<int> cellsIndex = new NativeArray<int>(numCell, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int cellCount = 0;
            for (int y = yRange.x; y <= yRange.y; y++)
            {
                for (int x = xRange.x; x <= xRange.y; x++)
                {
                    int indexCellOffset = cellId + mad(y, jNumCellMap, x);
                    cellsIndex[cellCount] = indexCellOffset;
                    cellCount++;
                }
            }
            //Get All Distances
            //==========================================================================================================
            NativeArray<float> distances = new NativeArray<float>(numCell, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < numCell; i++)
            {
                distances[i] = distancesq(jVertices[index].xz, jSamplesPos[cellsIndex[i]].xz);
            }
            //Get the nearest
            int nearestSample = KwGrid.IndexMin(distances, cellsIndex);
            jVerticesCellId[index] = nearestSample;
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct TextureIslandJob : IJobFor
    {
        [ReadOnly]private NativeArray<int> jIslandId;
        [ReadOnly]private NativeArray<int> jVerticesCellId;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<Color> jColors;

        public TextureIslandJob(NativeArray<Color> colors, NativeArray<int> verticesCellId, NativeArray<int> islandId)
        {
            jColors = colors;
            jVerticesCellId = verticesCellId;
            jIslandId = islandId;
        }

        public void Execute(int index)
        {
            bool found = false;
            for (int i = 0; i < jIslandId.Length; i++)
            {
                if (jIslandId[i] != jVerticesCellId[index]) continue;
                found = true;
                break;
            }
            jColors[index] = found ? Color.green : Color.white;
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct RandomPointsJob : IJobFor
    {
        [ReadOnly] private uint jSeed;
        [ReadOnly] private int jSize;
        [ReadOnly] private float jCellSize;

        [ReadOnly] private NativeArray<Bounds> jBounds;

        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<float3> jRandomPointsPosition;

        public RandomPointsJob(GeneralMapSettings gSettings, MapSettings mSettings, SamplesSettings sSettings, NativeArray<float3> cellPos, NativeArray<Bounds> bounds)
        {
            jSeed = gSettings.seed;
            jSize = mSettings.mapSize;
            jCellSize = sSettings.cellSize;
            jRandomPointsPosition = cellPos;
            jBounds = bounds;
        }
        public void Execute(int index)
        {
            Random rng = Random.CreateFromIndex((uint)mad(jSeed ,jBounds.Length , index));
            
            float cellRadius = jCellSize * 0.5f; //also jBounds[index].extents.x / z
            float midMapSize = jSize * -0.5f;
           
            // Get the current Position of the center of the cell
            float2 cellCenter = float3(jBounds[index].center).xz + float2(midMapSize);
            
            //Process Random
            float2 randDirection = rng.NextFloat2Direction();
            float2 sample = mad(randDirection, rng.NextFloat(0 , cellRadius), cellCenter);
            
            jRandomPointsPosition[index] = float3(sample.x, 0, sample.y);
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct IslandCoastJob : IJobFor
    {
        [ReadOnly] private uint jSeed;
        [ReadOnly] private int jMapSize;

        [ReadOnly] private NativeArray<float3> jSamplesPosition;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<int> jIslandSamplesID;

        public IslandCoastJob(uint seed, int mapSize, NativeArray<float3> samplesPosition, NativeArray<int> islandSamplesID)
        {
            jMapSize = mapSize;
            jSeed = seed;
            jSamplesPosition = samplesPosition;
            jIslandSamplesID = islandSamplesID;
        }

        public void Execute(int index)
        {
            Random islandRandom = Random.CreateFromIndex((uint)mad(jSeed ,jSamplesPosition.Length , index));
            
            const float ISLAND_FACTOR = 1.07f; // 1.0 means no small islands; 2.0 leads to a lot
            const float PI2 = PI * 2f;
            
            float midSize = jMapSize * 0.5f; // need to be added because calculated on the center of the map(mapSize)
            float3 sampleDisc = jSamplesPosition[index];
            
            float x = 2f * ( (sampleDisc.x + midSize) / jMapSize - 0.5f ); // midSize offset still needed, mesh is not center to 0,0
            float z = 2f * ( (sampleDisc.z + midSize) / jMapSize - 0.5f ); // ONLY Vertices ARE! (Same goes for random points btw)
            float3 point = float3(x, 0, z);

            //randoms
            //==========================================================================================================
            int bumps = islandRandom.NextInt(1, 6);
            float startAngle = islandRandom.NextFloat(PI2);
            float dipAngle = islandRandom.NextFloat(PI2);
            float dipWidth = islandRandom.NextFloat(0.2f, 0.7f); //threshold to improve generation
            
            //Length multiplier
            //==========================================================================================================
            float angle = atan2(point.z, point.x); //more like : where am i on the circle
            float lengthMul = 0.5f; // 0.1f : big island 1.0f = small island // by increasing by 0.1 island size is reduced by 1
            float totalLength = mad(lengthMul, max(abs(point.x), abs(point.z)), length(point));
            
            //Radials
            //==========================================================================================================
            float radialsBase = mad(bumps, angle, startAngle);
            float r1Sin = sin(radialsBase + cos((bumps + 3) * angle));
            float r2Sin = sin(radialsBase + sin((bumps + 2) * angle));
            
            float radial1 = 0.5f + 0.4f * r1Sin;
            float radial2 = 0.7f - 0.2f * r2Sin;

            //Not needed but improve shape
            if (abs(angle - dipAngle) < dipWidth 
                || abs(angle - dipAngle + PI2) < dipWidth 
                || abs(angle - dipAngle - PI2) < dipWidth)
            {
                radial1 = radial2 = 0.2f;
            }

            jIslandSamplesID[index] = select(-1,index,totalLength < radial1 || (totalLength > radial1 * ISLAND_FACTOR && totalLength < radial2));
        }
    }
}