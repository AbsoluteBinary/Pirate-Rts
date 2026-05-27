using _Project.Scripts.PlayerShip_Movement.Combat.ObjectPooling;
using ScriptableObjects.Combat;
using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class Turret : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform barrelPivot;
        [SerializeField] private Transform muzzleTip;

        private float nextFireTime = 0f;

        public void UpdateTurret(Transform target, TurretWeaponData data)
        {
            if (target == null || barrelPivot == null || data == null) return;

            Vector3 toTarget = target.position - barrelPivot.position;
            float sqrDist = toTarget.sqrMagnitude;

            // Out of range
            if (sqrDist > data.range * data.range) return;

            // Aim (flat naval style)
            Vector3 aimDirection = toTarget;
            aimDirection.y = 0f;

            if (aimDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(aimDirection, Vector3.up);
                barrelPivot.rotation = targetRot;
            }

            // Fire logic
            if (Time.time >= nextFireTime)
            {
                Fire(target, data);          // ← Pass target here
                nextFireTime = Time.time + data.fireRate;
            }
        }

        private void Fire(Transform target, TurretWeaponData data)
        {
            if (muzzleTip == null) return;

            // Muzzle Flash
            if (data.muzzleFlashPrefab != null)
            {
                Instantiate(data.muzzleFlashPrefab, muzzleTip.position, muzzleTip.rotation);
            }

            // Spawn Bullet from Pool
            GameObject bullet = ObjectPooler.Instance?.Spawn("Bullet", muzzleTip.position, muzzleTip.rotation);

            if (bullet != null)
            {
                Vector3 direction = target != null 
                    ? (target.position - muzzleTip.position).normalized 
                    : muzzleTip.forward;

                direction.y = 0f;

                if (bullet.TryGetComponent<BulletSelfDestruct>(out var bsd))
                {
                    bsd.Initialize(direction * data.bulletSpeed, 6f, "Bullet");
                }

                if (bullet.TryGetComponent<BulletDamage>(out var damageComp))
                {
                    damageComp.damageAmount = data.damage;
                }
            }
        }
    }
}