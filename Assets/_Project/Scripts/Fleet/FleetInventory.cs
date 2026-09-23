using System.Collections.Generic;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using _Project.Scripts.Persistence.TempSave;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public class FleetBlueprint : ScriptableObject
    {
        public string saveId;
        public string fleetName;
        public int flagIndex;
        public readonly ShipBlueprint[] ships = new ShipBlueprint[5];
    }

    [CreateAssetMenu(menuName = "ShipBuilder/Fleet Inventory", fileName = "FleetInventory")]
    public class FleetInventory : ScriptableObject
    {
        public List<FleetBlueprint> fleets = new List<FleetBlueprint>();

        public void EnsureHydrated(BuiltShipInventory yard)
        {
            ClearRuntime();
            var store = new JsonFleetSaveStore();
            if (!store.TryRead(out var data)) return;

            foreach (var record in data.fleets)
            {
                var fleet = ScriptableObject.CreateInstance<FleetBlueprint>();
                fleet.saveId = record.id;
                fleet.fleetName = record.fleetName;
                fleet.flagIndex = record.flagIndex;

                for (int i = 0; i < 5; i++)
                {
                    string id = record.shipIds != null && i < record.shipIds.Count
                        ? record.shipIds[i]
                        : "";
                    fleet.ships[i] = FindShip(yard, id);
                }

                fleets.Add(fleet);
            }

            Debug.Log($"<color=cyan>[Fleet] Hydrated {fleets.Count} fleet(s)</color>");
        }

        public void ClearRuntime()
        {
            fleets.Clear();
        }

        private static ShipBlueprint FindShip(BuiltShipInventory yard, string saveId)
        {
            if (yard == null || string.IsNullOrEmpty(saveId)) return null;
            foreach (var ship in yard.ships)
            {
                if (ship != null && ship.saveId == saveId)
                    return ship;
            }
            return null;
        }
    }
}