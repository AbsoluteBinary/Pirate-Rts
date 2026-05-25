using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.PlayerShip_Movement;

namespace _Project.Scripts.UI.Combat
{
    public class ShipHUDController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private Health playerHealth;

        private Label healthLabel;
        private ProgressBar healthBar;   // or VisualElement + style.width
        private Label resourcesLabel;

        private void OnEnable()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            
            VisualElement root = uiDocument.rootVisualElement;
            
            healthLabel = root.Q<Label>("HealthValue");
            healthBar = root.Q<ProgressBar>("HealthBar");
            resourcesLabel = root.Q<Label>("ResourcesValue");

            if (playerHealth == null)
                playerHealth = FindFirstObjectByType<Health>();

            if (playerHealth != null)
                playerHealth.OnHealthChanged += UpdateHealthUI; // Add this event to Health.cs
        }

        private void UpdateHealthUI(float current, float max)
        {
            if (healthLabel != null)
                healthLabel.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

            if (healthBar != null)
                healthBar.value = (current / max) * 100f;
        }

        // Example resource update (call from your resource system)
        public void UpdateResources(int wood, int iron, int gold)
        {
            if (resourcesLabel != null)
                resourcesLabel.text = $"Wood: {wood}  Iron: {iron}  Gold: {gold}";
        }
    }
}