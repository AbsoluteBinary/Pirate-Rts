using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Managers;
using _Project.Scripts.PlayerShip_Movement;

namespace _Project.Scripts.Combat.Projectiles
{
    /// <summary>
    /// Handles bullet collision, applies damage, and returns the bullet to the pool.
    /// </summary>
    public class BulletDamage : MonoBehaviour
    {
        [HideInInspector] public TurretWeaponData weaponData;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Health health))
            {
                float damageAmount = weaponData != null ? weaponData.damage : 25f;
                health.TakeDamage(damageAmount);

                // Optional impact effects
                if (weaponData != null && weaponData.impactPrefab != null)
                {
                    Instantiate(weaponData.impactPrefab, transform.position, Quaternion.identity);
                }

                if (weaponData != null && weaponData.hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(weaponData.hitSound, transform.position);
                }
            }

            ReturnToPool();
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