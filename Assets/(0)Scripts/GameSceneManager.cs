using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode
{
    public class GameSceneManager
    {
        private const string MAIN_MENU_ASSET = "MainMenu";
        private const string GAME_SCENE_ASSET = "Game";

        private AsyncOperationHandle<SceneInstance> handle;
    
        private static GameSceneManager instance;
        /*
        void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        */
        public static GameSceneManager Instance
        {
            get
            {
                instance ??= new GameSceneManager();
                return instance;
            }
        }

        private bool LoadSceneAddressable(string key)
        {
            Addressables.LoadSceneAsync(key, LoadSceneMode.Additive).Completed += OnLoadComplete;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return true;
            }
            return false;
        }

        private void OnLoadComplete(AsyncOperationHandle<SceneInstance> op)
        {
        
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("SCENE LOADED");
                handle = op;
            }
        }
/*
        private void UnloadScene()
        {
            Scene scene = SceneManager.GetSceneByName("Game");
            SceneInstance io = new SceneInstance();
            io.Scene = scene;
            SceneManager.UnloadSceneAsync(scene);
            //handle = scene;
            Addressables.UnloadSceneAsync(io,true);
        }
*/
        public bool LoadGameScene() => LoadSceneAddressable(GAME_SCENE_ASSET);
    }
}


