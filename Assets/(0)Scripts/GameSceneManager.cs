using System;
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
    public class GameSceneManager : MonoBehaviour
    {
        public AssetReference MainMenuAsset;
        
        private const string MAIN_MENU_ASSET = "MainMenu";
        private const string GAME_SCENE_ASSET = "Game";

        private static bool clearPreviousScene = false;
        //private static SceneInstance previousLoadedScene = new SceneInstance();

        private static AsyncOperationHandle<SceneInstance> previousLoadedScene;
    
        private GameSceneManager instance;

        
        private void Awake()
        {
            if (instance is null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            
            Addressables.LoadSceneAsync(MainMenuAsset, LoadSceneMode.Additive).Completed += (asyncHandle) =>
            {
                previousLoadedScene = asyncHandle;
            };
            Debug.Log("ACTUALL START!");
        }

        /*
        public static GameSceneManager Instance
        {
            get
            {
                instance ??= new GameSceneManager();
                return instance;
            }
        }
        */
        private static void LoadSceneAddressable(string key)
        {
            Addressables.LoadSceneAsync(key, LoadSceneMode.Additive).Completed += UnloadAddressableScene;
        }

        private static void OnLoadComplete(AsyncOperationHandle<SceneInstance> op)
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                previousLoadedScene = op;
            }
        }
        
        private static void UnloadAddressableScene(AsyncOperationHandle<SceneInstance> op)
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                Addressables.UnloadSceneAsync(previousLoadedScene).Completed += (asyncHandle) =>
                {
                    previousLoadedScene = op;
                    Debug.Log("UnloadComplete");
                };
            }
        }

        public static void LoadGameScene() => LoadSceneAddressable(GAME_SCENE_ASSET);
    }
}


