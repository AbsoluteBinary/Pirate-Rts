using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.Combat
{
    /// <summary>
    /// Reusable Health component.
    /// Single source of truth for entity health (player, enemies, structures, etc).
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Destruction Effects (Optional)")]
        [SerializeField] private GameObject destructionVFX;
        [SerializeField] private AudioClip destructionSound;
        [SerializeField] private float destructionDelay = 0.8f;
        
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;

        [Header("Events")]
        public UnityEvent<float> OnHealthChanged;
        public UnityEvent OnDeath;

        private float currentHealth;
        private bool isDead;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            ResetHealth();
        }

        /// <summary>
        /// Applies damage and checks for death.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (isDead || amount <= 0) return;

            currentHealth -= amount;
            currentHealth = Mathf.Max(0f, currentHealth);

            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Heals the entity (clamped to max health).
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead || amount <= 0) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke(currentHealth);
        }

        /// <summary>
        /// Instantly kills the entity.
        /// </summary>
        public void Kill()
        {
            if (!isDead)
            {
                currentHealth = 0f;
                Die();
            }
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            OnDeath?.Invoke();

            // Play destruction effects
            if (destructionVFX != null)
            {
                Instantiate(destructionVFX, transform.position, Quaternion.identity);
            }

            if (destructionSound != null)
            {
                AudioSource.PlayClipAtPoint(destructionSound, transform.position);
            }

            // Disable ship after short delay (so effects can play)
            StartCoroutine(DisableAfterDelay(destructionDelay));
        }

        private System.Collections.IEnumerator DisableAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Resets health to maximum and revives the entity.
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
            OnHealthChanged?.Invoke(currentHealth);
        }
    }
}