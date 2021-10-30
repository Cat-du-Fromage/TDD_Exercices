using System;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.UI.MainMenu;
using Unity.Rendering;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration.EditorPreview
{
    public class MapPreview : MonoBehaviour
    {
        private enum GenerationType
        {
            Noise,
            CoastLine,
        }

        [SerializeField] private GenerationType generationType;
        
        //==============================================================================================================
        
        [SerializeField] private GeneralMapSettings generalMapSettings;
        [SerializeField] private MapSettings mapSettings;
        [SerializeField] private NoiseSettings noiseSettings;

        [SerializeField] private GeneralSettingsInputs gInputs;
        [SerializeField] private MapSettingsInputs mInputs;
        [SerializeField] private NoiseSettingsInputs nInputs;
        
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        
        public TerrainType[] regions;
        
        public MainMenuInteractions mainMenu;
        public bool autoUpdate;
        public bool syncUI;

        private void OnValidate()
        {
            gInputs.CheckValues();
            mInputs.CheckValues();
            nInputs.CheckValues();
        }

        public void DrawMapInEditor()
        {
            if (generationType == GenerationType.Noise)
            {
                generalMapSettings.NewGame(gInputs);
                mapSettings.NewGame(mInputs);
                noiseSettings.NewGame(nInputs);
                gameObject.transform.position = Vector3.zero - new Vector3(mapSettings.mapSize/2f,2,mapSettings.mapSize/2f);
                meshRenderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromHeightMap(mapSettings, regions,Noise.GetNoiseMap(generalMapSettings, mapSettings, noiseSettings));
                meshFilter.mesh = MeshGenerator.GetTerrainMesh(generalMapSettings, mapSettings, noiseSettings);
            }
        }
        public void SyncWithUI(ref GeneralSettingsInputs uIgInputs, ref MapSettingsInputs uImInputs, ref NoiseSettingsInputs uInInputs, ref TerrainType[] regions)
        {
            uIgInputs = gInputs;
            uImInputs = mInputs;
            uInInputs = nInputs;
            regions = this.regions;
        }

        public void ResetMap()
        {
            meshFilter.mesh = null;
        }
    }
}