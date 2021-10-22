using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.MapGeneration;
using KaizerWaldCode.UI.MainMenu;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Unity.Mathematics.math;
using UnityAssert = UnityEngine.Assertions.Assert;

namespace PlayModeTest
{
    [TestFixture]
    public class MapGeneratorTests
    {
        private GameObject UIMainMenu;
        private MainMenuInteractions mainMenuComponent;
        /*
        [UnitySetUp]
        public IEnumerator UnitySetup()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity", new LoadSceneParameters(LoadSceneMode.Single));
            UIMainMenu = GameObject.Find("UIMainMenu");
            mainMenuComponent = UIMainMenu.GetComponent<MainMenuInteractions>();
        }
        */
        [OneTimeSetUp]
        public void Setup()
        {
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Game.unity", new LoadSceneParameters(LoadSceneMode.Single));
            
            UIMainMenu = GameObject.Find("UIMainMenu");
        }
        
        [Test]
        public void MapSettings_OnClickNewGame_CreateMapSettings_Exist()
        {
            //Arrange
            
            //Act
            MapSettings mapSettings = new MapSettings();
            //Assert
            Assert.NotNull(mapSettings);
        }
        
        [Test]
        public void MapSettings_OnClickNewGame_CreateMapSettings_EqualDefaultValue()
        {
            //Arrange
            string defaultName = "DefaultSaveName";
            int defaultChunkSize = 10;
            int defaultNumChunk = 4;
            int defaultPointPerMeter = 2;
            uint defaultSeed = 1;

            int defautlMapSize = 40;
            int defautlChunkPointPerAxis = 11;
            int defautlMapPointPerAxis = 41;

            MapSettings defaultMapSettings = new MapSettings(defaultName,defaultChunkSize,defaultNumChunk,defaultPointPerMeter,defaultSeed);
            //Act
            MapSettings mapSettings = new MapSettings();
            //Assert
            Assert.AreEqual(defaultName ,mapSettings.SaveName);
            Assert.AreEqual(defaultChunkSize ,mapSettings.ChunkSize);
            Assert.AreEqual(defaultNumChunk ,mapSettings.NumChunk);
            Assert.AreEqual(defaultPointPerMeter ,mapSettings.PointPerMeter);
            Assert.AreEqual(defaultSeed,mapSettings.Seed);
            
            Assert.AreEqual(defautlMapSize ,mapSettings.MapSize);
            Assert.AreEqual(defautlChunkPointPerAxis ,mapSettings.ChunkPointPerAxis);
            Assert.AreEqual(defautlMapPointPerAxis ,mapSettings.MapPointPerAxis);
        }
        
        [Test]
        public void MapGenerator_OnGetVertices_InitializeVerticesArray_Exist()
        {
            //Arrange
            GameObject mapGen = new GameObject("MapGenerator", typeof(MapGenerator));
            MapGenerator mapGenData = mapGen.GetComponent<MapGenerator>();
            MapSettings mapSettings = new MapSettings();
            //Act
            mapGenData.Initialize(mapSettings);
            //Assert
            Assert.IsNotNull(mapGenData.vertices);
        }
        
        [Test]
        public void MapGenerator_OnGetUvs_InitializeUvsArray_Exist()
        {
            //Arrange
            GameObject mapGen = new GameObject("MapGenerator", typeof(MapGenerator));
            MapGenerator mapGenData = mapGen.GetComponent<MapGenerator>();
            MapSettings mapSettings = new MapSettings();
            //Act
            mapGenData.Initialize(mapSettings);
            //Assert
            Assert.IsNotNull(mapGenData.uvs);
        }
    }
}