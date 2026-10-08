using System.Collections.Generic;
using _Project.Scripts.Combat;
using _Project.Scripts.Combat.Ship;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using _Project.Scripts.Persistence.TempSave;
using _Project.Scripts.PlayerShip_Movement.Combat;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public class FleetManager : MonoBehaviour
    {
        public static FleetManager Instance { get; private set; }
        

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
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            flagship = null;
            LogPendingFleet();
            SpawnFleetFromData();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        private void AssignFlagshipFromFile()
        {
            ActiveFleets.Load();
            ActiveFleetRecord fleet = null;
            if (ActiveFleets.Current?.fleets != null)
            {
                foreach (var candidate in ActiveFleets.Current.fleets)
                {
                    if (candidate != null && candidate.sceneName == ActiveFleets.CombatScene)
                        fleet = candidate;
                }
            }

            if (fleet == null || string.IsNullOrEmpty(fleet.flagshipShipId))
            {
                Debug.LogWarning("[Fleet] No flagship id on the active fleet file.");
                return;
            }

            foreach (var ship in activeShips)
            {
                if (ship == null) continue;
                var combat = ship.GetComponent<CombatShip>();
                if (combat?.Blueprint == null || combat.Blueprint.saveId != fleet.flagshipShipId)
                    continue;

                flagship = ship;
                ship.enabled = true;
                Debug.Log($"<color=lime>[Fleet] Flagship from file '{combat.Blueprint.shipName}'</color>");
                return;
            }

            Debug.LogWarning("[Fleet] Flagship id was on file, but that ship was not spawned.");
        }
        
        
        private void LogPendingFleet()
        {
            Debug.Log($"<color=cyan>[Fleet] Pending flagship slot {PendingLaunch.FlagShipIndex}</color>");
            
            if (PendingBattle.Active)
                Debug.Log($"<color=orange>[Battle] Engaging {PendingBattle.TargetId}</color>");

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
            shipObj.SetActive(true);

            var combatShip = GetOrAdd<CombatShip>(shipObj);
            var movement = GetOrAdd<PlayerCombatMovementController>(shipObj);
            var health = GetOrAdd<Health>(shipObj);
            var shipVFX = GetOrAdd<ShipVFX>(shipObj);

            combatShip.Initialize(bp);

            shipObj.SetActive(true);
            movement.Claim(shipObj.transform);
            health.enabled = true;
            shipVFX.enabled = true;

            var body = GetOrAdd<Rigidbody>(shipObj);
            body.isKinematic = true;
            body.useGravity = false;

            var hitBox = GetOrAdd<Collider, BoxCollider>(shipObj);
            hitBox.enabled = true;
            hitBox.isTrigger = true;

            FleetGovernor.Instance?.ApplyHealth(health, bp);
            FleetGovernor.Instance?.AttachCombatBody(movement, isFlagship);
            RegisterShip(movement, isFlagship);

            Debug.Log($"[FleetManager] Spawned {bp.shipName} | flagship={isFlagship} | VFX={shipVFX != null}");
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            return go.TryGetComponent(out T found) ? found : go.AddComponent<T>();
        }

        private static T GetOrAdd<T, TAdd>(GameObject go) where T : Component where TAdd : T
        {
            return go.TryGetComponent(out T found) ? found : go.AddComponent<TAdd>();
        }

        public void MoveFleetToLocation(Vector3 worldPosition)
        {
            if (FleetGovernor.Instance != null)
            {
                FleetGovernor.Instance.OrderMove(worldPosition);
                return;
            }
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