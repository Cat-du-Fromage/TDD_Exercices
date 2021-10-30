using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration.Data
{
    [Serializable]
    public struct GeneralSettingsInputs
    {
        //FIELDS
        //==============================================================================================================
        public uint seed;
        public string saveName;
        
        //CONSTRUCTOR
        //==============================================================================================================
        public GeneralSettingsInputs(string saveName, uint seed)
        {
            this.seed = seed;
            this.saveName = saveName;
        }
        
        //DEFAULT VALUE
        //==============================================================================================================
        public static readonly GeneralSettingsInputs Default = 
            new GeneralSettingsInputs("DefaultSaveName", 1);
        
        //CHECK VALUES
        //==============================================================================================================
        public void CheckValues()
        {
            seed = math.max(1,seed);
            saveName = saveName == string.Empty ? "DefaultSaveName" : saveName;
        }

        //IMPLICIT CONVERSION FROM SAVE-MAP-SETTINGS
        //==============================================================================================================
        public static implicit operator GeneralSettingsInputs(SaveMapSettings data)
        {
            return new GeneralSettingsInputs
            {
                saveName = data.saveName,
                seed = data.seed
            };
        }
    }
}