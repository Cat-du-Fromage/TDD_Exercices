using System;
using System.Collections.Generic;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.PersistentData;
using KaizerWaldCode.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Playables;
using UnityEngine.Rendering;


using static Unity.Mathematics.math;
using static Unity.Mathematics.float3;
using static KaizerWaldCode.Utils.KWmath;
using static KaizerWaldCode.Utils.NativeCollectionUtils;

namespace KaizerWaldCode.MapGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider colliderMesh;
        
        [SerializeField] private MapSettings mapSettings;
        [SerializeField] private NoiseSettings noiseSettings;

        [SerializeField] private GeneralMapSettings generalMapSettings;

        private SaveMapSettings saveMapSettings;
        public MapSettings GetMapSettings() => mapSettings;
        public GeneralMapSettings GetGeneralMapSettings() => generalMapSettings;
        public NoiseSettings GetNoiseSettings() => noiseSettings;
        
        public void NewGameSettings(GeneralSettingsInputs generalInputs, MapSettingsInputs mapInputs, NoiseSettingsInputs noiseInputs, TerrainType[] regions)
        {
            generalMapSettings.NewGame(generalInputs);
            mapSettings.NewGame(mapInputs);
            noiseSettings.NewGame(noiseInputs);
            SetPositionToZero();
            meshRenderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromHeightMap(mapSettings, regions,Noise.GetNoiseMap(generalMapSettings, mapSettings, noiseSettings));
            SetMesh();
            SaveSettings(generalMapSettings, mapSettings, noiseSettings);
        }

        private void SaveSettings(GeneralMapSettings generalInputs, MapSettings mapInputs, NoiseSettings noiseInputs)
        {
            saveMapSettings = new SaveMapSettings(generalInputs, mapInputs, noiseInputs);
            JsonHelper.ToJson(saveMapSettings, MapSaveDirectory.Instance(generalMapSettings.saveName).GetOrCreateMapSettings().FullName);
        }

        public void SetPositionToZero() => gameObject.transform.position = Vector3.zero - new Vector3(mapSettings.mapSize/2f,2,mapSettings.mapSize/2f);

        private void SetMesh()
        {
            meshFilter.sharedMesh = colliderMesh.sharedMesh = MeshGenerator.GetTerrainMesh(generalMapSettings, mapSettings, noiseSettings);
        }

    }
}