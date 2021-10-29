using System;
using Unity.Mathematics;

namespace KaizerWaldCode.MapGeneration.Data
{
    [Serializable]
    public struct NoiseSettingsInputs
    {
        //FIELDS
        //==============================================================================================================
        public int octaves;
        public float scale;
        public float persistence;
        public float lacunarity;
        public float heightMultiplier;
        public float2 offset;
        
        //CONSTRUCTOR
        //==============================================================================================================
        public NoiseSettingsInputs(int octaves, float scale, float persistence, float lacunarity, float heightMultiplier, float2 offset)
        {
            this.octaves = octaves;
            this.scale = scale;
            this.persistence = persistence;
            this.lacunarity = lacunarity;
            this.heightMultiplier = heightMultiplier;
            this.offset = offset;
        }
        
        //DEFAULT VALUE
        //==============================================================================================================
        public static readonly NoiseSettingsInputs Default = 
            new NoiseSettingsInputs(4, 20f, 0.5f, 2f, 2f, float2.zero);

        //IMPLICIT CONVERSION FROM SAVE-MAP-SETTINGS
        //==============================================================================================================
        public static implicit operator NoiseSettingsInputs(SaveMapSettings data)
        {
            return new NoiseSettingsInputs
            {
                octaves = data.octaves,
                scale = data.scale,
                persistence = data.persistence,
                lacunarity = data.lacunarity,
                heightMultiplier = data.heightMultiplier,
                offset = data.offset
            };
        }
    }
}