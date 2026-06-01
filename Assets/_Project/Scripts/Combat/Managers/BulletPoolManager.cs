using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Projectiles;

namespace _Project.Scripts.Combat.Managers
{
    /// <summary>
    /// Central manager for all projectile/bullet pools.
    /// Uses Unity's ObjectPool<T> with pre-warming per TurretWeaponData.
    /// Optimized for large-scale naval combat and barrage fire.
    /// </summary>
    public class BulletPoolManager : MonoBehaviour
    {
        public static BulletPoolManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool showPoolDebugLogs = false;

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
            PrewarmAllPools();
        }

        public void PrewarmAllPools()
        {
            foreach (var kvp in _pools)
            {
                if (kvp.Key.prewarmOnStart)
                    PrewarmPool(kvp.Key);
            }

            if (showPoolDebugLogs)
                Debug.Log($"[BulletPoolManager] Pre-warming complete. Pools: {_pools.Count}");
        }

        private void PrewarmPool(TurretWeaponData data)
        {
            if (!_pools.ContainsKey(data)) return;

            var pool = _pools[data];
            var prewarmList = new List<GameObject>();

            for (int i = 0; i < data.initialPoolSize; i++)
            {
                GameObject bullet = pool.Get();
                prewarmList.Add(bullet);
            }

            foreach (var bullet in prewarmList)
            {
                pool.Release(bullet);
            }
        }

        public void RegisterPool(TurretWeaponData data)
        {
            if (data == null || _pools.ContainsKey(data)) return;

            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreatePooledBullet(data),
                actionOnGet: OnBulletTakenFromPool,
                actionOnRelease: OnBulletReturnedToPool,
                actionOnDestroy: OnBulletDestroyed,
                collectionCheck: false,
                defaultCapacity: data.initialPoolSize,
                maxSize: data.maxPoolSize
            );

            _pools.Add(data, pool);

            if (showPoolDebugLogs)
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

            if (_bulletToDataMap.TryGetValue(bullet, out TurretWeaponData data) &&
                _pools.TryGetValue(data, out ObjectPool<GameObject> pool))
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

        private GameObject CreatePooledBullet(TurretWeaponData data)
        {
            if (data.bulletPrefab == null)
            {
                Debug.LogError($"[BulletPoolManager] bulletPrefab is NULL on TurretWeaponData: {data.name}");
                return new GameObject("MissingBulletPrefab_Error");
            }

            GameObject bullet = Instantiate(data.bulletPrefab);

            // Auto-add required components if missing
            if (!bullet.TryGetComponent<BulletSelfDestruct>(out _))
            {
                bullet.AddComponent<BulletSelfDestruct>();
            }

            if (!bullet.TryGetComponent<BulletDamage>(out _))
            {
                bullet.AddComponent<BulletDamage>();
            }

            bullet.SetActive(false);
            return bullet;
        }

        private void OnBulletTakenFromPool(GameObject bullet) => bullet.SetActive(true);
        private void OnBulletReturnedToPool(GameObject bullet) => bullet.SetActive(false);

        private void OnBulletDestroyed(GameObject bullet)
        {
            _bulletToDataMap.Remove(bullet);
            Destroy(bullet);
        }
    }
}