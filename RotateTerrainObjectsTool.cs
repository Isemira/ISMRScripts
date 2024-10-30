using System.Collections.Generic;
using UnityEngine;

namespace ISMR
{
    public class RotateTerrainObjectsTool : MonoBehaviour
    {
        public Terrain terrain;  // 対象のTerrain
        public Vector2 center;   // 回転を適用する範囲の中心（XをX軸に、YをZ軸に対応）
        public float radius;     // 回転を適用する範囲の半径
        public float rotationAngle; // 回転角度（度単位）

        [Header("Gizmo Settings")]
        public Color gizmoColor = Color.magenta; // デフォルトの色を紫に設定

        // Terrain, Trees, and Grassを回転させる処理
        public void RotateTerrain()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Rotate Terrain Heightmap");
#endif
            RotateHeightmap();

#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Rotate Terrain Trees");
#endif
            RotateTrees();

#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Rotate Terrain Grass");
#endif
            RotateGrass();
        }

        // 高さデータを回転する
        void RotateHeightmap()
        {
            int heightmapWidth = terrain.terrainData.heightmapResolution;
            int heightmapHeight = terrain.terrainData.heightmapResolution;
            float[,] heights = terrain.terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

            float[,] rotatedHeights = new float[heightmapWidth, heightmapHeight];

            // centerをワールド座標に変換しY軸を計算し、X と Z を入れ替えた中心を設定
            Vector3 centerWorldPos = new Vector3(center.y, 0, center.x) + terrain.transform.position;
            centerWorldPos.y = terrain.SampleHeight(centerWorldPos);

            for (int x = 0; x < heightmapWidth; x++)
            {
                for (int y = 0; y < heightmapHeight; y++)
                {
                    Vector3 worldPos = new Vector3(
                        x / (float)heightmapWidth * terrain.terrainData.size.x + terrain.transform.position.x,
                        0,
                        y / (float)heightmapHeight * terrain.terrainData.size.z + terrain.transform.position.z
                    );
                    worldPos.y = terrain.SampleHeight(worldPos);

                    if (Vector3.Distance(worldPos, centerWorldPos) <= radius)
                    {
                        Vector3 direction = worldPos - centerWorldPos;
                        float angleRad = rotationAngle * Mathf.Deg2Rad;
                        float cosAngle = Mathf.Cos(angleRad);
                        float sinAngle = Mathf.Sin(angleRad);

                        float newX = direction.x * cosAngle - direction.z * sinAngle;
                        float newZ = direction.x * sinAngle + direction.z * cosAngle;

                        Vector3 rotatedPos = new Vector3(newX, 0, newZ) + centerWorldPos;

                        int newXIndex = Mathf.Clamp(Mathf.RoundToInt((rotatedPos.x - terrain.transform.position.x) / terrain.terrainData.size.x * heightmapWidth), 0, heightmapWidth - 1);
                        int newYIndex = Mathf.Clamp(Mathf.RoundToInt((rotatedPos.z - terrain.transform.position.z) / terrain.terrainData.size.z * heightmapHeight), 0, heightmapHeight - 1);

                        rotatedHeights[newXIndex, newYIndex] = heights[x, y];
                    }
                    else
                    {
                        rotatedHeights[x, y] = heights[x, y];
                    }
                }
            }

            terrain.terrainData.SetHeights(0, 0, rotatedHeights);
        }

        // 木の回転
        void RotateTrees()
        {
            TreeInstance[] trees = terrain.terrainData.treeInstances;
            List<TreeInstance> rotatedTrees = new List<TreeInstance>();

            // centerをワールド座標に変換しY軸を計算し、X と Z を入れ替えた中心を設定
            Vector3 centerWorldPos = new Vector3(center.y, 0, center.x) + terrain.transform.position;
            centerWorldPos.y = terrain.SampleHeight(centerWorldPos);

            for (int i = 0; i < trees.Length; i++)
            {
                TreeInstance tree = trees[i];
                Vector3 treeWorldPos = Vector3.Scale(tree.position, terrain.terrainData.size) + terrain.transform.position;

                if (Vector3.Distance(treeWorldPos, centerWorldPos) <= radius)
                {
                    Vector3 direction = treeWorldPos - centerWorldPos;
                    float angleRad = rotationAngle * Mathf.Deg2Rad;
                    float cosAngle = Mathf.Cos(angleRad);
                    float sinAngle = Mathf.Sin(angleRad);

                    float newX = direction.x * cosAngle - direction.z * sinAngle;
                    float newZ = direction.x * sinAngle + direction.z * cosAngle;

                    Vector3 rotatedPosition = new Vector3(newX, direction.y, newZ) + centerWorldPos;

                    tree.position = Vector3.Scale(rotatedPosition - terrain.transform.position, new Vector3(1 / terrain.terrainData.size.x, 1 / terrain.terrainData.size.y, 1 / terrain.terrainData.size.z));

                    rotatedTrees.Add(tree);
                }
                else
                {
                    rotatedTrees.Add(tree);
                }
            }

            terrain.terrainData.treeInstances = rotatedTrees.ToArray();
        }

        // 草の回転
        void RotateGrass()
        {
            int detailLayerCount = terrain.terrainData.detailPrototypes.Length;

            // centerをワールド座標に変換しY軸を計算し、X と Z を入れ替えた中心を設定
            Vector3 centerWorldPos = new Vector3(center.y, 0, center.x) + terrain.transform.position;
            centerWorldPos.y = terrain.SampleHeight(centerWorldPos);

            for (int i = 0; i < detailLayerCount; i++)
            {
                int[,] detailLayer = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, i);

                for (int x = 0; x < terrain.terrainData.detailWidth; x++)
                {
                    for (int y = 0; y < terrain.terrainData.detailHeight; y++)
                    {
                        Vector3 grassWorldPos = new Vector3(
                            x / (float)terrain.terrainData.detailWidth * terrain.terrainData.size.x + terrain.transform.position.x,
                            0,
                            y / (float)terrain.terrainData.detailHeight * terrain.terrainData.size.z + terrain.transform.position.z
                        );
                        grassWorldPos.y = terrain.SampleHeight(grassWorldPos);

                        if (Vector3.Distance(grassWorldPos, centerWorldPos) <= radius)
                        {
                            // 必要に応じて草の回転や密度を変更
                        }
                    }
                }

                terrain.terrainData.SetDetailLayer(0, 0, i, detailLayer);
            }
        }

        // 効果範囲のギズモ表示
        void OnDrawGizmos()
        {
            Vector3 centerWorldPos = new Vector3(center.x, 0, center.y) + terrain.transform.position;
            centerWorldPos.y = terrain.SampleHeight(centerWorldPos);

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(centerWorldPos, radius);
        }
    }
}
