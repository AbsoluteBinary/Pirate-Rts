using UnityEngine;

namespace _Project.Scripts.Combat.Data
{
    [CreateAssetMenu(menuName = "Combat/VFX Profile", fileName = "New VFXProfile")]
    public class VFXProfile : ScriptableObject
    {
        [Header("Muzzle Effects")]
        public GameObject muzzleFlashVFX;
        public GameObject bulletTrailVFX;
        public ParticleSystem shellEjectionVFX;

        [Header("Impact / Hit Effects")]
        public GameObject impactVFX;

        [Header("Audio")]
        public AudioClip[] fireSounds;
        public AudioClip[] hitSounds;

        [Header("Optional Shared Effects")]
        public GameObject largeExplosionVFX;   // Can be used by ships/enemies too
        public GameObject fireLoopVFX;
    }
}