using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration.Data
{
    [Serializable]
    public class SaveMapSettings
    {
        //FIELDS
        //==============================================================================================================
        //MapSettings
        public int chunkSize;
        public int numChunk;
        public int pointPerMeter;

        //NoiseSettings
        public int octaves;
        public float scale;
        public float persistence;
        public float lacunarity;
        public float heightMultiplier;
        public float2 offset;
        
        //General Settings
        public uint seed;
        public string saveName;

        //CONSTRUCTOR
        //==============================================================================================================
        public SaveMapSettings(GeneralSettingsInputs gInputs = default, MapSettingsInputs mInputs = default, NoiseSettingsInputs nInputs = default)
        {
            saveName = gInputs.saveName;
            seed = gInputs.seed;
            chunkSize = mInputs.chunkSize;
            numChunk = mInputs.numChunk;
            pointPerMeter = mInputs.pointPerMeter;
            octaves = nInputs.octaves;
            scale = nInputs.scale;
            persistence = nInputs.persistence;
            lacunarity = nInputs.lacunarity;
            heightMultiplier = nInputs.heightMultiplier;
            offset = nInputs.offset;
        }
        
        public SaveMapSettings(GeneralMapSettings gInputs, MapSettings mInputs, NoiseSettings nInputs)
        {
            saveName = gInputs.saveName;
            seed = gInputs.seed;
            chunkSize = mInputs.chunkSize;
            numChunk = mInputs.numChunk;
            pointPerMeter = mInputs.pointPerMeter;
            octaves = nInputs.octaves;
            scale = nInputs.scale;
            persistence = nInputs.persistence;
            lacunarity = nInputs.lacunarity;
            heightMultiplier = nInputs.heightMultiplier;
            offset = nInputs.offset;
        }
    }
}