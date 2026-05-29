using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Managers;
using _Project.Scripts.Combat.Projectiles;

namespace _Project.Scripts.Combat.Turrets
{
    /// <summary>
    /// Controls individual turret behavior: aiming and firing.
    /// Uses the new BulletPoolManager for high-performance bullet spawning.
    /// </summary>
    public class Turret : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform barrelPivot;
        [SerializeField] private Transform muzzleTip;

        private float nextFireTime = 0f;

        /// <summary>
        /// Called every frame by NavalCombatManager (or similar).
        /// Handles aiming + firing decisions.
        /// </summary>
        public void UpdateTurret(Transform target, TurretWeaponData data)
        {
            if (target == null || barrelPivot == null || data == null) return;

            // Range check
            Vector3 toTarget = target.position - barrelPivot.position;
            if (toTarget.sqrMagnitude > data.range * data.range) return;

            // Aim (keep turret level for naval feel)
            Vector3 aimDirection = toTarget;
            aimDirection.y = 0f;

            if (aimDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(aimDirection, Vector3.up);
                barrelPivot.rotation = targetRot;
            }

            // Fire
            if (Time.time >= nextFireTime)
            {
                Fire(target, data);
                nextFireTime = Time.time + data.fireRate;
            }
        }

        private void Fire(Transform target, TurretWeaponData data)
        {
            if (muzzleTip == null || data == null || data.bulletPrefab == null) return;

            // 1. Muzzle Flash
            if (data.muzzleFlashPrefab != null)
            {
                Instantiate(data.muzzleFlashPrefab, muzzleTip.position, muzzleTip.rotation);
            }

            // 2. Get bullet from the new pooling system
            GameObject bullet = BulletPoolManager.Instance?.GetBullet(data);

            if (bullet == null) return;

            // Position & rotate bullet
            bullet.transform.position = muzzleTip.position;
            bullet.transform.rotation = muzzleTip.rotation;

            // Calculate direction
            Vector3 direction = target != null
                ? (target.position - muzzleTip.position).normalized
                : muzzleTip.forward;
            direction.y = 0f;

            // 3. Initialize BulletSelfDestruct
            if (bullet.TryGetComponent<BulletSelfDestruct>(out var bsd))
            {
                bsd.Initialize(direction * data.bulletSpeed, 6f, data);
            }

            // 4. Setup BulletDamage
            if (bullet.TryGetComponent<BulletDamage>(out var damageComp))
            {
                damageComp.weaponData = data;
            }
        }
    }
}