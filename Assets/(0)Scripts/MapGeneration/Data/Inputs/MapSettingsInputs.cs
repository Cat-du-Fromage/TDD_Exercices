using System;

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
        
        //CONSTRUCTOR
        //==============================================================================================================
        public MapSettingsInputs(int chunkSize, int numChunk, int pointPerMeter)
        {
            this.chunkSize = chunkSize;
            this.numChunk = numChunk;
            this.pointPerMeter = pointPerMeter;
        }
        
        //DEFAULT VALUE
        //==============================================================================================================
        public static readonly MapSettingsInputs Default = new MapSettingsInputs(10, 10, 2);
        
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