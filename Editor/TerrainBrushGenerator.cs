using UnityEditor;
using UnityEngine;

namespace ISMR
{
    public class TerrainBrushGenerator : EditorWindow
    {
        private enum Algorithm
        {
            PerlinNoise,
            FBM
        }

        private enum GradientType
        {
            Linear,
            Quadratic,
            SquareRoot
        }

        private Algorithm selectedAlgorithm = Algorithm.PerlinNoise;
        private GradientType selectedGradientType = GradientType.Linear;
        private int resolution = 256;  // ハイトマップの解像度
        private float scale = 20f;     // ノイズのスケール
        private int octaves = 4;       // fBm用のオクターブ数
        private float persistence = 0.5f;  // fBmの振幅の減衰
        private float lacunarity = 2.0f;   // fBmの周波数の増加
        private int seed = 0;  // 乱数シード
        private bool applyEdgeGradient = true;  // エッジグラデーション補正のオンオフ

        private Texture2D previewTexture;  // プレビュー用のテクスチャ

        [MenuItem("Tools/Terrain Brush Generator")]
        public static void ShowWindow()
        {
            GetWindow<TerrainBrushGenerator>("Terrain Brush Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Terrain Brush Generator", EditorStyles.boldLabel);

            selectedAlgorithm = (Algorithm)EditorGUILayout.EnumPopup("Algorithm", selectedAlgorithm);
            resolution = EditorGUILayout.IntField("Resolution", resolution);
            scale = EditorGUILayout.FloatField("Scale", scale);
            seed = EditorGUILayout.IntField("Seed", seed);

            if (selectedAlgorithm == Algorithm.FBM)
            {
                octaves = EditorGUILayout.IntField("Octaves", octaves);
                persistence = EditorGUILayout.FloatField("Persistence", persistence);
                lacunarity = EditorGUILayout.FloatField("Lacunarity", lacunarity);
            }

            applyEdgeGradient = EditorGUILayout.Toggle("Apply Edge Gradient", applyEdgeGradient);

            if (applyEdgeGradient)
            {
                selectedGradientType = (GradientType)EditorGUILayout.EnumPopup("Gradient Type", selectedGradientType);
            }

            if (GUILayout.Button("Generate Brush Texture"))
            {
                Texture2D texture = GenerateBrushTexture();
                SaveBrushTexture(texture);
            }

            if (GUILayout.Button("Preview"))
            {
                previewTexture = GenerateBrushTexture();
            }

            if (previewTexture != null)
            {
                GUILayout.Label("Preview", EditorStyles.boldLabel);
                GUILayout.Label(previewTexture, GUILayout.Width(256), GUILayout.Height(256));
            }
        }

        private Texture2D GenerateBrushTexture()
        {
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float[,] heightMap = new float[resolution, resolution];

            // 乱数シードを設定
            UnityEngine.Random.InitState(seed);

            switch (selectedAlgorithm)
            {
                case Algorithm.PerlinNoise:
                    heightMap = GeneratePerlinNoise();
                    break;
                case Algorithm.FBM:
                    heightMap = GenerateFBM();
                    break;
            }

            if (applyEdgeGradient)
            {
                ApplyEdgeGradient(heightMap);
            }

            // テクスチャに円形のアルファチャンネルを適用
            int centerX = resolution / 2;
            int centerY = resolution / 2;
            float maxDistance = resolution / 2.0f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float value = heightMap[x, y];
                    float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    float alpha = Mathf.Clamp01(1 - distance / maxDistance);
                    texture.SetPixel(x, y, new Color(value, value, value, alpha));
                }
            }

            texture.Apply();
            return texture;
        }

        private float[,] GeneratePerlinNoise()
        {
            float[,] heightMap = new float[resolution, resolution];

            float offsetX = UnityEngine.Random.Range(-1000f, 1000f);
            float offsetY = UnityEngine.Random.Range(-1000f, 1000f);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float sampleX = (x + offsetX) / resolution * scale;
                    float sampleY = (y + offsetY) / resolution * scale;
                    heightMap[x, y] = Mathf.PerlinNoise(sampleX, sampleY);
                }
            }

            return heightMap;
        }

        private float[,] GenerateFBM()
        {
            float[,] heightMap = new float[resolution, resolution];

            float offsetX = UnityEngine.Random.Range(-1000f, 1000f);
            float offsetY = UnityEngine.Random.Range(-1000f, 1000f);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x + offsetX) / resolution * scale * frequency;
                        float sampleY = (y + offsetY) / resolution * scale * frequency;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    heightMap[x, y] = Mathf.InverseLerp(-1, 1, noiseHeight);
                }
            }

            return heightMap;
        }

        private void ApplyEdgeGradient(float[,] heightMap)
        {
            int centerX = resolution / 2;
            int centerY = resolution / 2;
            float maxDistance = resolution / 2.0f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    float gradient = 1 - distance / maxDistance;

                    switch (selectedGradientType)
                    {
                        case GradientType.Linear:
                            gradient = Mathf.Clamp01(gradient);
                            break;
                        case GradientType.Quadratic:
                            gradient = Mathf.Clamp01(gradient * gradient);
                            break;
                        case GradientType.SquareRoot:
                            gradient = Mathf.Clamp01(Mathf.Sqrt(gradient));
                            break;
                    }

                    heightMap[x, y] *= gradient;
                }
            }
        }

        private void SaveBrushTexture(Texture2D texture)
        {
            byte[] bytes = texture.EncodeToPNG();
            string path = EditorUtility.SaveFilePanel("Save Brush Texture", "", "BrushTexture.png", "png");

            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("Brush texture saved at: " + path);
            }
        }
    }
}
