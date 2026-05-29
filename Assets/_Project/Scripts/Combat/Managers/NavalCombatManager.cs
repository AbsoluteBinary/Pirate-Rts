using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Turrets;

namespace _Project.Scripts.Combat.Managers
{
    /// <summary>
    /// High-level combat coordinator.
    /// Currently in transition: Uses the new modular Turret components while keeping old IMGUI for now.
    /// </summary>
    public class NavalCombatManager : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Transform player;
        [SerializeField] private float playerHealth = 100f;
        private float currentHealth;
        [SerializeField] private GameObject shipGameObject;

        [Header("Turrets (New Modular System)")]
        [Tooltip("Assign all Turret components here")]
        [SerializeField] private Turret[] turrets;

        [Tooltip("Assign matching TurretWeaponData for each turret (same order)")]
        [SerializeField] private TurretWeaponData[] turretWeaponData;

        [Header("Legacy Settings (will be removed later)")]
        [SerializeField] private GameObject trailPrefab;

        // IMGUI damage popup system (kept for now)
        private readonly List<DamagePopup> _damagePopups = new();
        private readonly GUIStyle damageStyle = new GUIStyle();

        private void Awake()
        {
            currentHealth = playerHealth;
            damageStyle.fontSize = 48;
            damageStyle.alignment = TextAnchor.MiddleCenter;
            damageStyle.richText = true;

            if (player == null)
                player = GameObject.FindWithTag("Player")?.transform;

            // Auto-find turrets if none assigned
            if (turrets == null || turrets.Length == 0)
                turrets = GetComponentsInChildren<Turret>();
        }

        private void Update()
        {
            // === NEW MODULAR TURRET SYSTEM ===
            if (turrets != null && turretWeaponData != null)
            {
                int count = Mathf.Min(turrets.Length, turretWeaponData.Length);

                for (int i = 0; i < count; i++)
                {
                    if (turrets[i] != null && turretWeaponData[i] != null)
                    {
                        turrets[i].UpdateTurret(player, turretWeaponData[i]);
                    }
                }
            }

            // === OLD IMGUI DAMAGE POPUPS (keep for now) ===
            for (int i = _damagePopups.Count - 1; i >= 0; i--)
            {
                _damagePopups[i].timer -= Time.unscaledDeltaTime;
                if (_damagePopups[i].timer <= 0f)
                    _damagePopups.RemoveAt(i);
            }
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;

            if (currentHealth <= 0f)
            {
                OnShipDestroyed();
            }
        }

        private void OnShipDestroyed()
        {
            if (shipGameObject != null)
                shipGameObject.SetActive(false);

            Debug.Log("[NavalCombatManager] Player ship destroyed.");
        }

        // IMGUI kept temporarily for debugging
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 300, 120), GUI.skin.box);
            GUILayout.Label("<size=20><b>Ship Status</b></size>");
            GUILayout.Label($"Health: {currentHealth:F0} / {playerHealth}");
            GUILayout.EndArea();
        }

        [System.Serializable]
        public class DamagePopup
        {
            public float amount;
            public float timer;
        }
    }
}