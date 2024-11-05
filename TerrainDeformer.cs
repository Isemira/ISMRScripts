using UnityEngine;

namespace ISMR
{
    public class TerrainDeformer : MonoBehaviour
    {
        [SerializeField]
        private Terrain targetTerrain; // 変形するターゲットのTerrainオブジェクト

        [SerializeField]
        private GameObject[] deformObjects; // 影響を与えるオブジェクトのリスト

        [SerializeField]
        private float rayOriginHeight = 10000f; // レイの原点高さ

        private float[,] originalHeights; // 元のハイトマップデータ

        public void DeformTerrain()
        {
            if (targetTerrain == null || deformObjects.Length == 0)
            {
                UnityEngine.Debug.LogWarning("ターゲットのTerrainまたは影響を与えるオブジェクトが設定されていません。");
                return;
            }

            TerrainData terrainData = targetTerrain.terrainData;
            int heightmapWidth = terrainData.heightmapResolution;
            int heightmapHeight = terrainData.heightmapResolution;

            // Undo用の処理
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(terrainData, "Deform Terrain");
#endif

            // 初期ハイトマップを保存（初めての変形時のみ）
            if (originalHeights == null)
            {
                originalHeights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
            }

            // 現在のハイトマップをコピーして編集
            float[,] heights = (float[,])originalHeights.Clone();

            // Terrain上の各ポイントからレイを下向きに飛ばす
            for (int z = 0; z < heightmapHeight; z++)
            {
                for (int x = 0; x < heightmapWidth; x++)
                {
                    // 各ハイトマップポイントのワールド座標を計算
                    float worldPosX = targetTerrain.transform.position.x + (x / (float)heightmapWidth) * terrainData.size.x;
                    float worldPosZ = targetTerrain.transform.position.z + (z / (float)heightmapHeight) * terrainData.size.z;

                    // レイを原点高さから下向きに飛ばす
                    Ray ray = new Ray(new Vector3(worldPosX, rayOriginHeight, worldPosZ), Vector3.down);
                    bool hitDetected = false;

                    foreach (GameObject deformObject in deformObjects)
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit))
                        {
                            // 複数の対象オブジェクトのうち、最初に衝突したものを使用
                            if (hit.collider.gameObject == deformObject)
                            {
                                float targetHeight = hit.point.y / terrainData.size.y; // 地形のサイズに基づいて正規化
                                heights[z, x] = targetHeight; // 衝突点の高さにハイトマップを設定
                                hitDetected = true;
                                break;
                            }
                        }
                    }

                    if (!hitDetected)
                    {
                        // 対象オブジェクトにヒットしなかった場合、元の高さに戻す
                        heights[z, x] = originalHeights[z, x];
                    }
                }
            }

            terrainData.SetHeights(0, 0, heights);
            UnityEngine.Debug.Log("Terrainのハイトマップが変形されました。");
        }
    }
}
