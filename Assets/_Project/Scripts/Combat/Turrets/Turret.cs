using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Managers;
using _Project.Scripts.Combat.Projectiles;
using _Project.Scripts.Fleet;
using _Project.Scripts.PlayerShip_Movement.Combat;

namespace _Project.Scripts.Combat.Turrets
{
    public class Turret : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField] private Transform barrelPivot;
        [SerializeField] private Transform muzzleTip;

        [Header("VFX & Audio")]
        [SerializeField] private TurretVFX turretVFX;

        [Header("Targeting")]
        [Tooltip("How often the turret updates its target (in seconds)")]
        [SerializeField] private float targetUpdateInterval = 1.5f;
        
        [Header("Weapon Data")]
        [SerializeField] private TurretWeaponData data;

        private float nextFireTime = 0f;
        private float nextTargetUpdateTime = 0f;
        private Transform currentTarget;

        private void Awake()
        {
            if (turretVFX == null)
                turretVFX = GetComponentInChildren<TurretVFX>();
        }

        public void Update()
        {
            // Update target periodically
            if (Time.time >= nextTargetUpdateTime)
            {
                UpdateTarget();
                nextTargetUpdateTime = Time.time + targetUpdateInterval;
            }

            if (currentTarget != null)
            {
                UpdateTurret(currentTarget);
            }
        }

        private void UpdateTarget()
        {
            var body = FleetGovernor.Instance != null ? FleetGovernor.Instance.FlagshipBody : null;
            currentTarget = body != null && body.gameObject.activeInHierarchy
                ? body.transform
                : null;
        }

        public void UpdateTurret(Transform target)
        {
            if (target == null || barrelPivot == null) return;

            // Range check
            Vector3 toTarget = target.position - barrelPivot.position;
            if (toTarget.sqrMagnitude > data.range * data.range) return;   // Note: 'data' needs to be assigned

            // Aim (naval style)
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
                Fire(target);
                nextFireTime = Time.time + data.fireRate;   // 'data' needs to be assigned per turret
            }
        }

        private void Fire(Transform target)
        {
            if (muzzleTip == null || data == null) return;

            turretVFX?.PlayFireEffects(muzzleTip, data);

            GameObject bullet = BulletPoolManager.Instance?.GetBullet(data);
            if (bullet == null) return;

            bullet.transform.position = muzzleTip.position;
            bullet.transform.rotation = muzzleTip.rotation;

            Vector3 direction = (target.position - muzzleTip.position).normalized;
            direction.y = 0f;

            // Clean Initialize call
            if (bullet.TryGetComponent<BulletSelfDestruct>(out var bsd))
            {
                bsd.Initialize(direction * data.bulletSpeed, 6f, data);
            }

            if (bullet.TryGetComponent<BulletDamage>(out var damageComp))
            {
                damageComp.weaponData = data;
                damageComp.turretVFX = turretVFX;
            }
        }
    }
}