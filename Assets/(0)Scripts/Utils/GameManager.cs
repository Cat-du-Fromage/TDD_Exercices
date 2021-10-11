using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlasticGui.WorkspaceWindow.BranchExplorer;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameManager
{
    /*
    public static async Task<bool> LoadGameScene()
    {
        Task.Run(() =>
        {
            Scene previousScene = SceneManager.GetSceneByName("MainMenu");
            if (previousScene.IsValid()) 
            {
                yield return SceneManager.UnloadSceneAsync(previousScene);
            }
 
            SceneManager.LoadScene("Scenes/Game", LoadSceneMode.Additive);
            Scene scene = SceneManager.GetSceneByName("Game");
            while (!scene.isLoaded)
            {
                await Task.Yield();
            }
        });
        return true;
    }
    */
}
