using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.IMGUI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.Main_Screen
{
    public class LoginMenuManager : MonoBehaviour
    {
        public static LoginMenuManager Instance { get; private set; }
        [Header("UI References")]
        [SerializeField] private Button toggleSceneButton;
        [SerializeField] private CanvasGroup loginCanvasGroup;

        [Header("Boot Cleanup References")]  // ← NEW
        [SerializeField] private GameObject bootComponentIOBox;           // Drag from Inspector (same as in MainMenuDataIOManager)
        [SerializeField] private string bootCameraTag = "BootCamera";     // Optional: tag your boot camera

        [Header("Timing")]
        [SerializeField] private float startupOverlayDelayBeforeLoginFadeIn = 0.8f;
        [SerializeField] private float loginFadeDuration = 0.7f;
        [SerializeField] private float delayAfterLoginFadeOutBeforeOverlay = 0.4f;

        private SceneLoader SceneLoaderRef => _sceneLoader ??= SceneLoader.Instance;
        private IMGUILoadingOverlay OverlayRef => _loadingOverlay ??= IMGUILoadingOverlay.Instance;

        private SceneLoader _sceneLoader;
        private IMGUILoadingOverlay _loadingOverlay;

        private void Awake()
        {
            // if (OverlayRef != null)
            // {
            //     OverlayRef.TriggerLoadingScreen();
            // }
            // Singleton setup: Ensure only one instance
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes

            if (loginCanvasGroup != null)
            {
                loginCanvasGroup.alpha = 0f;
                loginCanvasGroup.interactable = false;
                loginCanvasGroup.blocksRaycasts = false;
            }
        }

        private void Start()
        {
            // Button wiring...
            if (toggleSceneButton == null)
            {
                toggleSceneButton = GameObject.Find("ToggleSceneButton")?.GetComponent<Button>();
            }

            if (toggleSceneButton != null)
            {
                toggleSceneButton.onClick.AddListener(OnToggleButtonClicked);
            }

            if (OverlayRef != null)
            {
                DOVirtual.DelayedCall(startupOverlayDelayBeforeLoginFadeIn, FadeInLoginMenu);
            }
        }

        private void OnToggleButtonClicked()
        {
            toggleSceneButton.interactable = false;

            FadeOutLoginMenu(() =>
            {
                DOVirtual.DelayedCall(delayAfterLoginFadeOutBeforeOverlay, StartLoadingTransition);
            });
        }

        public void FadeInLoginMenu()
        {
            if (loginCanvasGroup == null) return;

            loginCanvasGroup.DOFade(1f, loginFadeDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    loginCanvasGroup.interactable = true;
                    loginCanvasGroup.blocksRaycasts = true;
                });
        }

        private void FadeOutLoginMenu(System.Action onComplete)
        {
            if (loginCanvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            loginCanvasGroup.interactable = false;
            loginCanvasGroup.blocksRaycasts = false;

            loginCanvasGroup.DOFade(0f, loginFadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void StartLoadingTransition()
        {
            if (OverlayRef != null)
            {
                OverlayRef.TriggerLoadingScreen();
            }

            if (SceneLoaderRef != null)
            {
                _ = SceneLoaderRef.ToggleNextSceneGroup();
            }
        }

        

        private void OnDestroy()
        {
            if (toggleSceneButton != null)
            {
                toggleSceneButton.onClick.RemoveListener(OnToggleButtonClicked);
            }
        }
    }
}
