#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

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
        public Shape selectedShape = Shape.Circle;  // 円形か矩形かを選択

        private TerrainData terrainData;
        private int terrainWidth;
        private int terrainHeight;

        public void ModifyHeight()
        {
            // TerrainDataを取得して高さマップのサイズを確認
            terrainData = terrain.terrainData;
            terrainWidth = terrainData.heightmapResolution;
            terrainHeight = terrainData.heightmapResolution;

            // Undoの対応（TerrainData全体の変更を記録）
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(terrainData, "Modify Terrain Height");
#endif

            // メートル単位の高さを正規化された値に変換
            float heightDelta = heightDeltaMeters / terrainData.size.y;

            // Terrainの全高マップを取得
            float[,] heights = terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);

            // Terrainのポジション（ワールド座標）
            Vector3 terrainPos = terrain.transform.position;

            // 中心座標をローカル座標に変換
            int centerX = Mathf.RoundToInt((center.x - terrainPos.x) / terrainData.size.x * terrainWidth);
            int centerZ = Mathf.RoundToInt((center.y - terrainPos.z) / terrainData.size.z * terrainHeight);

            if (selectedShape == Shape.Circle)
            {
                // 円形範囲内の地形の高さを相対的に変更
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
                                heights[z, x] += heightDelta;  // 高さを変更
                            }
                        }
                    }
                }
            }
            else if (selectedShape == Shape.Rectangle)
            {
                // 矩形範囲内の地形の高さを相対的に変更
                int rectWidth = Mathf.RoundToInt(rectSize.x / terrainData.size.x * terrainWidth);
                int rectHeight = Mathf.RoundToInt(rectSize.y / terrainData.size.z * terrainHeight);

                for (int x = centerX - rectWidth / 2; x <= centerX + rectWidth / 2; x++)
                {
                    for (int z = centerZ - rectHeight / 2; z <= centerZ + rectHeight / 2; z++)
                    {
                        if (x >= 0 && x < terrainWidth && z >= 0 && z < terrainHeight)
                        {
                            heights[z, x] += heightDelta;  // 高さを変更
                        }
                    }
                }
            }

            // 変更した高さをTerrainに反映
            terrainData.SetHeights(0, 0, heights);
        }

        // Gizmosを使って影響範囲を視覚的に表示
        void OnDrawGizmos()
        {
            if (terrain != null)
            {
                Gizmos.color = gizmoColor;
                Vector3 terrainPos = terrain.transform.position;

                // 中心のワールド座標を取得
                Vector3 centerWorldPos = new Vector3(center.x + terrainPos.x, terrain.SampleHeight(new Vector3(center.x, 0, center.y)) + terrainPos.y, center.y + terrainPos.z);

                if (selectedShape == Shape.Circle)
                {
                    // 円形の範囲を描画
                    Gizmos.DrawWireSphere(centerWorldPos, radius);
                }
                else if (selectedShape == Shape.Rectangle)
                {
                    // 矩形の範囲を描画
                    Gizmos.DrawWireCube(centerWorldPos, new Vector3(rectSize.x, 1, rectSize.y));
                }
            }
        }
    }
}

#endif