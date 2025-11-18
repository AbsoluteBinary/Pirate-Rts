using UnityEngine;

namespace Managers.World_Map.ObjectInteractions
{
    public class ButtonRegistrar : MonoBehaviour
    {
        [SerializeField] private ButtonManager.ButtonConfig config; // Configure in Inspector: uiDocument, buttonName, actionName
        private bool isRegistered = false;

        private void OnEnable()
        {
            if (config == null) return;

            // Set unique ID and owner for local variable access
            config.buttonId = $"{gameObject.name}_{config.buttonName}";
            config.owner = gameObject;

            // Queue for batching (or direct if !useLazyRegistration in ButtonManager)
            if (ButtonManager.Instance != null)
            {
                ButtonManager.Instance.QueueButtonRegistration(config);
            }
            else
            {
                Debug.LogWarning("ButtonManager not found! Button not registered.", this);
            }
        }

        private void OnDisable()
        {
            if (isRegistered && ButtonManager.Instance != null)
            {
                ButtonManager.Instance.UnregisterButton(config.buttonId);
                isRegistered = false;
            }
        }

        // Optional: Lazy registration trigger (e.g., call from OnMouseEnter if useLazyRegistration=true)
        public void TriggerLazyRegistration()
        {
            if (!isRegistered && ButtonManager.Instance != null)
            {
                ButtonManager.Instance.RegisterButton(config);
                isRegistered = true;
            }
        }
    }
}