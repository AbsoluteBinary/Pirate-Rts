using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class BulletDamage : MonoBehaviour
    {
        [HideInInspector] public float damageAmount = 25f;
        [SerializeField] private string playerTag = "PlayerShip";

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.CompareTag(playerTag))
            {
                var combatManager = FindFirstObjectByType<NavalCombatManager>();
                combatManager?.OnPlayerHit(damageAmount);
            }
            Destroy(gameObject);
        }
    }
}