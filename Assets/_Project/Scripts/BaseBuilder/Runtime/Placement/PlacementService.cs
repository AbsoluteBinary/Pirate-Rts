using System.Collections.Generic;
using _Project.Scripts.BaseBuilder.Runtime.Inventory;
using _Project.Scripts.Harbour.Data.SO;
using TGS;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Placement
{
    public class PlacedInfo
    {
        public GameObject Instance;
        public GameObject Prefab;
        public PlaceableKind Kind;
        public bool IsLandObject;       // keep for now if other code still uses it
        public int InventoryIndex;
        public int OriginCellIndex;
        public Vector2Int Size;
    }

    public class PlacedEntry
    {
        public GameObject instance;
        public GameObject prefab;
        public PlaceableKind kind;
        public bool isLand;
        public int inventoryIndex;
        public int originCellIndex;
        public Vector2Int size;
    }
    public class PlacementService
    {

        private readonly OccupationSystem occupation;
        private readonly PlacementValidator validator;

        private TerrainGridSystem landGrid;
        private TerrainGridSystem objectGrid;

        private readonly List<PlacedEntry> placedEntries = new List<PlacedEntry>();

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
        
        
        // ─────────────────────────────────────────────
        // Placement
        // ─────────────────────────────────────────────

        public GameObject Place(GameObject prefab, Vector3 worldPosition, Vector2Int size, bool isLandObject, int inventoryIndex = -1)
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

            int placedLayer = LayerMask.NameToLayer("PlacedObjects");
            if (placedLayer != -1)
                SetLayerRecursively(instance, placedLayer);

            // Record the entry
            var entry = new PlacedEntry
            {
                instance = instance,
                prefab = prefab,
                kind = isLandObject ? PlaceableKind.Land : PlaceableKind.Wall,
                isLand = isLandObject,
                inventoryIndex = inventoryIndex,
                originCellIndex = originCell.index,
                size = size
            };
            
            placedEntries.Add(entry);
            occupation.OccupyFootprint(originCell.index, size, grid.columnCount, isLandObject);

            return instance;
        }
        
        /// <summary>
        /// Removes tracking + frees occupation. Does NOT destroy the GameObject.
        /// Caller destroys after reading Prefab / restoring inventory.
        /// </summary>
        public bool TryPickUp(GameObject obj, out PlacedInfo info)
        {
            info = null;
            if (obj == null) return false;

            for (int i = 0; i < placedEntries.Count; i++)
            {
                if (placedEntries[i].instance != obj)
                    continue;

                var entry = placedEntries[i];
                
                TerrainGridSystem grid = entry.isLand ? landGrid : objectGrid;

                if (grid != null)
                    occupation.FreeFootprint(entry.originCellIndex, entry.size, grid.columnCount, entry.isLand);

                info = new PlacedInfo
                {
                    Instance = entry.instance,
                    Prefab = entry.prefab,
                    Kind = entry.kind,
                    IsLandObject = entry.isLand,
                    InventoryIndex = entry.inventoryIndex,
                    OriginCellIndex = entry.originCellIndex,
                    Size = entry.size
                };

                placedEntries.RemoveAt(i);
                return true;
            }
            

            return false;
        }

        // ─────────────────────────────────────────────
        // Removal
        // ─────────────────────────────────────────────

        public bool Remove(GameObject obj)
        {
            if (obj == null) return false;

            for (int i = 0; i < placedEntries.Count; i++)
            {
                if (placedEntries[i].instance == obj)
                {
                    var entry = placedEntries[i];
                    TerrainGridSystem grid = entry.isLand ? landGrid : objectGrid;

                    if (grid != null)
                        occupation.FreeFootprint(entry.originCellIndex, entry.size, grid.columnCount, entry.isLand);

                    placedEntries.RemoveAt(i);
                    Object.Destroy(obj);
                    return true;
                }
            }
            return false;
        }

        // ─────────────────────────────────────────────
        // Clear methods
        // ─────────────────────────────────────────────

        public void ClearAll()
        {
            for (int i = placedEntries.Count - 1; i >= 0; i--)
            {
                if (placedEntries[i].instance != null)
                    Object.Destroy(placedEntries[i].instance);
            }

            placedEntries.Clear();
            occupation.Clear();
        }

        /// <summary>
        /// Clears only Land Tiles that have no object on top of them.
        /// Restores the correct inventory counts.
        /// </summary>
        public void ClearLandTilesOnly(LandTileInventorySO landInventory)
        {
            if (landGrid == null) return;

            for (int i = placedEntries.Count - 1; i >= 0; i--)
            {
                var entry = placedEntries[i];

                if (!entry.isLand) continue;
                if (entry.instance == null)
                {
                    placedEntries.RemoveAt(i);
                    continue;
                }

                // Skip if any wall/building is sitting on top
                if (HasObjectOnTop(entry.instance.transform.position))
                    continue;

                // Restore inventory count
                if (landInventory != null && entry.inventoryIndex >= 0 &&
                    entry.inventoryIndex < landInventory.tiles.Count)
                {
                    landInventory.tiles[entry.inventoryIndex].count++;
                }

                // Free occupation + destroy
                occupation.FreeFootprint(entry.originCellIndex, entry.size, landGrid.columnCount, true);
                Object.Destroy(entry.instance);
                placedEntries.RemoveAt(i);
            }
        }

        /// <summary>
        /// Clears all Walls / Buildings and restores their inventory counts.
        /// </summary>
        public void ClearObjectsOnly(WallInventorySO wallInventory = null)
        {
            if (objectGrid == null) return;

            for (int i = placedEntries.Count - 1; i >= 0; i--)
            {
                var entry = placedEntries[i];

                if (entry.isLand) continue;
                if (entry.instance == null)
                {
                    placedEntries.RemoveAt(i);
                    continue;
                }

                // Restore wall inventory count
                if (wallInventory != null && entry.inventoryIndex >= 0 &&
                    entry.inventoryIndex < wallInventory.walls.Count)
                {
                    wallInventory.walls[entry.inventoryIndex].count++;
                }

                occupation.FreeFootprint(entry.originCellIndex, entry.size, objectGrid.columnCount, false);
                Object.Destroy(entry.instance);
                placedEntries.RemoveAt(i);
            }
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

        private bool HasObjectOnTop(Vector3 landWorldPosition)
        {
            if (objectGrid == null || landGrid == null) return false;

            Cell landCell = landGrid.CellGetAtWorldPosition(landWorldPosition, 0);
            if (landCell == null) return false;

            Vector3 landCenter = landGrid.CellGetPosition(landCell.index);
            float half = landGrid.cellSize.x * 0.5f;

            // 2×2 object cells that sit inside this land cell
            Vector3[] samplePoints =
            {
                landCenter + new Vector3(-half * 0.5f, 0f, -half * 0.5f),
                landCenter + new Vector3( half * 0.5f, 0f, -half * 0.5f),
                landCenter + new Vector3(-half * 0.5f, 0f,  half * 0.5f),
                landCenter + new Vector3( half * 0.5f, 0f,  half * 0.5f)
            };

            foreach (var point in samplePoints)
            {
                Cell objectCell = objectGrid.CellGetAtWorldPosition(point, 0);
                if (objectCell != null && occupation.IsObjectOccupied(objectCell.index))
                    return true;
            }

            return false;
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
    }
}