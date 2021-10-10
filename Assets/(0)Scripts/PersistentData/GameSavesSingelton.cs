using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static KaizerWaldCode.Utils.AddressablesUtils;

namespace KaizerWaldCode.PersistentData
{
    [CreateAssetMenu(fileName = "GameSaves", menuName = "KaizerWald/PersistentData", order = 1)]
    public class GameSavesSingelton : ScriptableObjectSingelton<GameSavesSingelton>
    {
        private const string GAME_SAVES_ASSET_NAME = "GameSavesData";
        
        private const string GAME_SAVES_NAME = @"\Game Saves";

        public static DirectoryInfo MainGameSaves;
        
        [SerializeField]
        private GameSavesSingelton _inst;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void FirstInitialize()
        {
            GetInstance(GAME_SAVES_ASSET_NAME);
            //Debug.Log($"Test If it loads");
        }

        public static GameSavesSingelton gameSavesSingelton
        {
            get
            {
                return GetInstance(GAME_SAVES_ASSET_NAME)._inst;
            }
        }
    }
}