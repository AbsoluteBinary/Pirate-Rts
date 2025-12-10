using UI.IMGUI;
using UnityEngine;

namespace PlayerShip_Movement
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;

        private void Awake() => currentHealth = maxHealth;

        public void TakeDamage(float amount)
        {
            FindObjectOfType<NavalCombatHUD>()?.OnDamageTaken(amount);  // ← UI feedback
            currentHealth -= amount;
            // ... rest unchanged
        }

        private void Die()
        {
            Debug.Log($"{name} DESTROYED");
            Destroy(gameObject);
        }
    }
}
