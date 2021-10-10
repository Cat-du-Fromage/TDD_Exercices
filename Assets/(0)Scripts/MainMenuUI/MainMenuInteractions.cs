using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.PersistentData;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizerWaldCode.UI.MainMenu
{
    public class MainMenuInteractions : MonoBehaviour
    {
        public Button NewGameButton;
        public Button ContinueButton;
        public Button LoadGameButton;
        public Button SettingsButton;
        public Button QuitButton;
        
        void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            NewGameButton = root.Q<Button>("newGame-button");
            ContinueButton = root.Q<Button>("continue-button");
            LoadGameButton = root.Q<Button>("loadGame-button");
            SettingsButton = root.Q<Button>("settings-button");
            QuitButton = root.Q<Button>("quit-button");

            NewGameButton.SetEnabled(true);
            ContinueButton.SetEnabled(IsContinueAvailable());
            LoadGameButton.SetEnabled(IsLoadAvailable());
        }

        public bool IsContinueAvailable()
        {
            bool available = MainSaveDirectory.Instance.GetNumSubfolders() > 0;
            if (ContinueButton.enabledSelf != available)
            {
                ContinueButton.SetEnabled(available);
            }
            return available;
        }
        
        public bool IsLoadAvailable()
        {
            bool available = MainSaveDirectory.Instance.GetNumSubfolders() > 0;
            if (LoadGameButton.enabledSelf != available)
            {
                LoadGameButton.SetEnabled(available);
            }
            return available;
        }
    }
}

