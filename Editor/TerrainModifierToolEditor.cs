#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainModifierTool))]
public class TerrainModifierToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // デフォルトのインスペクターを描画
        DrawDefaultInspector();

        // TerrainModifierToolスクリプトのターゲット
        TerrainModifierTool script = (TerrainModifierTool)target;

        // ボタンを表示し、クリックされたらメソッドを実行
        if (GUILayout.Button("Modify Terrain Height"))
        {
            script.ModifyHeight();
        }
    }
}
#endif
