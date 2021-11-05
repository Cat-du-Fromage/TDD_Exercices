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

using Random = Unity.Mathematics.Random;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.SpatialPartitionUtils;

namespace KaizerWaldCode.MapGeneration
{
    public static class IslandMeshGenerator
    {
        public static void GetIslandLayers(MapSettings mapSettings, int[] islandIds)
        {
            bool isSchedule = false;
            List<JobHandle> jobHandles = new List<JobHandle>(4);
            JobHandle currentJobHandle = new JobHandle();
            //While native array contains -1

            using NativeArray<int> layers = AllocNtvAry<int>(mapSettings.totalMapPoints);
            using NativeArray<int> islandLayers = AllocNtvAry<int>(mapSettings.totalMapPoints);
            using NativeArray<int> islands = ArrayToNativeArray(islandIds); 

            for (int i = 0; i < jobHandles.Count; i++)
            {
                if (i != 0)
                {
                    if (isSchedule && currentJobHandle.IsCompleted)
                    {
                        isSchedule = false;
                        IslandLayersJob job = new IslandLayersJob(i, islands, islandLayers);
                        jobHandles.Add(job.ScheduleParallel(mapSettings.totalMapPoints, JobsUtility.JobWorkerCount - 1, jobHandles[i-1]));
                        isSchedule = true;
                    }
                }
                else
                {
                    IslandLayersJob job = new IslandLayersJob(i, islands, islandLayers);
                    jobHandles.Add(job.ScheduleParallel(mapSettings.totalMapPoints, JobsUtility.JobWorkerCount - 1, default));
                    isSchedule = true;
                }
            }
            jobHandles[jobHandles.Count].Complete();
            //
            
            //
        }

        private struct IslandLayersJob : IJobFor
        {
            [ReadOnly] private int jLayer;
            [ReadOnly] private NativeArray<int> jIslands;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeArray<int> jIslandLayers;

            public IslandLayersJob(int layer, NativeArray<int> islands, NativeArray<int> islandLayers)
            {
                jLayer = layer;
                jIslands = islands;
                jIslandLayers = islandLayers;
            }
            
            public void Execute(int index)
            {
                
            }
        }
    }
}