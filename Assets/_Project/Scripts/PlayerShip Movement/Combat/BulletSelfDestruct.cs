using _Project.Scripts.PlayerShip_Movement.Combat.ObjectPooling;
using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class BulletSelfDestruct : MonoBehaviour
    {
        private Vector3 velocity;
        private string poolTag = "Bullet";

        public void Initialize(Vector3 vel, float lifetime, string poolTag = "Bullet")
        {
            this.velocity = vel;
            this.poolTag = poolTag;

            Invoke(nameof(Deactivate), Mathf.Max(lifetime, 3f));
        }

        private void Update()
        {
            transform.position += velocity * Time.deltaTime;
        }

        private void Deactivate()
        {
            ObjectPooler.Instance?.ReturnToPool(poolTag, gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            ObjectPooler.Instance?.ReturnToPool(poolTag, gameObject);

            // Handle damage
            if (collision.transform.TryGetComponent<Health>(out var health))
            {
                var combatManager = FindFirstObjectByType<NavalCombatManager>();
                combatManager?.OnPlayerHit(GetComponent<BulletDamage>().damageAmount);
            }
        }
    }
}
