using System;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.float2;

namespace KaizerWaldCode.MapGeneration.Data
{
    [CreateAssetMenu(fileName = "NoiseData", menuName = "MapGeneration/NoiseSettings")]
    public class NoiseSettings : ScriptableObject
    {
        //FIELDS
        //==============================================================================================================
        public int octaves = 4;
        public float scale = 20f;
        public float persistence = 0.5f;
        public float lacunarity = 2f;
        public float heightMultiplier = 1.5f;
        public float2 offset = float2.zero;
        
        private void OnValidate() => CheckValues();

        //NEW GAME
        //==============================================================================================================
        public void NewGame(NoiseSettingsInputs data)
        {
            octaves = data.octaves;
            scale =  data.scale;
            persistence = data.persistence;
            lacunarity = data.lacunarity;
            heightMultiplier = data.heightMultiplier;
            offset = data.offset;
            CheckValues();
        }
        
        //CHECK VALUES
        //==============================================================================================================
        private void CheckValues()
        {
            octaves = math.max(1,octaves);
            scale = math.max(0.001f,scale);
            persistence = math.clamp(persistence,0,1f);
            lacunarity = math.max(1f,lacunarity);
            heightMultiplier = math.max(1f,heightMultiplier);
        }
    }
}