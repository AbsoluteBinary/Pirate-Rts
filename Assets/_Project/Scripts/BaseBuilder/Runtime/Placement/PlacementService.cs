using System.Collections.Generic;
using _Project.Scripts.BaseBuilder.Runtime.Data;
using TGS;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Placement
{
    public class PlacementService
    {
        private readonly OccupationSystem occupation;
        private readonly PlacementValidator validator;

        private TerrainGridSystem landGrid;
        private TerrainGridSystem objectGrid;

        private readonly List<GameObject> placedObjects = new List<GameObject>();

        public IReadOnlyList<GameObject> PlacedObjects => placedObjects;

        public PlacementService(OccupationSystem occupationSystem, PlacementValidator placementValidator)
        {
            occupation = occupationSystem;
            validator = placementValidator;
        }

        public void SetGrids(TerrainGridSystem land, TerrainGridSystem objects)
        {
            landGrid = land;
            objectGrid = objects;
            validator.SetGrids(land, objects);
        }

        public GameObject Place(GameObject prefab, Vector3 worldPosition, Vector2Int size, bool isLandObject)
        {
            if (prefab == null) return null;

            TerrainGridSystem grid = isLandObject ? landGrid : objectGrid;
            if (grid == null) return null;

            if (!validator.CanPlace(worldPosition, size, isLandObject))
                return null;

            Cell originCell = grid.CellGetAtWorldPosition(worldPosition, 0);
            if (originCell == null) return null;

            Vector3 finalPos = validator.GetFootprintCenter(originCell, size, grid);

            GameObject instance = Object.Instantiate(prefab, finalPos, Quaternion.identity);
            instance.name = prefab.name;

            // Set layer if it exists
            int placedLayer = LayerMask.NameToLayer("PlacedObjects");
            if (placedLayer != -1)
                SetLayerRecursively(instance, placedLayer);

            placedObjects.Add(instance);
            occupation.OccupyFootprint(originCell.index, size, grid.columnCount, isLandObject);

            return instance;
        }

        public bool Remove(GameObject obj, Vector2Int size, bool isLandObject)
        {
            if (obj == null) return false;

            TerrainGridSystem grid = isLandObject ? landGrid : objectGrid;
            if (grid == null) return false;

            Cell originCell = grid.CellGetAtWorldPosition(obj.transform.position, 0);
            if (originCell != null)
            {
                // Adjust for footprint centre → origin
                float cellSize = grid.cellSize.x;
                Vector3 originPos = obj.transform.position;
                originPos.x -= (size.x - 1) * cellSize * 0.5f;
                originPos.z -= (size.y - 1) * cellSize * 0.5f;

                originCell = grid.CellGetAtWorldPosition(originPos, 0);
                if (originCell != null)
                    occupation.FreeFootprint(originCell.index, size, grid.columnCount, isLandObject);
            }

            placedObjects.Remove(obj);
            Object.Destroy(obj);
            return true;
        }

        public void ClearAll()
        {
            for (int i = placedObjects.Count - 1; i >= 0; i--)
            {
                if (placedObjects[i] != null)
                    Object.Destroy(placedObjects[i]);
            }

            placedObjects.Clear();
            occupation.Clear();
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
    }
}