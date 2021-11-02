using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Random = Unity.Mathematics.Random;
using static Unity.Mathematics.math;

namespace VisualDebugging
{
    public static class KwVisualTest
    {
        public static void TestIslandCoast(int[] samplesId, Vector3[] samplesPos, int mapSize, Random islandRandom)
        {
            VisualDebug.Clear();
            VisualDebug.Initialize();
            VisualDebug.BeginFrame("All points", true);
            VisualDebug.DrawPoints(samplesPos, .1f);
            for (int i = 0; i < samplesPos.Length; i++)
            {
                //samplesId[i] = RedBlobImplementation(123, samplesPos[i].xz, mSettings.mapSize) ? 1 : 0;
                
                float ISLAND_FACTOR = 1.27f; // 1.0 means no small islands; 2.0 leads to a lot
                float PI2 = PI*2;
            
                float midSize = mapSize / 2f;

                float x = 2f * ((samplesPos[i].x + midSize) / mapSize - 0.5f);
                float z = 2f * ((samplesPos[i].z + midSize) / mapSize - 0.5f);

                float3 point = new float3(x, 0, z);
                
                VisualDebug.BeginFrame("Point Location", true);
                VisualDebug.SetColour(Colours.lightRed, Colours.veryDarkGrey);
                VisualDebug.DrawPoint(point, .05f);

                int bumps = islandRandom.NextInt(1, 6);
                float startAngle = islandRandom.NextFloat(PI2); //radians 2 Pi = 360°
                float dipAngle = islandRandom.NextFloat(PI2);
                float dipWidth = islandRandom.NextFloat(0.2f, 0.7f); // = mapSize?

                float angle = atan2(point.z, point.x); // angle XZ
                const float lengthMul = 0.5f; // 0.1f : big island 1.0f = small island // by increasing by 0.1 island size is reduced by 1
                float totalLength = lengthMul * max(abs(point.x), abs(point.z)) + length(point); //(Mid Value from max component)
                
                VisualDebug.BeginFrame("totalLength", true);
                VisualDebug.DrawText(point + up(), $"Length {totalLength}");
                
                //Sin val Range[-1,1]
                float radialsBase = mad(bumps, angle, startAngle); // bump(1-6) * angle(0.x) + startangle(0.x)
                float r1Sin = sin(radialsBase + cos((bumps + 3) * angle));
                float r2Sin = sin(radialsBase + sin((bumps + 2) * angle));
                
                //VisualDebug.DrawText(point + up()*3, $"radialsBase {radialsBase}"); // Same

                VisualDebug.DrawLineSegment(point, point* r1Sin);
                VisualDebug.DrawLineSegment(point, point* r2Sin);
                
                VisualDebug.DrawText(point + up()*1.5f, $"r1Sin {r1Sin}");
                VisualDebug.DrawText(point + up()*2f, $"r2Sin {r2Sin}");
                
                float radial1 = 0.5f + 0.4f * r1Sin;
                float radial2 = 0.7f - 0.2f * r2Sin;
                VisualDebug.DrawText(point + up()*3, $"dipWidth {dipWidth}");
                
                //Not needed but improve generation

                if (   abs(angle - dipAngle) < dipWidth 
                    || abs(angle - dipAngle + PI2) < dipWidth 
                    || abs(angle - dipAngle - PI2) < dipWidth)
                {
                    radial1 = radial2 = 0.2f;
                }

                VisualDebug.DrawText(point + right()*1.5f, $"radial1 {radial1}");
                VisualDebug.DrawText(point + right()*2f + up(), $"radial2 {radial2}");
                
                VisualDebug.DrawLineSegment(point, point* radial1);
                VisualDebug.DrawLineSegment(point, point* radial2);

                samplesId[i] = totalLength < radial1 
                               || (totalLength > radial1 * ISLAND_FACTOR && totalLength < radial2) ? 1 : 0;
                
                if (samplesId[i] == 1)
                {
                    VisualDebug.SetColour(Colours.lightRed, Colours.darkGreen);
                }
                else
                {
                    VisualDebug.SetColour(Colours.lightRed, Colours.darkRed);
                }
                VisualDebug.DrawPoint(point, .05f);
            }

            VisualDebug.Save();
        }
    }
}