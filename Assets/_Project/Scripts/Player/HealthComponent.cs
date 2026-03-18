using UnityEngine;

namespace _Project.Scripts.Player
{
    public class HealthComponent : MonoBehaviour
    {
        [SerializeField, Min(0)] private int maxHealth = 100;
        [SerializeField, Min(0)] private int currentHealth = 100;

        private void Awake()
        {
            // Ensure current health doesn't exceed max health on start
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        public int GetHealth()
        {
            return currentHealth;
        }

        public void SetHealth(int health)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
        }

        public int GetMaxHealth()
        {
            return maxHealth;
        }

        // Optional: Method to modify health (e.g., for damage or healing)
        public void ModifyHealth(int amount)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        }
    }
}