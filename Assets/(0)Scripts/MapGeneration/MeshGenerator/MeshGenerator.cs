using System;
using System.Collections.Generic;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.PersistentData;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Playables;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;
using static Unity.Mathematics.float3;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

namespace KaizerWaldCode.MapGeneration
{
    public static class MeshGenerator
    {
        public static Mesh GetTerrainMesh(GeneralMapSettings gMapSettings, MapSettings mapSettings, NoiseSettings noiseSettings)
        {
            Mesh mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
                name = "MapTerrain"
            };
            mesh.SetVertices(ReinterpretArray<float3, Vector3>(ApplyNoise(gMapSettings, mapSettings, noiseSettings)));
            mesh.SetTriangles(GetTriangles(mapSettings),0);
            mesh.SetUVs(0, ReinterpretArray<float2, Vector2>(GetUvs(mapSettings)));
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
        
        //=====================================================================
        // GET DATA
        //=====================================================================
        
        private static float3[] GetVertices(MapSettings mapSettings, JobHandle dependency = default)
        {
            using NativeArray<float3> verticesTemp = AllocNtvAry<float3>(mapSettings.totalMapPoints);
            VerticesPosJob job = new VerticesPosJob
            {
                JSize = mapSettings.mapSize,
                JPointPerAxis = mapSettings.mapPointPerAxis,
                JSpacing = mapSettings.pointSpacing,
                JVertices = verticesTemp,
            };
            JobHandle jobHandle = job.ScheduleParallel(mapSettings.totalMapPoints, JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            return verticesTemp.ToArray();
        }
        
        private static float2[] GetUvs(MapSettings mapSettings, JobHandle dependency = default)
        {
            using NativeArray<float2> uvsTemp = AllocNtvAry<float2>(mapSettings.totalMapPoints);
            UvsJob job = new UvsJob
            {
                JMapPointPerAxis = mapSettings.mapPointPerAxis,
                JUvs = uvsTemp
            };
            JobHandle jobHandle = job.ScheduleParallel(mapSettings.totalMapPoints, JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            return uvsTemp.ToArray();
        }
        
        private static int[] GetTriangles(MapSettings mapSettings, JobHandle dependency = default)
        {
            int trianglesBufferSize = sq(mapSettings.mapPointPerAxis - 1) * 6;
            using NativeArray<int> trianglesTemp = AllocNtvAry<int>(trianglesBufferSize);
            TrianglesJob job = new TrianglesJob
            {
                JMapPointPerAxis = mapSettings.mapPointPerAxis,
                JTriangles = trianglesTemp
            };
            JobHandle jobHandle = job.ScheduleParallel(sq(mapSettings.mapPointPerAxis-1), JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            return trianglesTemp.ToArray();
        }
        
        private static float3[] ApplyNoise(GeneralMapSettings gMapSettings, MapSettings mapSettings, NoiseSettings noiseSettings)
        {
            using NativeArray<float> noiseTemp = ArrayToNativeArray<float>(Noise.GetNoiseMap(gMapSettings,mapSettings,noiseSettings));
            using NativeArray<float3> verticesTemp = ArrayToNativeArray<float3>(GetVertices(mapSettings));
            ApplyNoiseJob job = new ApplyNoiseJob
            {
                JNoise = noiseTemp,
                JVertices = verticesTemp
            };
            JobHandle jobHandle = job.ScheduleParallel(mapSettings.totalMapPoints, JobsUtility.JobWorkerCount - 1, default);
            jobHandle.Complete();
            return verticesTemp.ToArray();
        }
        
    }
}