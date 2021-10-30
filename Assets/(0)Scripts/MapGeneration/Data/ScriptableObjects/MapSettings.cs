using System;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration.Data
{
    [CreateAssetMenu(fileName = "MapData", menuName = "MapGeneration/MapSettings")]
    public class MapSettings : ScriptableObject
    {
        //Inputs
        public int chunkSize = 10;
        public int numChunk = 4;
        public int pointPerMeter = 2;
        //Calculated Ones
        public int mapSize;
        public int chunkPointPerAxis;
        public int mapPointPerAxis;
        public float pointSpacing;

        public int totalChunkPoints;
        public int totalMapPoints;

        public AnimationCurve meshHeightCurve;

        private void OnValidate()
        {
            CheckValues();
            CalculateProperties();
        }

        public void NewGame(MapSettingsInputs mapInputs)
        {
            chunkSize = mapInputs.chunkSize;
            numChunk = mapInputs.numChunk;
            pointPerMeter = mapInputs.pointPerMeter;
            meshHeightCurve = mapInputs.meshHeightCurve;
            CheckValues();
            CalculateProperties();
        }
        
        private void CalculateProperties()
        {
            mapSize = chunkSize * numChunk;
            pointSpacing = 1f / (pointPerMeter - 1f);
            chunkPointPerAxis = pointPerMeter + ((chunkSize - 1) * (pointPerMeter- 1));
            mapPointPerAxis = pointPerMeter + ((mapSize - 1) * (pointPerMeter- 1));
            totalChunkPoints = chunkPointPerAxis * chunkPointPerAxis;
            totalMapPoints = mapPointPerAxis * mapPointPerAxis;
        }

        private void CheckValues()
        {
            chunkSize = math.max(1,chunkSize);
            numChunk = math.max(1,numChunk);
            pointPerMeter = math.clamp(pointPerMeter,2,10);
        }
    }
}