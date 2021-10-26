using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

namespace KaizerWaldCode.MapGeneration
{
    public class BuildNavMesh : MonoBehaviour
    {
        private NavMeshSurface navMeshSurface;
        void Awake()
        {
            navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            navMeshSurface.BuildNavMesh();
        }
    }
}