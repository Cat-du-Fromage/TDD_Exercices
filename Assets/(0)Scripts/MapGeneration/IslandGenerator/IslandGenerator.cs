using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using Random = Unity.Mathematics.Random;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

namespace KaizerWaldCode.MapGeneration
{
    public static class IslandGenerator
    {
        //Generate RandomPoints
        //-> Generate Cells (spatial partition)
        public static void GenerateRandomPoints()
        {
            
        }
        
        //Radials
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct RandomPointsJob : IJobFor
    {
        [ReadOnly] private float jSize;
        [ReadOnly] private int jCellSize;
        [ReadOnly] private int jNumCellPerAxis;
        [ReadOnly] private uint jSeed;

        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<float3> jRandomPointsPosition;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<int> jRandomPointsId;

        public void Execute(int index)
        {
            Random prng = Random.CreateFromIndex(jSeed + (uint)index);

            int z = (int)floor((float)index / jNumCellPerAxis);
            int x = index - (z * jNumCellPerAxis);
            
            float midMapSize = jSize * -0.5f;
            // Get the current Position of the center of the cell
            float midCellSizePos = mad(jCellSize , 0.5f, midMapSize);
            float cellCenterX = mad(x, jCellSize, midCellSizePos);
            float cellCenterY = mad(z, jCellSize, midCellSizePos);
            float2 midCellPos = float2(cellCenterX, cellCenterY);
            
            //Process Random
            float2 randDirection = prng.NextFloat2Direction();
            float2 sample = mad(randDirection, prng.NextFloat(0 , midCellSizePos), midCellPos);
            
            jRandomPointsPosition[index] = float3(sample.x, 0, sample.y);
            jRandomPointsId[index] = index;
        }
    }
}