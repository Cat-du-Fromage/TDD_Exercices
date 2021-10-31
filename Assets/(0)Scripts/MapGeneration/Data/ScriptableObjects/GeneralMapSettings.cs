using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.MapGeneration.Data.Interface;
using Unity.Mathematics;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration.Data
{
    [CreateAssetMenu(fileName = "GeneralMapData", menuName = "MapGeneration/GeneralMapSettings")]
    public class GeneralMapSettings : ScriptableObject, ISettings<GeneralSettingsInputs>
    {
        //FIELDS
        //==============================================================================================================
        public uint seed;
        public string saveName;
        
        //NEW GAME
        //==============================================================================================================
        public void NewGame(GeneralSettingsInputs data)
        {
            seed = data.seed;
            saveName =  data.saveName;
            CheckValues();
        }
        
        //CHECK VALUES
        //==============================================================================================================
        public void CheckValues()
        {
            seed = math.max(1,seed);
            saveName = saveName == string.Empty ? "DefaultSaveName" : saveName;
        }
    }
}