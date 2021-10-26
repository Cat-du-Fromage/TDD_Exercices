using System.IO;
using UnityEngine;

namespace KaizerWaldCode.PersistentData
{
    public class MapSaveDirectory
    {
        private const string MAP_DATA_FOLDER = "MapData";
        
        private const string MAP_SETTINGS_FILE = "MapSettings.json";
        private string saveName;
        public DirectoryInfo mapSaveDirInfo { get; private set; }
        //==========================================================================================================================================
        //Singleton Pattern
        //==========================================================================================================================================
        private static MapSaveDirectory instance;
        private MapSaveDirectory(string currentSaveName)
        {
            this.saveName = currentSaveName;
            string currentSavePath = Path.Combine(MainSaveDirectory.Instance.MainSaveDirInfo.FullName, currentSaveName);
            mapSaveDirInfo = GetOrCreateSave(currentSavePath);
        }
        public static MapSaveDirectory Instance(string currentSaveName)
        {
            instance ??= new MapSaveDirectory(currentSaveName);
            return instance;
        }
        /// <summary>
        /// Create Main Save Directory if it doesn't already exist
        /// </summary>
        private DirectoryInfo GetOrCreateSave(string currentSavePath)
        {
            string mapFolderPath = Path.Combine(currentSavePath, MAP_DATA_FOLDER);
            if (!Directory.Exists(mapFolderPath))
            {
                mapSaveDirInfo = Directory.CreateDirectory(mapFolderPath);
            }
            return new DirectoryInfo(mapFolderPath);
        }
        //==========================================================================================================================================
        
        //==========================================================================================================================================
        //Methods
        //==========================================================================================================================================

        /// <summary>
        /// Get (or create if don't exist) the FileInfo for map settings
        /// </summary>
        /// <returns>(Json)MapSettings FileInfo</returns>
        public FileInfo GetOrCreateMapSettings()
        {
            string mapSettingsFilePath = Path.Combine(mapSaveDirInfo.FullName, MAP_SETTINGS_FILE);
            FileInfo file = new FileInfo(mapSettingsFilePath);
            if (!file.Exists)
            {
                file.Create();
            }
            return file;
        }
    }
}