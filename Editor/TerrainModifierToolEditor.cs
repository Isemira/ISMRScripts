#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainModifierTool))]
public class TerrainModifierToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // �f�t�H���g�̃C���X�y�N�^�[��`��
        DrawDefaultInspector();

        // TerrainModifierTool�X�N���v�g�̃^�[�Q�b�g
        TerrainModifierTool script = (TerrainModifierTool)target;

        // �{�^����\�����A�N���b�N���ꂽ�烁�\�b�h�����s
        if (GUILayout.Button("Modify Terrain Height"))
        {
            script.ModifyHeight();
        }
    }
}
#endif
