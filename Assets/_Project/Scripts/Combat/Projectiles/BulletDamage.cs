using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Managers;
using _Project.Scripts.Combat.Turrets;

namespace _Project.Scripts.Combat.Projectiles
{
    /// <summary>
    /// Handles bullet collision, damage application, and impact VFX/Audio.
    /// Now delegates impact effects to TurretVFX for consistency.
    /// </summary>
    public class BulletDamage : MonoBehaviour
    {
        [HideInInspector] public TurretWeaponData weaponData;

        // Optional: Reference to the turret that fired this bullet (for VFX)
        [HideInInspector] public TurretVFX turretVFX;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[BulletDamage] Hit: {other.gameObject.name} | Position: {other.transform.position}");

            if (other.TryGetComponent<Health>(out var health))
            {
                float damageAmount = weaponData != null ? weaponData.damage : 25f;
                health.TakeDamage(damageAmount);
            }

            PlayImpactEffects();
            ReturnToPool();
        }

        private void PlayImpactEffects()
        {
            if (weaponData?.vfxProfile == null) return;

            if (turretVFX != null)
            {
                turretVFX.PlayImpactEffect(transform.position, weaponData);
            }
            else if (CombatVFXManager.Instance != null)
            {
                // Fallback
                var profile = weaponData.vfxProfile;
                if (profile.impactVFX != null)
                    CombatVFXManager.Instance.SpawnVFX(profile.impactVFX, transform.position, Quaternion.identity);

                if (profile.hitSounds != null && profile.hitSounds.Length > 0)
                {
                    AudioClip clip = profile.hitSounds[Random.Range(0, profile.hitSounds.Length)];
                    CombatVFXManager.Instance.PlaySound(clip, transform.position, 0.85f);
                }
            }
        }

        private void ReturnToPool()
        {
            if (TryGetComponent(out BulletSelfDestruct selfDestruct))
            {
                selfDestruct.ForceReturnToPool();
            }
            else if (BulletPoolManager.Instance != null)
            {
                BulletPoolManager.Instance.ReturnBullet(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}