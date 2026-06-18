using _Project.Scripts.Combat.Ship;
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
        
        public UnityEvent OnPlayerDeath;

        [Header("Events")]
        public UnityEvent<float> OnHealthChanged;
        public UnityEvent OnDeath;
        
        [Header("VFX")]
        [SerializeField] private ShipVFX shipVFX;

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

            // Only play damage hit VFX (not death)
            if (shipVFX != null)
            {
                shipVFX.PlayDamageHitVFX();   // ← Changed to damage hit only
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Sets a new maximum health value and optionally resets current health.
        /// </summary>
        /// <summary>
        /// Sets a new maximum health and optionally resets current health
        /// </summary>
        public void SetMaxHealth(float newMaxHealth, bool resetCurrentHealth = true)
        {
            if (newMaxHealth <= 0f)
            {
                Debug.LogWarning("SetMaxHealth: newMaxHealth must be > 0");
                return;
            }

            maxHealth = newMaxHealth;

            if (resetCurrentHealth || currentHealth > maxHealth)
                currentHealth = maxHealth;

            OnHealthChanged?.Invoke(currentHealth);
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

            Debug.Log($"[Health] 🔥 OnDeath event being invoked on {gameObject.name}");

            OnDeath?.Invoke();
            OnPlayerDeath?.Invoke();

            // Let ShipVFX handle the death explosion (via event)
            // We removed the direct call here

            if (destructionVFX != null)
                Instantiate(destructionVFX, transform.position, Quaternion.identity);

            if (destructionSound != null)
                AudioSource.PlayClipAtPoint(destructionSound, transform.position);

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