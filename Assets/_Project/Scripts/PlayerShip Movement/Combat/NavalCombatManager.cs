using UnityEngine;
using _Project.Scripts.PlayerShip_Movement;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class NavalCombatManager : MonoBehaviour
    {
        [Header("Player Reference")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Health playerHealth;

        [Header("Turrets")]
        [SerializeField] private Transform[] turretBarrelPivots;   
        [SerializeField] private Transform[] muzzleTips;           
        [SerializeField] private GameObject bulletPrefab;

        [Header("Combat Settings")]
        [SerializeField] private float turretRange = 60f;
        [SerializeField] private float fireRate = 2.5f;      // Slow for testing
        [SerializeField] private float bulletSpeed = 15f;    // Slow for testing
        [SerializeField] private float damagePerShot = 25f;

        private float[] nextFireTimes;

        private void Awake()
        {
            if (playerTransform == null)
                playerTransform = GameObject.FindWithTag("Player")?.transform;

            if (playerHealth == null && playerTransform != null)
                playerHealth = playerTransform.GetComponent<Health>();

            nextFireTimes = new float[turretBarrelPivots.Length];
        }

        private void Update()
        {
            if (playerTransform == null) return;

            for (int i = 0; i < turretBarrelPivots.Length; i++)
            {
                Transform pivot = turretBarrelPivots[i];
                if (pivot == null) continue;

                Vector3 toPlayer = playerTransform.position - pivot.position;
                float sqrDist = toPlayer.sqrMagnitude;

                if (sqrDist <= turretRange * turretRange && sqrDist > 0.01f)
                {
                    AimAtPlayer(pivot, toPlayer);
                    
                    if (Time.time >= nextFireTimes[i])
                    {
                        Fire(i);
                        nextFireTimes[i] = Time.time + fireRate;
                    }
                }
            }
        }

        private void AimAtPlayer(Transform barrelPivot, Vector3 toPlayer)
        {
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.01f) return;

            barrelPivot.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        }

        private void Fire(int index)
        {
            if (bulletPrefab == null) 
            {
                Debug.LogWarning("Bullet Prefab not assigned!");
                return;
            }

            Transform muzzle = (muzzleTips != null && index < muzzleTips.Length && muzzleTips[index] != null)
                ? muzzleTips[index]
                : turretBarrelPivots[index];

            GameObject bulletGO = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

            Vector3 direction = (playerTransform.position - muzzle.position).normalized;
            direction.y = 0f;

            float currentSpeed = bulletSpeed > 5f ? bulletSpeed : 25f; // safety

            if (bulletGO.TryGetComponent<BulletSelfDestruct>(out var bsd))
            {
                bsd.Initialize(direction * currentSpeed, 6f);   // 6 seconds lifetime
            }
            else
            {
                Debug.LogError("Bullet is missing BulletSelfDestruct component!");
            }

            if (bulletGO.TryGetComponent<BulletDamage>(out var dmg))
                dmg.damageAmount = damagePerShot;

            Debug.Log($"[Fire] Bullet spawned at {muzzle.position} towards player");
        }

        public void OnPlayerHit(float damage)
        {
            playerHealth?.TakeDamage(damage);
        }
    }
}