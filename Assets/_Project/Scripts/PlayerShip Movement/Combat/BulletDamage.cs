using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class BulletDamage : MonoBehaviour
    {
        [HideInInspector] public float damageAmount = 25f;
        
        [Header("Collision Settings")]
        [SerializeField] private string playerTag = "Player";     // ← Change this if your ship uses a different tag
        [SerializeField] private bool destroyOnAnyHit = true;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.CompareTag(playerTag))
            {
                // Direct damage to Health component (Best approach now)
                if (collision.transform.TryGetComponent<Health>(out var health))
                {
                    health.TakeDamage(damageAmount);
                    Debug.Log($"Bullet hit {collision.transform.name} for {damageAmount} damage!");
                }
                else
                {
                    Debug.LogWarning($"Player hit but no Health component found on {collision.transform.name}");
                }
            }
            else
            {
                // Optional: Log other collisions for debugging
                // Debug.Log($"Bullet hit non-player: {collision.transform.name}");
            }

            if (destroyOnAnyHit)
            {
                Destroy(gameObject);
            }
        }
    }
}