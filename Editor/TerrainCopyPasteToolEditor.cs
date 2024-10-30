using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainCopyPasteTool))]
public class TerrainCopyPasteToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 対象のスクリプトへの参照
        TerrainCopyPasteTool tool = (TerrainCopyPasteTool)target;

        // 通常のインスペクターの描画
        DrawDefaultInspector();

        // 範囲の形状選択
        tool.shape = (TerrainCopyPasteTool.CopyShape)EditorGUILayout.EnumPopup("Shape", tool.shape);

        // 矩形と円形の選択に応じて、表示するフィールドを変更
        if (tool.shape == TerrainCopyPasteTool.CopyShape.Rectangle)
        {
            // 矩形のとき、幅と高さを入力
            tool.width = EditorGUILayout.IntField("Width", tool.width);
            tool.height = EditorGUILayout.IntField("Height", tool.height);
        }
        else if (tool.shape == TerrainCopyPasteTool.CopyShape.Circle)
        {
            // 円形のとき、半径を入力
            tool.radius = EditorGUILayout.FloatField("Radius", tool.radius);
        }

        // インスペクターにボタンを追加して、コピー操作を実行
        if (GUILayout.Button("Copy Terrain Data"))
        {
            // ボタンが押されたときにCopyTerrainメソッドを呼び出す
            tool.CopyTerrain();
        }
    }
}
