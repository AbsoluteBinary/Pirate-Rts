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
            LogPendingFleet();
            
            SpawnFleetFromData();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void LogPendingFleet()
        {
            Debug.Log($"<color=cyan>[Fleet] Pending flagship slot {PendingLaunch.FlagShipIndex}</color>");

            for (int i = 0; i < PendingLaunch.Size; i++)
            {
                var ship = PendingLaunch.Ships[i];
                string name = ship != null ? ship.shipName : "(empty)";
                string mark = i == PendingLaunch.FlagShipIndex ? "  FLAG" : "";
                Debug.Log($"[Fleet] Slot {i}: {name}{mark}");
            }
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
            if (fleetSpawnPoint == null)
            {
                Debug.LogWarning("[Fleet] No spawn point on FleetManager.");
                return;
            }

            foreach (var ship in activeShips)
                if (ship != null) Destroy(ship.gameObject);
            activeShips.Clear();

            bool launched = PendingLaunch.Ships[PendingLaunch.FlagShipIndex] != null;
            if (launched)
            {
                for (int i = 0; i < PendingLaunch.Size; i++)
                    SpawnOne(PendingLaunch.Ships[i], i, i == PendingLaunch.FlagShipIndex);
                return;
            }

            if (currentFleetData == null) return;

            for (int i = 0; i < currentFleetData.ships.Count; i++)
                SpawnOne(currentFleetData.ships[i], i, i == 0);
        }

        private void SpawnOne(ShipBlueprint bp, int index, bool isFlagship)
        {
            if (bp == null) return;

            if (bp.shipPrefab == null)
            {
                Debug.LogWarning($"[Fleet] '{bp.shipName}' has no combat prefab. Assign it on the hull.");
                return;
            }

            Vector3 spawnPos = fleetSpawnPoint.position + new Vector3(index * 15f, 0, -index * 10f);
            GameObject shipObj = Instantiate(bp.shipPrefab, spawnPos, Quaternion.identity);
            shipObj.name = bp.shipName;

            var combatShip = shipObj.GetComponent<CombatShip>() ?? shipObj.AddComponent<CombatShip>();
            var movement = shipObj.GetComponent<PlayerCombatMovementController>() ?? shipObj.AddComponent<PlayerCombatMovementController>();
            var health = shipObj.GetComponent<Health>() ?? shipObj.AddComponent<Health>();
            var shipVFX = shipObj.GetComponent<ShipVFX>() ?? shipObj.AddComponent<ShipVFX>();

            combatShip.Initialize(bp);
            RegisterShip(movement, isFlagship);

            Debug.Log($"[FleetManager] Spawned {bp.shipName} | flagship={isFlagship} | VFX={shipVFX != null}");
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