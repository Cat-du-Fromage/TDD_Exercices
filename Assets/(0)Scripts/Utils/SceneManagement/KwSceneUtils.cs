using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class KwSceneUtils
{
    public enum KwScene
    {
        MainMenu,
        Game,
    }

    private static Dictionary<KwScene ,SceneInstance> sceneInstances;
    private static List<SceneInstance> LsceneInstances;

    private static void Attempt1()
    {
        sceneInstances = new Dictionary<KwScene ,SceneInstance>(Enum.GetValues(typeof(KwScene)).Length);

        Array sceneObjects = Enum.GetValues(typeof(KwScene));
        Debug.Log($"Length Register == {sceneObjects.Length-1}");
        for (int i = 0; i < sceneObjects.Length-1; i++)
        {
            KwScene sceneObj = (KwScene)sceneObjects.GetValue(i);

            Addressables.LoadSceneAsync(sceneObj.ToString(), LoadSceneMode.Single).Completed += (currentHandle) =>
            {
                if (i == sceneObjects.Length)
                {
                    Debug.Log($"i : {i} AND sceneObjects.Length : {sceneObjects.Length}");
                    sceneInstances.TryGetValue(KwScene.MainMenu, out SceneInstance mainMenuSi);
                    Addressables.LoadSceneAsync(mainMenuSi, LoadSceneMode.Additive).Completed += (noUseHandle) =>
                    {
                        UnloadScene(currentHandle);
                    };
                }
                else
                {
                    UnloadScene(currentHandle);
                }
                Register(sceneObj, currentHandle.Result);
            };
        }
    }
    
    private static void Attempt3()
    {
        foreach (IResourceLocator locator in Addressables.ResourceLocators)
        {
            IList<IResourceLocation> locations = new List<IResourceLocation>();
            var t = locator.Locate("SceneAsset", typeof(SceneInstance), out locations);
            foreach (IResourceLocation v in locations)
            {
                SceneProvider sceneProvider = new SceneProvider();

                sceneProvider.ProvideScene(new ResourceManager(), v, LoadSceneMode.Additive, false, 100).Completed +=
                    (handle) =>
                    {
                        Register(GetKwSceneByString(v.PrimaryKey), handle.Result);
                    };
                
                
                
                Debug.Log($"v InternalId : {v.InternalId}");
                Debug.Log($"v PrimaryKey : {v.PrimaryKey}");
                Debug.Log($"v ProviderId : {v.ProviderId}");
                Debug.Log($"v Data : {v.ResourceType}");
            }
            
        }
    }
    
    static KwSceneUtils()
    {
        
        IList<IResourceLocation> IloadedLocations = new List<IResourceLocation>();
        GetAll("SceneAsset", IloadedLocations);

    }
    
    public static async Task GetAll(string label, IList<IResourceLocation> loadedLocations)
    {
        IList<IResourceLocation> unloadedLocations = await Addressables.LoadResourceLocationsAsync(label).Task;

        foreach (var location in unloadedLocations)
            loadedLocations.Add(location);
        
        foreach (IResourceLocation v in loadedLocations)
        {
            Register(GetKwSceneByString(v.PrimaryKey), (SceneInstance)v.Data);
            Debug.Log($"v Data Type : {v.Data.GetType()}");
            Debug.Log($"v Data : {v.Data.ToString()}");
                Debug.Log($"v InternalId : {v.InternalId}");
                Debug.Log($"v PrimaryKey : {v.PrimaryKey}");
                Debug.Log($"v ProviderId : {v.ProviderId}");
        }
    }

    private static void attempt2()
    {
        foreach (IResourceLocator locator in Addressables.ResourceLocators)
        {
            IList<IResourceLocation> locations = new List<IResourceLocation>();
            var t = locator.Locate("SceneAsset", typeof(SceneInstance), out locations);
            foreach (IResourceLocation v in locations)
            {
                Register(GetKwSceneByString(v.PrimaryKey), (SceneInstance)v.Data);
                
                Debug.Log($"v InternalId : {v.InternalId}");
                Debug.Log($"v PrimaryKey : {v.PrimaryKey}");
                Debug.Log($"v ProviderId : {v.ProviderId}");
                Debug.Log($"v Data : {v.ResourceType}");
            }
            
        }
    }

    private static KwScene GetKwSceneByString(string str)
    {
        Array sceneObjects = Enum.GetValues(typeof(KwScene));
        
        for (int i = 0; i < sceneObjects.Length - 1; i++)
        {
            KwScene sceneObj = (KwScene)sceneObjects.GetValue(i);
            if (sceneObj.ToString().Equals(str))
            {
                Debug.Log($"Length Register == {sceneObj.ToString()}");
                return sceneObj;
            }
        }
        return KwScene.MainMenu;
    }
    
    private static void Register(KwScene scene, SceneInstance sceneInstance) => sceneInstances.Add(scene,sceneInstance);

    private static void UnloadScene(AsyncOperationHandle<SceneInstance> currentSi)
    {
        Addressables.UnloadSceneAsync(currentSi, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
    }

    public static bool Init() => true;

    public static void Load(KwScene scene)
    {
        Addressables.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
    }
    
}
