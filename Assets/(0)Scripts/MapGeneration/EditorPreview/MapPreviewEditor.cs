using System;
using System.Collections;
using System.Collections.Generic;
using KaizerWaldCode.MapGeneration;
using KaizerWaldCode.UI.MainMenu;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration.EditorPreview
{

    [CustomEditor(typeof(MapPreview))]
    public class MapPreviewEditor : Editor
    {
        public MainMenuInteractions mainMenu;
        
        public override void OnInspectorGUI()
        {
            MapPreview mapPreview = (MapPreview)target;
            if (mainMenu is null)
            {
                mainMenu = mapPreview.mainMenu is null ? GameObject.Find("UIMainMenu").GetComponent<MainMenuInteractions>() : mapPreview.mainMenu;
            }
            if (DrawDefaultInspector())
            {
                if(mapPreview.autoUpdate == true)
                {
                    mapPreview.DrawMapInEditor();
                }

                if (mapPreview.syncUI)
                {
                    mapPreview.SyncWithUI(ref mainMenu.generalInputs, ref mainMenu.mapInputs, ref mainMenu.noiseInputs, ref mainMenu.regions);
                }
            }
    
            if(GUILayout.Button("Generate"))
            {
                mapPreview.DrawMapInEditor();
                //IslandGenerator.GetCellsBounds(mainMenu.mapInputs.chunkSize * mainMenu.mapInputs.numChunk, 4);
            }
            
            if(GUILayout.Button("Reset"))
            {
                mapPreview.ResetMap();
            }
            
        }
    }
}