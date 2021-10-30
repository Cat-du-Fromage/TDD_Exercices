using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KaizerWaldCode.MapGeneration;
using KaizerWaldCode.MapGeneration.Data;
using KaizerWaldCode.PersistentData;
using KaizerWaldCode.Utils;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UIElements;

using static KaizerWaldCode.Utils.UIUtils;

namespace KaizerWaldCode.UI.MainMenu
{
    public class MainMenuInteractions : MonoBehaviour
    {
        private VisualElement root;
        
        public Button NewGameButton;
        public Button ContinueButton;
        public Button LoadGameButton;
        public Button SettingsButton;
        public Button QuitButton;
        
        public NoiseSettingsInputs noiseInputs;
        public MapSettingsInputs mapInputs;
        public GeneralSettingsInputs generalInputs;

        public TerrainType[] regions;
        void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            
            NewGameButton  = InitButton(in root,"newGame-button", OnClickNewGame);
            ContinueButton = InitButton(in root,"continue-button", OnClickContinue, SubFoldersExist());
            LoadGameButton = InitButton(in root,"loadGame-button");
            SettingsButton = InitButton(in root,"settings-button");
            QuitButton     = InitButton(in root,"quit-button", OnQuit);
        }
        
        private bool SubFoldersExist() => MainSaveDirectory.Instance.GetNumSubfolders() > 0;

        private void UpdateState(ref Button b, bool realState)
        {
            if (b.enabledSelf != realState)
            {
                b.SetEnabled(realState);
            }
        }
        
        public bool IsContinueAvailable()
        {
            bool available = SubFoldersExist();
            UpdateState(ref ContinueButton, available);
            return available;
        }
        
        public bool IsLoadAvailable()
        {
            bool available = SubFoldersExist();
            UpdateState(ref LoadGameButton, available);
            return available;
        }
        
        public void OnClickNewGame()
        {
            KwSceneUtils.Load(KwSceneUtils.KwScene.Game).Completed += (handle) =>
            {
                GameObject mapGen = GameObject.FindObjectOfType<MapGenerator>().gameObject;
                MapGenerator mapGenData = mapGen.GetComponent<MapGenerator>();

                mapGenData.NewGameSettings(generalInputs, mapInputs, noiseInputs, regions);
            };
        }

        public void OnClickContinue()
        {
            Task clickTask = new Task(() =>KwSceneUtils.Load(KwSceneUtils.KwScene.Game));
            clickTask.RunSynchronously();
        }
        
        private void OnClickLoadGame()
        {
            Debug.Log("Open LoadGame");
        }
    }
}

