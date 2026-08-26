using _Project.Scripts.Harbour.Data.SO;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Inventory
{
    public enum PlaceableKind
    {
        Land,
        Wall,
        Building,
        Water,
        Turret
    }

    /// <summary>
    /// Single place to consume / restore inventory by kind.
    /// </summary>
    public class BuilderInventoryFacade
    {
        private readonly LandTileInventorySO _land;
        private readonly WallInventorySO _walls;
        private readonly BuildingsInventorySO _buildings;
        private readonly TurretInventorySO _turrets;

        public BuilderInventoryFacade(
            LandTileInventorySO land,
            WallInventorySO walls,
            BuildingsInventorySO buildings = null,
            TurretInventorySO turrets = null)
        {
            _land = land;
            _walls = walls;
            _buildings = buildings;
            _turrets = turrets;
        }

        public int GetCount(PlaceableKind kind, int index)
        {
            if (index < 0) return 0;

            switch (kind)
            {
                case PlaceableKind.Land:
                    return _land != null ? _land.GetCount(index) : 0;
                case PlaceableKind.Wall:
                    return _walls != null ? _walls.GetCount(index) : 0;
                case PlaceableKind.Building:
                    return _buildings != null ? _buildings.GetCount(index) : 0;
                case PlaceableKind.Turret:
                    return _turrets != null ? _turrets.GetCount(index) : 0;
                default:
                    return 0;
            }
        }

        public bool Consume(PlaceableKind kind, int index)
        {
            if (index < 0) return false;

            switch (kind)
            {
                case PlaceableKind.Land:
                    return _land != null && _land.ConsumeTile(index);
                case PlaceableKind.Wall:
                    return _walls != null && _walls.Consume(index);
                case PlaceableKind.Building:
                    return _buildings != null && _buildings.Consume(index);
                case PlaceableKind.Turret:
                    return _turrets != null && _turrets.Consume(index);
                default:
                    return false;
            }
        }

        public void Restore(PlaceableKind kind, int index, int amount = 1)
        {
            if (index < 0) return;

            switch (kind)
            {
                case PlaceableKind.Land:
                    _land?.Restore(index, amount);
                    break;
                case PlaceableKind.Wall:
                    _walls?.Restore(index, amount);
                    break;
                case PlaceableKind.Building:
                    _buildings?.Restore(index, amount);
                    break;
                case PlaceableKind.Turret:
                    _turrets?.Restore(index, amount);
                    break;
            }
        }
    }
}