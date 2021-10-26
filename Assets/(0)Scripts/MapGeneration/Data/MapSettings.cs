using System;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration.Data
{
    [CreateAssetMenu(fileName = "MapData", menuName = "MapGeneration/MapSettings")]
    public class MapSettings : ScriptableObject
    {
        //Inputs
        public int ChunkSize = 10;
        public int NumChunk = 4;
        public int PointPerMeter = 2;
        public uint Seed = 1;

        //Calculated Ones
        public int MapSize;
        public int ChunkPointPerAxis;
        public int MapPointPerAxis;
        public float PointSpacing;

        public int TotalChunkPoints;
        public int TotalMapPoints;
        
        //NAME
        public string SaveName = "DefaultSaveName";

        private void OnValidate()
        {
            CheckValues();
            CalculateProperties();
        }

        public void NewGame(MapSettingsInputs mapInputs, string saveName = "DefaultSaveName")
        {
            SaveName = saveName;
            ChunkSize = mapInputs.chunkSize;
            NumChunk = mapInputs.numChunk;
            PointPerMeter = mapInputs.pointPerMeter;
            Seed = mapInputs.seed;
            CheckValues();
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

        private void CheckValues()
        {
            ChunkSize = math.max(1,ChunkSize);
            NumChunk = math.max(1,NumChunk);
            PointPerMeter = math.max(2,PointPerMeter);
            Seed = math.max(1,Seed);
        }
    }
}