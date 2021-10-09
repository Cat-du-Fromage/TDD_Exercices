using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        
        void Awake()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            NewGameButton = root.Q<Button>("newGame-button");
            ContinueButton = root.Q<Button>("continue-button");
            LoadGameButton = root.Q<Button>("loadGame-button");
            SettingsButton = root.Q<Button>("settings-button");
            QuitButton = root.Q<Button>("quit-button");

            NewGameButton.SetEnabled(true);
        }

        public bool IsContinueAvailable()
        {
            //1 : Check in Application.PersistentData if Directory "Game Saves" Exist?
            //2 : if Yes : Check if the number of folders inside "Game Saves" > 0; 
            return true;
        }
    }
}

