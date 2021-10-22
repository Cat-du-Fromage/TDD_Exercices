using System;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration
{
    public class MapSettings
    {
        //Inputs
        public int ChunkSize{ get; private set; }
        public int NumChunk{ get; private set; }
        public int PointPerMeter{ get; private set; }
        public uint Seed{ get; private set; }

        //Calculated Ones
        public int MapSize { get; private set; }
        public int ChunkPointPerAxis { get; private set; }
        public int MapPointPerAxis { get; private set; }
        public float PointSpacing { get; private set; }
        
        public int TotalChunkPoints { get; private set; }
        public int TotalMapPoints { get; private set; }
        
        //NAME
        public string SaveName{ get; private set; }

        public MapSettings(string saveName = "DefaultSaveName", int chunkSize = 10, int numChunk = 4, int pointPerMeter = 2, uint seed = 1)
        {
            SaveName = saveName;
            ChunkSize = chunkSize;
            NumChunk = numChunk;
            PointPerMeter = pointPerMeter;
            Seed = seed;
            
            CalculateProperties();
        }

        private void CalculateProperties()
        {
            MapSize = ChunkSize * NumChunk;
            PointSpacing = 1f / (PointPerMeter - 1f);
            ChunkPointPerAxis = PointPerMeter + ((ChunkSize - 1) * (PointPerMeter- 1));
            MapPointPerAxis = PointPerMeter + ((MapSize - 1) * (PointPerMeter- 1));
            TotalChunkPoints = ChunkPointPerAxis * ChunkPointPerAxis;
            TotalMapPoints = MapPointPerAxis * MapPointPerAxis;
        }
    }
}