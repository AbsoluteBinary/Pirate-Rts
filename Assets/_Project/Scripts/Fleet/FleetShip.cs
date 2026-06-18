using UnityEngine;
using _Project.Scripts.PlayerShip_Movement.Combat;

namespace _Project.Scripts.Fleet
{
    /// <summary>
    /// Attach this to every ship that should be part of the player fleet.
    /// Automatically registers the ship with FleetManager.
    /// </summary>
    [RequireComponent(typeof(PlayerCombatMovementController))]
    public class FleetShip : MonoBehaviour
    {
        [Header("Fleet Settings")]
        [SerializeField] private bool isFlagship = false;

        private void Awake()
        {
            // Auto-register when the ship is loaded
            FleetManager.Instance?.RegisterShip(
                GetComponent<PlayerCombatMovementController>(), 
                isFlagship);
        }

        private void OnDestroy()
        {
            // Optional: Clean up if needed in the future
        }
    }
}