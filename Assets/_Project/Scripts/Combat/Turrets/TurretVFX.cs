using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Managers;

namespace _Project.Scripts.Combat.Turrets
{
    public class TurretVFX : MonoBehaviour
    {
        public void PlayFireEffects(Transform muzzleTip, TurretWeaponData data)
        {
            if (muzzleTip == null || data?.vfxProfile == null) return;
            var profile = data.vfxProfile;

            if (CombatVFXManager.Instance == null) return;

            if (profile.muzzleFlashVFX != null)
                CombatVFXManager.Instance.SpawnVFX(profile.muzzleFlashVFX, muzzleTip.position, muzzleTip.rotation, 1.5f);

            if (profile.bulletTrailVFX != null)
                CombatVFXManager.Instance.SpawnVFX(profile.bulletTrailVFX, muzzleTip.position, muzzleTip.rotation, 4f);

            if (profile.fireSounds != null && profile.fireSounds.Length > 0)
            {
                AudioClip clip = profile.fireSounds[Random.Range(0, profile.fireSounds.Length)];
                CombatVFXManager.Instance.PlaySound(clip, muzzleTip.position, 0.9f);
            }
        }

        public void PlayImpactEffect(Vector3 position, TurretWeaponData data)
        {
            if (data?.vfxProfile == null) return;
            var profile = data.vfxProfile;

            if (CombatVFXManager.Instance == null) return;

            if (profile.impactVFX != null)
                CombatVFXManager.Instance.SpawnVFX(profile.impactVFX, position, Quaternion.identity, 2f);

            if (profile.hitSounds != null && profile.hitSounds.Length > 0)
            {
                AudioClip clip = profile.hitSounds[Random.Range(0, profile.hitSounds.Length)];
                CombatVFXManager.Instance.PlaySound(clip, position, 0.85f);
            }
        }
    }
}