using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.UI.InGameMenu;
using KaizerWaldCode.UI.MainMenu;
using UnityEngine.InputSystem;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace PlayModeTest
{
    [TestFixture]
    public class InGameMenuTests
    {
        private GameObject UIParentGO;
        private GameObject inGameMenu;
        private InGameMenuInteractions inGameMenuComponent;

        
        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Game.unity",new LoadSceneParameters(LoadSceneMode.Single));
            //=========================================================================================
            //Tricks to find disabled object by using the parent : Careful this is an expensive action!
            UIParentGO = GameObject.Find("UIInGame");
            inGameMenu = UIParentGO.transform.Find("InGameMenu").gameObject; 
            //=========================================================================================
            inGameMenuComponent = inGameMenu.GetComponent<InGameMenuInteractions>();
        }

        [UnityTest]
        public IEnumerator InGameMenu_IsMenuEnable_False()
        {
            //Arrange
            
            //Act
            yield return new WaitForEndOfFrame();
            //Assert
            Assert.IsFalse(inGameMenu.activeSelf);
        }
        
        [Test]
        public void InGameMenu_IsMenuEnable_OnEscapeClick_True()
        {
            //Arrange
            inGameMenu.SetActive(false);
            //Act
            inGameMenuComponent.OnEscClick();
            //Assert
            Assert.IsTrue(inGameMenu.activeSelf);
        }
        
        [Test]
        public void InGameMenu_IsMenuEnable_OnResumeClick_True()
        {
            //Arrange
            inGameMenu.SetActive(true);
            //Act
            inGameMenuComponent.OnResumeClick();
            //Assert
            Assert.IsFalse(inGameMenu.activeSelf);
        }
        
        [UnityTest]
        public IEnumerator InGameMenu_OnMainMenuClicked_MainMenuSceneLoaded_True()
        {
            //Arrange
            inGameMenu.SetActive(true);
            //Act
            inGameMenuComponent.OnMainMenuClick();
            yield return new WaitForSeconds(0.1f);
            Scene gameScene = SceneManager.GetSceneByName("MainMenu");
            //Assert
            Assert.IsTrue(gameScene.isLoaded);
        }
    }
}