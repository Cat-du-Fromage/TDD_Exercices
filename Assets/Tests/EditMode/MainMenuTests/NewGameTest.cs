using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EditModeTest
{
    public class NewGameTest
    {
        [OneTimeSetUp] public void OneTimeSetup() => EditorSceneManager.LoadScene("Assets/Scenes/MainMenu.unity", new LoadSceneParameters(LoadSceneMode.Single));
 
        [UnityTest]
        public IEnumerator Test1()
        {
            GameObject a = GameObject.Find("UiMainMenu");
            Debug.Assert(a != null);
            yield return null;
        }
    
        /*
        [Test]
        public void UIMenuInteractions_IsNewGameEnable_True()
        {
            //Arrange
            
            //Act
            
            //Assert
            
        }
        */
    }
}

