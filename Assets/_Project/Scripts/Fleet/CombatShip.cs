using UnityEngine;
using _Project.Scripts.Combat;
using _Project.Scripts.Combat.Ship;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using _Project.Scripts.PlayerShip_Movement.Combat;

namespace _Project.Scripts.Fleet
{
    /// <summary>
    /// Main bridge component for each instantiated ship in combat.
    /// Holds the ShipBlueprint and coordinates Health, VFX, and movement.
    /// </summary>
    [RequireComponent(typeof(PlayerCombatMovementController))]
    [RequireComponent(typeof(Health))]
    public class CombatShip : MonoBehaviour
    {
        [Header("Data")]
        public ShipBlueprint Blueprint { get; private set; }

        [Header("Components")]
        public PlayerCombatMovementController Movement { get; private set; }
        public Health Health { get; private set; }

        private ShipVFX _shipVFX;

        private void Awake()
        {
            Movement = GetComponent<PlayerCombatMovementController>();
            Health = GetComponent<Health>();
            _shipVFX = GetComponent<ShipVFX>();
        }

        /// <summary>
        /// Initialize the ship with data from ShipBlueprint
        /// </summary>
        public void Initialize(ShipBlueprint blueprint)
        {
            if (blueprint == null) return;

            Blueprint = blueprint;
            gameObject.name = blueprint.shipName;

            // Apply health from blueprint
            if (Health != null)
            {
                Health.SetMaxHealth(blueprint.totalHealth, true);
            }

            // Apply VFX profile
            if (_shipVFX != null)
            {
                _shipVFX.vfxProfile = blueprint.vfxProfile;
            }

            Debug.Log($"[CombatShip] Initialized: {blueprint.shipName}");
        }

        /// <summary>
        /// Returns true if this ship is the current flagship
        /// </summary>
        public bool IsFlagship()
        {
            return FleetManager.Instance != null && FleetManager.Instance.flagship == Movement;
        }
    }
}