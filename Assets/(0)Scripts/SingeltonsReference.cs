using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.PersistentData;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KaizerWaldCode
{
    public class SingeltonsReference : MonoBehaviour
    {
        //[SerializeField]private ScriptableObject[] references;
        [SerializeField]private AssetReferenceT<ScriptableObject>[] references;

        private void Start()
        {
            /*
            //GameSavesSingelton.Instance;
            Debug.Log($"RuntimePath Path == {Addressables.RuntimePath}"); // Library/com.unity.addressables/aa/Windows
            Debug.Log($"Build Path == {Addressables.BuildPath}"); // Library/com.unity.addressables/aa/Windows
            Debug.Log($"LibraryPath Path == {Addressables.LibraryPath}"); // Library/com.unity.addressables/
            Debug.Log($"RuntimeKey Path == {references[0].RuntimeKey}"); //5104efe7244ca0a46b12387fde2224db
            Debug.Log($"AssetGUID Path == {references[0].AssetGUID}"); //5104efe7244ca0a46b12387fde2224db
            Debug.Log($"references[0] Path == {references[0].ToString()}"); //  [5104efe7244ca0a46b12387fde2224db]GameSavesData (KaizerWaldCode.PersistentData.GameSavesSingelton)
            Debug.Log($"Addressables ResourceManager  == {Addressables.ResourceManager}");
            */
        }
    }
}
