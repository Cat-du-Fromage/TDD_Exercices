#define DEBUG_EXAMPLE_ALGORITHM
using System.Collections.Generic;
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
using static KaizerWaldCode.Utils.KwManagedContainerUtils;
using static KaizerWaldCode.Utils.SpatialPartitionUtils;
using static Unity.Collections.NativeArrayExtensions;

namespace KaizerWaldCode.MapGeneration
{
    public static class IslandMeshGenerator
    {
        public static void GetIslandDstLayers(in MapSettings mapSettings, int[] islandIds, int[] verticesCellId, Vector3[] verticesPos)
        {
            using NativeArray<int> layers = AllocNtvAry<int>(mapSettings.totalMapPoints);

            NativeArray<int> activeLayer; //set of vertices from where we will check around
            NativeList<int> islandBuffer; //vertices around activeLayer
            
            //ISLAND LAYER
            //==========================================================================================================
            using (islandBuffer = new NativeList<int>(mapSettings.totalMapPoints, Allocator.TempJob))
            {
                GetIslandLayer(in mapSettings.totalMapPoints, layers, verticesCellId, islandIds, islandBuffer);
                activeLayer = AllocNtvAry<int>(islandBuffer.Length);
                activeLayer.CopyFrom(islandBuffer);
            }
            //DISTANCE FROM ISLAND LAYERS
            //==========================================================================================================
            int layer = 1;
            while (layers.Contains(-1))
            {
                using (islandBuffer = new NativeList<int>(mapSettings.totalMapPoints, Allocator.TempJob))
                {
                    GetDistancesFromIsland(in mapSettings, layers, activeLayer, islandBuffer).Complete();
                    //also return buffer needed for next iteration -> buffer = next layer to work on(activeLayer)
                    activeLayer.Dispose();
                    
                    HashSet<int> uniqueValBuffer = new HashSet<int>(islandBuffer.ToArray()); //get unique Values
                    using NativeArray<int> layerToApply = uniqueValBuffer.ToNativeArray();
                    
                    ApplyLayerIsland(in layer, layers, layerToApply).Complete();

                    //buffer reallocate to the activeLayer to be worked
                    activeLayer = AllocNtvAry<int>(layerToApply.Length);
                    activeLayer.CopyFrom(layerToApply);
                }
                layer++;
            }
            activeLayer.Dispose();

#if DEBUG_EXAMPLE_ALGORITHM
            VisualDebug.Clear();
            VisualDebug.Initialize();
            for (int i = 0; i < verticesPos.Length; i++)
            {
                VisualDebug.BeginFrame("Point Location", true);
                VisualDebug.SetColour(Colours.lightRed, Colours.veryDarkGrey);
                VisualDebug.DrawPointWithLabel(verticesPos[i], .05f, layers[i].ToString());
            }
        
            VisualDebug.Save();
#endif
        }
        
        private static JobHandle ApplyLayerIsland(in int iLayer, NativeArray<int> layers, NativeArray<int> uniqueValBuffer, JobHandle dependency = default)
        {
            ApplyLayerJob applyJob = new ApplyLayerJob(iLayer, uniqueValBuffer, layers);
            JobHandle jh = applyJob.ScheduleParallel(uniqueValBuffer.Length, JobsUtility.JobWorkerCount - 1, dependency);
            return jh;
        }
        
        private static JobHandle GetDistancesFromIsland(in MapSettings mapSettings, NativeArray<int> layers, NativeArray<int> activeLayer, NativeList<int> buffer, JobHandle dependency = default)
        {
            DistanceFromIslandJob job = new DistanceFromIslandJob(mapSettings, activeLayer, layers, buffer);
            //Get indices needed to change
            JobHandle jh = job.ScheduleParallel(activeLayer.Length, JobsUtility.JobWorkerCount - 1, dependency);
            return jh;
        }

