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
        public static float[] GetNoiseMap(GeneralMapSettings generalMapSettings, MapSettings mapSettings, NoiseSettings noiseSettings, JobHandle dependency = default)
        {
            //RANDOM OFFSETS
            //==========================================================================================================
            using NativeArray<float2> noiseOffsetsMap = AllocNtvAry<float2>(noiseSettings.octaves);
            
            OffsetNoiseRandomJob offsetsNoiseJ = new OffsetNoiseRandomJob(generalMapSettings.seed, noiseSettings.offset, noiseOffsetsMap);
            JobHandle offsetsJH = offsetsNoiseJ.ScheduleParallel(noiseSettings.octaves,JobsUtility.JobWorkerCount - 1, dependency);
            
            //PERLIN NOISE
            //==========================================================================================================
            using NativeArray<float> perlinNoiseMap = AllocNtvAry<float>(mapSettings.totalMapPoints);
            
            PerlinNoiseJob perlinNoiseJ = new PerlinNoiseJob(mapSettings.mapPointPerAxis, noiseSettings, noiseOffsetsMap, perlinNoiseMap);
            JobHandle perlinNoiseJH = perlinNoiseJ.ScheduleParallel(mapSettings.totalMapPoints,JobsUtility.JobWorkerCount - 1, offsetsJH);
            perlinNoiseJH.Complete();
            
            return perlinNoiseMap.ToArray();
        }
        
        //==============================================================================================================
        // JOB SYSTEM
        //==============================================================================================================
        
        // RANDOM OFFSETS
        //==============================================================================================================
        [BurstCompile(CompileSynchronously = true)]
        private struct OffsetNoiseRandomJob : IJobFor
        {
            [ReadOnly] private uint                 JSeed;
            [ReadOnly] private float2               JOffset;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeArray<float2> JOctavesOffset;

            public OffsetNoiseRandomJob(uint seed, float2 offset, NativeArray<float2> offsets)
            {
                JSeed          = seed;
                JOffset        = offset;
                JOctavesOffset = offsets;
            }
            
            public void Execute(int index)
            {
                Random prng = Random.CreateFromIndex(JSeed + (uint)index);
                float offsetX = prng.NextFloat(-100000f, 100000f) + JOffset.x;
                float offsetY = prng.NextFloat(-100000f, 100000f) - JOffset.y;
                JOctavesOffset[index] = float2(offsetX, offsetY);
            }
        }
        
        // NOISE/HEIGHT MAP
        //==============================================================================================================
        [BurstCompile(CompileSynchronously = true)]
        private struct PerlinNoiseJob : IJobFor
        {
            [ReadOnly] private int                 JNumPointPerAxis;
            [ReadOnly] private int                 JOctaves;
            [ReadOnly] private float               JLacunarity;
            [ReadOnly] private float               JPersistence;
            [ReadOnly] private float               JScale;
            [ReadOnly] private float               JHeightMul;
            [ReadOnly] private NativeArray<float2> JOctOffsetArray;
        
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeArray<float> JNoiseMap;

            public PerlinNoiseJob(int numPoints, NoiseSettings noiseSettings, NativeArray<float2> offsets, NativeArray<float> noiseMap)
            {
                JNumPointPerAxis = numPoints;
                JOctOffsetArray  = offsets;
                JNoiseMap        = noiseMap;
                JOctaves         = noiseSettings.octaves;
                JLacunarity      = noiseSettings.lacunarity;
                JPersistence     = noiseSettings.persistence;
                JScale           = noiseSettings.scale;
                JHeightMul       = noiseSettings.heightMultiplier;
            }

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