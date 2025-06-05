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
        [SerializeField] private float fillSmoothingSpeed = 5f;
        [SerializeField] private SceneGroup[] sceneGroups;
        [SerializeField] private float minLoadingTime = 3f;
        
        // UI Component References
        [SerializeField] private Image backgroundImage;          // Background image shown during loading
        [SerializeField] private Image loadingBarBorder;        // Static border of the loading bar
        [SerializeField] private Image loadingBarFill;          // Fill image that progresses
        [SerializeField] private TextMeshProUGUI loadingText;   // Text displaying "Loading..." or progress
        [SerializeField] private CanvasGroup loadingUICanvasGroup; // Group containing loading UI elements
        [SerializeField] private Canvas loginUICanvas;          // Login UI canvas (will be found if in another scene)
        [SerializeField] private Camera loadingCamera;          // Camera for rendering loading screen

        private float targetProgress;
        private bool isLoading;
        private int currentGroupIndex = 0;
        private CanvasGroup loginUICanvasGroup; // Cached reference to login UI CanvasGroup

        public readonly SceneGroupManager manager = new SceneGroupManager();

        private void Awake()
        {
            DOTween.Init(); // Initialize DOTween
            manager.OnSceneLoaded += sceneName => Debug.Log("Loaded: " + sceneName);
            manager.OnSceneUnloaded += sceneName => Debug.Log("Unloaded: " + sceneName);
            manager.OnSceneGroupLoaded += () => Debug.Log("Scene group loaded.");

            // Set initial state
            if (backgroundImage != null)
            {
                backgroundImage.gameObject.SetActive(true);
                backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f); // Start transparent
                backgroundImage.DOFade(1.0f, 1.0f); // Fade in to opaque
            }
            if (loadingUICanvasGroup != null)
            {
                loadingUICanvasGroup.gameObject.SetActive(true);
                loadingUICanvasGroup.alpha = 1f;
            }
            if (loadingBarFill != null) loadingBarFill.fillAmount = 0f;
            if (loadingText != null) loadingText.text = "Loading...";
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
                ShowLoginUI();
            }
        }

        private void ShowLoadingUI()
        {
            if (loadingCamera != null) loadingCamera.gameObject.SetActive(true);
            if (loadingUICanvasGroup != null)
            {
                loadingUICanvasGroup.DOFade(1f, 0.5f);
            }
        }

        private void HideLoadingUI()
        {
            Sequence seq = DOTween.Sequence();
            if (backgroundImage != null) seq.Append(backgroundImage.DOFade(0f, 0.5f));
            if (loadingUICanvasGroup != null) seq.Join(loadingUICanvasGroup.DOFade(0f, 0.5f));
            seq.OnComplete(() =>
            {
                if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
                if (loadingUICanvasGroup != null) loadingUICanvasGroup.gameObject.SetActive(false);
                if (loadingCamera != null) loadingCamera.gameObject.SetActive(false);
            });
        }

        private void ShowLoginUI()
        {
            // If loginUICanvas is in the Boot scene, use the serialized reference
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
                // Otherwise, find it in the loaded scene (e.g., MainMenu)
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
                else
                {
                    Debug.LogWarning("LoginMenuCanvas not found.");
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

            // Fade in 3D background objects at 90% progress
            if (targetProgress >= 0.9f && !DOTween.IsTweening("FadeInBackgroundObjects"))
            {
                FadeInBackgroundObjects();
            }
        }

        private void FadeInBackgroundObjects()
        {
            // Assuming 3D objects have a script to handle fading
            var backgroundObjects = FindObjectsOfType<BackgroundObject>();
            foreach (var obj in backgroundObjects)
            {
                obj.FadeIn();
            }
        }

        public void ToggleNextSceneGroup()
        {
            if (sceneGroups == null || sceneGroups.Length == 0)
            {
                Debug.LogWarning("No scene groups assigned to SceneLoader.");
                return;
            }

            currentGroupIndex = (currentGroupIndex + 1) % sceneGroups.Length;
            LoadSceneGroup(currentGroupIndex);
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

    // Example script for 3D background objects (create separately if needed)
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