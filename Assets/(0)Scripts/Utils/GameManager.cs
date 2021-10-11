using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode.Utils
{
    /*
    public static class GameManager
    {
        
        public static async Task<bool> LoadGameScene()
        {
            Scene scene = SceneManager.GetSceneByName("Game");
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Scenes/Game", LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            while (!asyncOperation.isDone)
            {
                //return Task.Yield();
                await Task.Yield();
            }
            asyncOperation.allowSceneActivation = true;
            
            Scene previousScene = SceneManager.GetSceneByName("MainMenu");
            if (previousScene.IsValid()) 
            {
                SceneManager.UnloadSceneAsync(previousScene);
            }
            
            return true;
        }
        
    }
    */
}
