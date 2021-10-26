using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.MapGeneration;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.UI.MainMenu;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;
using UnityAssert = UnityEngine.Assertions.Assert;

namespace PlayModeTest
{
    [TestFixture]
    public class MapGeneratorTests
    {
        private GameObject UIMainMenu;
        private MainMenuInteractions mainMenuComponent;
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
        
        /*
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Game"))
            {
                EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Game.unity", new LoadSceneParameters(LoadSceneMode.Single));
            }
            Debug.Log(SceneManager.GetActiveScene().path);
            noiseSettings = new NoiseSettings();
            mapGenObj = GameObject.Find("MapGenerator");
            mapGenComp = mapGenObj.GetComponent<MapGenerator>();
            
        }
        */
        /*
        [SetUp]
        public void Setup()
        {
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Game"))
            {
                EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Game.unity", new LoadSceneParameters(LoadSceneMode.Single));
            }
            
            noiseSettings = new NoiseSettings();
            mapGenObj = GameObject.Find("MapGenerator");
            mapGenComp = mapGenObj.GetComponent<MapGenerator>();
            
        }
        */
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
            //Act
            mapGenComp.NewGameSettings(mapInputs, noiseInputs,"TestMapGen");
            //Assert
            Assert.IsNotNull(mapGenComp.vertices);
        }
        
        [Test]
        public void MapGenerator_OnGetUvs_InitializeUvsArray_Exist()
        {
            //Arrange
            mapGenObj = GameObject.Find("MapGenerator");
            mapGenComp = mapGenObj.GetComponent<MapGenerator>();
            //Act
            mapGenComp.NewGameSettings(mapInputs, noiseInputs,"TestMapGen");
            //Assert
            Assert.IsNotNull(mapGenComp.uvs);
        }
        
        [Test]
        public void MapGenerator_Triangles_ArraySizeEqualTrianglesVertices_True()
        {
            //Arrange
            Debug.Log($"Scene Name = {SceneManager.GetActiveScene().name}");
            mapGenObj = GameObject.Find("MapGenerator");
            Debug.Log($"MapGener Name = {mapGenObj.name}");
            mapGenComp = mapGenObj.GetComponent<MapGenerator>();
            Debug.Log($"MapGener Name = {mapGenComp.name}");
            int expectedTriVertices = 24;
            //Act
            mapGenComp.NewGameSettings(mapInputs, noiseInputs,"TestMapGen");
            //Assert
            Assert.AreEqual(expectedTriVertices, mapGenComp.triangles.Length);
        }
        
        [Test]
        public void MapGenerator_Triangles_AreVerticesAlign_True()
        {
            //Arrange
            int[] firstQuad = new []{3,4,0,1,0,4};
            int[] secondQuad = new []{4,5,1,2,1,5};
            int[] thirdQuad = new []{6,7,3,4,3,7};
            int[] forthQuad = new []{7,8,4,5,4,8};
            //Act
            mapGenComp.NewGameSettings(mapInputs, noiseInputs,"TestMapGen");
            //Assert
            Assert.AreEqual(firstQuad, mapGenComp.triangles[0.. 6]);
            Assert.AreEqual(secondQuad, mapGenComp.triangles[6.. 12]);
            Assert.AreEqual(thirdQuad, mapGenComp.triangles[12.. 18]);
            Assert.AreEqual(forthQuad, mapGenComp.triangles[18.. 24]);
        }
    }
}