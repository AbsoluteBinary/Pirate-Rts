using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class TurretController : MonoBehaviour
    {
        [Header("Turret Settings")]
        [SerializeField] private Transform barrelPivot;     // BarrelAimPivot
        [SerializeField] private Transform muzzleTip;       // Tip
        [SerializeField] private GameObject bulletPrefab;

        [Header("Combat Stats")]
        [SerializeField] private float range = 60f;
        [SerializeField] private float fireRate = 1.8f;
        [SerializeField] private float bulletSpeed = 30f;
        [SerializeField] private float damagePerShot = 25f;

        private float nextFireTime;

        public void Initialize(Transform player)
        {
            // Can be used later for more advanced behavior
        }

        public void UpdateTurret(Transform playerTransform)
        {
            if (playerTransform == null || barrelPivot == null) return;

            float sqrDist = (playerTransform.position - barrelPivot.position).sqrMagnitude;

            if (sqrDist <= range * range && sqrDist > 0.01f)
            {
                AimAtPlayer(playerTransform);

                if (Time.time >= nextFireTime)
                {
                    Fire(playerTransform);
                    nextFireTime = Time.time + fireRate;
                }
            }
        }

        private void AimAtPlayer(Transform playerTransform)
        {
            Vector3 toPlayer = playerTransform.position - barrelPivot.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude < 0.01f) return;

            barrelPivot.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        }

        private void Fire(Transform playerTransform)
        {
            if (bulletPrefab == null || muzzleTip == null) return;

            GameObject bulletGO = Instantiate(bulletPrefab, muzzleTip.position, muzzleTip.rotation);

            Vector3 direction = (playerTransform.position - muzzleTip.position).normalized;
            direction.y = 0f;

            if (bulletGO.TryGetComponent<BulletSelfDestruct>(out var bsd))
            {
                bsd.Initialize(direction * bulletSpeed, 8f);
            }

            if (bulletGO.TryGetComponent<BulletDamage>(out var dmg))
                dmg.damageAmount = damagePerShot;
        }

        // Getters for NavalCombatManager if needed
        public float Range => range;
    }
}