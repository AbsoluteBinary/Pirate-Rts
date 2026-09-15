using System;
using _Project.Scripts.Harbour.ShipBuilder;
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