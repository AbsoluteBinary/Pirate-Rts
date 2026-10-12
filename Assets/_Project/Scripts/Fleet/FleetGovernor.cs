using System.Collections.Generic;
using _Project.Scripts.Combat;
using _Project.Scripts.Persistence.TempSave;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using _Project.Scripts.PlayerShip_Movement;
using _Project.Scripts.PlayerShip_Movement.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Fleet
{
    public class FleetGovernor : MonoBehaviour
    {
        public static FleetGovernor Instance { get; private set; }

        public string FleetName { get; private set; }
        public int FlagIndex { get; private set; } = 2;
        public PlayerCombatMovementController FlagshipBody { get; private set; }
        public MapSailController MapBody { get; private set; }

        private readonly List<ShipBlueprint> _ships = new List<ShipBlueprint>();
        private readonly List<float> _current = new List<float>();
        private readonly List<float> _max = new List<float>();
        private string _scene = "FreeRoamScene";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            Bind(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this)
                Instance = null;
        }

        public void TakeLaunch(string fleetName, ShipBlueprint[] ships, int flagIndex)
        {
            FleetName = fleetName ?? "";
            FlagIndex = flagIndex;
            FlagshipBody = null;
            _ships.Clear();
            _current.Clear();
            _max.Clear();

            if (ships != null)
            {
                foreach (var ship in ships)
                {
                    if (ship == null)
                        continue;
                    float max = ship.totalHealth > 0f ? ship.totalHealth : 100f;
                    _ships.Add(ship);
                    _max.Add(max);
                    _current.Add(max);
                }
            }

            _scene = "FreeRoamScene";
            WriteActiveFleet();
            Debug.Log($"<color=cyan>[Governor] Holding '{FleetName}' ({_ships.Count} ship(s))</color>");
        }

        public void AttachCombatBody(PlayerCombatMovementController body, bool isFlagship)
        {
            if (body == null || !isFlagship)
                return;

            FlagshipBody = body;
            body.enabled = true;
            Debug.Log($"<color=lime>[Governor] Combat flagship '{body.gameObject.name}'</color>");
        }

        public void ApplyHealth(Health health, ShipBlueprint blueprint)
        {
            int index = IndexOf(blueprint);
            if (health == null || index < 0)
                return;
            health.Apply(_max[index], _current[index]);
        }

        public void NoteDamage(Health health)
        {
            if (health == null)
                return;

            var combat = health.GetComponent<CombatShip>();
            int index = IndexOf(combat != null ? combat.Blueprint : null);
            if (index < 0)
                return;

            _current[index] = health.CurrentHealth;
            _max[index] = health.MaxHealth;
        }

        public void OrderMove(Vector3 worldPosition)
        {
            if (FlagshipBody != null && FlagshipBody.gameObject.activeInHierarchy)
            {
                FlagshipBody.enabled = true;
                FlagshipBody.MoveToLocation(worldPosition);
                Debug.Log($"[Governor] '{FlagshipBody.gameObject.name}' to {worldPosition}");
                return;
            }

            if (MapBody != null)
            {
                var helm = MapBody.Helm;
                if (helm != null)
                    helm.enabled = true;
                MapBody.OrderMove(worldPosition);
                Debug.Log($"[Governor] Map pawn to {worldPosition}");
                return;
            }

            Debug.LogWarning("[Governor] OrderMove found no body.");
        }

        public void Commit()
        {
            if (MapBody != null)
                _scene = "FreeRoamScene";
            WriteActiveFleet();
            WriteShipHealth();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Bind(scene);
        }

        private void Bind(Scene scene)
        {
            if (scene.name == "FreeRoamScene")
                BindFreeRoam();
            else if (scene.name == "CombatScene")
                _scene = "CombatScene";
        }

        private void BindFreeRoam()
        {
            var player = FindAnyObjectByType<MapSailController>();
            PlayerCombatMovementController helm = null;
            var found = FindObjectsByType<PlayerCombatMovementController>(FindObjectsSortMode.None);
            foreach (var candidate in found)
            {
                if (candidate != null && candidate.gameObject.scene.name == "FreeRoamScene")
                {
                    helm = candidate;
                    break;
                }
            }
            if (player == null || helm == null)
                return;

            MapBody = player;
            var flag = FlagshipBlueprint();
            if (flag == null || flag.shipPrefab == null)
            {
                Debug.LogWarning("[Governor] FreeRoam opened with no flagship.");
                return;
            }

            player.AttachFlagship(flag.shipPrefab, flag.shipName, helm.transform);
            helm.enabled = true;
            MapFleets.Register(
                string.IsNullOrWhiteSpace(FleetName) ? flag.shipName : FleetName,
                1f,
                helm.transform);
            Debug.Log($"<color=lime>[Governor] Map body is '{flag.shipName}'</color>");
        }

        private ShipBlueprint FlagshipBlueprint()
        {
            if (_ships.Count == 0 && PendingLaunch.Ships != null)
                TakeLaunch(PendingLaunch.FleetName, PendingLaunch.Ships, PendingLaunch.FlagShipIndex);

            if (PendingLaunch.Ships != null
                && FlagIndex >= 0
                && FlagIndex < PendingLaunch.Size
                && PendingLaunch.Ships[FlagIndex] != null)
                return PendingLaunch.Ships[FlagIndex];

            return _ships.Count > 0 ? _ships[0] : null;
        }

        private int IndexOf(ShipBlueprint blueprint)
        {
            if (blueprint == null)
                return -1;
            for (int i = 0; i < _ships.Count; i++)
            {
                if (_ships[i] == blueprint || _ships[i].saveId == blueprint.saveId)
                    return i;
            }
            return -1;
        }

        private void WriteActiveFleet()
        {
            var data = new ActiveFleetSaveData();
            var record = new ActiveFleetRecord
            {
                id = string.IsNullOrEmpty(FleetName) ? "fleet" : FleetName,
                fleetName = FleetName,
                sceneName = _scene,
                position = MapBody != null ? MapBody.transform.position : Vector3.zero
            };

            for (int i = 0; i < _ships.Count; i++)
            {
                var ship = _ships[i];
                record.ships.Add(new ActiveFleetShipRecord
                {
                    shipId = ship.saveId,
                    shipName = ship.shipName,
                    currentHealth = _current[i],
                    maxHealth = _max[i],
                    alive = _current[i] > 0f
                });
                if (ship == FlagshipBlueprint())
                    record.flagshipShipId = ship.saveId;
            }

            data.fleets.Add(record);
            new JsonActiveFleetSaveStore().Write(data);
        }

        private void WriteShipHealth()
        {
            var store = new JsonShipSaveStore();
            if (!store.TryRead(out var data) || data?.ships == null)
                return;

            bool changed = false;
            for (int i = 0; i < _ships.Count; i++)
            {
                foreach (var record in data.ships)
                {
                    if (record.id != _ships[i].saveId)
                        continue;
                    record.maxHealth = _max[i];
                    record.currentHealth = _current[i];
                    changed = true;
                }
            }

            if (changed)
                store.Write(data);
        }
    }
}
