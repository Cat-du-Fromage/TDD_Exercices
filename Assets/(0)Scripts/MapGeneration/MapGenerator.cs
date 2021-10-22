using System;
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
        private MapSettings mapSettings;

        public float3[] vertices { get; private set; }
        public float2[] uvs{ get; private set; }
        public int[] triangles{ get; private set; }

        [SerializeField]private Material testMaterial;

        public void Initialize(MapSettings mapSettings)
        {
            this.mapSettings ??= mapSettings;
            vertices = new float3[mapSettings.TotalMapPoints];
            uvs = new float2[mapSettings.TotalMapPoints];
            triangles = new int[sq(mapSettings.MapPointPerAxis-1) * 6];
            
            GetVertices();
            GetUvs();
            GetTriangles();
            
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = NewMeshAPITest();
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = testMaterial;
        }
        
        public void GetVertices(in JobHandle dependency = new JobHandle())
        {
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
        
        public void GetUvs(in JobHandle dependency = new JobHandle())
        {
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
        
        public void GetTriangles(in JobHandle dependency = new JobHandle())
        {
            using NativeArray<int> trianglesTemp = AllocNtvAry<int>(sq(mapSettings.MapPointPerAxis-1) * 6);
            TrianglesJob job = new TrianglesJob
            {
                JMapPointPerAxis = mapSettings.MapPointPerAxis,
                JTriangles = trianglesTemp
            };
            JobHandle jobHandle = job.ScheduleParallel(sq(mapSettings.MapPointPerAxis-1), JobsUtility.JobWorkerCount - 1, dependency);
            jobHandle.Complete();
            trianglesTemp.CopyTo(triangles);
        }
        
        //=====================================================================
        // MESH CREATION
        //=====================================================================
        
        private Mesh NewMeshAPITest()
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.name = $"TerrainMap";
                
            using NativeArray<float3> verticesBuffer = new NativeArray<float3>(mapSettings.TotalMapPoints, Allocator.Temp);
            using NativeArray<float2> uvsBuffer = new NativeArray<float2>(mapSettings.TotalMapPoints, Allocator.Temp);
            using NativeArray<int> trianglesBuffer = new NativeArray<int>(sq(mapSettings.MapPointPerAxis-1) * 6, Allocator.Temp);
            
            verticesBuffer.CopyFrom(vertices);
            uvsBuffer.CopyFrom(uvs);
            trianglesBuffer.CopyFrom(triangles);
            
            VertexAttributeDescriptor[] layout = new VertexAttributeDescriptor[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                //new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                //new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.UNorm8, 4),
            };
            mesh.SetVertexBufferParams(verticesBuffer.Length, layout);
            mesh.SetVertexBufferData(verticesBuffer, 0, 0, verticesBuffer.Length, 0);
            mesh.SetIndexBufferParams(triangles.Length, IndexFormat.UInt32);
            mesh.SetIndexBufferData(trianglesBuffer, 0, 0, trianglesBuffer.Length);
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor() 
            {
                baseVertex = 0,
                bounds = default,
                indexStart = 0,
                indexCount = trianglesBuffer.Length,
                firstVertex = 0,
                topology = MeshTopology.Triangles,
                vertexCount = verticesBuffer.Length
            });
            
            mesh.UploadMeshData(false);
            mesh.SetUVs(0, uvsBuffer);
            mesh.RecalculateBounds();
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
                int z = (int)floor((float)index / JMapPointPerAxis);
                int x = index - (z * JMapPointPerAxis);
                int baseTriIndex = index * 6;

                if (z > (JMapPointPerAxis - 1) || x > (JMapPointPerAxis - 1)) return;

                int4 trianglesVertex = int4(index, index + JMapPointPerAxis + 1, index + JMapPointPerAxis, index + 1);

                JTriangles[baseTriIndex] = trianglesVertex.z;
                JTriangles[baseTriIndex + 1] = trianglesVertex.y;
                JTriangles[baseTriIndex + 2] = trianglesVertex.x;
                baseTriIndex += 3;
                JTriangles[baseTriIndex] = trianglesVertex.w;
                JTriangles[baseTriIndex + 1] = trianglesVertex.x;
                JTriangles[baseTriIndex + 2] = trianglesVertex.y;
            }
        }
    }
}