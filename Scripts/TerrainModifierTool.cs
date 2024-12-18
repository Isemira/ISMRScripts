using UnityEngine;

namespace ISMR
{
    public class TerrainModifierTool : MonoBehaviour
    {
        public Terrain terrain;  // Terrainオブジェクトをアタッチ
        public Vector2 center;   // 中心（x, z軸）
        public float radius = 5f; // 影響範囲の半径（円の場合）
        public Vector2 rectSize = new Vector2(10f, 10f); // 影響範囲のサイズ（矩形の場合）
        public float heightDeltaMeters = 1.0f;  // 高さの増減（メートル単位）
        public Color gizmoColor = Color.red;  // ギズモの色（デフォルト赤）

        public enum Shape { Circle, Rectangle }
        public enum GradientDirection { None, XPositive, XNegative, YPositive, YNegative }  // グラデーションの方向を設定
        public enum GradientType { Linear, Quadratic, SquareRoot } // グラデーションのパターン

        public Shape selectedShape = Shape.Circle;  // 円形か矩形かを選択
        public GradientDirection gradientDirection = GradientDirection.None;  // グラデーションの方向
        public GradientType gradientType = GradientType.Linear;  // グラデーションのパターン

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

            // グラデーションパターンに応じて非線形変換
            switch (gradientType)
            {
                case GradientType.Quadratic:
                    gradientFactor = Mathf.Pow(gradientFactor, 2);  // 二次変換
                    break;
                case GradientType.SquareRoot:
                    gradientFactor = Mathf.Sqrt(gradientFactor);  // 平方根変換
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
