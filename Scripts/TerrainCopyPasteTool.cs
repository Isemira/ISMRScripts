using UnityEngine;
using System.Collections.Generic;

namespace ISMR
{
    public class TerrainCopyPasteTool : MonoBehaviour
    {
        public enum CopyShape { Rectangle, Circle }  // 範囲の形状
        public CopyShape shape = CopyShape.Rectangle; // デフォルトは矩形

        public Terrain sourceTerrain;                // コピー元のTerrain
        public Terrain destinationTerrain;           // コピー先のTerrain
        public Vector2 sourceCenterLocalPosition;    // コピー元の範囲の中心座標（ローカル）
        public Vector2 destinationCenterLocalPosition; // コピー先の範囲の中心座標（ローカル）
        public int width, height;                    // 矩形の場合の幅と高さ（グリッド単位）
        public float radius;                         // 円形の場合の半径
        public float heightOffset = 0f;              // コピー時に高さに加算するオフセット

        public Color sourceGizmoColor = Color.green;   // コピー元の範囲を表示するGizmoの色
        public Color destinationGizmoColor = Color.blue; // コピー先の範囲を表示するGizmoの色

        // コピー実行メソッド
        public void CopyTerrain()
        {
            if (sourceTerrain == null || destinationTerrain == null)
            {
                UnityEngine.Debug.LogError("Source Terrain or Destination Terrain is not assigned.");
                return;
            }

            // Undo操作の記録開始
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCompleteObjectUndo(destinationTerrain.terrainData, "Copy Terrain Data");
#endif

            // 1. コピー元とコピー先の中心座標からグリッド座標を取得
            Vector2Int sourceCoords = LocalToTerrainCoordinates(sourceTerrain, sourceCenterLocalPosition);
            Vector2Int destinationCoords = LocalToTerrainCoordinates(destinationTerrain, destinationCenterLocalPosition);

            // 2. 地形データのコピー
            if (shape == CopyShape.Rectangle)
            {
                CopyRectangle(sourceCoords, destinationCoords);
            }
            else if (shape == CopyShape.Circle)
            {
                CopyCircle(sourceCoords, destinationCoords);
            }

            UnityEngine.Debug.Log("Terrain data copied successfully.");
        }

        // 矩形範囲のコピー
        private void CopyRectangle(Vector2Int sourceCenter, Vector2Int destinationCenter)
        {
            int sourceX = sourceCenter.x - width / 2;
            int sourceY = sourceCenter.y - height / 2;
            int destinationX = destinationCenter.x - width / 2;
            int destinationY = destinationCenter.y - height / 2;

            // 1. 高さデータをコピーし、オフセットを加算
            float[,] heights = sourceTerrain.terrainData.GetHeights(sourceX, sourceY, width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    heights[y, x] += heightOffset / sourceTerrain.terrainData.size.y; // オフセットを追加
                }
            }
            destinationTerrain.terrainData.SetHeights(destinationX, destinationY, heights);

            // 2. 草のデータをコピー
            for (int i = 0; i < sourceTerrain.terrainData.detailPrototypes.Length; i++)
            {
                int[,] detailLayer = sourceTerrain.terrainData.GetDetailLayer(sourceX, sourceY, width, height, i);
                destinationTerrain.terrainData.SetDetailLayer(destinationX, destinationY, i, detailLayer);
            }

            // 3. 木のデータをコピー
            TreeInstance[] sourceTreeInstances = sourceTerrain.terrainData.treeInstances;
            List<TreeInstance> copiedTrees = new List<TreeInstance>();

            foreach (TreeInstance tree in sourceTreeInstances)
            {
                if (IsTreeInRectangle(tree.position, sourceX, sourceY, width, height, sourceTerrain))
                {
                    copiedTrees.Add(tree);
                }
            }

            destinationTerrain.terrainData.treeInstances = copiedTrees.ToArray();

            // 4. 地面のテクスチャ（SplatMap）をコピー
            int splatMapWidth = width;
            int splatMapHeight = height;
            float[,,] splatMap = sourceTerrain.terrainData.GetAlphamaps(sourceX, sourceY, splatMapWidth, splatMapHeight);
            destinationTerrain.terrainData.SetAlphamaps(destinationX, destinationY, splatMap);
        }

