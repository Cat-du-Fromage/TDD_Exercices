using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.PersistentData;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class ScriptableObjectSingelton<T> : ScriptableObject where T : ScriptableObject
{
    private static string _label = String.Empty;
    
    private static T instance = null;

    protected static T GetInstance(string label)
    {
        _label = label;
        return Instance;
    }

    public static T Instance
    {
        get
        {
            if (!instance)
            {
                AsyncOperationHandle<T> csHandle = Addressables.LoadAssetAsync<T>(_label);
                csHandle.WaitForCompletion();
                instance = csHandle.Result;
                Debug.Log($"Asset LOADED : {instance.name}");
            }
            return instance;
        }
    }
}
