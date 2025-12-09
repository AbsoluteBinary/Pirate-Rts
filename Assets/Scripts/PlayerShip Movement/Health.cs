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
            currentHealth -= amount;
            Debug.Log($"[Health] {name} took {amount} damage → {currentHealth}/{maxHealth}");

            if (currentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            Debug.Log($"{name} DESTROYED");
            Destroy(gameObject);
        }
    }
}
