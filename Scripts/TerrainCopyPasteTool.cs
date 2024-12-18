using UnityEngine;
using System.Collections.Generic;

namespace ISMR
{
    public class TerrainCopyPasteTool : MonoBehaviour
    {
        public enum CopyShape { Rectangle, Circle }  // �͈͂̌`��
        public CopyShape shape = CopyShape.Rectangle; // �f�t�H���g�͋�`

        public Terrain sourceTerrain;                // �R�s�[����Terrain
        public Terrain destinationTerrain;           // �R�s�[���Terrain
        public Vector2 sourceCenterLocalPosition;    // �R�s�[���͈̔͂̒��S���W�i���[�J���j
        public Vector2 destinationCenterLocalPosition; // �R�s�[��͈̔͂̒��S���W�i���[�J���j
        public int width, height;                    // ��`�̏ꍇ�̕��ƍ����i�O���b�h�P�ʁj
        public float radius;                         // �~�`�̏ꍇ�̔��a
        public float heightOffset = 0f;              // �R�s�[���ɍ����ɉ��Z����I�t�Z�b�g

        public Color sourceGizmoColor = Color.green;   // �R�s�[���͈̔͂�\������Gizmo�̐F
        public Color destinationGizmoColor = Color.blue; // �R�s�[��͈̔͂�\������Gizmo�̐F

        // �R�s�[���s���\�b�h
        public void CopyTerrain()
        {
            if (sourceTerrain == null || destinationTerrain == null)
            {
                UnityEngine.Debug.LogError("Source Terrain or Destination Terrain is not assigned.");
                return;
            }

            // Undo����̋L�^�J�n
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCompleteObjectUndo(destinationTerrain.terrainData, "Copy Terrain Data");
#endif

            // 1. �R�s�[���ƃR�s�[��̒��S���W����O���b�h���W���擾
            Vector2Int sourceCoords = LocalToTerrainCoordinates(sourceTerrain, sourceCenterLocalPosition);
            Vector2Int destinationCoords = LocalToTerrainCoordinates(destinationTerrain, destinationCenterLocalPosition);

            // 2. �n�`�f�[�^�̃R�s�[
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

        // ��`�͈͂̃R�s�[
        private void CopyRectangle(Vector2Int sourceCenter, Vector2Int destinationCenter)
        {
            int sourceX = sourceCenter.x - width / 2;
            int sourceY = sourceCenter.y - height / 2;
            int destinationX = destinationCenter.x - width / 2;
            int destinationY = destinationCenter.y - height / 2;

            // 1. �����f�[�^���R�s�[���A�I�t�Z�b�g�����Z
            float[,] heights = sourceTerrain.terrainData.GetHeights(sourceX, sourceY, width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    heights[y, x] += heightOffset / sourceTerrain.terrainData.size.y; // �I�t�Z�b�g��ǉ�
                }
            }
            destinationTerrain.terrainData.SetHeights(destinationX, destinationY, heights);

            // 2. ���̃f�[�^���R�s�[
            for (int i = 0; i < sourceTerrain.terrainData.detailPrototypes.Length; i++)
            {
                int[,] detailLayer = sourceTerrain.terrainData.GetDetailLayer(sourceX, sourceY, width, height, i);
                destinationTerrain.terrainData.SetDetailLayer(destinationX, destinationY, i, detailLayer);
            }

            // 3. �؂̃f�[�^���R�s�[
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

            // 4. �n�ʂ̃e�N�X�`���iSplatMap�j���R�s�[
            int splatMapWidth = width;
            int splatMapHeight = height;
            float[,,] splatMap = sourceTerrain.terrainData.GetAlphamaps(sourceX, sourceY, splatMapWidth, splatMapHeight);
            destinationTerrain.terrainData.SetAlphamaps(destinationX, destinationY, splatMap);
        }

