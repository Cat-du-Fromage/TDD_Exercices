#define DEBUG_EXAMPLE_ALGORITHM
using System.Collections.Generic;
//using System.Linq;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using VisualDebugging;

using Random = Unity.Mathematics.Random;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using static KaizerWaldCode.Utils.KwManagedContainerUtils;
using static KaizerWaldCode.Utils.SpatialPartitionUtils;
using NativeArrayExtensions = Unity.Collections.NativeArrayExtensions;

namespace KaizerWaldCode.MapGeneration
{
    public static class IslandMeshGenerator
    {
        public static void GetIslandLayers(MapSettings mapSettings, int[] islandIds, int[] verticesCellId, Vector3[] verticesPos)
        {
            using NativeArray<int> layers = AllocNtvAry<int>(mapSettings.totalMapPoints);
            using NativeArray<int> islands = islandIds.ToNativeArray(); //ONLY CELL ID not vertices!
            using NativeArray<int> vCellId = verticesCellId.ToNativeArray(); //Cell attribution to each vertices

            NativeArray<int> activeLayer;
            NativeList<int> islandBuffer;

            int[] temp;
            int numUniqueBufferValue;
            //FIRST LAYER
            //==========================================================================================================
            using (islandBuffer = new NativeList<int>(mapSettings.totalMapPoints, Allocator.TempJob))
            {
                FirstLayerIsland(mapSettings.totalMapPoints, layers, vCellId, islands, islandBuffer);
            
                activeLayer = AllocNtvAry<int>(islandBuffer.Length,Allocator.Persistent);
                activeLayer.CopyFrom(islandBuffer);
            }
            
            //OTHER LAYER
            //==========================================================================================================
            for (int i = 1; i < 10; i++)
            {
                using (islandBuffer = new NativeList<int>(mapSettings.totalMapPoints, Allocator.TempJob))
                {
                    OthersLayerIsland(mapSettings, layers, activeLayer, islandBuffer).Complete();
                    //get unique Val
                    HashSet<int> uniqueValBuffer = new HashSet<int>(islandBuffer.ToArray());
                    using NativeArray<int> layerToApply = uniqueValBuffer.ToArray().ToNativeArray();
                    
                    ApplyLayerIsland(i, layers, layerToApply).Complete();

                    activeLayer.Dispose();
                    //buffer reallocate to the activeLayer to be worked
                    activeLayer = AllocNtvAry<int>(layerToApply.Length);
                    activeLayer.CopyFrom(layerToApply);
                }
            }
            activeLayer.Dispose();
            
            /*
            //Need to return uniqueValBuffer.ToArray().ToNativeArray()
            using (islandBuffer = new NativeList<int>(mapSettings.totalMapPoints, Allocator.TempJob))
            {
                IslandLayersJob layersJob = new IslandLayersJob(mapSettings, activeLayer, layers, islandBuffer);
                JobHandle jh = layersJob.ScheduleParallel(activeLayer.Length, JobsUtility.JobWorkerCount - 1, default);
                jh.Complete();

                uniqueValBuffer = new HashSet<int>(islandBuffer.ToArray());
                numUniqueBufferValue = uniqueValBuffer.Count;
                temp = islandBuffer.ToArray();
            }
            Debug.Log($"HashSet Size ? : {numUniqueBufferValue}");
            //APPLY LAYER -> layers
            //==========================================================================================================
            using (NativeArray<int> layerToApply =  uniqueValBuffer.ToArray().ToNativeArray() )
            {
                ApplyLayerJob applyJob = new ApplyLayerJob(1, layerToApply,layers);
                JobHandle jh = applyJob.ScheduleParallel(numUniqueBufferValue, JobsUtility.JobWorkerCount - 1, default);
                jh.Complete();
                
            }
            
            //DISPOSE
            //==========================================================================================================
            */
            //temp = activeLayer.ToArray();
            //islandBuffer.Dispose();
            //if(activeLayer.IsCreated)
                //activeLayer.Dispose();
            
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
        
        private static JobHandle ApplyLayerIsland(int ilayer, NativeArray<int> layers, NativeArray<int> uniqueValBuffer)
        {
            ApplyLayerJob applyJob = new ApplyLayerJob(ilayer, uniqueValBuffer, layers);
            JobHandle jh = applyJob.ScheduleParallel(uniqueValBuffer.Length, JobsUtility.JobWorkerCount - 1, default);
            return jh;
        }
        
        private static JobHandle OthersLayerIsland(MapSettings mapSettings, NativeArray<int> layers, NativeArray<int> activeLayer, NativeList<int> buffer)
        {
            IslandLayersJob job = new IslandLayersJob(mapSettings, activeLayer, layers, buffer);
            //Get indices needed to change
            JobHandle jh = job.ScheduleParallel(activeLayer.Length, JobsUtility.JobWorkerCount - 1, default);
            return jh;
        }

        /// <summary>
        /// FIRST ISLAND LAYER
        /// </summary>
        /// <param name="totalMapPoints"></param>
        /// <param name="layers"></param>
        /// <param name="vCellId"></param>
        /// <param name="islands"></param>
        /// <param name="buffer"></param>
        private static void FirstLayerIsland(int totalMapPoints, NativeArray<int> layers, NativeArray<int> vCellId, NativeArray<int> islands, NativeList<int> buffer)
        {
            FirstLayerJob layerJob = new FirstLayerJob(layers, vCellId, islands, buffer);
            JobHandle jh = layerJob.ScheduleParallel(totalMapPoints, JobsUtility.JobWorkerCount - 1, default);
            jh.Complete();
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct ApplyLayerJob : IJobFor
        {
            [ReadOnly] private int jLayer;
            [ReadOnly] private NativeArray<int> jActiveLayer;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeArray<int> jIslandLayers;

            public ApplyLayerJob(int jLayer, NativeArray<int> jActiveLayer, NativeArray<int> jIslandLayers)
            {
                this.jLayer = jLayer;
                this.jActiveLayer = jActiveLayer;
                this.jIslandLayers = jIslandLayers;
            }

            public void Execute(int index)
            {
                //Set islands map at activeLayer to layer
                jIslandLayers[jActiveLayer[index]] = jLayer;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct FirstLayerJob : IJobFor
        {
            [ReadOnly]private NativeArray<int> jIslandId;
            [ReadOnly]private NativeArray<int> jVerticesCellId;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeArray<int> jIslandLayers;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeList<int>.ParallelWriter jBuffer;

            public FirstLayerJob(NativeArray<int> islandLayer, NativeArray<int> verticesCellId, NativeArray<int> islandId, NativeList<int> buffer)
            {
                jIslandLayers = islandLayer;
                jVerticesCellId = verticesCellId;
                jIslandId = islandId;
                jBuffer = buffer.AsParallelWriter();
            }

            public void Execute(int index)
            {
                bool found = NativeArrayExtensions.Contains(jIslandId, jVerticesCellId[index]);
                
                jIslandLayers[index] = select(-1,0,found);
                jBuffer.AddNoResizeIf(found,index);
            }
        }

        private struct IslandLayersJob : IJobFor
        {
            [ReadOnly] private int jPointsPerAxis;
            [ReadOnly] private NativeArray<int> jIslandLayers;
            [ReadOnly] private NativeArray<int> jActiveLayer;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeList<int>.ParallelWriter jIslandIndiceBuffer;

            public IslandLayersJob(MapSettings mapSettings, NativeArray<int> activeLayer, NativeArray<int> islandLayers, NativeList<int> islandBuffer)
            {
                jPointsPerAxis = mapSettings.mapPointPerAxis;
                jIslandLayers = islandLayers;
                jIslandIndiceBuffer = islandBuffer.AsParallelWriter();
                jActiveLayer = activeLayer;
            }
            
            public void Execute(int index)
            {
                int2 coords = KwGrid.GetXY2(jActiveLayer[index], jPointsPerAxis);
                NativeList<int> neighbors = new NativeList<int>(Allocator.Temp);
                GetNeighborVertices(coords, ref neighbors);
                
                if (neighbors.IsEmpty) return;
                for (int i = 0; i < neighbors.Length; i++)
                {
                    jIslandIndiceBuffer.AddNoResize(neighbors[i]);
                }
            }

            private void GetNeighborVertices(int2 coord, ref NativeList<int> neighbors)
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