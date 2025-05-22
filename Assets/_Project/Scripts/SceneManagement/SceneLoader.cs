using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] Image loadingBar;
        [SerializeField] float fillSmoothing = 5f; // Controls smoothing speed (higher = faster, less smooth)
        [SerializeField] Canvas loadingCanvas;
        [SerializeField] Camera loadingCamera;
        [SerializeField] SceneGroup[] sceneGroups;
        [SerializeField] TextMeshProUGUI progressText;
        [SerializeField] float minLoadingTime = 3f; // Seconds to keep loading UI visible

        float targetProgress;
        bool isLoading;

        public readonly SceneGroupManager manager = new SceneGroupManager();
        private SceneComponentManager _componentManager;

        void Awake()
        {
            _componentManager = new SceneComponentManager(manager);
            manager.OnSceneLoaded += sceneName => Debug.Log("Loaded: " + sceneName);
            manager.OnSceneUnloaded += sceneName => Debug.Log("Unloaded: " + sceneName);
            manager.OnSceneGroupLoaded += () => Debug.Log("Scene group loaded");
        }

        async void Start()
        {
            if (manager.ActiveSceneGroup == null) // Avoid reloading if already set
            {
                await LoadSceneGroup(0);
            }
        }

        
        void Update()
        {
            if (!isLoading) return;

            float currentFillAmount = loadingBar.fillAmount;
            loadingBar.fillAmount = Mathf.Lerp(currentFillAmount, targetProgress, Time.deltaTime * fillSmoothing);
            if (progressText != null)
                progressText.text = $"{Mathf.RoundToInt(targetProgress * 100f)}%"; // Update TextMeshProUGUI
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

            loadingBar.fillAmount = 0f;
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
                // Wait for minimum loading time
                await Task.Delay(TimeSpan.FromSeconds(minLoadingTime));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load scene group {index}: {ex.Message}");
            }
            finally
            {
                EnableLoadingCanvas(false);
                loadingBar.fillAmount = 1f;
            }
        }

        void EnableLoadingCanvas(bool enable = true)
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
                if (mainMenuCanvas != null) mainMenuCanvas.enabled = true;
            }
        }
    }

    public class LoadingProgress : IProgress<float>
    {
        public event Action<float> Progressed;
        const float ratio = 1f;

        public void Report(float value)
        {
            float normalizedValue = Mathf.Clamp01(value / ratio);
            Progressed?.Invoke(normalizedValue);
        }
    }
}