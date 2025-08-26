using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using _Project.Scripts.EventBus; // For EventBus
using System.Linq;
using _Project.Scripts.MainMenu_Controls; // For FirstOrDefault

namespace _Project.Scripts.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        // Singleton Instance
        public static SceneLoader Instance { get; private set; }
        
        //private BootUiControl _bootUiControl;

        // Event binding for LoadSceneGroupEvent
        private EventBinding<LoadSceneGroupEvent> loadSceneGroupBinding;

        
        // Configuration for loading bar animation speed
        [SerializeField] private float fillSmoothingSpeed = 5f;
        // Array of scene groups to load
        [SerializeField] private SceneGroup[] sceneGroups;
        // Minimum time to display loading screen
        [SerializeField] private float minLoadingTime = 3f;
        
        // UI Component References
        [SerializeField] private Image backgroundImage;          // Background image shown during loading
        [SerializeField] private Image loadingBarBorder;        // Static border of the loading bar
        [SerializeField] private Image loadingBarFill;          // Fill image that progresses
        [SerializeField] private TextMeshProUGUI loadingText;   // Text displaying "Loading..." or progress
        [SerializeField] private CanvasGroup loadingUICanvasGroup; // Group containing loading UI elements
        //[SerializeField] private Canvas loginUICanvas;          // Login UI canvas (will be found if in another scene)
        // Reference to a single background object assigned in the Inspector
        //[SerializeField] private GameObject backgroundObject; // Reference to background object for activation
        
        // Tracks the target progress of the loading bar
        private float targetProgress;
        // Flag to indicate if a scene group is currently loading
        public bool isLoading;
        // Index of the current scene group
        private int currentGroupIndex = 0;
        // Cached reference to login UI CanvasGroup
        private CanvasGroup loginUICanvasGroup;

        // Solution 3: Flag to indicate if preparing for a new group (prevents enabling)
        private bool isPreparingNewGroup;

        // Manages scene loading and unloading
        public readonly SceneGroupManager manager = new SceneGroupManager();
        
        private void OnApplicationQuit()
        {
            Debug.Log("Application quitting or exiting play mode detected.");
            ResetUi(); // Call your function here
        }

        private void Awake()
        {
            // if (loadingCamera != null)
            // {
            //     loadingCamera.clearFlags = CameraClearFlags.Skybox; // Use skybox instead of solid color
            //     loadingCamera.backgroundColor = Color.black; // Fallback color if no skybox
            // }
            // Singleton setup: Ensure only one instance
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            
            // Initialize DOTween for animations
            DOTween.Init();
            // Subscribe to scene manager events for logging
            manager.OnSceneLoaded += sceneName => Debug.Log("Loaded: " + sceneName);
            manager.OnSceneUnloaded += sceneName => Debug.Log("Unloaded: " + sceneName);
            manager.OnSceneGroupLoaded += () => Debug.Log("Scene group loaded.");

            // Register event listener for LoadSceneGroupEvent
            loadSceneGroupBinding = new EventBinding<LoadSceneGroupEvent>(OnLoadSceneGroupEvent);
            EventBus<LoadSceneGroupEvent>.Register(loadSceneGroupBinding);

            // Set initial UI state
            if (backgroundImage != null)
            {
                backgroundImage.gameObject.SetActive(true);
                backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f);
                backgroundImage.DOFade(1.0f, 1.0f);
            }
            if (loadingUICanvasGroup != null)
            {
                loadingUICanvasGroup.gameObject.SetActive(true);
                loadingUICanvasGroup.alpha = 1f;
            }
            if (loadingBarFill != null) loadingBarFill.fillAmount = 0f;
            if (loadingText != null) loadingText.text = "Loading...";

            // New: Ensure Boot container starts enabled (for initial load)
            //if (_bootUiControl != null) _bootUiControl.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            // Deregister event to prevent memory leaks
            EventBus<LoadSceneGroupEvent>.Deregister(loadSceneGroupBinding);
        }

        private void OnLoadSceneGroupEvent(LoadSceneGroupEvent e)
        {
            _ = LoadSpecificSceneGroup(e.groupIndex);
        }

        private async void Start()
        {
            if (manager.ActiveSceneGroup == null)
            {
                await LoadSceneGroup(0);
            }
        }

        public async Task LoadSceneGroup(int index)
        {
            Debug.Log("Loading scene group...");
            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError("Invalid scene group index: " + index);
                return;
            }

            currentGroupIndex = index;
            isLoading = true;

            LoadingProgress progress = new LoadingProgress();
            progress.Progressed += target => targetProgress = Mathf.Clamp01(target);

            ShowLoadingUI();

            try
            {
                Task loadTask = manager.LoadScenes(sceneGroups[index], progress);
                await loadTask;
                await Task.Yield(); // Wait for the next frame
                await Task.Delay(TimeSpan.FromSeconds(minLoadingTime));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load scene group {index}: {ex.Message}");
            }
            finally
            {
                HideLoadingUI();
                isLoading = false; // Reset flag here to allow future loads
            }
        }

        private void ShowLoadingUI()
        {
            if (loadingUICanvasGroup != null)
            {
                loadingUICanvasGroup.DOFade(1f, 0.4f);
            }
        }

        private void HideLoadingUI()
        {
            Sequence seq = DOTween.Sequence();
            if (backgroundImage != null) seq.Append(backgroundImage.DOFade(0f, 0.5f));
            if (loadingUICanvasGroup != null) seq.Join(loadingUICanvasGroup.DOFade(0f, 0.5f));
    
            // Insert callback at end of fade (almost faded out) to show login UI if Boot group
            seq.AppendCallback(() =>
            {
                if (currentGroupIndex == 0)
                {
                    ShowLoginUI();
                }
                else
                {
                    Debug.Log("Skipping ShowLoginUI() for non-Boot group: " + currentGroupIndex);
                }
            });

            seq.OnComplete(() =>
            {
                if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
                if (loadingUICanvasGroup != null) loadingUICanvasGroup.gameObject.SetActive(false);
                isPreparingNewGroup = false;
            });
        }

    
        private void ShowLoginUI()
        {
            var mainMenuManager = MainMenuDataIOManager.Instance;
            if (mainMenuManager != null)
            {
                // Access methods robustly
                mainMenuManager.ToggleCanvasOn();
                mainMenuManager.ToggleBackgroundOn();// Or ToggleBackgroundOn(), etc.
                // Example: mainMenuManager.SaveUIState(); if needed
            }
            else
            {
                Debug.LogWarning("MainMenuDataIOManager instance not found.");
            }
        }
        
        private void HideLoginUIBootOut()
        {
            var mainMenuManager = MainMenuDataIOManager.Instance;
            if (mainMenuManager != null)
            {
                // Access methods robustly
                mainMenuManager.ToggleBackgroundOff();
                mainMenuManager.ToggleCanvasOff();
                mainMenuManager.DisableComponentIOBox();
                //mainMenuManager.
                // Or ToggleBackgroundOn(), etc.
                // Example: mainMenuManager.SaveUIState(); if needed
            }
            else
            {
                Debug.LogWarning("MainMenuDataIOManager instance not found.");
            }
        }

        private void ResetUi()
        {
            var mainMenuManager = MainMenuDataIOManager.Instance;
            if (mainMenuManager != null)
            {
                // When Boot is finished and game is loaded in
                // Set 3D background Off and Login Ui off 
                mainMenuManager.SaveUIState();
            }
        }

        private void Update()
        {
            if (!isLoading || loadingBarFill == null) return;

            float currentFillAmount = loadingBarFill.fillAmount;
            loadingBarFill.fillAmount = Mathf.Lerp(currentFillAmount, targetProgress, Time.deltaTime * fillSmoothingSpeed);
            if (loadingText != null)
                loadingText.text = $"Loading... {Mathf.RoundToInt(targetProgress * 100f)}%";
        }

        public void LoadNextSceneGroupForButton()
        {
            Debug.Log("Load next Scene Group");
            //if (backgroundObject != null) backgroundObject.SetActive(false);
            //if (loginUICanvas != null) loginUICanvas.gameObject.SetActive(false);
            
            if (isLoading)
            {
                Debug.LogWarning("Cannot load next scene group while loading is in progress.");
                return;
            }

            ToggleNextSceneGroup();
        }

        // New method: Load a specific scene group by index (for external calls)
        public async Task LoadSpecificSceneGroup(int index)
        {
            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError("Invalid scene group index: " + index);
                return;
            }

            // Cache old group for unloading after new load
            SceneGroup oldGroup = manager.ActiveSceneGroup;

            // Toggle Boot container if needed
            //AssignAndDisableOldComponents();
            // if (_bootUiControl != null)
            // {
            //     _bootUiControl.gameObject.SetActive(index == 0); // Enable only for Boot (index 0)
            // }

            currentGroupIndex = index;
            NewGroupPrep(); // Prepare flag

            // Load new group first
            await LoadSceneGroup(index);

            // Then unload old if exists and different
            if (oldGroup != null && oldGroup != sceneGroups[index])
            {
                Debug.Log("Testing unload: Unloading previous scene group...");
                await manager.UnloadScenes();
            }
        }

        public async Task ToggleNextSceneGroup() // Already async
        {
            
            HideLoginUIBootOut();
            //BootUiControl.Instance.gameObject.SetActive(false);
            if (sceneGroups == null || sceneGroups.Length == 0)
            {
                Debug.LogWarning("No scene groups assigned to SceneLoader.");
                return;
            }
            currentGroupIndex = (currentGroupIndex + 1) % sceneGroups.Length;
    
            // Cache old group
            SceneGroup oldGroup = manager.ActiveSceneGroup;

            // Load new group
            await LoadSceneGroup(currentGroupIndex);
            NewGroupPrep();

            // Unload old
            if (oldGroup != null && oldGroup != sceneGroups[currentGroupIndex])
            {
                Debug.Log("Testing unload: Unloading previous scene group...");
                await manager.UnloadScenes();
            }
            
            
            // New: Cache and disable old
            //AssignAndDisableOldComponents();
            

            // Load new...
            await LoadSceneGroup(currentGroupIndex);

            // Unload old...
            if (oldGroup != null && oldGroup != sceneGroups[currentGroupIndex])
            {
                await manager.UnloadScenes();
            }
        }
        
        private void NewGroupPrep()
        {
            isPreparingNewGroup = true;
        }
        
        // New helper method
        // private void AssignAndDisableOldComponents()
        // {
        //     // Assuming old scene has known names/tags; adjust as needed
        //     componentIOBox = GameObject.FindWithTag("ComponentIOBox"); // Or Find("CameraContainer")?.GetComponent<Camera>()
        //     //oldEventSystem = GameObject.FindWithTag("EventSystem"); // Or similar
        //
        //     if (componentIOBox != null)
        //     {
        //         componentIOBox.SetActive(false);
        //         Debug.Log("Disabled old Camera.");
        //     }
        // }
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