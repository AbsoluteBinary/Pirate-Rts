using _Project.Scripts.BaseBuilder.Runtime.Inventory;
using _Project.Scripts.Harbour.Data.SO;
using UnityEngine;

namespace _Project.Scripts.Persistence.TempSave
{
    public interface IPrefabCatalog
    {
        bool TryGetPrefab(PlaceableKind kind, int index, out GameObject prefab);
        bool TryGetSize(PlaceableKind kind, int index, out Vector2Int size);
        
        int GetSlotCount(PlaceableKind kind);
    }

    public class InventoryPrefabCatalog : IPrefabCatalog
    {
        private readonly LandTileInventorySO _land;
        private readonly WallInventorySO _walls;
        private readonly BuildingsInventorySO _buildings;
        private readonly TurretInventorySO _turrets;

        public InventoryPrefabCatalog(
            LandTileInventorySO land,
            WallInventorySO walls,
            BuildingsInventorySO buildings,
            TurretInventorySO turrets)
        {
            _land = land;
            _walls = walls;
            _buildings = buildings;
            _turrets = turrets;
        }
        

        public bool TryGetPrefab(PlaceableKind kind, int index, out GameObject prefab)
        {
            prefab = null;
            if (index < 0) return false;

            switch (kind)
            {
                case PlaceableKind.Land:
                    if (_land == null || index >= _land.tiles.Count) return false;
                    prefab = _land.tiles[index].prefab;
                    break;

                case PlaceableKind.Wall:
                    if (_walls == null || index >= _walls.walls.Count) return false;
                    prefab = _walls.walls[index].prefab;
                    break;

                case PlaceableKind.Building:
                    if (_buildings == null || index >= _buildings.buildings.Count) return false;
                    prefab = _buildings.buildings[index].prefab;
                    break;

                case PlaceableKind.Turret:
                    if (_turrets == null || index >= _turrets.turrets.Count) return false;
                    prefab = _turrets.turrets[index].prefab;
                    break;

                default:
                    return false;
            }

            return prefab != null;
        }
        
        public int GetSlotCount(PlaceableKind kind)
        {
            switch (kind)
            {
                case PlaceableKind.Land:
                    return _land != null && _land.tiles != null ? _land.tiles.Count : 0;
                case PlaceableKind.Wall:
                    return _walls != null && _walls.walls != null ? _walls.walls.Count : 0;
                case PlaceableKind.Building:
                    return _buildings != null && _buildings.buildings != null ? _buildings.buildings.Count : 0;
                case PlaceableKind.Turret:
                    return _turrets != null && _turrets.turrets != null ? _turrets.turrets.Count : 0;
                default:
                    return 0;
            }
        }

        public bool TryGetSize(PlaceableKind kind, int index, out Vector2Int size)
        {
            size = Vector2Int.one;
            if (index < 0) return false;

            switch (kind)
            {
                case PlaceableKind.Land:
                case PlaceableKind.Wall:
                case PlaceableKind.Turret:
                    return TryGetPrefab(kind, index, out _);

                case PlaceableKind.Building:
                    if (_buildings == null || index >= _buildings.buildings.Count) return false;
                    size = _buildings.buildings[index].size;
                    return _buildings.buildings[index].prefab != null;

                default:
                    return false;
            }
        }
    }
}