using System.Collections;
using System.IO;
using KaizerWaldCode.MapGeneration;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.PersistentData;
using KaizerWaldCode.UI.MainMenu;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace PlayModeTest
{
    [TestFixture]
    public class MapSaveDirectoryTest
    {
        private NoiseSettings noiseSettings;
        private GameObject mapGenObj;
        private MapGenerator mapGenComp;

        private NoiseSettingsInputs noiseInputs;
        private MapSettingsInputs mapInputs;
        private GeneralSettingsInputs generalInputs;
        
        TerrainType[] regions;
        
        [UnitySetUp]
        public IEnumerator UnitySetup()
        {
            yield return EditorSceneManager
                .LoadSceneAsyncInPlayMode("Assets/Scenes/Game.unity", new LoadSceneParameters(LoadSceneMode.Single));
            this.mapGenObj = GameObject.Find("MapGenerator");
            this.mapGenComp = mapGenObj.GetComponent<MapGenerator>();

            regions = new TerrainType[2];

            mapInputs = new MapSettingsInputs
            {
                chunkSize = 1,
                numChunk = 2,
                pointPerMeter = 2,
                meshHeightCurve = default,
            };

            noiseInputs = new NoiseSettingsInputs
            {
                octaves = 4,
                scale = 10f,
                persistence = 0.5f,
                lacunarity = 2f,
                heightMultiplier = 1f,
                offset = float2.zero
            };

            generalInputs = new GeneralSettingsInputs
            {
                saveName = "DefaultSaveName",
                seed = 1
            };
        }

        [Test]
        public void MapSaveDirectory_OnNewGame_CreateSaveFolder_Exist()
        {
            //Arrange
            
            //Act
            mapGenComp.NewGameSettings(generalInputs,mapInputs,noiseInputs,regions);
            DirectoryInfo saveDir = MapSaveDirectory.Instance(mapGenComp.GetGeneralMapSettings().saveName).GetCurrentSave();
            Debug.Log(MapSaveDirectory.Instance(mapGenComp.GetGeneralMapSettings().saveName).GetCurrentSave().FullName);
            //Assert
            DirectoryAssert.Exists(saveDir);
        }
        
        [Test]
        public void MapSaveDirectory_OnNewGame_CreateMapSettingsFile_Exist()
        {
            //Arrange
            
            //Act
            mapGenComp.NewGameSettings(generalInputs,mapInputs,noiseInputs,regions);
            //DirectoryInfo saveDir = MapSaveDirectory.GetOrCreateSave(mapGenComp.currentSaveName);
            FileInfo fileInfo = MapSaveDirectory.Instance(mapGenComp.GetGeneralMapSettings().saveName).GetOrCreateMapSettings();
            
            Debug.Log(MapSaveDirectory.Instance(mapGenComp.GetGeneralMapSettings().saveName).GetOrCreateMapSettings().FullName);
            //Assert
            FileAssert.Exists(fileInfo.FullName);
        }
        
        [Test]
        public void MapSaveDirectory_OnNewGame_SetGeneralMapSettings_Equals()
        {
            //Arrange
            string Gsettings_SaveName = "default";
            uint Gsettings_Seed = 2;
            generalInputs = new GeneralSettingsInputs
            {
                saveName = "default",
                seed = 2
            };
            
            //Act
            mapGenComp.NewGameSettings(generalInputs, mapInputs, noiseInputs,regions);
            
            //Assert
            Debug.Log($"Expected : {Gsettings_SaveName} , Result : {mapGenComp.GetGeneralMapSettings().saveName}");
            Debug.Log($"Expected : {Gsettings_Seed} , Result : {mapGenComp.GetGeneralMapSettings().seed}");
            Assert.AreEqual(Gsettings_SaveName,mapGenComp.GetGeneralMapSettings().saveName);
            Assert.AreEqual(Gsettings_Seed,mapGenComp.GetGeneralMapSettings().seed);
        }
        
        [Test]
        public void MapSaveDirectory_OnNewGame_SetMapSettings_Equals()
        {
            //Arrange
            int mapSet_chunkSize = 10;
            int mapSet_numChunk = 4;
            int mapSet_pointPerMeter = 3;
            mapInputs = new MapSettingsInputs
            {
                chunkSize = 10,
                numChunk = 4,
                pointPerMeter = 3,
            };
            
            //Act
            mapGenComp.NewGameSettings(generalInputs, mapInputs, noiseInputs,regions);
            
            //Assert
            Debug.Log($"Expected : {mapSet_chunkSize} , Result : {mapGenComp.GetMapSettings().chunkSize}");
            Debug.Log($"Expected : {mapSet_numChunk} , Result : {mapGenComp.GetMapSettings().numChunk}");
            Debug.Log($"Expected : {mapSet_pointPerMeter} , Result : {mapGenComp.GetMapSettings().pointPerMeter}");
            Assert.AreEqual(mapSet_chunkSize,mapGenComp.GetMapSettings().chunkSize);
            Assert.AreEqual(mapSet_numChunk,mapGenComp.GetMapSettings().numChunk);
            Assert.AreEqual(mapSet_pointPerMeter,mapGenComp.GetMapSettings().pointPerMeter);
        }
        
        [Test]
        public void MapSaveDirectory_OnNewGame_SetNoiseSettings_Equals()
        {
            //Arrange
            int noiseSet_octaves = 6;
            float noiseSet_scale = 100f;
            float noiseSet_persistence = 0.6f;
            float noiseSet_lacunarity = 1.5f;
            float noiseSet_heightMultiplier = 30f;
            float2 noiseSet_offset = new float2(1,1);
            
            noiseInputs = new NoiseSettingsInputs
            {
                octaves = 6,
                scale = 100f,
                persistence = 0.6f,
                lacunarity = 1.5f,
                heightMultiplier = 30f,
                offset = new float2(1,1)
            };
            
            //Act
            mapGenComp.NewGameSettings(generalInputs, mapInputs, noiseInputs,regions);
            
            //Assert
            Assert.AreEqual(noiseSet_octaves,mapGenComp.GetNoiseSettings().octaves);
            Assert.AreEqual(noiseSet_scale,mapGenComp.GetNoiseSettings().scale);
            Assert.AreEqual(noiseSet_persistence,mapGenComp.GetNoiseSettings().persistence);
            Assert.AreEqual(noiseSet_lacunarity,mapGenComp.GetNoiseSettings().lacunarity);
            Assert.AreEqual(noiseSet_heightMultiplier,mapGenComp.GetNoiseSettings().heightMultiplier);
            Assert.AreEqual(noiseSet_offset,mapGenComp.GetNoiseSettings().offset);
        }
    }
}