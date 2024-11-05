using UnityEditor;
using UnityEngine;

namespace ISMR
{
    [CustomEditor(typeof(TerrainDeformer))]
    public class TerrainDeformerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TerrainDeformer terrainDeformer = (TerrainDeformer)target;

            if (GUILayout.Button("Deform Terrain"))
            {
                terrainDeformer.DeformTerrain();
            }
        }
    }
}
