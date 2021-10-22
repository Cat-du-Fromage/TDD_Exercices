using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaizerWaldCode.PersistentData;
using KaizerWaldCode.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

using static KaizerWaldCode.Utils.UIUtils;

namespace KaizerWaldCode.UI.InGameMenu
{
    public class InGameMenuInteractions : MonoBehaviour
    {
        private KwControls controls;
        
        private UIDocument inGameMenuScreen;
        private VisualElement root;

        public Button ResumeButton;
        public Button SettingsButton;
        public Button MainMenuButton;
        public Button QuitButton;

        private void Awake()
        {
            inGameMenuScreen = gameObject.GetComponent<UIDocument>();
            root = inGameMenuScreen.rootVisualElement;
        }

        private void OnEnable()
        {
            KeyboardControls();
            GameObject parent = GameObject.Find("UIInGame");
            inGameMenuScreen = parent.GetComponentInChildren<UIDocument>();
            root = inGameMenuScreen.rootVisualElement;
            InitializeButtons();
            Debug.Log(GameSavesSingleton.inst);
        }

        private void Start()
        {
            inGameMenuScreen.gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            controls.Disable(); //If we don't disable it, we can access it outside of the scene!
        }

        private void InitializeButtons()
        {
            ResumeButton = InitButton(in root,"resume-button", OnResumeClick);
            SettingsButton = InitButton(in root,"settings-button");
            MainMenuButton = InitButton(in root,"mainMenu-button", OnMainMenuClick);
            QuitButton = InitButton(in root,"quit-button", OnQuit);
        }
        
        private void KeyboardControls()
        {
            if (controls is null)
            {
                controls = new KwControls();
                controls.Enable();
            }
            controls.UI.Cancel.performed += ctx => OnEscClick();
        }
        
        public void OnEscClick()
        {
            inGameMenuScreen.gameObject.SetActive(inGameMenuScreen.gameObject.activeSelf == false);
        }

        public void OnResumeClick()
        {
            inGameMenuScreen.gameObject.SetActive(false);
        }

        public void OnMainMenuClick()
        {
            Task clickTask = new Task(() => KwSceneUtils.Load(KwSceneUtils.KwScene.MainMenu));
            clickTask.RunSynchronously();
        }
    }
}
