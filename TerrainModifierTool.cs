#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace ISMR
{
    public class TerrainModifierTool : MonoBehaviour
    {
        public Terrain terrain;  // Terrain�I�u�W�F�N�g���A�^�b�`
        public Vector2 center;   // ���S�ix, z���j
        public float radius = 5f; // �e���͈͂̔��a�i�~�̏ꍇ�j
        public Vector2 rectSize = new Vector2(10f, 10f); // �e���͈͂̃T�C�Y�i��`�̏ꍇ�j
        public float heightDeltaMeters = 1.0f;  // �����̑����i���[�g���P�ʁj
        public Color gizmoColor = Color.red;  // �M�Y���̐F�i�f�t�H���g�ԁj

        public enum Shape { Circle, Rectangle }
        public Shape selectedShape = Shape.Circle;  // �~�`����`����I��

        private TerrainData terrainData;
        private int terrainWidth;
        private int terrainHeight;

        public void ModifyHeight()
        {
            // TerrainData���擾���č����}�b�v�̃T�C�Y���m�F
            terrainData = terrain.terrainData;
            terrainWidth = terrainData.heightmapResolution;
            terrainHeight = terrainData.heightmapResolution;

            // Undo�̑Ή��iTerrainData�S�̂̕ύX���L�^�j
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(terrainData, "Modify Terrain Height");
#endif

            // ���[�g���P�ʂ̍����𐳋K�����ꂽ�l�ɕϊ�
            float heightDelta = heightDeltaMeters / terrainData.size.y;

            // Terrain�̑S���}�b�v���擾
            float[,] heights = terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);

            // Terrain�̃|�W�V�����i���[���h���W�j
            Vector3 terrainPos = terrain.transform.position;

            // ���S���W�����[�J�����W�ɕϊ�
            int centerX = Mathf.RoundToInt((center.x - terrainPos.x) / terrainData.size.x * terrainWidth);
            int centerZ = Mathf.RoundToInt((center.y - terrainPos.z) / terrainData.size.z * terrainHeight);

            if (selectedShape == Shape.Circle)
            {
                // �~�`�͈͓��̒n�`�̍����𑊑ΓI�ɕύX
                int range = Mathf.RoundToInt(radius / terrainData.size.x * terrainWidth);
                for (int x = centerX - range; x <= centerX + range; x++)
                {
                    for (int z = centerZ - range; z <= centerZ + range; z++)
                    {
                        if (x >= 0 && x < terrainWidth && z >= 0 && z < terrainHeight)
                        {
                            float distance = Vector2.Distance(new Vector2(centerX, centerZ), new Vector2(x, z));
                            if (distance < range)
                            {
                                heights[z, x] += heightDelta;  // ������ύX
                            }
                        }
                    }
                }
            }
            else if (selectedShape == Shape.Rectangle)
            {
                // ��`�͈͓��̒n�`�̍����𑊑ΓI�ɕύX
                int rectWidth = Mathf.RoundToInt(rectSize.x / terrainData.size.x * terrainWidth);
                int rectHeight = Mathf.RoundToInt(rectSize.y / terrainData.size.z * terrainHeight);

                for (int x = centerX - rectWidth / 2; x <= centerX + rectWidth / 2; x++)
                {
                    for (int z = centerZ - rectHeight / 2; z <= centerZ + rectHeight / 2; z++)
                    {
                        if (x >= 0 && x < terrainWidth && z >= 0 && z < terrainHeight)
                        {
                            heights[z, x] += heightDelta;  // ������ύX
                        }
                    }
                }
            }

            // �ύX����������Terrain�ɔ��f
            terrainData.SetHeights(0, 0, heights);
        }

        // Gizmos���g���ĉe���͈͂����o�I�ɕ\��
        void OnDrawGizmos()
        {
            if (terrain != null)
            {
                Gizmos.color = gizmoColor;
                Vector3 terrainPos = terrain.transform.position;

                // ���S�̃��[���h���W���擾
                Vector3 centerWorldPos = new Vector3(center.x + terrainPos.x, terrain.SampleHeight(new Vector3(center.x, 0, center.y)) + terrainPos.y, center.y + terrainPos.z);

                if (selectedShape == Shape.Circle)
                {
                    // �~�`�͈̔͂�`��
                    Gizmos.DrawWireSphere(centerWorldPos, radius);
                }
                else if (selectedShape == Shape.Rectangle)
                {
                    // ��`�͈̔͂�`��
                    Gizmos.DrawWireCube(centerWorldPos, new Vector3(rectSize.x, 1, rectSize.y));
                }
            }
        }
    }
}

#endif