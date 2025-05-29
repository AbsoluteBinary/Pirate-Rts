using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Scripts.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private Image loadingBar;
        [SerializeField] private float fillSmoothingSpeed = 5f; // Controls smoothing speed (higher = faster, lower = smoother)
        [SerializeField] private Canvas loadingCanvas;
        [SerializeField] private Camera loadingCamera;
        [SerializeField] private SceneGroup[] sceneGroups;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private float minLoadingTime = 3f; // Seconds to keep loading UI visible

        private float targetProgress;
        private bool isLoading;
        private int currentGroupIndex = 0; // Tracks the current scene group index

        public readonly SceneGroupManager manager = new SceneGroupManager();
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Bootstrapper") return;

            var rootObjects = scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                // Disable AudioListeners
                var audioListeners = root.GetComponentsInChildren<AudioListener>(true);
                foreach (var listener in audioListeners)
                {
                    listener.enabled = false;
                    Debug.Log($"Disabled AudioListener on {listener.gameObject.name} in scene {scene.name} during load");
                }

                // Disable EventSystems
                var eventSystems = root.GetComponentsInChildren<EventSystem>(true);
                foreach (var eventSystem in eventSystems)
                {
                    eventSystem.enabled = false;
                    Debug.Log($"Disabled EventSystem on {eventSystem.gameObject.name} in scene {scene.name} during load");
                }
            }
        }

        private void Awake()
        {
            manager.OnSceneLoaded += sceneName => Debug.Log("Loaded: " + sceneName);
            manager.OnSceneUnloaded += sceneName => Debug.Log("Unloaded: " + sceneName);
            manager.OnSceneGroupLoaded += () =>
            {
                Debug.Log("Scene group loaded.");
                ManageComponents();
            };
        }

        private async void Start()
        {
            if (manager.ActiveSceneGroup == null) // Avoid reloading if already set
            {
                await LoadSceneGroup(0);
            }
        }

        void Update()
        {
            if (!isLoading || loadingBar == null) return;

            float currentFillAmount = loadingBar.fillAmount;
            loadingBar.fillAmount = Mathf.Lerp(currentFillAmount, targetProgress, Time.deltaTime * fillSmoothingSpeed);
            if (progressText != null)
                progressText.text = $"{Mathf.RoundToInt(targetProgress * 100f)}%";
            if (targetProgress >= 1f && Mathf.Approximately(loadingBar.fillAmount, 1f))
                isLoading = false;
        }

        public async Task LoadSceneGroup(int index)
        {
            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError("Invalid scene group index: " + index);
                return;
            }

            if (loadingBar != null)
            {
                loadingBar.fillAmount = 0f;
            }
            targetProgress = 0f;
            isLoading = true;

            LoadingProgress progress = new LoadingProgress();
            progress.Progressed += target =>
            {
                targetProgress = Mathf.Clamp01(target);
            };

            EnableLoadingCanvas();
            try
            {
                await manager.LoadScenes(sceneGroups[index], progress);
                await Task.Delay(TimeSpan.FromSeconds(minLoadingTime));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load scene group {index}: {ex.Message}");
            }
            finally
            {
                EnableLoadingCanvas(false);
                if (loadingBar != null)
                {
                    loadingBar.fillAmount = 1f; // Only set if loadingBar is still valid
                }
            }
        }

        private void DisableAllAudioListenersExceptLoadingCamera()
        {
            AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
            foreach (var listener in allListeners)
            {
                // Assuming 'loadingCamera' is a reference to your loading camera GameObject
                if (listener.gameObject != loadingCamera.gameObject)
                {
                    listener.enabled = false;
                }
            }
        }
        
        private void DisableAllEventSystems()
        {
            EventSystem[] allEventSystems = FindObjectsOfType<EventSystem>();
            foreach (var eventSystem in allEventSystems)
            {
                eventSystem.enabled = false;
            }
        }

        private void EnableLoadingCanvas(bool enable = true)
        {
            isLoading = enable;
            if (loadingCanvas != null)
            {
                loadingCanvas.gameObject.SetActive(enable);
            }
            if (loadingCamera != null)
            {
                loadingCamera.gameObject.SetActive(enable);
            }
            // Ensure MainMenuCanvas is visible after loading
            if (!enable)
            {
                var mainMenuCanvas = GameObject.Find("MainMenuCanvas")?.GetComponent<Canvas>();
                if (mainMenuCanvas != null)
                    mainMenuCanvas.enabled = true;
            }
        }

        // Toggles to the next scene group in the array
        public void ToggleNextSceneGroup()
        {
            if (sceneGroups == null || sceneGroups.Length == 0)
            {
                Debug.LogWarning("No scene groups assigned to SceneLoader.");
                return;
            }

            currentGroupIndex = (currentGroupIndex + 1) % sceneGroups.Length; // Cycle to next index
            LoadSceneGroup(currentGroupIndex);
        }

        // Manages components like EventSystem, Camera, AudioListener
        private void ManageComponents()
        {
            // Define component priorities based on SceneType
            var componentPriorities = new[]
            {
                (typeof(EventSystem), SceneType.UserInterface),
                (typeof(Camera), SceneType.ActiveScene),
                (typeof(AudioListener), SceneType.ActiveScene)
            };

            foreach (var (componentType, preferredSceneType) in componentPriorities)
            {
                ManageComponent(componentType, preferredSceneType);
            }
        }

        // Generic method to manage a specific component type
        private void ManageComponent(Type componentType, SceneType preferredSceneType)
        {
            // Find the preferred scene for this component
            string preferredSceneName = manager.ActiveSceneGroup?.FindSceneNameByType(preferredSceneType);
            if (string.IsNullOrEmpty(preferredSceneName))
            {
                Debug.LogWarning($"No scene found for {preferredSceneType} to manage {componentType.Name}.");
                return;
            }

            // Find all components of this type
            var components = UnityEngine.Object.FindObjectsOfType(componentType) as Component[];
            if (components == null || components.Length == 0)
            {
                Debug.LogWarning($"No {componentType.Name} found in loaded scenes.");
                return;
            }

            // Enable the component from the preferred scene, disable others
            Component primaryComponent = null;
            foreach (var component in components)
            {
                bool isPrimary = component.gameObject.scene.name == preferredSceneName;
                if (component is Behaviour behaviour) // Check if component is a Behaviour
                {
                    behaviour.enabled = isPrimary;
                    if (isPrimary)
                    {
                        primaryComponent = component;
                    }
                    else
                    {
                        Debug.Log($"Disabled {componentType.Name} on {component.gameObject.name} in scene {component.gameObject.scene.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Component {componentType.Name} on {component.gameObject.name} is not a Behaviour and cannot be enabled/disabled.");
                }
            }

            if (primaryComponent != null)
            {
                Debug.Log($"Primary {componentType.Name}: {primaryComponent.gameObject.name} in scene {primaryComponent.gameObject.scene.name}");
            }
            else
            {
                Debug.LogWarning($"No {componentType.Name} found in preferred scene {preferredSceneName}.");
            }
        }
    }

    public class LoadingProgress : IProgress<float>
    {
        public event Action<float> Progressed;
        private const float ratio = 1f;

        public void Report(float value)
        {
            float normalizedValue = Mathf.Clamp01(value / ratio);
            Progressed?.Invoke(normalizedValue);
        }
    }
}