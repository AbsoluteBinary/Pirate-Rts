using System;
using System.Collections.Generic;
using _Project.Scripts.Harbour.Modules;
using _Project.Scripts.Harbour.ShipBuilder;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using UnityEngine;

namespace _Project.Scripts.Persistence.TempSave
{
    public class ShipSaveService
    {
        private readonly IShipSaveStore _store;

        public ShipSaveService(IShipSaveStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public ShipBuildRecord Capture(ShipLoadout loadout, string shipName)
        {
            if (loadout?.hull == null) return null;

            var record = new ShipBuildRecord
            {
                id = Guid.NewGuid().ToString("N"),
                shipName = string.IsNullOrWhiteSpace(shipName) ? loadout.hull.hullName : shipName,
                hullId = loadout.hull.name,
                totalWeight = loadout.totalWeight,
                totalCost = loadout.totalCost,
                totalPowerDraw = loadout.totalPowerDraw,
                totalDamageOutput = loadout.totalDamageOutput
            };

            if (loadout.equippedSlots == null) return record;

            foreach (var eq in loadout.equippedSlots)
            {
                if (eq?.module == null) continue;
                record.slots.Add(new EquippedSlotRecord
                {
                    slotId = eq.slotId,
                    moduleId = eq.module.name,
                    moduleType = eq.module.type
                });
            }

            return record;
        }
        
        public ShipBlueprint Rebuild(ShipBuildRecord record, HullData[] hulls, ModuleDatabase modules)
        {
            if (record == null) return null;

            modules?.Initialize();

            var bp = ScriptableObject.CreateInstance<ShipBlueprint>();
            bp.name = record.shipName;
            bp.shipName = record.shipName;
            bp.totalWeight = record.totalWeight;
            bp.totalHealth = 100f;

            if (hulls != null)
            {
                foreach (var h in hulls)
                {
                    if (h == null) continue;
                    if (h.name != record.hullId && h.hullName != record.hullId) continue;
                    bp.hull = h;
                    bp.storageImage = h.hullImage;
                    break;
                }
            }

            if (record.slots != null && modules != null)
            {
                foreach (var slot in record.slots)
                {
                    var m = modules.GetModuleById(slot.moduleId);
                    if (m == null) continue;
                    switch (m)
                    {
                        case WeaponData w: bp.equippedWeapons.Add(w); break;
                        case ArmourData a: bp.equippedArmour.Add(a); break;
                        case EngineData e: bp.equippedEngines.Add(e); break;
                        case ComponentData c: bp.equippedComponents.Add(c); break;
                    }
                }
            }

            bp.CalculateStats();
            return bp;
        }

        public List<ShipBlueprint> RebuildAll(HullData[] hulls, ModuleDatabase modules)
        {
            var list = new List<ShipBlueprint>();
            if (!TryLoad(out var data) || data.ships == null) return list;

            foreach (var rec in data.ships)
            {
                var bp = Rebuild(rec, hulls, modules);
                if (bp != null) list.Add(bp);
            }

            return list;
        }

        public void AppendAndWrite(ShipBuildRecord record)
        {
            if (record == null) return;

            var data = _store.TryRead(out var existing) && existing != null
                ? existing
                : new ShipInventorySaveData { version = JsonShipSaveStore.CurrentVersion };

            data.ships.Add(record);
            _store.Write(data);
        }

        public bool TryLoad(out ShipInventorySaveData data) => _store.TryRead(out data);
    }
}