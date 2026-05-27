using UnityEngine;
using _Project.Scripts.PlayerShip_Movement;
using ScriptableObjects.Combat;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class NavalCombatManager : MonoBehaviour
    {
        [Header("Player Reference")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Health playerHealth;

        [Header("Turrets")]
        [SerializeField] private Turret[] turrets;           // Better structure for multiple turrets

        [Header("Weapon Data")]
        [SerializeField] private TurretWeaponData weaponData;

        private void Awake()
        {
            // Auto-find player
            if (playerTransform == null)
                playerTransform = GameObject.FindWithTag("Player")?.transform;

            if (playerHealth == null && playerTransform != null)
                playerHealth = playerTransform.GetComponent<Health>();
        }

        private void Update()
        {
            if (playerTransform == null || weaponData == null) return;

            foreach (var turret in turrets)
            {
                if (turret != null)
                    turret.UpdateTurret(playerTransform, weaponData);
            }
        }

        // Called from BulletDamage when hitting player
        public void OnPlayerHit(float damage)
        {
            playerHealth?.TakeDamage(damage);
        }
    }
}