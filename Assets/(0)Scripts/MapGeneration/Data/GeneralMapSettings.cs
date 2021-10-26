using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWaldCode.MapGeneration.Data
{
    [Serializable]
    public class GeneralMapSettings
    {
        public uint seed;
        public string saveName;
        public MapSettings mapSettings;
        public NoiseSettings noiseSettings;
    }
}