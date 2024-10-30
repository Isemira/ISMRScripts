using UnityEditor;
using UnityEngine;

namespace ISMR
{
    [CustomEditor(typeof(RotateTerrainObjectsTool))]
    public class RotateTerrainObjectsToolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            RotateTerrainObjectsTool script = (RotateTerrainObjectsTool)target;
            if (GUILayout.Button("Rotate Terrain"))
            {
                Undo.RecordObject(script.terrain.terrainData, "Rotate Terrain");
                script.RotateTerrain();
            }
        }
    }
}
