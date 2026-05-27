using UnityEngine;

namespace ScriptableObjects.Combat
{
    [CreateAssetMenu(menuName = "Combat/Turret Weapon Data")]
    public class TurretWeaponData : ScriptableObject
    {
        [Header("Stats")]
        public float damage = 25f;
        public float fireRate = 1.8f;
        public float bulletSpeed = 35f;
        public float range = 60f;

        [Header("Visuals")]
        public GameObject muzzleFlashPrefab;
        public GameObject impactPrefab;
        public GameObject bulletTrailPrefab;     // Optional tracer
        public ParticleSystem shellEjectionPrefab;

        [Header("Audio")]
        public AudioClip fireSound;
        public AudioClip hitSound;
    }
}