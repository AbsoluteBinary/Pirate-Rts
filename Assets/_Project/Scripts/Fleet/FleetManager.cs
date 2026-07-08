using System.Collections.Generic;
using _Project.Scripts.Combat;
using _Project.Scripts.Combat.Ship;
using _Project.Scripts.Fleet.Data;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using _Project.Scripts.PlayerShip_Movement.Combat;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public class FleetManager : MonoBehaviour
    {
        public static FleetManager Instance { get; private set; }

        [Header("Fleet Data")]
        public FleetData currentFleetData;

        [Header("Spawn Settings")]
        public Transform fleetSpawnPoint;

        [Header("Runtime Ships")]
        public List<PlayerCombatMovementController> activeShips = new List<PlayerCombatMovementController>();
        public PlayerCombatMovementController flagship;

        public readonly List<PlayerCombatMovementController> selectedShips = new List<PlayerCombatMovementController>();

        // === NEW: Event for HUD refresh ===
        public event System.Action OnFleetChanged;

        private void Awake()
        {
            SpawnFleetFromData();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // Trigger this whenever fleet changes
        private void NotifyFleetChanged()
        {
            OnFleetChanged?.Invoke();
        }

        public void RegisterShip(PlayerCombatMovementController ship, bool isFlagship = false)
        {
            if (ship == null) return;
            activeShips.Add(ship);

            if (isFlagship)
                flagship = ship;

            NotifyFleetChanged();
        }
    
        public void SpawnFleetFromData()
        {
            if (currentFleetData == null || fleetSpawnPoint == null) return;

            // Clear old ships
            foreach (var ship in activeShips)
                if (ship != null) Destroy(ship.gameObject);
            activeShips.Clear();

            for (int i = 0; i < currentFleetData.ships.Count; i++)
            {
                ShipBlueprint bp = currentFleetData.ships[i];
                if (bp?.shipPrefab == null) continue;

                Vector3 spawnPos = fleetSpawnPoint.position + new Vector3(i * 15f, 0, -i * 10f);

                GameObject shipObj = Instantiate(bp.shipPrefab, spawnPos, Quaternion.identity);
                shipObj.name = bp.shipName;

                // FORCE ALL REQUIRED COMPONENTS
                var combatShip = shipObj.GetComponent<CombatShip>() ?? shipObj.AddComponent<CombatShip>();
                var movement = shipObj.GetComponent<PlayerCombatMovementController>() ?? shipObj.AddComponent<PlayerCombatMovementController>();
                var health = shipObj.GetComponent<Health>() ?? shipObj.AddComponent<Health>();
                var shipVFX = shipObj.GetComponent<ShipVFX>() ?? shipObj.AddComponent<ShipVFX>();   // ← This line is key

                combatShip.Initialize(bp);

                RegisterShip(movement, i == 0);

                Debug.Log($"[FleetManager] Spawned {bp.shipName} | ShipVFX attached: {shipVFX != null}");
            }
        }

        public void MoveFleetToLocation(Vector3 worldPosition)
        {
            var shipsToMove = selectedShips.Count > 0 ? selectedShips : activeShips;

            for (int i = 0; i < shipsToMove.Count; i++)
            {
                var ship = shipsToMove[i];
                if (ship == null) continue;

                Vector3 offset = Vector3.zero; // TODO: Formation later
                ship.MoveToLocation(worldPosition + offset);
            }

            NotifyFleetChanged();
        }

        public void SelectSingle(PlayerCombatMovementController ship)
        {
            selectedShips.Clear();
            if (ship != null) selectedShips.Add(ship);
            NotifyFleetChanged();
        }

        public void SelectAll()
        {
            selectedShips.Clear();
            selectedShips.AddRange(activeShips);
            NotifyFleetChanged();
        }
    }
}