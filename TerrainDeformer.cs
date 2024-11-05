using UnityEngine;

namespace ISMR
{
    public class TerrainDeformer : MonoBehaviour
    {
        [SerializeField]
        private Terrain targetTerrain; // �ό`����^�[�Q�b�g��Terrain�I�u�W�F�N�g

        [SerializeField]
        private GameObject[] deformObjects; // �e����^����I�u�W�F�N�g�̃��X�g

        [SerializeField]
        private float rayOriginHeight = 10000f; // ���C�̌��_����

        private float[,] originalHeights; // ���̃n�C�g�}�b�v�f�[�^

        public void DeformTerrain()
        {
            if (targetTerrain == null || deformObjects.Length == 0)
            {
                UnityEngine.Debug.LogWarning("�^�[�Q�b�g��Terrain�܂��͉e����^����I�u�W�F�N�g���ݒ肳��Ă��܂���B");
                return;
            }

            TerrainData terrainData = targetTerrain.terrainData;
            int heightmapWidth = terrainData.heightmapResolution;
            int heightmapHeight = terrainData.heightmapResolution;

            // Undo�p�̏���
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(terrainData, "Deform Terrain");
#endif

            // �����n�C�g�}�b�v��ۑ��i���߂Ă̕ό`���̂݁j
            if (originalHeights == null)
            {
                originalHeights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
            }

            // ���݂̃n�C�g�}�b�v���R�s�[���ĕҏW
            float[,] heights = (float[,])originalHeights.Clone();

            // Terrain��̊e�|�C���g���烌�C���������ɔ�΂�
            for (int z = 0; z < heightmapHeight; z++)
            {
                for (int x = 0; x < heightmapWidth; x++)
                {
                    // �e�n�C�g�}�b�v�|�C���g�̃��[���h���W���v�Z
                    float worldPosX = targetTerrain.transform.position.x + (x / (float)heightmapWidth) * terrainData.size.x;
                    float worldPosZ = targetTerrain.transform.position.z + (z / (float)heightmapHeight) * terrainData.size.z;

                    // ���C�����_�������牺�����ɔ�΂�
                    Ray ray = new Ray(new Vector3(worldPosX, rayOriginHeight, worldPosZ), Vector3.down);
                    bool hitDetected = false;

                    foreach (GameObject deformObject in deformObjects)
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit))
                        {
                            // �����̑ΏۃI�u�W�F�N�g�̂����A�ŏ��ɏՓ˂������̂��g�p
                            if (hit.collider.gameObject == deformObject)
                            {
                                float targetHeight = hit.point.y / terrainData.size.y; // �n�`�̃T�C�Y�Ɋ�Â��Đ��K��
                                heights[z, x] = targetHeight; // �Փ˓_�̍����Ƀn�C�g�}�b�v��ݒ�
                                hitDetected = true;
                                break;
                            }
                        }
                    }

                    if (!hitDetected)
                    {
                        // �ΏۃI�u�W�F�N�g�Ƀq�b�g���Ȃ������ꍇ�A���̍����ɖ߂�
                        heights[z, x] = originalHeights[z, x];
                    }
                }
            }

            terrainData.SetHeights(0, 0, heights);
            UnityEngine.Debug.Log("Terrain�̃n�C�g�}�b�v���ό`����܂����B");
        }
    }
}
