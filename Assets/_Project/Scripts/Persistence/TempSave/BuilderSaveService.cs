using System.Collections.Generic;
using _Project.Scripts.BaseBuilder.Runtime.Inventory;
using _Project.Scripts.BaseBuilder.Runtime.Placement;
using TGS;
using UnityEngine;

namespace _Project.Scripts.Persistence.TempSave
{
    public class BuilderSaveService
    {
        private static readonly PlaceableKind[] Kinds =
        {
            PlaceableKind.Land,
            PlaceableKind.Wall,
            PlaceableKind.Building,
            PlaceableKind.Turret
        };

        public BuilderSaveData Capture(
            IReadOnlyList<PlacedEntry> entries,
            BuilderInventoryFacade inventory,
            IPrefabCatalog catalog)
        {
            var data = new BuilderSaveData { version = JsonFileSaveStore.CurrentVersion };

            foreach (var kind in Kinds)
            {
                int slots = catalog.GetSlotCount(kind);
                for (int i = 0; i < slots; i++)
                {
                    data.counts.Add(new SlotCountRecord
                    {
                        kind = kind,
                        index = i,
                        count = inventory.GetCount(kind, i)
                    });
                }
            }

            if (entries != null)
            {
                foreach (var e in entries)
                {
                    if (e == null || e.instance == null) continue;

                    data.placed.Add(new PlacedRecord
                    {
                        kind = e.kind,
                        inventoryIndex = e.inventoryIndex,
                        originCellIndex = e.originCellIndex,
                        sizeX = e.size.x,
                        sizeY = e.size.y,
                        isLand = e.isLand,
                        isLocked = e.isLocked,
                        yaw = e.rotation.eulerAngles.y
                    });
                }
            }

            foreach (var rec in data.placed)
            {
                switch (rec.kind)
                {
                    case PlaceableKind.Land:     data.landPlaced++; break;
                    case PlaceableKind.Wall:     data.wallPlaced++; break;
                    case PlaceableKind.Building: data.buildingPlaced++; break;
                    case PlaceableKind.Turret:   data.turretPlaced++; break;
                }
            }

            data.note = $"Placed L{data.landPlaced} W{data.wallPlaced} B{data.buildingPlaced} T{data.turretPlaced}";

            Debug.Log($"[Save] Captured counts={data.counts.Count} placed={data.placed.Count} | {data.note}");
            return data;
        }

        public void Apply(
            BuilderSaveData data,
            PlacementService placement,
            BuilderInventoryFacade inventory,
            IPrefabCatalog catalog,
            TerrainGridSystem landGrid,
            TerrainGridSystem objectGrid)
        {
            if (data == null || placement == null || inventory == null || catalog == null)
            {
                Debug.LogError("[Save] Apply missing dependency");
                return;
            }

            placement.ClearAll();

            if (data.counts != null)
            {
                foreach (var slot in data.counts)
                    inventory.SetCount(slot.kind, slot.index, slot.count);
            }

            int placed = 0;
            int skipped = 0;

            if (data.placed != null)
            {
                foreach (var rec in data.placed)
                {
                    if (!catalog.TryGetPrefab(rec.kind, rec.inventoryIndex, out GameObject prefab))
                    {
                        Debug.LogWarning($"[Save] Skip place – no prefab {rec.kind}[{rec.inventoryIndex}]");
                        skipped++;
                        continue;
                    }

                    TerrainGridSystem grid = rec.isLand ? landGrid : objectGrid;
                    if (grid == null || rec.originCellIndex < 0 || rec.originCellIndex >= grid.numCells)
                    {
                        Debug.LogWarning($"[Save] Skip place – bad cell {rec.originCellIndex} kind={rec.kind}");
                        skipped++;
                        continue;
                    }

                    Vector3 worldPos = grid.CellGetPosition(rec.originCellIndex);
                    Vector2Int size = new Vector2Int(Mathf.Max(1, rec.sizeX), Mathf.Max(1, rec.sizeY));
                    Quaternion rot = Quaternion.Euler(0f, rec.yaw, 0f);

                    GameObject instance = placement.Place(
                        prefab,
                        worldPos,
                        size,
                        rec.isLand,
                        rec.inventoryIndex,
                        rec.kind,
                        rot);

                    if (instance == null)
                    {
                        skipped++;
                        continue;
                    }

                    if (rec.isLocked)
                        placement.TrySetLocked(instance, true);

                    placed++;
                }
            }

            Debug.Log($"<color=cyan>[Save] Applied placed={placed} skipped={skipped}</color>");
        }
    }
}