using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.UI.MainMenu;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class MainMenuInteractionsTests
{
    private GameObject UIMainMenu;
    
    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity", new LoadSceneParameters(LoadSceneMode.Single));
        UIMainMenu = GameObject.Find("UIMainMenu");
    }
/*
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        UIMainMenu = GameObject.Find("UIMainMenu");
    }
*/
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
        //Arrange
        //GameObject UIMainMenu = GameObject.Find("UIMainMenu");
        Button newGameButton = UIMainMenu.GetComponent<MainMenuInteractions>().NewGameButton;
        //Assert
        Assert.NotNull(newGameButton);
    }
    
    [Test]
    public void UIMainMenu_ContinueButtonExist_True()
    {
        //Arrange
        //GameObject UIMainMenu = GameObject.Find("UIMainMenu");
        Button continueButton = UIMainMenu.GetComponent<MainMenuInteractions>().ContinueButton;
        //Assert
        Assert.NotNull(continueButton);
    }
    
    [Test]
    public void UIMainMenu_LoadButtonExist_True()
    {
        //Arrange
        //GameObject UIMainMenu = GameObject.Find("UIMainMenu");
        Button loadButton = UIMainMenu.GetComponent<MainMenuInteractions>().LoadGameButton;
        //Assert
        Assert.NotNull(loadButton);
    }
    
    [Test]
    public void UIMainMenu_SettingsButtonExist_True()
    {
        //Arrange
        //GameObject UIMainMenu = GameObject.Find("UIMainMenu");
        Button settingsButton = UIMainMenu.GetComponent<MainMenuInteractions>().SettingsButton;
        //Assert
        Assert.NotNull(settingsButton);
    }
    #endregion TEST BUTTON EXIST
    
    [Test]
    public void UIMainMenu_NewGameButtonIsEnable_True()
    {
        //Arrange
        //GameObject UIMainMenu = GameObject.Find("UIMainMenu");
        Button newGameButton = UIMainMenu.GetComponent<MainMenuInteractions>().NewGameButton;
        //Assert
        Assert.IsTrue(newGameButton.enabledSelf);
    }
    /*
    [Test]
    public void UIMainMenu_ContinueCheckEnableCondition_False()
    {
        //Arrange
        GameObject UIMainMenu = GameObject.Find("UIMainMenu");
        MainMenuInteractions mMInteracts = UIMainMenu.GetComponent<MainMenuInteractions>();
        //Act
        //In Here we assume the directory containing the data we need don't exist
        bool ccntinueButtonEnable = mMInteracts.IsContinueAvailable();
        //Assert
        Assert.IsFalse(ccntinueButtonEnable);
    }
    */
    
    

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
