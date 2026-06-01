using Sirenix.OdinInspector;
using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Turrets;

namespace _Project.Scripts.Combat.Managers
{
    /// <summary>
    /// High-level naval combat coordinator.
    /// 
    /// Current Responsibilities:
    /// - Controls all Turret components using TurretWeaponData
    /// - Provides access to player Health (single source of truth)
    /// 
    /// Note: Legacy IMGUI will be replaced by ShipHUDController + UIToolkit later.
    /// </summary>
    public class NavalCombatManager : MonoBehaviour
    {
        #region Inspector - Player & Ship

        [Title("Player & Ship")]
        [InfoBox("The Health component is the single source of truth for player health.")]
        [PropertyTooltip("Reference to the player transform (used for turret targeting)")]
        [SerializeField] private Transform player;

        [PropertyTooltip("The main ship GameObject that will be disabled on death")]
        [SerializeField] private GameObject shipGameObject;

        [PropertyTooltip("Health component on the player ship. This is the single source of truth.")]
        [SerializeField] private Health playerHealth;

        #endregion

        #region Inspector - Turrets

        [Title("Turrets")]
        [InfoBox("Assign all Turret components and their matching TurretWeaponData assets in the same order.")]
        [PropertyTooltip("All Turret components that should be updated every frame")]
        [SerializeField] private Turret[] turrets;

        [PropertyTooltip("TurretWeaponData for each turret (must match the order of the Turrets array)")]
        [SerializeField] private TurretWeaponData[] turretWeaponData;

        #endregion

        #region Inspector - Debug

        [Title("Debug")]
        [PropertyTooltip("Show debug logs from this manager")]
        [SerializeField] private bool showDebugLogs = false;

        #endregion

        // ==================== RUNTIME ====================

        private void Awake()
        {
            InitializeReferences();
        }

        private void Update()
        {
            UpdateAllTurrets();
        }

        private void InitializeReferences()
        {
            // Auto-find Health if not assigned
            if (playerHealth == null && player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }

            if (playerHealth == null)
            {
                Debug.LogWarning("[NavalCombatManager] No Health component found on player!", this);
            }

            // Auto-find turrets if none assigned
            if (turrets == null || turrets.Length == 0)
            {
                turrets = GetComponentsInChildren<Turret>(true);
                if (showDebugLogs)
                    Debug.Log($"[NavalCombatManager] Auto-found {turrets.Length} turrets.");
            }
        }

        private void UpdateAllTurrets()
        {
            if (turrets == null || turretWeaponData == null) return;

            int count = Mathf.Min(turrets.Length, turretWeaponData.Length);

            for (int i = 0; i < count; i++)
            {
                if (turrets[i] != null && turretWeaponData[i] != null)
                {
                    turrets[i].UpdateTurret(player, turretWeaponData[i]);
                }
            }
        }

        /// <summary>
        /// Applies damage to the player through the Health component.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(amount);
            }
            else if (showDebugLogs)
            {
                Debug.LogWarning("[NavalCombatManager] TakeDamage called but no Health component is assigned.");
            }
        }

        /// <summary>
        /// Called when the player ship is destroyed.
        /// </summary>
        public void OnPlayerShipDestroyed()
        {
            if (shipGameObject != null)
            {
                shipGameObject.SetActive(false);
            }

            if (showDebugLogs)
                Debug.Log("[NavalCombatManager] Player ship has been destroyed.");
        }
    }
}