        // 円形範囲のコピー
        private void CopyCircle(Vector2Int sourceCenter, Vector2Int destinationCenter)
        {
            int radiusInGridUnits = Mathf.FloorToInt(radius / (sourceTerrain.terrainData.size.x / sourceTerrain.terrainData.heightmapResolution));

            // 1. 高さデータを円形にコピーし、オフセットを加算
            for (int x = -radiusInGridUnits; x <= radiusInGridUnits; x++)
            {
                for (int y = -radiusInGridUnits; y <= radiusInGridUnits; y++)
                {
                    if (x * x + y * y <= radiusInGridUnits * radiusInGridUnits)
                    {
                        int sourceX = sourceCenter.x + x;
                        int sourceY = sourceCenter.y + y;
                        int destinationX = destinationCenter.x + x;
                        int destinationY = destinationCenter.y + y;

                        float[,] heights = sourceTerrain.terrainData.GetHeights(sourceX, sourceY, 1, 1);
                        heights[0, 0] += heightOffset / sourceTerrain.terrainData.size.y; // オフセットを追加
                        destinationTerrain.terrainData.SetHeights(destinationX, destinationY, heights);

                        // SplatMapのコピー
                        float[,,] splatMap = sourceTerrain.terrainData.GetAlphamaps(sourceX, sourceY, 1, 1);
                        destinationTerrain.terrainData.SetAlphamaps(destinationX, destinationY, splatMap);
                    }
                }
            }

            // 2. 草や木のデータも同様に円形範囲内を処理
            CopyCircleDetails(sourceCenter, destinationCenter, radiusInGridUnits);
        }

        // 草や木を円形範囲でコピー
        private void CopyCircleDetails(Vector2Int sourceCenter, Vector2Int destinationCenter, int radiusInGridUnits)
        {
            for (int i = 0; i < sourceTerrain.terrainData.detailPrototypes.Length; i++)
            {
                for (int x = -radiusInGridUnits; x <= radiusInGridUnits; x++)
                {
                    for (int y = -radiusInGridUnits; y <= radiusInGridUnits; y++)
                    {
                        if (x * x + y * y <= radiusInGridUnits * radiusInGridUnits)
                        {
                            int sourceX = sourceCenter.x + x;
                            int sourceY = sourceCenter.y + y;
                            int destinationX = destinationCenter.x + x;
                            int destinationY = destinationCenter.y + y;

                            int[,] detailLayer = sourceTerrain.terrainData.GetDetailLayer(sourceX, sourceY, 1, 1, i);
                            destinationTerrain.terrainData.SetDetailLayer(destinationX, destinationY, i, detailLayer);
                        }
                    }
                }
            }

            // 木のコピー（円形範囲内）
            TreeInstance[] sourceTreeInstances = sourceTerrain.terrainData.treeInstances;
            List<TreeInstance> copiedTrees = new List<TreeInstance>();

            foreach (TreeInstance tree in sourceTreeInstances)
            {
                if (IsTreeInCircle(tree.position, sourceCenter, radiusInGridUnits, sourceTerrain))
                {
                    copiedTrees.Add(tree);
                }
            }

            destinationTerrain.terrainData.treeInstances = copiedTrees.ToArray();
        }

        // ローカル座標をグリッド座標に変換する
        public Vector2Int LocalToTerrainCoordinates(Terrain terrain, Vector2 localPosition)
        {
            TerrainData terrainData = terrain.terrainData;

            float gridSizeX = terrainData.size.x / terrainData.heightmapResolution;
            float gridSizeZ = terrainData.size.z / terrainData.heightmapResolution;

            int coordX = Mathf.FloorToInt(localPosition.x / gridSizeX);
            int coordZ = Mathf.FloorToInt(localPosition.y / gridSizeZ);

            return new Vector2Int(coordX, coordZ);
        }

