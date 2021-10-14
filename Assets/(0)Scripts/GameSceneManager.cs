using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;

namespace KaizerWaldCode
{
    public class GameSceneManager : MonoBehaviour
    {
        public AssetReference MainMenuSceneAsset;
        public AssetReference GameSceneAsset;
        
        private const string MAIN_MENU_ASSET = "MainMenu";
        private const string GAME_SCENE_ASSET = "Game";

        private static AsyncOperationHandle<SceneInstance> previousLoadedScene;

        private static GameSceneManager instance;
        public static GameSceneManager Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = FindObjectOfType<GameSceneManager>();
                    if (instance is null)
                    {
                        instance = new GameObject("GameSceneManager").AddComponent<GameSceneManager>();
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            /*
            if (instance is not null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance.enabled = true;
            }
            */
        }

        private void Start()
        {
            //KwSceneUtils.ProvideScene();
            //FirstSceneLoading();
        }

        /// <summary>
        /// Load the very first main menu when we enter the game
        /// We dont want to unload anything here
        /// </summary>
        private void FirstSceneLoading()
        {
            Addressables.LoadSceneAsync(MainMenuSceneAsset, LoadSceneMode.Additive).Completed += (asyncHandle) =>
            {
                previousLoadedScene = asyncHandle;
            };
        }
        
        //=====================================================================================================================
        //Load then on complete unload the previous scene loaded
        //======================================================
        private void LoadScene(string key)
        {
            Addressables.LoadSceneAsync(key, LoadSceneMode.Additive).Completed += UnloadScene;
        }

        private void UnloadScene(AsyncOperationHandle<SceneInstance> op)
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                Addressables.UnloadSceneAsync(previousLoadedScene).Completed += (asyncHandle) => previousLoadedScene = op;
            }
        }
        //=====================================================================================================================

        public void LoadGameScene() => LoadScene(GAME_SCENE_ASSET);
    }
}


