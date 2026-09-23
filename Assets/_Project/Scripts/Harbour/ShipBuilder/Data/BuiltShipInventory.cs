using System.Collections.Generic;
using _Project.Scripts.Harbour.ShipBuilder;
using _Project.Scripts.Harbour.Modules;
using _Project.Scripts.Persistence.TempSave;
using UnityEngine;

namespace _Project.Scripts.Harbour.ShipBuilder.Data
{
    [CreateAssetMenu(menuName = "ShipBuilder/Built Ship Inventory", fileName = "BuiltShipInventory")]
    public class BuiltShipInventory : ScriptableObject
    {
        [Header("Catalogues (project assets)")]
        public HullData[] hullCatalog;
        public ModuleDatabase moduleDatabase;

        [Header("Runtime yard — filled from save in Play Mode")]
        public List<ShipBlueprint> ships = new List<ShipBlueprint>();
        

        private bool _hydrated;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            EnsureHydrated();
        }

        private void OnDisable()
        {
            ships.Clear();
            _hydrated = false;
        }

        public void EnsureHydrated()
        {
            _hydrated = true;
            ships.Clear();

            var save = new ShipSaveService(new JsonShipSaveStore());
            ships.AddRange(save.RebuildAll(hullCatalog, moduleDatabase));
            Debug.Log($"<color=cyan>[Yard] Hydrated {ships.Count} ship(s) from save</color>");
        }

        public void ClearRuntime()
        {
            ships.Clear();
            _hydrated = false;
        }

        public void Register(ShipBlueprint ship)
        {
            if (ship == null || ships.Contains(ship)) return;
            ships.Add(ship);
            Debug.Log($"<color=lime>[Yard] Registered '{ship.shipName}' | count={ships.Count}</color>");
        }
    }
}