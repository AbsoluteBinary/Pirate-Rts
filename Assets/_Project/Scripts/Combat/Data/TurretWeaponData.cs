using UnityEngine;

namespace _Project.Scripts.Combat.Data
{
    [CreateAssetMenu(menuName = "Combat/Turret Weapon Data", fileName = "New TurretWeaponData")]
    public class TurretWeaponData : ScriptableObject
    {
        [Header("Stats")]
        public float damage = 25f;
        public float fireRate = 1.8f;
        public float bulletSpeed = 35f;
        public float range = 60f;

        [Header("Visuals")]
        public GameObject bulletPrefab;          // ← Main bullet prefab (used for pooling)
        public GameObject muzzleFlashPrefab;
        public GameObject impactPrefab;
        public GameObject bulletTrailPrefab;     // Optional tracer
        public ParticleSystem shellEjectionPrefab;

        [Header("Audio")]
        public AudioClip fireSound;
        public AudioClip hitSound;

        [Header("Pooling (Bullet Pool Settings)")]
        [Tooltip("How many bullets to pre-warm for this weapon type at scene start.")]
        public int initialPoolSize = 80;

        [Tooltip("Maximum number of bullets allowed in the pool for this weapon type.")]
        public int maxPoolSize = 400;

        [Tooltip("Should this pool be pre-warmed automatically when the level loads?")]
        public bool prewarmOnStart = true;
    }
}