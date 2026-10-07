using System.Collections.Generic;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using _Project.Scripts.PlayerShip_Movement;
using _Project.Scripts.PlayerShip_Movement.Combat;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public class FleetGovernor : MonoBehaviour
    {
        public static FleetGovernor Instance { get; private set; }

        public string FleetName { get; private set; }
        public int FlagIndex { get; private set; }
        public readonly List<ShipBlueprint> Ships = new List<ShipBlueprint>();

        public MapSailController MapBody { get; private set; }
        public PlayerCombatMovementController FlagshipBody { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void TakeLaunch(string fleetName, ShipBlueprint[] ships, int flagIndex)
        {
            FleetName = fleetName;
            FlagIndex = flagIndex;
            Ships.Clear();
            FlagshipBody = null;

            if (ships == null)
                return;

            foreach (var ship in ships)
            {
                if (ship != null)
                    Ships.Add(ship);
            }

            Debug.Log($"<color=cyan>[Governor] Holding '{FleetName}' ({Ships.Count} ship(s)), flag slot {FlagIndex}</color>");
        }

        public void AttachMap(MapSailController map)
        {
            MapBody = map;
        }

        public void AttachCombatBody(PlayerCombatMovementController body, bool isFlagship)
        {
            if (body == null || !isFlagship)
                return;

            FlagshipBody = body;
            body.enabled = true;
            Debug.Log($"<color=lime>[Governor] Combat flagship is '{body.gameObject.name}'</color>");
        }

        public void OrderMove(Vector3 worldPosition)
        {
            var body = LiveBody();
            if (body == null)
            {
                Debug.LogWarning("[Governor] OrderMove found no body.");
                return;
            }

            body.enabled = true;
            body.MoveToLocation(worldPosition);
            Debug.Log($"[Governor] '{body.gameObject.name}' to {worldPosition}");
        }

        private PlayerCombatMovementController LiveBody()
        {
            if (FlagshipBody != null && FlagshipBody.gameObject.activeInHierarchy)
                return FlagshipBody;

            var helm = MapBody != null ? MapBody.Helm : null;
            if (helm != null && helm.gameObject.activeInHierarchy)
                return helm;

            var found = FindObjectsByType<PlayerCombatMovementController>(FindObjectsSortMode.None);
            foreach (var ship in found)
            {
                if (ship == null || !ship.gameObject.activeInHierarchy)
                    continue;

                FlagshipBody = ship;
                Debug.Log($"[Governor] Adopted '{ship.gameObject.name}'");
                return ship;
            }

            return null;
        }
    }
}
