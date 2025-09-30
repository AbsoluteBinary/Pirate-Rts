using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Managers.World_Map.ObjectInteractions
{
    public class ButtonManager : MonoBehaviour
    {
        // Singleton instance
        public static ButtonManager Instance { get; private set; }

        // Event Bus for button clicks and cross-scene flags
        public event Action<string, object> OnButtonEvent; // Action name, optional data (e.g., owner GameObject)

        [SerializeField] private float clickFeedbackDuration = 0.2f; // Duration of click animation
        [SerializeField] private int batchSize = 50; // Buttons to process per frame (tune for performance)
        [SerializeField] private bool useLazyRegistration = false; // Optional: Enable lazy for non-essential buttons

        // Dictionary for registered buttons (key: unique ID, value: config)
        private Dictionary<string, ButtonConfig> buttonConfigs = new Dictionary<string, ButtonConfig>();
        private Queue<ButtonConfig> registrationQueue = new Queue<ButtonConfig>();

        // Config for each button
        [System.Serializable]
        public class ButtonConfig
        {
            public string buttonId; // Unique ID (e.g., "Hub1_EnergyButton")
            public UIDocument uiDocument; // UIDocument containing the button
            public string buttonName; // VisualElement name in UXML (e.g., "EnergyButton")
            public string actionName; // Action identifier (e.g., "HarvestEnergy")
            public GameObject owner; // Hub/Base owning the button (for local variables)
            [HideInInspector] public VisualElement buttonElement; // Cached VisualElement
        }

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Initialize DOTween
            DOTween.Init();

            // Start batching coroutine
            StartCoroutine(ProcessRegistrationQueue());
        }

        // Queue a button for registration (called by ButtonRegistrar)
        public void QueueButtonRegistration(ButtonConfig config)
        {
            if (!buttonConfigs.ContainsKey(config.buttonId))
            {
                registrationQueue.Enqueue(config);
                Debug.Log($"Queued button '{config.buttonId}' for batch registration.");
            }
        }

        // Immediate registration (for non-batched or lazy cases)
        public void RegisterButton(ButtonConfig config)
        {
            if (useLazyRegistration)
            {
                QueueButtonRegistration(config);
            }
            else
            {
                ProcessSingleRegistration(config);
            }
        }

        // Process a single config (used in batching)
        private void ProcessSingleRegistration(ButtonConfig config)
        {
            if (config.uiDocument != null)
            {
                config.buttonElement = config.uiDocument.rootVisualElement.Q<VisualElement>(config.buttonName);
                if (config.buttonElement != null)
                {
                    // Register click event
                    config.buttonElement.RegisterCallback<ClickEvent>(evt => HandleButtonClick(config));
                    buttonConfigs.Add(config.buttonId, config);
                    Debug.Log($"Registered button '{config.buttonId}'.");
                }
                else
                {
                    Debug.LogWarning($"Button '{config.buttonName}' not found in UIDocument for '{config.buttonId}'!", config.owner);
                }
            }
            else
            {
                Debug.LogWarning($"UIDocument not assigned for button '{config.buttonId}'!", config.owner);
            }
        }

        // Coroutine to batch process registrations over frames
        private IEnumerator ProcessRegistrationQueue()
        {
            while (true)
            {
                int processedThisFrame = 0;
                while (registrationQueue.Count > 0 && processedThisFrame < batchSize)
                {
                    var config = registrationQueue.Dequeue();
                    ProcessSingleRegistration(config);
                    processedThisFrame++;
                }

                // Yield to next frame if work was done
                if (processedThisFrame > 0)
                {
                    yield return null; // Spreads load over frames
                }
                else
                {
                    yield return new WaitForSeconds(0.1f); // Idle check every 0.1s
                }
            }
        }

        // Unregister a button (e.g., on Hub disable/destroy)
        public void UnregisterButton(string buttonId)
        {
            if (buttonConfigs.TryGetValue(buttonId, out var config))
            {
                if (config.buttonElement != null)
                {
                    config.buttonElement.UnregisterCallback<ClickEvent>(evt => HandleButtonClick(config));
                }
                buttonConfigs.Remove(buttonId);
                Debug.Log($"Unregistered button '{buttonId}'.");
            }
        }

        // Handle button click
        private void HandleButtonClick(ButtonConfig config)
        {
            // Play click feedback animation using DG.Tweening
            if (config.buttonElement != null)
            {
                config.buttonElement.transform.scale = Vector3.one; // Reset scale
                DOTween.To(
                        () => config.buttonElement.transform.scale, // Getter
                        value => config.buttonElement.transform.scale = value, // Setter
                        Vector3.one * 1.1f, // Target scale (1.1)
                        clickFeedbackDuration / 2f) // Half duration for yoyo
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.InOutQuad);
            }

            // Publish to Event Bus (e.g., for cross-scene flags)
            OnButtonEvent?.Invoke(config.actionName, config.owner);

            // Access local variables from owner (e.g., HubState)
            // Example: var hubState = config.owner.GetComponent<HubState>();
            // if (hubState.energy > 0) { /* Harvest logic */ }
            Debug.Log($"Button '{config.buttonName}' (ID: {config.buttonId}) clicked with action '{config.actionName}' on {config.owner?.name}");
        }

        // Global control example: Enable/disable all buttons
        public void SetAllButtonsEnabled(bool enabled)
        {
            foreach (var config in buttonConfigs.Values)
            {
                if (config.buttonElement != null)
                {
                    config.buttonElement.SetEnabled(enabled);
                }
            }
            OnButtonEvent?.Invoke("ButtonsEnabled", enabled);
            Debug.Log($"All buttons {(enabled ? "enabled" : "disabled")} via Event Bus.");
        }

        private void OnDestroy()
        {
            // Cleanup
            StopAllCoroutines();
            DOTween.Kill(this);
            buttonConfigs.Clear();
            registrationQueue.Clear();
            OnButtonEvent = null;
        }
    }
}