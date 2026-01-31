using System;
using _Project.Scripts.SceneManagement;
using UI.IMGUI;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace Main_Screen
{
    public class LoginMenuManager : MonoBehaviour
    {
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
            if (OverlayRef != null)
            {
                OverlayRef.TriggerLoadingScreen();
            }

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

        private void StartLoadingTransition()
        {
            if (OverlayRef != null)
            {
                OverlayRef.TriggerLoadingScreen();
            }

            if (SceneLoaderRef != null)
            {
                SceneLoaderRef.ToggleNextSceneGroup();
            }

            DisableOtherObjects();  // ← This is where we clean up Boot stuff
        }

        private void DisableOtherObjects()
        {
            // 1. Disable the entire boot container
            if (bootComponentIOBox != null)
            {
                bootComponentIOBox.SetActive(false);
                Debug.Log("Disabled bootComponentIOBox GameObject.");
            }
            else
            {
                Debug.LogWarning("bootComponentIOBox not assigned in LoginMenuManager.", this);
            }

            // 2. Disable AudioListener (search in children or on self)
            AudioListener listener = bootComponentIOBox?.GetComponentInChildren<AudioListener>(true);
            if (listener != null)
            {
                listener.enabled = false;
                Debug.Log("Disabled AudioListener on bootComponentIOBox.");
            }

            // 3. Disable EventSystem (search in children or on self)
            EventSystem eventSystem = bootComponentIOBox?.GetComponentInChildren<EventSystem>(true);
            if (eventSystem != null)
            {
                eventSystem.enabled = false;
                Debug.Log("Disabled EventSystem on bootComponentIOBox.");
            }

            // 4. Optional: Disable Boot camera if tagged
            GameObject bootCamera = GameObject.FindWithTag(bootCameraTag);
            if (bootCamera != null)
            {
                bootCamera.SetActive(false);
                Debug.Log($"Disabled Boot camera tagged '{bootCameraTag}'.");
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
