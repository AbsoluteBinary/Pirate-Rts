using UnityEngine;
using _Project.Scripts.UI.IMGUI; // if you still use it

namespace _Project.Scripts.PlayerShip_Movement
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;

        public event System.Action<float, float> OnHealthChanged; // For UI later

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            
            Debug.Log($"[Health] {gameObject.name} took {amount} damage. Remaining: {currentHealth}/{maxHealth}");

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Optional: Visual feedback
            FindFirstObjectByType<NavalCombatHUD>()?.OnDamageTaken(amount);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"💥 {gameObject.name} DESTROYED!");
            // TODO: Add sinking animation later instead of instant destroy
            Destroy(gameObject);
        }

        // For testing
        public float GetCurrentHealth() => currentHealth;
    }
}