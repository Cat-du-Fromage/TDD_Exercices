using System;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration.Data
{
    [Serializable]
    public struct MapSettingsInputs
    {
        //FIELDS
        //==============================================================================================================
        public int chunkSize;
        public int numChunk;
        public int pointPerMeter;

        public AnimationCurve meshHeightCurve;
        //CONSTRUCTOR
        //==============================================================================================================
        public MapSettingsInputs(int chunkSize, int numChunk, int pointPerMeter, AnimationCurve meshHeightCurve = default)
        {
            this.chunkSize = chunkSize;
            this.numChunk = numChunk;
            this.pointPerMeter = pointPerMeter;
            this.meshHeightCurve = meshHeightCurve;
        }
        
        //DEFAULT VALUE
        //==============================================================================================================
        public static readonly MapSettingsInputs Default = new MapSettingsInputs(10, 10, 2, default);
        
        //CHECK VALUES
        //==============================================================================================================
        public void CheckValues()
        {
            chunkSize = math.max(1,chunkSize);
            numChunk = math.max(1,numChunk);
            pointPerMeter = math.clamp(pointPerMeter,2,10);
        }
        
        //IMPLICIT CONVERSION FROM SAVE-MAP-SETTINGS
        //==============================================================================================================
        public static implicit operator MapSettingsInputs(SaveMapSettings data)
        {
            return new MapSettingsInputs()
            {
                chunkSize = data.chunkSize,
                numChunk = data.numChunk,
                pointPerMeter = data.pointPerMeter,
            };
        }
    }
}