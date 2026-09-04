using System;
using System.Reflection;
using System.Collections.Generic;
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
    /// Single writer for inventory counts by kind.
    /// HUD only listens to each SO OnCountChanged.
    /// </summary>
    public class BuilderInventoryFacade
    {
        private readonly LandTileInventorySO _land;
        private readonly WallInventorySO _walls;
        private readonly BuildingsInventorySO _buildings;
        private readonly TurretInventorySO _turrets;

        private readonly Dictionary<(PlaceableKind kind, int index), int> _startCounts
            = new Dictionary<(PlaceableKind, int), int>();

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

        public void SetCount(PlaceableKind kind, int index, int value)
        {
            if (index < 0) return;
            value = Mathf.Max(0, value);

            switch (kind)
            {
                case PlaceableKind.Land:
                    if (_land == null || index >= _land.tiles.Count) return;
                    _land.tiles[index].count = value;
                    RaiseEvent(_land, "OnCountChanged", index);
                    break;

                case PlaceableKind.Wall:
                    if (_walls == null || index >= _walls.walls.Count) return;
                    _walls.walls[index].count = value;
                    RaiseEvent(_walls, "OnCountChanged", index);
                    break;

                case PlaceableKind.Building:
                    if (_buildings == null || index >= _buildings.buildings.Count) return;
                    _buildings.buildings[index].count = value;
                    RaiseEvent(_buildings, "OnCountChanged", index);
                    break;

                case PlaceableKind.Turret:
                    if (_turrets == null || index >= _turrets.turrets.Count) return;
                    _turrets.turrets[index].count = value;
                    RaiseEvent(_turrets, "OnCountChanged", index);
                    break;
            }
        }

        public void SnapshotStarts()
        {
            _startCounts.Clear();
            Capture(PlaceableKind.Land, SlotCount(PlaceableKind.Land));
            Capture(PlaceableKind.Wall, SlotCount(PlaceableKind.Wall));
            Capture(PlaceableKind.Building, SlotCount(PlaceableKind.Building));
            Capture(PlaceableKind.Turret, SlotCount(PlaceableKind.Turret));
        }

        public void ResetToStart()
        {
            foreach (var kv in _startCounts)
                SetCount(kv.Key.kind, kv.Key.index, kv.Value);
        }

        private void Capture(PlaceableKind kind, int slotCount)
        {
            for (int i = 0; i < slotCount; i++)
                _startCounts[(kind, i)] = GetCount(kind, i);
        }

        private int SlotCount(PlaceableKind kind)
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

        private static void RaiseEvent(object target, string eventName, int arg)
        {
            if (target == null) return;
            var fi = target.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fi == null) return;
            var dlg = fi.GetValue(target) as System.Delegate;
            dlg?.DynamicInvoke(arg);
        }
    }
}