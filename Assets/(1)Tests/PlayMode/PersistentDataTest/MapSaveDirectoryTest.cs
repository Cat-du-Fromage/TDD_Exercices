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
        
        [UnitySetUp]
        public IEnumerator UnitySetup()
        {
            yield return EditorSceneManager
                .LoadSceneAsyncInPlayMode("Assets/Scenes/Game.unity", new LoadSceneParameters(LoadSceneMode.Single));
            this.mapGenObj = GameObject.Find("MapGenerator");
            this.mapGenComp = mapGenObj.GetComponent<MapGenerator>();

            mapInputs = new MapSettingsInputs
            {
                chunkSize = 1,
                numChunk = 2,
                pointPerMeter = 2,
                seed = 1
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
        }

        [Test]
        public void MapSaveDirectory_OnNewGame_CreateSaveFolder_Exist()
        {
            //Arrange
            
            //Act
            mapGenComp.NewGameSettings(mapInputs,noiseInputs);
            DirectoryInfo saveDir = MapSaveDirectory.Instance(mapGenComp.currentSaveName).mapSaveDirInfo;
            Debug.Log(MapSaveDirectory.Instance(mapGenComp.currentSaveName).mapSaveDirInfo.FullName);
            //Assert
            DirectoryAssert.Exists(saveDir);
        }
        
        [Test]
        public void MapSaveDirectory_OnNewGame_CreateMapSettingsFile_Exist()
        {
            //Arrange
            
            //Act
            mapGenComp.NewGameSettings(mapInputs,noiseInputs);
            DirectoryInfo saveDir = MapSaveDirectory.Instance(mapGenComp.currentSaveName).mapSaveDirInfo;
            FileInfo fileInfo = MapSaveDirectory.Instance(mapGenComp.currentSaveName).GetOrCreateMapSettings();
            
            Debug.Log(MapSaveDirectory.Instance(mapGenComp.currentSaveName).GetOrCreateMapSettings().FullName);
            //Assert
            FileAssert.Exists(fileInfo.FullName);
        }
    }
}