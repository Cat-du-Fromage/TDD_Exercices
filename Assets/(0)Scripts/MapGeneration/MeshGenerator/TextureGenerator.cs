using System;
using KaizerWaldCode.MapGeneration.Data;
using UnityEngine;
using Unity.Mathematics;
using KaizerWaldCode.Utils;
using Unity.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

using static Unity.Mathematics.math;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

namespace KaizerWaldCode.MapGeneration
{
    public static class TextureGenerator
    {
        public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point; //just fill the triangles / trillinear/Billinear = blend with surrounding cube
            texture.wrapMode = TextureWrapMode.Clamp; // what happen when we go outside of the limits? (in this case stretch value continu as if it was part of the range)
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }

        public static Texture2D TextureFromHeightMap(MapSettings mapSettings, TerrainType[] terrains, float[] heightMap)
        {
            //====================================
            //COLOR IS DEFINED HERE
            //====================================
            using NativeArray<Color> colorMap = AllocNtvAry<Color>(mapSettings.totalMapPoints);
            (NativeArray<float> terrainsHeights, NativeArray<Color> terrainsColor) = GetArraysTerrains(terrains);
            using NativeArray<float> noiseMap = ArrayToNativeArray(heightMap);
            
            Texture2DJob textureJob = new Texture2DJob(terrainsHeights,terrainsColor, noiseMap, colorMap);
            JobHandle jobHandle = textureJob.ScheduleParallel(mapSettings.totalMapPoints, JobsUtility.JobWorkerCount - 1, default);
            jobHandle.Complete();
            
            //====================================
            terrainsHeights.Dispose();
            terrainsColor.Dispose();
            return TextureFromColourMap(colorMap.ToArray(), mapSettings.mapPointPerAxis, mapSettings.mapPointPerAxis);
            
        }

        private static (NativeArray<float>, NativeArray<Color>) GetArraysTerrains(TerrainType[] terrains)
        {
            NativeArray<float> terrainHeights = AllocNtvAry<float>(terrains.Length);
            NativeArray<Color> terrainColor = AllocNtvAry<Color>(terrains.Length);
            for (int i = 0; i < terrains.Length; i++)
            {
                terrainHeights[i] = terrains[i].height;
                terrainColor[i] = terrains[i].colour;
            }

            return (terrainHeights, terrainColor);
        }
        
        // FALLOFF
        //==============================================================================================================
        [BurstCompile(CompileSynchronously = true)]
        private struct Texture2DJob : IJobFor
        {
            [ReadOnly] private NativeArray<float> JTerrainsHeight;
            [ReadOnly] private NativeArray<Color> JTerrainsColor;
            [ReadOnly] private NativeArray<float> JNoiseMap;
            [NativeDisableParallelForRestriction]
            [WriteOnly] private NativeArray<Color> JColorMap;

            public Texture2DJob(NativeArray<float> terrainsHeight, NativeArray<Color> terrainsColor, NativeArray<float> noiseMap, NativeArray<Color> colorMap)
            {
                JTerrainsHeight = terrainsHeight;
                JTerrainsColor = terrainsColor;
                JNoiseMap = noiseMap;
                JColorMap = colorMap;
            }
            
            public void Execute(int index)
            {
                for(int i = 0; i < JTerrainsHeight.Length; i++)
                {
                    if(JNoiseMap[index] <= JTerrainsHeight[i])
                    {
                        JColorMap[index] = JTerrainsColor[i];
                        break;
                    }
                }
            }
        }
    }
    
    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }
}