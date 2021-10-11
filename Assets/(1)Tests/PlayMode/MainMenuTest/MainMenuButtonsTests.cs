using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode;
using KaizerWaldCode.PersistentData;
using KaizerWaldCode.UI.MainMenu;
using KaizerWaldCode.Utils;
using NUnit.Framework;
using UnityEditor.Build.Content;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace PlayModeTest
{
    [TestFixture]
    public class MainMenuButtonsTests
    {
        private GameObject UIMainMenu;
        private MainMenuInteractions mainMenuComponent;
        
        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity", new LoadSceneParameters(LoadSceneMode.Single));
            UIMainMenu = GameObject.Find("UIMainMenu");
            mainMenuComponent = UIMainMenu.GetComponent<MainMenuInteractions>();
        }

        [Test]
        public void UIMainMenu_MainMenuGameObjectExist_True()
        {
            //Assert
            Assert.NotNull(UIMainMenu);
        }

        //=============================================
        //TEST BUTTONS EXISTS
        //=============================================
        #region TEST BUTTON EXIST
        [Test]
        public void UIMainMenu_NewGameButtonExist_True()
        {
            //Act
            Button newGameButton = UIMainMenu.GetComponent<MainMenuInteractions>().NewGameButton;
            //Assert
            Assert.NotNull(newGameButton);
        }
        
        [Test]
        public void UIMainMenu_ContinueButtonExist_True()
        {
            //Act
            Button continueButton = UIMainMenu.GetComponent<MainMenuInteractions>().ContinueButton;
            //Assert
            Assert.NotNull(continueButton);
        }
        
        [Test]
        public void UIMainMenu_LoadButtonExist_True()
        {
            //Act
            Button loadButton = UIMainMenu.GetComponent<MainMenuInteractions>().LoadGameButton;
            //Assert
            Assert.NotNull(loadButton);
        }
        
        [Test]
        public void UIMainMenu_SettingsButtonExist_True()
        {
            //Act
            Button settingsButton = UIMainMenu.GetComponent<MainMenuInteractions>().SettingsButton;
            //Assert
            Assert.NotNull(settingsButton);
        }
        #endregion TEST BUTTON EXIST
        
        [Test]
        public void UIMainMenu_NewGameButtonIsEnable_True()
        {
            //Act
            Button newGameButton = UIMainMenu.GetComponent<MainMenuInteractions>().NewGameButton;
            //Assert
            Assert.IsTrue(newGameButton.enabledSelf);
        }
        
        //=============================================
        //TEST BUTTONS CONTINUE IS ENABLE ?
        //=============================================
        
        [Test]
        public void UIMainMenu_IsContinueAvailable_False()
        {
            //Arrange
            Directory.Delete(MainSaveDirectory.Instance.MainSaveDirInfo.FullName, true);
            MainMenuInteractions mMInteracts = UIMainMenu.GetComponent<MainMenuInteractions>();
            //Act
            bool continueButtonEnable = mMInteracts.IsContinueAvailable();
            //Assert
            Assert.IsFalse(continueButtonEnable);
        }
        
        
        [Test]
        public void UIMainMenu_ContinueButtonIsEnable_WithOneSave_True()
        {
            //Arrange
            Directory.Delete(MainSaveDirectory.Instance.MainSaveDirInfo.FullName, true);
            string sub1 = "sub1";
            Button continueButton = mainMenuComponent.ContinueButton;
            //Act
            MainSaveDirectory.Instance.MainSaveDirInfo.CreateSubdirectory(sub1);
            mainMenuComponent.IsContinueAvailable();
            //Assert
            Assert.IsTrue(continueButton.enabledSelf);
        }
        
        [Test]
        public void UIMainMenu_ContinueButtonIsEnable_WhenNoSaves_False()
        {
            //Arrange
            Directory.Delete(MainSaveDirectory.Instance.MainSaveDirInfo.FullName, true);
            Button continueButton = mainMenuComponent.ContinueButton;
            //Act
            mainMenuComponent.IsContinueAvailable();
            //Assert
            Assert.IsFalse(continueButton.enabledSelf);
        }
        
        //=============================================
        //TEST BUTTONS LOAD GAME IS ENABLE ?
        //=============================================
        
        [Test]
        public void UIMainMenu_IsLoadAvailable_True()
        {
            //Arrange
            Directory.Delete(MainSaveDirectory.Instance.MainSaveDirInfo.FullName, true);
            string sub1 = "sub1";
            //Act
            MainSaveDirectory.Instance.MainSaveDirInfo.CreateSubdirectory(sub1);
            bool loadButtonEnable = mainMenuComponent.IsLoadAvailable();
            //Assert
            Assert.IsTrue(loadButtonEnable);
        }
        
        [Test]
        public void UIMainMenu_LoadButtonIsEnable_WithOneSave_True()
        {
            //Arrange
            Directory.Delete(MainSaveDirectory.Instance.MainSaveDirInfo.FullName, true);
            string sub1 = "sub1";
            Button loadButton = mainMenuComponent.LoadGameButton;
            //Act
            MainSaveDirectory.Instance.MainSaveDirInfo.CreateSubdirectory(sub1);
            mainMenuComponent.IsLoadAvailable();
            //Assert
            Assert.IsTrue(loadButton.enabledSelf);
        }
        
        [Test]
        public void UIMainMenu_LoadButtonIsEnable_WhenNoSaves_False()
        {
            //Arrange
            Directory.Delete(MainSaveDirectory.Instance.MainSaveDirInfo.FullName, true);
            //Act
            Button loadButton = mainMenuComponent.LoadGameButton;
            mainMenuComponent.IsLoadAvailable();
            //Assert
            Assert.IsFalse(loadButton.enabledSelf);
        }
        
        [Test]
        public void UIMainMenu_OnContinueClicked_False()
        {
            //Arrange
            Directory.Delete(MainSaveDirectory.Instance.MainSaveDirInfo.FullName, true);
            //Create Directory
            string sub1 = "sub1";
            MainSaveDirectory.Instance.MainSaveDirInfo.CreateSubdirectory(sub1);
            //Act
            //Button continueButton = mainMenuComponent.ContinueButton;
            mainMenuComponent.OnClickContinue();
            bool gameSceneLoaded = GameSceneManager.Instance.LoadGameScene();
            //Assert
            Assert.IsTrue(gameSceneLoaded);
        }
    }
}


/*
public class LoadSceneAttribute : NUnitAttribute, IOuterUnityTestAction
{
    private string scene;
 
    public LoadSceneAttribute(string scene) => this.scene = scene;
 
    IEnumerator IOuterUnityTestAction.BeforeTest(ITest test)
    {
        Debug.Assert(scene.EndsWith(".unity"));
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(scene, new LoadSceneParameters(LoadSceneMode.Single));
    }
 
    IEnumerator IOuterUnityTestAction.AfterTest(ITest test)
    {
        yield return null;
    }
}
*/
