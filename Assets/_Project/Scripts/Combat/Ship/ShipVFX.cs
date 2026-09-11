using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Managers;

namespace _Project.Scripts.Combat.Ship
{
    public class ShipVFX : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health health;

        [Header("Effects")]
        public VFXProfile vfxProfile;

        [Header("Settings")]
        public float destroyDelay = 2.5f;

        private AudioSource _audioSource;

        private void Awake()
        {
            if (health == null)
                health = GetComponent<Health>();

            _audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0f;

            // Debug log to help track which ShipVFX is active
            Debug.LogWarning($"[ShipVFX] Awake on: {gameObject.name} | Pos: {transform.position}");
        }

        private void OnEnable()
        {
            if (health != null)
                health.OnDeath.AddListener(PlayDeathVFX);
        }

        private void OnDisable()
        {
            if (health != null)
                health.OnDeath.RemoveListener(PlayDeathVFX);
        }

        private void OnHealthChanged(float currentHealth)
        {
            PlayDamageHitVFX();
        }

        public void PlayDamageHitVFX()
        {
            // === VERY DEFENSIVE VERSION ===
            Vector3 pos;

            if (health != null && health.gameObject != null)
            {
                pos = health.gameObject.transform.position + Vector3.up * 2f;
            }
            else
            {
                pos = transform.position + Vector3.up * 2f;
            }

            // Extra safety filter - block anything near world origin
            if (Vector3.Distance(pos, Vector3.zero) < 8f)
            {
                Debug.LogWarning($"[ShipVFX] Blocked damage VFX near origin on {gameObject.name}");
                return;
            }

            Debug.Log($"[ShipVFX] DAMAGE HIT | Object: {gameObject.name} | Pos: {pos}");

            if (vfxProfile == null || CombatVFXManager.Instance == null) return;

            if (vfxProfile.impactVFX != null)
                CombatVFXManager.Instance.SpawnVFX(vfxProfile.impactVFX, pos, Quaternion.identity, 2f);
        }

        public void PlayDeathVFX()
        {
            Vector3 pos = transform.position;

            if (vfxProfile == null || CombatVFXManager.Instance == null) return;

            if (vfxProfile.largeExplosionVFX != null)
                CombatVFXManager.Instance.SpawnVFX(vfxProfile.largeExplosionVFX, pos, Quaternion.identity, 6f);

            if (vfxProfile.hitSounds != null && vfxProfile.hitSounds.Length > 0)
            {
                AudioClip clip = vfxProfile.hitSounds[Random.Range(0, vfxProfile.hitSounds.Length)];
                CombatVFXManager.Instance.PlaySound(clip, pos, 1.3f);
            }
        }
    }
}