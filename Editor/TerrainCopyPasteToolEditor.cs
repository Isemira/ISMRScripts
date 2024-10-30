using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainCopyPasteTool))]
public class TerrainCopyPasteToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // �Ώۂ̃X�N���v�g�ւ̎Q��
        TerrainCopyPasteTool tool = (TerrainCopyPasteTool)target;

        // �ʏ�̃C���X�y�N�^�[�̕`��
        DrawDefaultInspector();

        // �͈͂̌`��I��
        tool.shape = (TerrainCopyPasteTool.CopyShape)EditorGUILayout.EnumPopup("Shape", tool.shape);

        // ��`�Ɖ~�`�̑I���ɉ����āA�\������t�B�[���h��ύX
        if (tool.shape == TerrainCopyPasteTool.CopyShape.Rectangle)
        {
            // ��`�̂Ƃ��A���ƍ��������
            tool.width = EditorGUILayout.IntField("Width", tool.width);
            tool.height = EditorGUILayout.IntField("Height", tool.height);
        }
        else if (tool.shape == TerrainCopyPasteTool.CopyShape.Circle)
        {
            // �~�`�̂Ƃ��A���a�����
            tool.radius = EditorGUILayout.FloatField("Radius", tool.radius);
        }

        // �C���X�y�N�^�[�Ƀ{�^����ǉ����āA�R�s�[��������s
        if (GUILayout.Button("Copy Terrain Data"))
        {
            // �{�^���������ꂽ�Ƃ���CopyTerrain���\�b�h���Ăяo��
            tool.CopyTerrain();
        }
    }
}
