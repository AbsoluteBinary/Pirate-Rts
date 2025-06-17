using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace _Project.Scripts.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
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
        [SerializeField] private Canvas loginUICanvas;          // Login UI canvas (will be found if in another scene)
        //[SerializeField] private Camera loadingCamera;          // Camera for rendering loading screen
        // Reference to a single background object assigned in the Inspector
        [SerializeField] private GameObject backgroundObject; // Reference to background object for activation

        // Tracks the target progress of the loading bar
        private float targetProgress;
        // Flag to indicate if a scene group is currently loading
        private bool isLoading;
        // Index of the current scene group
        private int currentGroupIndex = 0;
        // Cached reference to login UI CanvasGroup
        private CanvasGroup loginUICanvasGroup;

        // Manages scene loading and unloading
        public readonly SceneGroupManager manager = new SceneGroupManager();

        private void Awake()
        {
            // Initialize DOTween for animations
            DOTween.Init();
            // Subscribe to scene manager events for logging
            manager.OnSceneLoaded += sceneName => Debug.Log("Loaded: " + sceneName);
            manager.OnSceneUnloaded += sceneName => Debug.Log("Unloaded: " + sceneName);
            manager.OnSceneGroupLoaded += () => Debug.Log("Scene group loaded.");

            // Set initial UI state
            if (backgroundImage != null)
            {
                backgroundImage.gameObject.SetActive(true);
                // Ensure background starts opaque
                backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f);
                // Fade in background image
                backgroundImage.DOFade(1.0f, 1.0f);
            }
            if (loadingUICanvasGroup != null)
            {
                // Activate and set loading UI to fully visible
                loadingUICanvasGroup.gameObject.SetActive(true);
                loadingUICanvasGroup.alpha = 1f;
            }
            if (loadingBarFill != null) loadingBarFill.fillAmount = 0f; // Reset loading bar
            if (loadingText != null) loadingText.text = "Loading..."; // Set initial loading text
        }

        private async void Start()
        {
            // Load the first scene group if none is active
            if (manager.ActiveSceneGroup == null)
            {
                await LoadSceneGroup(0);
            }
        }

        // Loads a scene group by index asynchronously
        public async Task LoadSceneGroup(int index)
        {
            // Validate the scene group index
            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError("Invalid scene group index: " + index);
                return;
            }

            currentGroupIndex = index;
            isLoading = true;

            // Create progress tracker for loading
            LoadingProgress progress = new LoadingProgress();
            progress.Progressed += target => targetProgress = Mathf.Clamp01(target);

            // Display loading UI
            ShowLoadingUI();

            try
            {
                // Load scenes asynchronously
                Task loadTask = manager.LoadScenes(sceneGroups[index], progress);
                await loadTask;
                await Task.Yield(); // Ensure UI updates
                // Enforce minimum loading time
                await Task.Delay(TimeSpan.FromSeconds(minLoadingTime));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load scene group {index}: {ex.Message}");
            }
            finally
            {
                // Clean up by hiding loading UI and showing login UI
                HideLoadingUI();
                ShowLoginUI();
            }
        }

        // Activates the loading UI elements
        private void ShowLoadingUI()
        {
            //if (loadingCamera != null) loadingCamera.gameObject.SetActive(true);
            if (loadingUICanvasGroup != null)
            {
                // Fade in loading UI
                loadingUICanvasGroup.DOFade(1f, 0.4f);
            }
        }

        // Hides the loading UI with a fade-out animation
        private void HideLoadingUI()
        {
            // Create a sequence for coordinated fading
            Sequence seq = DOTween.Sequence();
            if (backgroundImage != null) seq.Append(backgroundImage.DOFade(0f, 0.5f));
            if (loadingUICanvasGroup != null) seq.Join(loadingUICanvasGroup.DOFade(0f, 0.5f));
            if (backgroundObject != null) backgroundObject.SetActive(true);
            
            seq.OnComplete(() =>
            {
                // Deactivate UI elements after fading
                if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
                if (loadingUICanvasGroup != null) loadingUICanvasGroup.gameObject.SetActive(false);
                //if (loadingCamera != null) loadingCamera.gameObject.SetActive(false);
            });
        }

        // Displays the login UI, either from serialized reference or by finding it
        private void ShowLoginUI()
        {
            // Check if login canvas is serialized (e.g., in Boot scene)
            if (loginUICanvas != null)
            {
                loginUICanvasGroup = loginUICanvas.GetComponent<CanvasGroup>();
                if (loginUICanvasGroup != null)
                {
                    // Fade in login UI
                    loginUICanvasGroup.alpha = 0f;
                    loginUICanvas.gameObject.SetActive(true);
                    loginUICanvasGroup.DOFade(1f, 0.5f);
                }
                else
                {
                    // Activate without fading if no CanvasGroup
                    loginUICanvas.gameObject.SetActive(true);
                }
            }
            else
            {
                // Find login canvas in the loaded scene (e.g., MainMenu)
                var loginCanvasObj = GameObject.Find("LoginMenuCanvas");
                if (loginCanvasObj != null)
                {
                    loginUICanvasGroup = loginCanvasObj.GetComponent<CanvasGroup>();
                    if (loginUICanvasGroup != null)
                    {
                        // Fade in found login UI
                        loginUICanvasGroup.alpha = 0f;
                        loginCanvasObj.SetActive(true);
                        loginUICanvasGroup.DOFade(1f, 0.5f);
                    }
                    else
                    {
                        // Activate without fading
                        loginCanvasObj.SetActive(true);
                    }
                }
                else
                {
                    Debug.LogWarning("LoginMenuCanvas not found.");
                }
            }
        }

        // Updates the loading bar and text during loading
        private void Update()
        {
            if (!isLoading || loadingBarFill == null) return;

            // Smoothly interpolate the loading bar fill
            float currentFillAmount = loadingBarFill.fillAmount;
            loadingBarFill.fillAmount = Mathf.Lerp(currentFillAmount, targetProgress, Time.deltaTime * fillSmoothingSpeed);
            if (loadingText != null)
                loadingText.text = $"Loading... {Mathf.RoundToInt(targetProgress * 100f)}%";

            // Trigger background object fade-in at 90% progress
            if (targetProgress >= 0.9f && !DOTween.IsTweening("FadeInBackgroundObjects"))
            {
                //FadeInBackgroundObjects();
            }
        }

        // Loads the next scene group, suitable for button OnClick events
        public void LoadNextSceneGroupForButton()
        {
            if (isLoading)
            {
                Debug.LogWarning("Cannot load next scene group while loading is in progress.");
                return;
            }
            ToggleNextSceneGroup();
        }

        // Switches to the next scene group in the array
        public void ToggleNextSceneGroup()
        {
            if (sceneGroups == null || sceneGroups.Length == 0)
            {
                Debug.LogWarning("No scene groups assigned to SceneLoader.");
                return;
            }
            // Cycle to the next scene group
            currentGroupIndex = (currentGroupIndex + 1) % sceneGroups.Length;
            LoadSceneGroup(currentGroupIndex);

            NewGroupPrep();
        }
        
        private void NewGroupPrep()
        {
            //Toggle assets off not needed in next SceneGroup
            if (backgroundObject != null) backgroundObject.SetActive(false);
            if (loginUICanvas != null) loginUICanvas.gameObject.SetActive(false);
        }
    }

    // Tracks and reports loading progress
    public class LoadingProgress : IProgress<float>
    {
        // Event triggered when progress updates
        public event Action<float> Progressed;
        private const float ratio = 1f;

        // Reports normalized progress value
        public void Report(float value)
        {
            float normalizedValue = Mathf.Clamp01(value / ratio);
            Progressed?.Invoke(normalizedValue);
        }
    }

    // Handles fading for 3D background objects
    public class BackgroundObject : MonoBehaviour
    {
        private Renderer rend;

        private void Awake()
        {
            // Initialize renderer with transparent color
            rend = GetComponent<Renderer>();
            if (rend != null)
            {
                Color color = rend.material.color;
                rend.material.color = new Color(color.r, color.g, color.b, 0f);
            }
        }

        // Fades in the object
        public void FadeIn()
        {
            if (rend != null)
            {
                rend.material.DOFade(1f, 1f).SetId("FadeInBackgroundObjects");
            }
        }
    }
}