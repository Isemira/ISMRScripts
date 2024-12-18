using UnityEngine;

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
        public enum GradientDirection { None, XPositive, XNegative, YPositive, YNegative }  // �O���f�[�V�����̕�����ݒ�
        public enum GradientType { Linear, Quadratic, SquareRoot } // �O���f�[�V�����̃p�^�[��

        public Shape selectedShape = Shape.Circle;  // �~�`����`����I��
        public GradientDirection gradientDirection = GradientDirection.None;  // �O���f�[�V�����̕���
        public GradientType gradientType = GradientType.Linear;  // �O���f�[�V�����̃p�^�[��

        private TerrainData terrainData;
        private int terrainWidth;
        private int terrainHeight;

        public void ModifyHeight()
        {
            terrainData = terrain.terrainData;
            terrainWidth = terrainData.heightmapResolution;
            terrainHeight = terrainData.heightmapResolution;

#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCompleteObjectUndo(terrainData, "Modify Terrain Height");
#endif

            float heightDelta = heightDeltaMeters / terrainData.size.y;
            float[,] heights = terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);

            Vector3 terrainPos = terrain.transform.position;
            int centerX = Mathf.RoundToInt((center.x - terrainPos.x) / terrainData.size.x * terrainWidth);
            int centerZ = Mathf.RoundToInt((center.y - terrainPos.z) / terrainData.size.z * terrainHeight);

            if (selectedShape == Shape.Circle)
            {
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
                                float gradientFactor = CalculateGradientFactor(x, z, centerX, centerZ, range);
                                heights[z, x] += heightDelta * gradientFactor;
                            }
                        }
                    }
                }
            }
            else if (selectedShape == Shape.Rectangle)
            {
                int rectWidth = Mathf.RoundToInt(rectSize.x / terrainData.size.x * terrainWidth);
                int rectHeight = Mathf.RoundToInt(rectSize.y / terrainData.size.z * terrainHeight);

                for (int x = centerX - rectWidth / 2; x <= centerX + rectWidth / 2; x++)
                {
                    for (int z = centerZ - rectHeight / 2; z <= centerZ + rectHeight / 2; z++)
                    {
                        if (x >= 0 && x < terrainWidth && z >= 0 && z < terrainHeight)
                        {
                            float gradientFactor = CalculateGradientFactor(x, z, centerX, centerZ, Mathf.Max(rectWidth, rectHeight) / 2);
                            heights[z, x] += heightDelta * gradientFactor;
                        }
                    }
                }
            }

            terrainData.SetHeights(0, 0, heights);
        }

        private float CalculateGradientFactor(int x, int z, int centerX, int centerZ, int range)
        {
            float gradientFactor = 1.0f;

            switch (gradientDirection)
            {
                case GradientDirection.XPositive:
                    gradientFactor = Mathf.InverseLerp(centerX - range, centerX + range, x);
                    break;
                case GradientDirection.XNegative:
                    gradientFactor = Mathf.InverseLerp(centerX + range, centerX - range, x);
                    break;
                case GradientDirection.YPositive:
                    gradientFactor = Mathf.InverseLerp(centerZ - range, centerZ + range, z);
                    break;
                case GradientDirection.YNegative:
                    gradientFactor = Mathf.InverseLerp(centerZ + range, centerZ - range, z);
                    break;
                default:
                    gradientFactor = 1.0f;
                    break;
            }

            // �O���f�[�V�����p�^�[���ɉ����Ĕ���`�ϊ�
            switch (gradientType)
            {
                case GradientType.Quadratic:
                    gradientFactor = Mathf.Pow(gradientFactor, 2);  // �񎟕ϊ�
                    break;
                case GradientType.SquareRoot:
                    gradientFactor = Mathf.Sqrt(gradientFactor);  // �������ϊ�
                    break;
                case GradientType.Linear:
                default:
                    break;
            }

            return gradientFactor;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (terrain != null)
            {
                Gizmos.color = gizmoColor;
                Vector3 terrainPos = terrain.transform.position;
                Vector3 centerWorldPos = new Vector3(center.x + terrainPos.x, terrain.SampleHeight(new Vector3(center.x, 0, center.y)) + terrainPos.y, center.y + terrainPos.z);

                if (selectedShape == Shape.Circle)
                {
                    Gizmos.DrawWireSphere(centerWorldPos, radius);
                }
                else if (selectedShape == Shape.Rectangle)
                {
                    Gizmos.DrawWireCube(centerWorldPos, new Vector3(rectSize.x, 1, rectSize.y));
                }
            }
        }
#endif
    }
}