        // 矩形範囲内に木があるか判定
        public bool IsTreeInRectangle(Vector3 treePosition, int startX, int startY, int width, int height, Terrain terrain)
        {
            Vector2Int treeCoords = LocalToTerrainCoordinates(terrain, new Vector2(treePosition.x, treePosition.z));
            return treeCoords.x >= startX && treeCoords.x < startX + width && treeCoords.y >= startY && treeCoords.y < startY + height;
        }

        // 円形範囲内に木があるか判定
        public bool IsTreeInCircle(Vector3 treePosition, Vector2Int center, int radiusInGridUnits, Terrain terrain)
        {
            Vector2Int treeCoords = LocalToTerrainCoordinates(terrain, new Vector2(treePosition.x, treePosition.z));
            int dx = treeCoords.x - center.x;
            int dy = treeCoords.y - center.y;
            return (dx * dx + dy * dy) <= (radiusInGridUnits * radiusInGridUnits);
        }

        // シーンビューに範囲を表示するGizmos描画
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (sourceTerrain != null)
            {
                Gizmos.color = sourceGizmoColor;
                DrawGizmoForShape(sourceTerrain, sourceCenterLocalPosition, width, height, radius);
            }

            if (destinationTerrain != null)
            {
                Gizmos.color = destinationGizmoColor;
                DrawGizmoForShape(destinationTerrain, destinationCenterLocalPosition, width, height, radius);
            }
        }

        // 選択した形状（矩形または円形）に基づいてGizmosを描画
        private void DrawGizmoForShape(Terrain terrain, Vector2 centerLocalPosition, int width, int height, float radius)
        {
            if (shape == CopyShape.Rectangle)
            {
                DrawRectangleGizmo(terrain, centerLocalPosition, width, height);
            }
            else if (shape == CopyShape.Circle)
            {
                DrawCircleGizmo(terrain, centerLocalPosition, radius);
            }
        }

        // 矩形の範囲をシーンビューにGizmosで描画
        private void DrawRectangleGizmo(Terrain terrain, Vector2 centerLocalPosition, int width, int height)
        {
            Vector3 worldCenterPosition = terrain.transform.position + new Vector3(centerLocalPosition.x, 0, centerLocalPosition.y);
            float terrainHeight = terrain.SampleHeight(worldCenterPosition) + terrain.transform.position.y;

            // 矩形の四隅を計算
            Vector3 startWorldPosition = new Vector3(
                worldCenterPosition.x - (width * terrain.terrainData.size.x / terrain.terrainData.heightmapResolution) / 2,
                terrainHeight,
                worldCenterPosition.z - (height * terrain.terrainData.size.z / terrain.terrainData.heightmapResolution) / 2
            );

            Vector3 endWorldPosition = new Vector3(
                worldCenterPosition.x + (width * terrain.terrainData.size.x / terrain.terrainData.heightmapResolution) / 2,
                terrainHeight,
                worldCenterPosition.z + (height * terrain.terrainData.size.z / terrain.terrainData.heightmapResolution) / 2
            );

            // 矩形の範囲を描画
            Gizmos.DrawLine(startWorldPosition, new Vector3(endWorldPosition.x, terrainHeight, startWorldPosition.z));
            Gizmos.DrawLine(startWorldPosition, new Vector3(startWorldPosition.x, terrainHeight, endWorldPosition.z));
            Gizmos.DrawLine(new Vector3(endWorldPosition.x, terrainHeight, startWorldPosition.z), endWorldPosition);
            Gizmos.DrawLine(new Vector3(startWorldPosition.x, terrainHeight, endWorldPosition.z), endWorldPosition);
        }

        // 円形の範囲をシーンビューにGizmosで描画
        private void DrawCircleGizmo(Terrain terrain, Vector2 centerLocalPosition, float radius)
        {
            Vector3 worldCenterPosition = terrain.transform.position + new Vector3(centerLocalPosition.x, 0, centerLocalPosition.y);
            float terrainHeight = terrain.SampleHeight(worldCenterPosition) + terrain.transform.position.y;

            // 中心座標をワールド座標に変換
            Vector3 centerWorldPosition = new Vector3(
                worldCenterPosition.x,
                terrainHeight,
                worldCenterPosition.z
            );

            // 円を描画
            Gizmos.DrawWireSphere(centerWorldPosition, radius);
        }
#endif
    }
}
