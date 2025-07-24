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
        // Reference to a single background object assigned in the Inspector
        [SerializeField] private GameObject backgroundObject; // Reference to background object for activation

        // New: Container for Boot's UI/Camera objects (assign in Inspector)
        [SerializeField] private GameObject bootUIContainer; // Parent holding Boot's UI, Camera, etc.

        // Tracks the target progress of the loading bar
        private float targetProgress;
        // Flag to indicate if a scene group is currently loading
        private bool isLoading;
        // Index of the current scene group
        private int currentGroupIndex = 0;
        // Cached reference to login UI CanvasGroup
        private CanvasGroup loginUICanvasGroup;

        // Solution 3: Flag to indicate if preparing for a new group (prevents enabling)
        private bool isPreparingNewGroup;

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
            if (bootUIContainer != null) bootUIContainer.SetActive(true);
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
                await Task.Yield();
                await Task.Delay(TimeSpan.FromSeconds(minLoadingTime));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load scene group {index}: {ex.Message}");
            }
            finally
            {
                HideLoadingUI();
                ShowLoginUI();
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
            if (backgroundObject != null && !isPreparingNewGroup)
                backgroundObject.SetActive(true);
            
            seq.OnComplete(() =>
            {
                if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
                if (loadingUICanvasGroup != null) loadingUICanvasGroup.gameObject.SetActive(false);
                isPreparingNewGroup = false;
            });
        }

        private void ShowLoginUI()
        {
            if (isPreparingNewGroup) return;

            if (loginUICanvas != null)
            {
                loginUICanvasGroup = loginUICanvas.GetComponent<CanvasGroup>();
                if (loginUICanvasGroup != null)
                {
                    loginUICanvasGroup.alpha = 0f;
                    loginUICanvas.gameObject.SetActive(true);
                    loginUICanvasGroup.DOFade(1f, 0.5f);
                }
                else
                {
                    loginUICanvas.gameObject.SetActive(true);
                }
            }
            else
            {
                var loginCanvasObj = GameObject.Find("LoginMenuCanvas");
                if (loginCanvasObj != null)
                {
                    loginUICanvasGroup = loginCanvasObj.GetComponent<CanvasGroup>();
                    if (loginUICanvasGroup != null)
                    {
                        loginUICanvasGroup.alpha = 0f;
                        loginCanvasObj.SetActive(true);
                        loginUICanvasGroup.DOFade(1f, 0.5f);
                    }
                    else
                    {
                        loginCanvasObj.SetActive(true);
                    }
                }
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
            if (backgroundObject != null) backgroundObject.SetActive(false);
            if (loginUICanvas != null) loginUICanvas.gameObject.SetActive(false);
            
            if (isLoading)
            {
                Debug.LogWarning("Cannot load next scene group while loading is in progress.");
                return;
            }

            ToggleNextSceneGroup();
        }

        public void ToggleNextSceneGroup()
        {
            if (sceneGroups == null || sceneGroups.Length == 0)
            {
                Debug.LogWarning("No scene groups assigned to SceneLoader.");
                return;
            }
            currentGroupIndex = (currentGroupIndex + 1) % sceneGroups.Length;
            
            // New: Toggle Boot container before loading (disable if not Boot group)
            if (bootUIContainer != null)
            {
                bootUIContainer.SetActive(currentGroupIndex == 0); // Enable only for Boot (index 0), disable otherwise
            }

            LoadSceneGroup(currentGroupIndex);
            NewGroupPrep();
        }
        
        private void NewGroupPrep()
        {
            isPreparingNewGroup = true;
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

    public class BackgroundObject : MonoBehaviour
    {
        private Renderer rend;

        private void Awake()
        {
            rend = GetComponent<Renderer>();
            if (rend != null)
            {
                Color color = rend.material.color;
                rend.material.color = new Color(color.r, color.g, color.b, 0f);
            }
        }

        public void FadeIn()
        {
            if (rend != null)
            {
                rend.material.DOFade(1f, 1f).SetId("FadeInBackgroundObjects");
            }
        }
    }
}