using System;
using Unity.Mathematics;

namespace KaizerWaldCode.MapGeneration.Data
{
    [Serializable]
    public struct NoiseSettingsInputs
    {
        public int octaves;
        public float scale;
        public float persistence;
        public float lacunarity;
        public float heightMultiplier;
        public float2 offset;
    }
}