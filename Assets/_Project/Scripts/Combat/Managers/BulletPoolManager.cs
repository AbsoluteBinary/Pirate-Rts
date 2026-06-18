using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Projectiles;

namespace _Project.Scripts.Combat.Managers
{
    /// <summary>
    /// Central manager for bullet pooling.
    /// Uses Unity's ObjectPool for high performance in large-scale naval combat.
    /// </summary>
    public class BulletPoolManager : MonoBehaviour
    {
        public static BulletPoolManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private readonly Dictionary<TurretWeaponData, ObjectPool<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, TurretWeaponData> _bulletToDataMap = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            PrewarmPools();
        }

        /// <summary>
        /// Pre-warms pools that have prewarmOnStart enabled in their TurretWeaponData.
        /// </summary>
        public void PrewarmPools()
        {
            foreach (var data in _pools.Keys)
            {
                if (data.prewarmOnStart)
                    PrewarmPool(data);
            }

            if (showDebugLogs)
                Debug.Log($"[BulletPoolManager] Pre-warming complete. Active pools: {_pools.Count}");
        }

        private void PrewarmPool(TurretWeaponData data)
        {
            if (!_pools.TryGetValue(data, out var pool)) return;

            var tempList = new List<GameObject>();

            for (int i = 0; i < data.initialPoolSize; i++)
            {
                tempList.Add(pool.Get());
            }

            foreach (var bullet in tempList)
            {
                pool.Release(bullet);
            }
        }

        public void RegisterPool(TurretWeaponData data)
        {
            if (data == null || _pools.ContainsKey(data)) return;

            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreateBullet(data),
                actionOnGet: bullet => bullet.SetActive(true),
                actionOnRelease: bullet => bullet.SetActive(false),
                actionOnDestroy: DestroyBullet,
                collectionCheck: false,
                defaultCapacity: data.initialPoolSize,
                maxSize: data.maxPoolSize
            );

            _pools.Add(data, pool);

            if (showDebugLogs)
                Debug.Log($"[BulletPoolManager] Registered pool for: {data.name}");
        }

        public GameObject GetBullet(TurretWeaponData data)
        {
            if (data == null) return null;

            if (!_pools.ContainsKey(data))
                RegisterPool(data);

            GameObject bullet = _pools[data].Get();
            _bulletToDataMap[bullet] = data;
            return bullet;
        }

        public void ReturnBullet(GameObject bullet)
        {
            if (bullet == null) return;

            if (_bulletToDataMap.TryGetValue(bullet, out var data) &&
                _pools.TryGetValue(data, out var pool))
            {
                pool.Release(bullet);
                _bulletToDataMap.Remove(bullet);
            }
            else
            {
                bullet.SetActive(false);
            }
        }

        // ==================== INTERNAL ====================

        private GameObject CreateBullet(TurretWeaponData data)
        {
            if (data.bulletPrefab == null)
            {
                Debug.LogError($"[BulletPoolManager] Missing bulletPrefab on {data.name}");
                return new GameObject("MissingBulletPrefab");
            }

            GameObject bullet = Instantiate(data.bulletPrefab);

            // Ensure required components exist
            if (!bullet.TryGetComponent<BulletSelfDestruct>(out _))
                bullet.AddComponent<BulletSelfDestruct>();

            if (!bullet.TryGetComponent<BulletDamage>(out _))
                bullet.AddComponent<BulletDamage>();

            bullet.SetActive(false);
            return bullet;
        }

        private void DestroyBullet(GameObject bullet)
        {
            _bulletToDataMap.Remove(bullet);
            Destroy(bullet);
        }
    }
}