        // �~�`�͈͂̃R�s�[
        private void CopyCircle(Vector2Int sourceCenter, Vector2Int destinationCenter)
        {
            int radiusInGridUnits = Mathf.FloorToInt(radius / (sourceTerrain.terrainData.size.x / sourceTerrain.terrainData.heightmapResolution));

            // 1. �����f�[�^���~�`�ɃR�s�[���A�I�t�Z�b�g�����Z
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
                        heights[0, 0] += heightOffset / sourceTerrain.terrainData.size.y; // �I�t�Z�b�g��ǉ�
                        destinationTerrain.terrainData.SetHeights(destinationX, destinationY, heights);

                        // SplatMap�̃R�s�[
                        float[,,] splatMap = sourceTerrain.terrainData.GetAlphamaps(sourceX, sourceY, 1, 1);
                        destinationTerrain.terrainData.SetAlphamaps(destinationX, destinationY, splatMap);
                    }
                }
            }

            // 2. ����؂̃f�[�^�����l�ɉ~�`�͈͓�������
            CopyCircleDetails(sourceCenter, destinationCenter, radiusInGridUnits);
        }

        // ����؂��~�`�͈͂ŃR�s�[
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

            // �؂̃R�s�[�i�~�`�͈͓��j
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

        // ���[�J�����W���O���b�h���W�ɕϊ�����
        public Vector2Int LocalToTerrainCoordinates(Terrain terrain, Vector2 localPosition)
        {
            TerrainData terrainData = terrain.terrainData;

            float gridSizeX = terrainData.size.x / terrainData.heightmapResolution;
            float gridSizeZ = terrainData.size.z / terrainData.heightmapResolution;

            int coordX = Mathf.FloorToInt(localPosition.x / gridSizeX);
            int coordZ = Mathf.FloorToInt(localPosition.y / gridSizeZ);

            return new Vector2Int(coordX, coordZ);
        }

        // ��`�͈͓��ɖ؂����邩����
        public bool IsTreeInRectangle(Vector3 treePosition, int startX, int startY, int width, int height, Terrain terrain)
        {
            Vector2Int treeCoords = LocalToTerrainCoordinates(terrain, new Vector2(treePosition.x, treePosition.z));
            return treeCoords.x >= startX && treeCoords.x < startX + width && treeCoords.y >= startY && treeCoords.y < startY + height;
        }

        // �~�`�͈͓��ɖ؂����邩����
        public bool IsTreeInCircle(Vector3 treePosition, Vector2Int center, int radiusInGridUnits, Terrain terrain)
        {
            Vector2Int treeCoords = LocalToTerrainCoordinates(terrain, new Vector2(treePosition.x, treePosition.z));
            int dx = treeCoords.x - center.x;
            int dy = treeCoords.y - center.y;
            return (dx * dx + dy * dy) <= (radiusInGridUnits * radiusInGridUnits);
        }

        // �V�[���r���[�ɔ͈͂�\������Gizmos�`��
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

        // �I�������`��i��`�܂��͉~�`�j�Ɋ�Â���Gizmos��`��
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

        // ��`�͈̔͂��V�[���r���[��Gizmos�ŕ`��
        private void DrawRectangleGizmo(Terrain terrain, Vector2 centerLocalPosition, int width, int height)
        {
            Vector3 worldCenterPosition = terrain.transform.position + new Vector3(centerLocalPosition.x, 0, centerLocalPosition.y);
            float terrainHeight = terrain.SampleHeight(worldCenterPosition) + terrain.transform.position.y;

            // ��`�̎l�����v�Z
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

            // ��`�͈̔͂�`��
            Gizmos.DrawLine(startWorldPosition, new Vector3(endWorldPosition.x, terrainHeight, startWorldPosition.z));
            Gizmos.DrawLine(startWorldPosition, new Vector3(startWorldPosition.x, terrainHeight, endWorldPosition.z));
            Gizmos.DrawLine(new Vector3(endWorldPosition.x, terrainHeight, startWorldPosition.z), endWorldPosition);
            Gizmos.DrawLine(new Vector3(startWorldPosition.x, terrainHeight, endWorldPosition.z), endWorldPosition);
        }

        // �~�`�͈̔͂��V�[���r���[��Gizmos�ŕ`��
        private void DrawCircleGizmo(Terrain terrain, Vector2 centerLocalPosition, float radius)
        {
            Vector3 worldCenterPosition = terrain.transform.position + new Vector3(centerLocalPosition.x, 0, centerLocalPosition.y);
            float terrainHeight = terrain.SampleHeight(worldCenterPosition) + terrain.transform.position.y;

            // ���S���W�����[���h���W�ɕϊ�
            Vector3 centerWorldPosition = new Vector3(
                worldCenterPosition.x,
                terrainHeight,
                worldCenterPosition.z
            );

            // �~��`��
            Gizmos.DrawWireSphere(centerWorldPosition, radius);
        }
#endif
    }
}
