using System;
using System.Linq;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.UI.MainMenu;
using KaizerWaldCode.Utils;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEditor;
using UnityEngine;

using static KaizerWaldCode.Utils.NativeCollectionUtils;

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
        public bool sampleDebug;
        public bool autoUpdate;
        public bool syncUI;

        public float3[] samplePositions;
        public int[] islandId;
        private int[] verticesCellAssignment;
        //public int[] verticesCellId;

        private void OnValidate()
        {
            gInputs.CheckValues();
            mInputs.CheckValues();
            nInputs.CheckValues();
        }

        public void DrawMapInEditor()
        {
            samplePositions = null;
            if (generationType == GenerationType.Noise)
            {
                generalMapSettings.NewGame(gInputs);
                mapSettings.NewGame(mInputs);
                noiseSettings.NewGame(nInputs);
                //SetPositionToZero();
                meshRenderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromHeightMap(mapSettings, regions,Noise.GetNoiseMap(generalMapSettings, mapSettings, noiseSettings));
                meshFilter.mesh = MeshGenerator.GetTerrainMesh(generalMapSettings, mapSettings, noiseSettings);
            }
            else if (generationType == GenerationType.CoastLine)
            {
                
                generalMapSettings.NewGame(gInputs);
                mapSettings.NewGame(mInputs);
                noiseSettings.NewGame(nInputs);
                
                int numCell = 25;
                SamplesSettings samplesSettings = new SamplesSettings(mapSettings, numCell);
                
                //SetPositionToZero();
                meshFilter.mesh = MeshGenerator.GetTerrainMesh(generalMapSettings, mapSettings, noiseSettings, false);
                samplePositions = IslandGenerator.GenerateRandomPoints(generalMapSettings, mapSettings, samplesSettings.numCellPerAxis);
                islandId = IslandGenerator.GetCoastLine(samplePositions, generalMapSettings, mapSettings);
                CheckSamplesOutOfMap();
                verticesCellAssignment = IslandGenerator.GetCellsClosestVertices(samplesSettings, meshFilter.sharedMesh.vertices.ReinterpretArray<Vector3, float3>(),samplePositions);
                meshRenderer.sharedMaterial.mainTexture = IslandGenerator.SetTextureOnIsland(mapSettings, verticesCellAssignment, islandId);
                IslandMeshGenerator.GetIslandDstLayers(mapSettings, islandId, verticesCellAssignment, meshFilter.sharedMesh.vertices);
            }
        }

        private void CheckSamplesOutOfMap()
        {
            for (int i = 0; i < samplePositions.Length; i++)
            {
                if (samplePositions[i].x < -(mapSettings.mapSize * 0.5f))
                {
                    Debug.Log($"Samples at {i} : {samplePositions[i]}");
                }
            }
        }

        private void SetPositionToZero()
        {
            float midPos = mapSettings.mapSize * 0.5f;
            gameObject.transform.position = Vector3.zero - new Vector3(midPos,2,midPos);
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

        private void OnDrawGizmos()
        {
            if (generationType == GenerationType.CoastLine && sampleDebug && samplePositions is not null && samplePositions.Length != 0)
            {
                for (int i = 0; i < samplePositions.Length; i++)
                {
                    Gizmos.color = islandId.Contains(i) ? Color.green : Color.red;
                    Gizmos.DrawSphere(samplePositions[i], 0.1f);
                }
            }
        }
    }
}