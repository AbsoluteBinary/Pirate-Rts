using System.Collections.Generic;
using _Project.Scripts.Combat;
using _Project.Scripts.Persistence.TempSave;
using _Project.Scripts.Harbour.ShipBuilder;
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

        public void Release()
        {
            FleetName = "";
            FlagshipBody = null;
            MapBody = null;
            _ships.Clear();
            _current.Clear();
            _max.Clear();
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
            MapFleetBodies.Clear();
            ActiveFleets.Load();

            var scene = helm.gameObject.scene;
            Vector3 spawn = helm.transform.position;
            int placed = 0;
            bool helmUsed = false;

            if (ActiveFleets.Current?.fleets == null)
                return;

            foreach (var fleet in ActiveFleets.Current.fleets)
            {
                if (fleet == null || fleet.sceneName != ActiveFleets.FreeRoamScene)
                    continue;

                var blueprint = BlueprintFor(fleet.flagshipShipId);
                if (blueprint == null || blueprint.shipPrefab == null)
                    continue;

                bool drives = fleet.fleetName == FleetName;
                if (!helmUsed && string.IsNullOrEmpty(FleetName))
                {
                    drives = true;
                    FleetName = fleet.fleetName;
                }

                Vector3 position = fleet.hasPosition ? fleet.position : spawn;

                if (drives && !helmUsed)
                {
                    player.AttachFlagship(blueprint.shipPrefab, blueprint.shipName, helm.transform);
                    if (fleet.hasPosition)
                        helm.transform.position = position;
                    helm.enabled = true;
                    MapFleetBodies.Remember(fleet.fleetName, helm.transform);
                    helmUsed = true;
                }
                else
                {
                    var visual = Instantiate(blueprint.shipPrefab, position, Quaternion.identity);
                    visual.name = string.IsNullOrEmpty(blueprint.shipName) ? fleet.fleetName : blueprint.shipName;
                    SceneManager.MoveGameObjectToScene(visual, scene);

                    var extraHelm = visual.GetComponent<PlayerCombatMovementController>();
                    if (extraHelm != null)
                        extraHelm.enabled = false;

                    MapFleetBodies.Remember(fleet.fleetName, visual.transform);
                }

                placed++;
            }

            Debug.Log($"<color=lime>[Governor] {placed} ship(s) on the water</color>");
        }

        private ShipBlueprint BlueprintFor(string saveId)
        {
            if (string.IsNullOrEmpty(saveId))
                return null;

            var store = new JsonShipSaveStore();
            if (!store.TryRead(out var data) || data?.ships == null)
                return null;

            ShipBuildRecord record = null;
            foreach (var ship in data.ships)
            {
                if (ship != null && ship.id == saveId)
                {
                    record = ship;
                    break;
                }
            }

            if (record == null)
                return null;

            var blueprint = ScriptableObject.CreateInstance<ShipBlueprint>();
            blueprint.shipName = record.shipName;
            blueprint.saveId = record.id;

            foreach (var hull in HullLibrary.All)
            {
                if (hull == null)
                    continue;
                if (hull.name != record.hullId && hull.hullName != record.hullId)
                    continue;

                blueprint.shipPrefab = hull.combatPrefab;
                break;
            }

            return blueprint.shipPrefab != null ? blueprint : null;
        }

        private ShipBlueprint FlagshipFromFile()
        {
            ActiveFleets.Load();
            string shipId = null;

            if (ActiveFleets.Current?.fleets != null)
            {
                foreach (var fleet in ActiveFleets.Current.fleets)
                {
                    if (fleet == null || fleet.sceneName != ActiveFleets.FreeRoamScene)
                        continue;
                    if (string.IsNullOrEmpty(fleet.flagshipShipId))
                        continue;

                    shipId = fleet.flagshipShipId;
                    break;
                }
            }

            if (string.IsNullOrEmpty(shipId))
                return null;

            var store = new JsonShipSaveStore();
            if (!store.TryRead(out var data) || data?.ships == null)
                return null;

            ShipBuildRecord record = null;
            foreach (var ship in data.ships)
            {
                if (ship != null && ship.id == shipId)
                {
                    record = ship;
                    break;
                }
            }

            if (record == null)
                return null;

            var blueprint = ScriptableObject.CreateInstance<ShipBlueprint>();
            blueprint.shipName = record.shipName;
            blueprint.saveId = record.id;

            foreach (var hull in HullLibrary.All)
            {
                if (hull == null)
                    continue;
                if (hull.name != record.hullId && hull.hullName != record.hullId)
                    continue;

                blueprint.shipPrefab = hull.combatPrefab;
                break;
            }

            return blueprint.shipPrefab != null ? blueprint : null;
        }

        private ShipBlueprint FlagshipBlueprint()
        {
            var fromFile = FlagshipFromFile();
            if (fromFile != null)
            {
                Debug.Log($"<color=lime>[Governor] Flagship from the file: {fromFile.shipName}</color>");
                return fromFile;
            }

            Debug.LogWarning("[Governor] No flagship in ActiveFleets. Using the launch memory.");

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
            if (string.IsNullOrEmpty(FleetName))
                return;

            ActiveFleets.Load();
            var data = ActiveFleets.Current;
            if (data.fleets == null)
                data.fleets = new List<ActiveFleetRecord>();

            ActiveFleetRecord record = null;
            for (int i = 0; i < data.fleets.Count; i++)
            {
                if (data.fleets[i] != null && data.fleets[i].fleetName == FleetName)
                {
                    record = data.fleets[i];
                    break;
                }
            }

            if (record == null)
            {
                record = new ActiveFleetRecord
                {
                    id = FleetName,
                    fleetName = FleetName,
                    slotIndex = data.fleets.Count
                };
                data.fleets.Add(record);
            }

            record.sceneName = _scene;
            // PENDING: MapSailController has no thePawn, so this block is held back.
            // if (MapBody != null && MapBody.thePawn != null)
            // {
            //     record.position = MapBody.thePawn.position;
            //     record.hasPosition = true;
            // }

            record.ships = new List<ActiveFleetShipRecord>();
            record.flagshipShipId = "";

            for (int i = 0; i < _ships.Count; i++)
            {
                var ship = _ships[i];
                if (ship == null)
                    continue;

                record.ships.Add(new ActiveFleetShipRecord
                {
                    shipId = ship.saveId,
                    shipName = ship.shipName,
                    currentHealth = _current[i],
                    maxHealth = _max[i],
                    alive = _current[i] > 0f
                });

                if (i == FlagIndex)
                    record.flagshipShipId = ship.saveId;
            }

            ActiveFleets.Save();
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
