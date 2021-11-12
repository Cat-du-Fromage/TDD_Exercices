//#define DEBUG_EXAMPLE_ALGORITHM
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
        public static Vector3[] ElevateIsland(in MapSettings mapSettings,in NoiseSettings noiseSettings, int[] islandIds, int[] verticesCellId, Vector3[] verticesPos)
        {
            (int[] layers, int maxLayer) = GetIslandDstLayers(in mapSettings, islandIds, verticesCellId, verticesPos);
            using NativeArray<float3> vertices = verticesPos.ReinterpretArray<Vector3, float3>().ToNativeArray();
            using NativeArray<int> layersNtvAry = layers.ToNativeArray();
            ElevateIslandJob job = new ElevateIslandJob(10f, maxLayer, layersNtvAry, vertices);
            JobHandle jh = job.ScheduleParallel(verticesPos.Length, JobsUtility.JobWorkerCount - 1, default);
            jh.Complete();

            float3[] temp = new float3[verticesPos.Length];
            vertices.CopyTo(temp);
            for (int i = 0; i < temp.Length; i++)
            {
                if (layersNtvAry[i] == 0) continue;
                float height = min(Evaluate(in noiseSettings, temp[i]), 1);
                Debug.Log($"height at {i} == {height}");
                temp[i] = float3(temp[i].x, height * temp[i].y, temp[i].z);
            }
            
            return temp.ReinterpretArray<float3, Vector3>();
        }
        
        private static float Evaluate(in NoiseSettings noiseSettings, float3 point)
        {
            float noiseValue = 0;
            float frequency = 1;
            float amplitude = 1;

            for (int i = 0; i < noiseSettings.octaves ; i++)
            {
                float v = noise.snoise(point * frequency);
                //float v = noise.Evaluate(point * frequency + settings.centre);
                noiseValue += (v + 1) * .5f * amplitude;
                frequency *= noiseSettings.lacunarity;
                amplitude *= noiseSettings.persistence;
            }

            //noiseValue *= 0.25f;
            return noiseValue * noiseSettings.heightMultiplier;
        }
        
        //==========================================================================================================
        /// <summary>
        /// Get the gris map with value of each vertices representing
        /// the distance from the vertices to the island
        /// </summary>
        /// <param name="mapSettings"></param>
        /// <param name="islandIds"></param>
        /// <param name="verticesCellId"></param>
        /// <param name="verticesPos"></param>
        public static (int[], int) GetIslandDstLayers(in MapSettings mapSettings, int[] islandIds, int[] verticesCellId, Vector3[] verticesPos)
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
            
            int layer = 0;
            while (layers.Contains(-1))
            {
                layer++;
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
            }
            activeLayer.Dispose();
            
            /*
            int layer = 0;
            for (layer = 1; layer < 40; layer++)
            {
                using (islandBuffer = new NativeList<int>(mapSettings.totalMapPoints, Allocator.TempJob))
                {
                    Debug.Log($"tot num vertices {mapSettings.totalMapPoints}");
                    GetDistancesFromIsland(in mapSettings, layers, activeLayer, islandBuffer).Complete();
                    if(layer == 1) Debug.Log($"BufferSize == {islandBuffer.Length}");
                    //also return buffer needed for next iteration -> buffer = next layer to work on(activeLayer)
                    activeLayer.Dispose();
                    
                    HashSet<int> uniqueValBuffer = new HashSet<int>(islandBuffer.ToArray()); //get unique Values
                    using NativeArray<int> layerToApply = uniqueValBuffer.ToNativeArray();
                    
                    ApplyLayerIsland(in layer, layers, layerToApply).Complete();

                    //buffer reallocate to the activeLayer to be worked
                    activeLayer = AllocNtvAry<int>(layerToApply.Length);
                    activeLayer.CopyFrom(layerToApply);
                }
            }
            activeLayer.Dispose();
            */
//#if DEBUG_EXAMPLE_ALGORITHM
/*
            VisualDebug.Clear();
            VisualDebug.Initialize();
            for (int i = 0; i < verticesPos.Length; i++)
            {
                VisualDebug.BeginFrame("Point Location", true);
                VisualDebug.SetColour(Colours.lightRed, Colours.veryDarkGrey);
                VisualDebug.DrawPointWithLabel(verticesPos[i], .05f, layers[i].ToString());
            }
            VisualDebug.Save();
           */ 
//#endif
            return (layers.ToArray(), layer);
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
        
        //JOB SYSTEM
        //==============================================================================================================
        [BurstCompile(CompileSynchronously = true)]
        private struct ElevateIslandJob : IJobFor
        {
            [ReadOnly] private float jIslandHeight;
            [ReadOnly] private int jMaxDistanceLayer;
            [ReadOnly] private NativeArray<int> jLayers;
            [NativeDisableParallelForRestriction]
            private NativeArray<float3> jVertices;

            public ElevateIslandJob(float jIslandHeight, int jMaxDistanceLayer, NativeArray<int> jLayers, NativeArray<float3> jVertices)
            {
                this.jIslandHeight = jIslandHeight;
                this.jLayers = jLayers;
                this.jMaxDistanceLayer = jMaxDistanceLayer;
                this.jVertices = jVertices;
            }

            public void Execute(int index)
            {
                if (jLayers[index] == 0)
                {
                    jVertices[index] = float3(jVertices[index].x, jIslandHeight, jVertices[index].z);
                    return;
                }
                float heightValue = remap(jMaxDistanceLayer,0,0,jIslandHeight,jLayers[index]);
                jVertices[index] = new float3(jVertices[index].x, heightValue, jVertices[index].z);
            }
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
                NativeList<int> neighbors = new NativeList<int>(8,Allocator.Temp);
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
                //Corners
                int topLeftId = KwGrid.GetTopLeftIndex(coord, jPointsPerAxis);
                int topRightId = KwGrid.GetTopRightIndex(coord, jPointsPerAxis);
                int bottomLeftId = KwGrid.GetBottomLeftIndex(coord, jPointsPerAxis);
                int bottomRightId = KwGrid.GetBottomRightIndex(coord, jPointsPerAxis);

                if(leftId != -1 && jIslandLayers[leftId] == -1)               neighbors.Add(leftId);
                if(rightId != -1 && jIslandLayers[rightId] == -1)             neighbors.Add(rightId);
                if(topId != -1 && jIslandLayers[topId] == -1)                 neighbors.Add(topId);
                if(bottomId != -1 && jIslandLayers[bottomId] == -1)           neighbors.Add(bottomId);
                //corners
                if(topLeftId != -1 && jIslandLayers[topLeftId] == -1)         neighbors.Add(topLeftId);
                if(topRightId != -1 && jIslandLayers[topRightId] == -1)       neighbors.Add(topRightId);
                if(bottomLeftId != -1 && jIslandLayers[bottomLeftId] == -1)   neighbors.Add(bottomLeftId);
                if(bottomRightId != -1 && jIslandLayers[bottomRightId] == -1) neighbors.Add(bottomRightId);
            }
        }
    }
}