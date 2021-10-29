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
    //=====================================================================
    // JOB SYSTEM
    //=====================================================================
    
    //VERTICES POSITION
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct VerticesPosJob : IJobFor
    {
        [ReadOnly] public int JSize;
        [ReadOnly] public int JPointPerAxis;
        [ReadOnly] public float JSpacing;

        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> JVertices;
        public void Execute(int index)
        {
            int z = (int)floor(index / (float)JPointPerAxis);
            int x = index - (z * JPointPerAxis);
            
            float3 pointPosition = float3(x, 0, z) * float3(JSpacing);
            JVertices[index] = pointPosition;
        }
    }
    
    //UVS
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct UvsJob : IJobFor
    {
        [ReadOnly] public int JMapPointPerAxis;
        [NativeDisableParallelForRestriction] [WriteOnly] public NativeArray<float2> JUvs;
        public void Execute(int index)
        {
            float z = floor((float)index / JMapPointPerAxis);
            float x = index - (z * JMapPointPerAxis);
            JUvs[index] = float2(x / JMapPointPerAxis, z / JMapPointPerAxis);
        }
    }
    
    //TRIANGLES
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct TrianglesJob : IJobFor
    {
        [ReadOnly] public int JMapPointPerAxis;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<int> JTriangles;
        public void Execute(int index)
        {
            int mapPoints = JMapPointPerAxis - 1;
            
            int z = (int)floor((float)index / mapPoints);
            int x = index - (z * mapPoints);
            int baseTriIndex = index * 6;
            
            int vertexIndex = index + select(z,1 + z, x > mapPoints);
            int4 trianglesVertex = int4(vertexIndex, vertexIndex + JMapPointPerAxis + 1, vertexIndex + JMapPointPerAxis, vertexIndex + 1);

            JTriangles[baseTriIndex] = trianglesVertex.z;
            JTriangles[baseTriIndex + 1] = trianglesVertex.y;
            JTriangles[baseTriIndex + 2] = trianglesVertex.x;
            baseTriIndex += 3;
            JTriangles[baseTriIndex] = trianglesVertex.w;
            JTriangles[baseTriIndex + 1] = trianglesVertex.x;
            JTriangles[baseTriIndex + 2] = trianglesVertex.y;
        }
    }
    
    //APPLY NOISE
    //==================================================================================================================
    [BurstCompile(CompileSynchronously = true)]
    public struct ApplyNoiseJob : IJobFor
    {
        [ReadOnly] public NativeArray<float> JNoise;
        [NativeDisableParallelForRestriction] public NativeArray<float3> JVertices;
        public void Execute(int index)
        {
            float3 noisePos = float3(JVertices[index].x,JNoise[index], JVertices[index].z);
            JVertices[index] = noisePos;
        }
    }
}