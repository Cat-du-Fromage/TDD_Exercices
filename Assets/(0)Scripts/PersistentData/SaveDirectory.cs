using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KaizerWaldCode.PersistentData
{
    public class MainSaveDirectory
    {
        private const string GAME_SAVES_NAME = "Game Saves";
        
        public readonly DirectoryInfo MainSaveDirInfo;
        
        private MainSaveDirectory()
        {
            MainSaveDirInfo = Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, GAME_SAVES_NAME));
        }
        
        //Singelton Pattern
        private static MainSaveDirectory instance = null;
        public static MainSaveDirectory Instance
        {
            get
            {
                instance ??= new MainSaveDirectory();
                instance.CreateMainSave();
                return instance;
            }
        }

        private void CreateMainSave()
        {
            if (!Directory.Exists(MainSaveDirInfo.FullName))
            {
                MainSaveDirInfo.Create();
            }
        }
        
        public bool Exist() => Directory.Exists(MainSaveDirInfo.FullName);

        public int GetNumSubfolders() => MainSaveDirInfo.GetDirectories().Length;
    }
}

