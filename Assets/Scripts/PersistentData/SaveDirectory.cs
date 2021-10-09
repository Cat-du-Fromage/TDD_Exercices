using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KaizerWaldCode.PersistentData
{
    public static class SaveDirectory
    {
        private const string GAME_SAVES_NAME = @"\Game Saves";
        
        public static readonly DirectoryInfo GameSavesPath;
        // Start is called before the first frame update
        static SaveDirectory()
        {
            GameSavesPath = Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, GAME_SAVES_NAME));
        }
    }
}

