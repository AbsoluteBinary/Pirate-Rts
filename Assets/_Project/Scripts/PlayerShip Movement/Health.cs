using _Project.Scripts.UI.IMGUI;
using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;

        public event System.Action<float, float> OnHealthChanged;

        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
    
            FindFirstObjectByType<NavalCombatHUD>()?.OnDamageTaken(amount);
    
            if (currentHealth <= 0) Die();
        }

        private void Die()
        {
            Debug.Log($"{name} DESTROYED");
            Destroy(gameObject);
        }
    }
}
