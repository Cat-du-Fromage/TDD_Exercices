using KaizerWaldCode.MapGeneration.Data;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace KaizerWaldCode.MapGeneration
{
    public class SamplesSettings
    {
        public int numCellPerAxis{ get; private set; }
        public float cellSize { get; private set;}
        
        public int totalNumCells { get; private set;}

        public SamplesSettings(MapSettings mapSettings, int numCells)
        {
            this.numCellPerAxis = min(numCells, mapSettings.mapSize);
            cellSize = (float)mapSettings.mapSize / numCells;
            totalNumCells = this.numCellPerAxis * this.numCellPerAxis;
        }

        public void CheckValues()
        {
            
        }
    }
}