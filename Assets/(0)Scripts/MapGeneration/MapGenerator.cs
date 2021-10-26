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
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private BoxCollider colliderMesh;
        
        [SerializeField] private MapSettings mapSettings;
        [SerializeField] private NoiseSettings noiseSettings;

        private GeneralMapSettings generalMapSettings;

        public string currentSaveName { get; private set; }
        
        public float3[] vertices { get; private set; }
        public float2[] uvs { get; private set; }
        public int[] triangles { get; private set; }

        public float[] noiseMap{ get; private set; }

        public void NewGameSettings(MapSettingsInputs mapInputs, NoiseSettingsInputs noiseInputs, string saveName = "DefaultSaveName")
        {
            currentSaveName = saveName;
            
            mapSettings.NewGame(mapInputs, saveName);
            noiseSettings.NewGame(noiseInputs);
            Initialize();
            SaveSettings(saveName, mapInputs);
        }

        private void SaveSettings(string saveName, MapSettingsInputs mapInputs)
        {
            
            generalMapSettings = new GeneralMapSettings
            {
                seed = 1,
                saveName = saveName,
                mapSettings = this.mapSettings,
                noiseSettings = this.noiseSettings
            };
            Debug.Log(JsonUtility.ToJson(mapInputs));
            //JsonHelper.ToJson<MapSettingsInputs>(mapInputs, MainSaveDirectory.Instance.GetMainSavePath());
        }

        public void Initialize()
        {
            noiseMap = Noise.GetNoiseMap(mapSettings, noiseSettings);
            gameObject.transform.position = Vector3.zero - new Vector3(mapSettings.MapSize/2f,2,mapSettings.MapSize/2f);
            
            GetVertices();
            GetUvs();
            GetTriangles();
            //ApplyNoise();
            SetMesh();
        }
        
        
        

        private void SetMesh()
        {
            Mesh sharedMesh = meshFilter.sharedMesh = CreateMesh();
            colliderMesh.center = sharedMesh.bounds.center;
            colliderMesh.size = sharedMesh.bounds.size;
        }
/*
        private void TestMethod()
        {
            Func<JobHandle,JobHandle> testFunc;
            Action testAction;
            List<Func<JobHandle,JobHandle>> jobList = new List<Func<JobHandle,JobHandle>>();
            //jobList.Add(GetVertices);
        }
        */
        //=====================================================================
        // GET DATAS
        //=====================================================================
        
        public void GetVertices(JobHandle dependency = default)
        {
            vertices = new float3[mapSettings.TotalMapPoints];
            using NativeArray<float3> verticesTemp = AllocNtvAry<float3>(mapSettings.TotalMapPoints);
            VerticesPosJob job = new VerticesPosJob
            {
                JSize = mapSettings.MapSize,
                JPointPerAxis = mapSettings.MapPointPerAxis,
                JSpacing = mapSettings.PointSpacing,
                JVertices = verticesTemp,
            };
            JobHandle jobHandle = job.ScheduleParallel(mapSettings.TotalMapPoints, JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            verticesTemp.CopyTo(vertices);
        }
        
        public void GetUvs(JobHandle dependency = default)
        {
            uvs = new float2[mapSettings.TotalMapPoints];
            using NativeArray<float2> uvsTemp = AllocNtvAry<float2>(mapSettings.TotalMapPoints);
            UvsJob job = new UvsJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JUvs = uvsTemp
            };
            JobHandle jobHandle = job.ScheduleParallel(mapSettings.TotalMapPoints, JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            uvsTemp.CopyTo(uvs);
        }
        
        public void GetTriangles(JobHandle dependency = default)
        {
            int trianglesBufferSize = sq(mapSettings.MapPointPerAxis - 1) * 6;
            triangles = new int[trianglesBufferSize];
            using NativeArray<int> trianglesTemp = AllocNtvAry<int>(trianglesBufferSize);
            TrianglesJob job = new TrianglesJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JTriangles = trianglesTemp
            };
            JobHandle jobHandle = job.ScheduleParallel(sq(mapSettings.MapPointPerAxis-1), JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            trianglesTemp.CopyTo(triangles);
        }

        public void ApplyNoise()
        {
            using NativeArray<float> noiseTemp = ArrayToNativeArray<float>(noiseMap);
            using NativeArray<float3> verticesTemp = ArrayToNativeArray<float3>(vertices);
            ApplyNoiseJob job = new ApplyNoiseJob
            {
                JNoise = noiseTemp,
                JVertices = verticesTemp
            };
            JobHandle jobHandle = job.ScheduleParallel(mapSettings.TotalMapPoints, JobsUtility.JobWorkerCount - 1, default);
            jobHandle.Complete();
            verticesTemp.CopyTo(vertices);
        }
        
        //=====================================================================
        // MESH CREATION
        //=====================================================================

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
                name = "MapTerrain"
            };
            mesh.SetVertices(ReinterpretArray<float3, Vector3>(vertices));
            mesh.SetTriangles(triangles,0);
            mesh.SetUVs(0, ReinterpretArray<float2, Vector2>(uvs));
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
        
        //=====================================================================
        // JOB SYSTEM
        //=====================================================================
        
        /// <summary>
        /// Process Vertices Positions
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        private struct VerticesPosJob : IJobFor
        {
            //MapSettings
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
        
        /// <summary>
        /// Calculate Uvs
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        private struct UvsJob : IJobFor
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
        
        /// <summary>
        /// Calculate Triangles
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        private struct TrianglesJob : IJobFor
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
        
        /// <summary>
        /// Apply Noise
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        private struct ApplyNoiseJob : IJobFor
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
}