        /// <summary>
        /// FIRST ISLAND LAYER
        /// </summary>
        /// <param name="totalMapPoints"></param>
        /// <param name="layers"></param>
        /// <param name="vCellId"></param>
        /// <param name="islandIds"></param>
        /// <param name="buffer"></param>
        /// <param name="dependency"></param>
        private static void GetIslandLayer(in int totalMapPoints, NativeArray<int> layers, int[] vCellId, int[] islandIds, NativeList<int> buffer, JobHandle dependency = default)
        {
            using NativeArray<int> ntvCellId = vCellId.ToNativeArray();
            using NativeArray<int> islands = islandIds.ToNativeArray(); //carefull you can't complete job outside with that!
            IslandLayerJob layerJob = new IslandLayerJob(layers, ntvCellId, islands, buffer);
            JobHandle jh = layerJob.ScheduleParallel(totalMapPoints, JobsUtility.JobWorkerCount - 1, dependency);
            jh.Complete();
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct ApplyLayerJob : IJobFor
        {
            [ReadOnly] private int jDistance;
            [ReadOnly] private NativeArray<int> jActiveLayer;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeArray<int> jIslandLayers;

            public ApplyLayerJob(int distance, NativeArray<int> activeLayer, NativeArray<int> islandLayers)
            {
                jDistance = distance;
                jActiveLayer = activeLayer;
                jIslandLayers = islandLayers;
            }

            public void Execute(int index)
            {
                //Set islands map at activeLayer to layer
                jIslandLayers[jActiveLayer[index]] = jDistance;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct IslandLayerJob : IJobFor
        {
            [ReadOnly] private NativeArray<int> jIslandId;
            [ReadOnly] private NativeArray<int> jVerticesCellId;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeArray<int> jIslandLayers;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeList<int>.ParallelWriter jBuffer;

            public IslandLayerJob(NativeArray<int> islandLayer, NativeArray<int> verticesCellId, NativeArray<int> islandId, NativeList<int> buffer)
            {
                jIslandLayers = islandLayer;
                jVerticesCellId = verticesCellId;
                jIslandId = islandId;
                jBuffer = buffer.AsParallelWriter();
            }

            public void Execute(int index)
            {
                bool found = jIslandId.Contains(jVerticesCellId[index]);
                jIslandLayers[index] = select(-1,0,found);
                jBuffer.AddNoResizeIf(found,index);
            }
        }

        private struct DistanceFromIslandJob : IJobFor
        {
            [ReadOnly] private int jPointsPerAxis;
            [ReadOnly] private NativeArray<int> jIslandLayers;
            [ReadOnly] private NativeArray<int> jActiveLayer;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeList<int>.ParallelWriter jIslandIndicesBuffer;

            public DistanceFromIslandJob(MapSettings mapSettings, NativeArray<int> activeLayer, NativeArray<int> islandLayers, NativeList<int> islandBuffer)
            {
                jPointsPerAxis = mapSettings.mapPointPerAxis;
                jIslandLayers = islandLayers;
                jIslandIndicesBuffer = islandBuffer.AsParallelWriter();
                jActiveLayer = activeLayer;
            }
            
            public void Execute(int index)
            {
                int2 coords = KwGrid.GetXY2(jActiveLayer[index], jPointsPerAxis);
                NativeList<int> neighbors = new NativeList<int>(4,Allocator.Temp);
                GetNeighborVertices(in coords, ref neighbors);
                
                if (neighbors.IsEmpty) return;
                for (int i = 0; i < neighbors.Length; i++)
                {
                    jIslandIndicesBuffer.AddNoResize(neighbors[i]);
                }
            }

            private void GetNeighborVertices(in int2 coord, ref NativeList<int> neighbors)
            {
                int leftId = KwGrid.GetLeftIndex(coord, jPointsPerAxis);
                int rightId = KwGrid.GetRightIndex(coord, jPointsPerAxis);
                int topId = KwGrid.GetTopIndex(coord, jPointsPerAxis);
                int bottomId = KwGrid.GetBottomIndex(coord, jPointsPerAxis);
                
                if(leftId != -1 && jIslandLayers[leftId] == -1) 
                    neighbors.Add(leftId);
                if(rightId != -1 && jIslandLayers[rightId] == -1) 
                    neighbors.Add(rightId);
                if(topId != -1 && jIslandLayers[topId] == -1) 
                    neighbors.Add(topId);
                if(bottomId != -1 && jIslandLayers[bottomId] == -1) 
                    neighbors.Add(bottomId);
            }
        }
    }
}