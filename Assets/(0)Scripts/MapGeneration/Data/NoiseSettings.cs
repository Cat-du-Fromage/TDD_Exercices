using System;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.float2;

namespace KaizerWaldCode.MapGeneration.Data
{
    [CreateAssetMenu(fileName = "NoiseData", menuName = "MapGeneration/NoiseSettings")]
    public class NoiseSettings : ScriptableObject
    {
        public int Octaves = 4;
        public float Scale = 20f;
        public float Persistence = 0.5f;
        public float Lacunarity = 2f;
        public float HeightMultiplier = 1.5f;
        public float2 Offset = float2.zero;
        
        private void OnValidate() => CheckValues();

        /// <summary>
        /// Use To Create a new game using the input entered in the interface
        /// </summary>
        /// <param name="data">inputs entered in the NewGame menu</param>
        public void NewGame(NoiseSettingsInputs data)
        {
            Octaves = data.octaves;
            Scale =  data.scale;
            Persistence = data.persistence;
            Lacunarity = data.lacunarity;
            HeightMultiplier = data.heightMultiplier;
            Offset = data.offset;
            CheckValues();
        }
        
        private void CheckValues()
        {
            Octaves = math.max(1,Octaves);
            Scale = math.max(0.001f,Scale);
            Persistence = math.clamp(Persistence,0,1f);
            Lacunarity = math.max(1f,Lacunarity);
            HeightMultiplier = math.max(1f,HeightMultiplier);
        }
        
        
    }
}