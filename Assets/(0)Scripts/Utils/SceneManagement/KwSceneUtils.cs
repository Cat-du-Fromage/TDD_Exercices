using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

public static class KwSceneUtils
{
    public static void LoadScene()
    {
        foreach (IResourceLocator locator in Addressables.ResourceLocators)
        {

            IList<IResourceLocation> locations = new List<IResourceLocation>();

            bool success = locator.Locate("Game", typeof(SceneInstance), out locations);

            string s = locations[0].ProviderId;
            var t = locations[0].ResourceType;
            Debug.Log($"type is {t.ToString()}");
            Debug.Log(s);
        }

    }

    public static void GetSceneInstance()
    {
        ResourceManager rm = new ResourceManager();
        //rm.GetResourceProvider(SceneInstance, "test");
    }
}
