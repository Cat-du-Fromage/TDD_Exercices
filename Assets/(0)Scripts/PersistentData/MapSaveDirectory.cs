using System.IO;
using UnityEngine;

namespace KaizerWaldCode.PersistentData
{
    public class MapSaveDirectory
    {
        //FIELDS
        //==========================================================================================================================================
        private const string MAP_DATA_FOLDER = "MapData";
        
        private const string MAP_SETTINGS_FILE = "MapSettings.json";
        private string saveName;
        private static string currentSavePath;
        public static DirectoryInfo mapSaveDirInfo { get; private set; }
        
        //SINGLETON PATTERN
        //==========================================================================================================================================
        private static MapSaveDirectory instance;
        private MapSaveDirectory(string currentSavePath) => mapSaveDirInfo = CreateSave();
        
        public static MapSaveDirectory Instance(string currentSaveName)
        {
            currentSavePath = Path.Combine(MainSaveDirectory.Instance.MainSaveDirInfo.FullName, currentSaveName);
            instance ??= new MapSaveDirectory(currentSavePath);
            mapSaveDirInfo = CreateSave();
            return instance;
        }
        /// <summary>
        /// Create Main Save Directory if it doesn't already exist
        /// </summary>
        private static DirectoryInfo CreateSave()
        {
            string mapFolderPath = Path.Combine(currentSavePath, MAP_DATA_FOLDER);
            if (!Directory.Exists(mapFolderPath))
            {
                mapSaveDirInfo = Directory.CreateDirectory(mapFolderPath);
            }
            return new DirectoryInfo(mapFolderPath);
        }

        public DirectoryInfo GetCurrentSave() => mapSaveDirInfo;
        
        //METHODS
        //==========================================================================================================================================
        /// <summary>
        /// Get (or create if don't exist) the FileInfo for map settings
        /// </summary>
        /// <returns>(Json)MapSettings FileInfo</returns>
        public FileInfo GetOrCreateMapSettings()
        {
            string mapSettingsFilePath = Path.Combine(mapSaveDirInfo.FullName, MAP_SETTINGS_FILE);
            if (!File.Exists(mapSettingsFilePath))
            {
                File.Create(mapSettingsFilePath).Close();
            }
            return new FileInfo(mapSettingsFilePath);
        }
    }
}