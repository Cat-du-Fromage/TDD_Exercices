using KaizerWaldCode.MapGeneration.Data;
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
        //Generate RandomPoints
        //-> Generate Cells (spatial partition)
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
        
        //Radials
        public static int[] GetCoastLine(float3[] samplesPos, GeneralMapSettings gSettings, MapSettings mSettings)
        {
            int[] samplesId = new int[samplesPos.Length];
            VisualDebug.Initialize();
            VisualDebug.BeginFrame("All points", true);
            
            Vector3[] points = ReinterpretArray<float3, Vector3>(samplesPos);
            VisualDebug.DrawPoints(points, .1f);
            
            for (int i = 0; i < samplesPos.Length; i++)
            {
                samplesId[i] = RedBlobImplementation(123, samplesPos[i].xz, mSettings.mapSize) ? 1 : 0;
            }

            VisualDebug.Save();
            return samplesId;
        }
        
        static bool RedBlobImplementation(uint seed, float2 sampleDisc, int mapSize)
        {
            float ISLAND_FACTOR = 1.27f; // 1.0 means no small islands; 2.0 leads to a lot
            float PI2 = PI*2;
            
            float midSize = mapSize / 2f;

            float x = 2f * ((sampleDisc.x+midSize) / mapSize - 0.5f);
            float z = 2f * ((sampleDisc.y+midSize) / mapSize - 0.5f);

            float3 point = new float3(x, 0, z);
            //Debug.Log($"x = {x}// z = {z}");
            Random islandRandom = Random.CreateFromIndex(seed);

            int bumps = islandRandom.NextInt(1, 6);
            float startAngle = islandRandom.NextFloat(PI2); //radians 2 Pi = 360°
            float dipAngle = islandRandom.NextFloat(PI2);
            float dipWidth = islandRandom.NextFloat(0.2f, 0.7f); // = mapSize?

            float angle = atan2(point.z, point.x);
            float lengthMul = 0.5f; // 0.1f : big island 1.0f = small island // by increasing by 0.1 island size is reduced by 1
            float totalLength = lengthMul * max(abs(point.x), abs(point.z)) + length(point);
            
            //Sin val Range[-1,1]
            float radialsBase = mad(bumps, angle, startAngle); // bump(1-6) * angle(0.x) + startangle(0.x)
            float r1Sin = sin(radialsBase + cos((bumps + 3) * angle));
            float r2Sin = sin(radialsBase + sin((bumps + 2) * angle));
            
            //r1 = 0.5f // r2 = 0.7f
            float radial1 = 0.5f + 0.4f * r1Sin;
            float radial2 = 0.7f - 0.2f * r2Sin;

            if (abs(angle - dipAngle) < dipWidth || abs(angle - dipAngle + PI2) < dipWidth || abs(angle - dipAngle - PI2) < dipWidth)
            {
                radial1 = radial2 = 0.2f;
            }

            if (totalLength < radial1 || (totalLength > radial1 * ISLAND_FACTOR && totalLength < radial2)) { return true; }
            return false;
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct RandomPointsJob : IJobFor
    {
        [ReadOnly] private int jSize;
        [ReadOnly] private float jCellSize;
        [ReadOnly] private int jNumCellPerAxis;
        [ReadOnly] private uint jSeed;

        [ReadOnly] private NativeArray<Bounds> jBounds;

        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<float3> jRandomPointsPosition;

        public RandomPointsJob(GeneralMapSettings gSettings, MapSettings mSettings, SamplesSettings sSettings, NativeArray<float3> cellPos, NativeArray<Bounds> bounds)
        {
            jSize = mSettings.mapSize;
            jCellSize = sSettings.cellSize;
            jNumCellPerAxis = sSettings.numCellPerAxis;
            jSeed = gSettings.seed;
            jRandomPointsPosition = cellPos;
            jBounds = bounds;
        }
        public void Execute(int index)
        {
            Random prng = Random.CreateFromIndex(jSeed + (uint)index);
            
            float cellRadius = jCellSize * 0.5f; //also jBounds[index].extents.x / z
            float midMapSize = jSize * -0.5f;
           
            // Get the current Position of the center of the cell
            float2 cellCenter = float3(jBounds[index].center).xz + float2(midMapSize);
            
            //Process Random
            float2 randDirection = prng.NextFloat2Direction();
            float2 sample = mad(randDirection, prng.NextFloat(0 , cellRadius), cellCenter);
            
            jRandomPointsPosition[index] = float3(sample.x, 0, sample.y);
        }

        private float2 GetSampleCenter(int i)
        {
            int z = (int)floor((float)i / jNumCellPerAxis);
            int x = i - (z * jNumCellPerAxis);
            
            float midMapSize = jSize * -0.5f;
            // Get the current Position of the center of the cell
            float midCellSizePos = jCellSize * 0.5f;
            float cellCenterX = mad(x, jCellSize, midCellSizePos);
            float cellCenterY = mad(z, jCellSize, midCellSizePos);
            return float2(cellCenterX + midMapSize, cellCenterY + midMapSize);
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct IslandCoastJob : IJobFor
    {
        [ReadOnly] private int jMapSize;
        [ReadOnly] private uint jSeed;

        [ReadOnly] private NativeArray<float3> jSamplesPosition;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<int> jIslandSamplesID;

        public void Execute(int index)
        {
            Random islandRandom = Random.CreateFromIndex(jSeed);
            
            float ISLAND_FACTOR = 1.27f; // 1.0 means no small islands; 2.0 leads to a lot
            float PI2 = PI*2;
            float midSize = jMapSize / 2f; // need to be added because calculated on the center of the map(mapSize)
            float3 sampleDisc = jSamplesPosition[index];
            
            float x = 2f * (sampleDisc.x / jMapSize - 0.5f);
            float z = 2f * (sampleDisc.z / jMapSize - 0.5f);
            float3 point = new float3(x, 0, z);

            int bumps = islandRandom.NextInt(1, 6);
            float startAngle = islandRandom.NextFloat(PI2); //radians 2 Pi = 360°
            float dipAngle = islandRandom.NextFloat(PI2);
            float dipWidth = islandRandom.NextFloat(0.2f, 0.7f); // = mapSize?

            float angle = atan2(point.z, point.x); //more like : where am i on the circle
            float lengthMul = 0.5f; // 0.1f : big island 1.0f = small island // by increasing by 0.1 island size is reduced by 1
            float totalLength = mad(lengthMul, max(abs(point.x), abs(point.z)), length(point));
            
            float radialsBase = mad(bumps, angle, startAngle); // bump(1-6) * angle(0.x) + startangle(0.x)
            float r1Sin = sin(radialsBase + cos((bumps + 3) * angle));
            float r2Sin = sin(radialsBase + sin((bumps + 2) * angle));
            
            float radial1 = 0.5f + 0.4f * r1Sin;
            float radial2 = 0.7f - 0.2f * r2Sin;

            if (abs(angle - dipAngle) < dipWidth || abs(angle - dipAngle + PI2) < dipWidth || abs(angle - dipAngle - PI2) < dipWidth)
            {
                radial1 = radial2 = 0.2f;
            }

            jIslandSamplesID[index] = select(-1,index,totalLength < radial1 || (totalLength > radial1 * ISLAND_FACTOR && totalLength < radial2));
        }
    }
}