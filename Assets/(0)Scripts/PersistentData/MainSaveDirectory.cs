using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KaizerWaldCode.PersistentData
{
    public class MainSaveDirectory
    {
        private const string MAIN_SAVES_NAME = "Game Saves";
        
        public readonly DirectoryInfo MainSaveDirInfo;

        //==========================================================================================================================================
        //Singleton Pattern
        //==========================================================================================================================================
        private static MainSaveDirectory instance;
        private MainSaveDirectory()
        {
            MainSaveDirInfo = Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, MAIN_SAVES_NAME));
        }
        public static MainSaveDirectory Instance
        {
            get
            {
                instance ??= new MainSaveDirectory();
                instance.CreateMainSave();
                return instance;
            }
        }
        /// <summary>
        /// Create Main Save Directory if it doesn't already exist
        /// </summary>
        private void CreateMainSave()
        {
            if (!Directory.Exists(MainSaveDirInfo.FullName))
            {
                MainSaveDirInfo.Create();
            }
        }
        //==========================================================================================================================================
        public void CreateNewSave(string saveName)
        {
            string fullPath = Path.Combine(MainSaveDirInfo.FullName, saveName);
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            MainSaveDirInfo.CreateSubdirectory(saveName);
        }
        
        public bool Exist() => Directory.Exists(MainSaveDirInfo.FullName);

        public int GetNumSubfolders() => MainSaveDirInfo.GetDirectories().Length;
    }
}

