using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Managers;

namespace _Project.Scripts.Combat
{
    public class CombatLevelStarter : MonoBehaviour
    {
        [Header("VFX Pre-warming")]
        [Tooltip("Add all VFXProfiles used in this level")]
        public VFXProfile[] vfxProfilesToPrewarm;

        [Tooltip("How many instances to pre-warm per effect")]
        public int prewarmCount = 15;

        private void Start()
        {
            if (CombatVFXManager.Instance == null)
            {
                Debug.LogWarning("[CombatLevelStarter] CombatVFXManager not found!");
                return;
            }

            foreach (var profile in vfxProfilesToPrewarm)
            {
                CombatVFXManager.Instance.PrewarmEffects(profile, prewarmCount);
            }

            Debug.Log("[CombatLevelStarter] VFX Pre-warming completed.");
        }
    }
}