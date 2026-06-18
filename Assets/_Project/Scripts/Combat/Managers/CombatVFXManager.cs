using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using _Project.Scripts.Combat.Data;

namespace _Project.Scripts.Combat.Managers
{
    public class CombatVFXManager : MonoBehaviour
    {
        public static CombatVFXManager Instance { get; private set; }

        [Header("Pooling Settings")]
        [Tooltip("Default pool size for most VFX")]
        public int defaultPoolSize = 20;
        [Tooltip("Max pool size for VFX")]
        public int maxPoolSize = 80;

        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _vfxPools = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, float lifetime = 3f)
        {
            if (prefab == null) return;

            GameObject instance = GetFromPool(prefab);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);

            StartCoroutine(ReturnToPoolAfterDelay(instance, prefab, lifetime));
        }
        
        /// <summary>
        /// Pre-warm important VFX at the start of a level
        /// </summary>
        public void PrewarmEffects(VFXProfile profile, int count = 15)
        {
            if (profile == null) return;

            if (profile.muzzleFlashVFX != null)
                PrewarmSingle(profile.muzzleFlashVFX, count);

            if (profile.impactVFX != null)
                PrewarmSingle(profile.impactVFX, count);

            if (profile.bulletTrailVFX != null)
                 PrewarmSingle(profile.bulletTrailVFX, count);

            if (profile.largeExplosionVFX != null)
                PrewarmSingle(profile.largeExplosionVFX, 8); // Less needed
        }

        private void PrewarmSingle(GameObject prefab, int count)
        {
            if (prefab == null) return;

            for (int i = 0; i < count; i++)
            {
                var obj = GetFromPool(prefab);
                ReleaseToPool(obj, prefab); // Immediately return to pool
            }

            // if (showDebugLogs)
            //     Debug.Log($"[CombatVFXManager] Pre-warmed {count}x {prefab.name}");
        }

        private void ReleaseToPool(GameObject obj, GameObject prefab)
        {
            if (_vfxPools.TryGetValue(prefab, out var pool))
                pool.Release(obj);
        }

        private GameObject GetFromPool(GameObject prefab)
        {
            if (!_vfxPools.TryGetValue(prefab, out var pool))
            {
                pool = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab),
                    actionOnGet: obj => obj.SetActive(true),
                    actionOnRelease: obj => obj.SetActive(false),
                    actionOnDestroy: Destroy,
                    collectionCheck: false,
                    defaultCapacity: defaultPoolSize,
                    maxSize: maxPoolSize
                );
                _vfxPools[prefab] = pool;
            }

            return pool.Get();
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject obj, GameObject prefab, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null && _vfxPools.ContainsKey(prefab))
                _vfxPools[prefab].Release(obj);
            else if (obj != null)
                Destroy(obj);
        }

        public void PlaySound(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        // Optional: Pre-warm important effects
        public void PrewarmVFX(GameObject prefab, int count)
        {
            if (prefab == null) return;
            for (int i = 0; i < count; i++)
            {
                var obj = GetFromPool(prefab);
                _vfxPools[prefab].Release(obj);
            }
        }
    }
}