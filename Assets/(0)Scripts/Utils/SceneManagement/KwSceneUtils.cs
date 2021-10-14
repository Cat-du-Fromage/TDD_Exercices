using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class KwSceneUtils
{
    public enum KwScene
    {
        MainMenu,
        Game,
    }

    private static Dictionary<KwScene ,SceneInstance> sceneInstances;
    private static List<SceneInstance> LsceneInstances;
    static KwSceneUtils()
    {
        sceneInstances = new Dictionary<KwScene ,SceneInstance>(Enum.GetValues(typeof(KwScene)).Length);
        /*
        List<string> keys = new List<string>(){"MainMenu", "Game"};
        var test = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Intersection, typeof(SceneInstance));
        foreach (var t in test.Result)
        {
            //Addressables.ResourceManager.ProvideScene(test.Result, );
            //LsceneInstances.Add(test.Result.GetEnumerator());
        }
        */
        foreach (KwScene scene in Enum.GetValues(typeof(KwScene)))
        {
            Addressables.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single).Completed += (handle) =>
            {
                Addressables.UnloadSceneAsync(handle.Result).Completed += (handle2) =>
                {
                    sceneInstances.Add(scene,handle2.Result);
                };
            };
        }

        //sceneInstances.TryGetValue(KwScene.MainMenu, out SceneInstance mainMenuSi);
        //Debug.Log(mainMenuSi.Scene.Equals(SceneManager.GetSceneByName("MainMenu")));
        //Addressables.LoadSceneAsync(mainMenuSi, LoadSceneMode.Single);
    }

    public static bool Init() => true;

    public static void Load(KwScene scene)
    {
        Addressables.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
    }
    
}
