using System;

namespace KaizerWaldCode.MapGeneration.Data
{
    [Serializable]
    public struct MapSettingsInputs
    {
        public int chunkSize;
        public int numChunk;
        public int pointPerMeter;
        public uint seed;
    }
}