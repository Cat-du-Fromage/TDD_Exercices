using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.noise;
using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;
using Random = Unity.Mathematics.Random;

namespace KaizerWaldCode.MapGeneration
{
    public static class Noise
    {
        public static float[] GetNoiseMap(MapSettings mapSettings, NoiseSettings noiseSettings, JobHandle dependency = default)
        {
            using NativeArray<float2> noiseOffsetsMap = AllocNtvAry<float2>(noiseSettings.Octaves);
            OffsetNoiseRandomJob offsetsNoiseJ = new OffsetNoiseRandomJob
            {
                JSeed = mapSettings.Seed,
                JOffset = noiseSettings.Offset,
                JOctavesOffset = noiseOffsetsMap,
            };
            JobHandle offsetsJH = offsetsNoiseJ.ScheduleParallel(noiseSettings.Octaves,JobsUtility.JobWorkerCount - 1, dependency);
            //=================
            //PERLIN NOISE
            //=================
            using NativeArray<float> perlinNoiseMap = AllocNtvAry<float>(mapSettings.TotalMapPoints);
            
            PerlinNoiseJob perlinNoiseJ = new PerlinNoiseJob
            {
                JNumPointPerAxis = mapSettings.MapPointPerAxis,
                JOctaves = noiseSettings.Octaves,
                JLacunarity = noiseSettings.Lacunarity,
                JPersistence = noiseSettings.Persistence,
                JScale = noiseSettings.Scale,
                JHeightMul = noiseSettings.HeightMultiplier,
                JOctOffsetArray = noiseOffsetsMap,
                JNoiseMap = perlinNoiseMap,
            };
            JobHandle perlinNoiseJH = perlinNoiseJ.ScheduleParallel(mapSettings.TotalMapPoints,JobsUtility.JobWorkerCount - 1, offsetsJH);
            perlinNoiseJH.Complete();
            return perlinNoiseMap.ToArray();
        }

        //=====================================================================
        // JOB SYSTEM
        //=====================================================================
        
        /// <summary>
        /// Process RandomJob
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        private struct OffsetNoiseRandomJob : IJobFor
        {
            [ReadOnly] public uint JSeed;
            [ReadOnly] public float2 JOffset;
            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<float2> JOctavesOffset;
            public void Execute(int index)
            {
                Random prng = Random.CreateFromIndex(JSeed + (uint)index);
                float offsetX = prng.NextFloat(-100000f, 100000f) + JOffset.x;
                float offsetY = prng.NextFloat(-100000f, 100000f) - JOffset.y;
                JOctavesOffset[index] = float2(offsetX, offsetY);
            }
        }
        
        /// <summary>
        /// Noise Height Map
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        private struct PerlinNoiseJob : IJobFor
        {
            [ReadOnly] public int JNumPointPerAxis;
            [ReadOnly] public int JOctaves;
            [ReadOnly] public float JLacunarity;
            [ReadOnly] public float JPersistence;
            [ReadOnly] public float JScale;
            [ReadOnly] public float JHeightMul;
            [ReadOnly] public NativeArray<float2> JOctOffsetArray;
        
            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<float> JNoiseMap;

            public void Execute(int index)
            {
                float halfMapSize = mul(JNumPointPerAxis, 0.5f);

                int z = (int)floor((float)index / JNumPointPerAxis);
                int x = index - (z * JNumPointPerAxis);

                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0;
                //Not needed in parallel! it's a layering of noise so it must be done contigiously
                for (int i = 0; i < JOctaves; i++)
                {
                    float sampleX = mul((x - halfMapSize + JOctOffsetArray[i].x) / JScale, frequency);
                    float sampleY = mul((z - halfMapSize + JOctOffsetArray[i].y) / JScale, frequency);
                    float2 sampleXY = float2(sampleX, sampleY);

                    float pNoiseValue = snoise(sampleXY);
                    noiseHeight = mad(pNoiseValue, amplitude, noiseHeight);
                    amplitude = mul(amplitude, JPersistence);
                    frequency = mul(frequency, JLacunarity);
                }
                float noiseVal = noiseHeight;
                noiseVal = abs(noiseVal);
                JNoiseMap[index] = mul(noiseVal, max(1,JHeightMul));
            }
        }
    }
}