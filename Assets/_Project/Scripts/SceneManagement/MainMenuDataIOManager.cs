using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.IO;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;
using DG.Tweening; // Ensure DOTween is imported

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Project.Scripts.SceneManagement
{
    [System.Serializable]
    public class MainMenuIOData
    {
        public bool isCanvasEnabled;
        public bool isBackgroundEnabled;
    }

    public class MainMenuDataIOManager : SerializedMonoBehaviour
    {
        public static MainMenuDataIOManager Instance { get; private set; } // Singleton access

        [SerializeField] private Canvas loginUiCanvas;
        [SerializeField] private CanvasGroup loginCanvasGroup; // New: Assign CanvasGroup on canvas GO in Inspector
        [SerializeField] private GameObject backgroundDisplay;
        [OdinSerialize, ShowInInspector] private MainMenuIOData mainMenuIOData = new MainMenuIOData { isCanvasEnabled = false, isBackgroundEnabled = false };

        private string savePath;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this; // Set singleton
            }
            else
            {
                Destroy(gameObject); // Prevent duplicates
                return;
            }

            savePath = Application.persistentDataPath + "/uiState.dat";
            LoadUIState();
        }

        void Start()
        {
            ApplyUIState();
        }

        private void ApplyUIState()
        {
            if (loginUiCanvas != null)
            {
                SetCanvasEnabled(mainMenuIOData.isCanvasEnabled);
            }
            if (backgroundDisplay != null)
            {
                SetBackgroundEnabled(mainMenuIOData.isBackgroundEnabled);
            }
            Canvas.ForceUpdateCanvases();
        }

        private void SetCanvasEnabled(bool enable)
        {
            if (loginUiCanvas == null || loginCanvasGroup == null) return; // Safety check

            // Kill any ongoing tweens to prevent conflicts
            DOTween.Kill(loginCanvasGroup);

            if (enable)
            {
                // Fade in: Activate, set alpha 0, fade to 1, then enable
                loginUiCanvas.gameObject.SetActive(true);
                loginCanvasGroup.alpha = 0f;
                loginCanvasGroup.DOFade(1f, 0.5f).OnComplete(() =>
                {
                    loginUiCanvas.enabled = true;
#if UNITY_EDITOR
                    if (!Application.isPlaying) EditorUtility.SetDirty(loginUiCanvas);
#endif
                });
            }
            else
            {
                // Fade out: Fade to 0, then deactivate and disable
                loginCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    loginUiCanvas.gameObject.SetActive(false);
                    loginUiCanvas.enabled = false;
#if UNITY_EDITOR
                    if (!Application.isPlaying) EditorUtility.SetDirty(loginUiCanvas);
#endif
                });
            }

#if UNITY_EDITOR
            if (!Application.isPlaying) Undo.RecordObject(loginUiCanvas, "Set Canvas Enabled");
#endif
        }

        private void SetBackgroundEnabled(bool enable)
        {
            backgroundDisplay.SetActive(enable);
        }

        [Button] public void ToggleCanvasOn() { ToggleInternal(true, true); }
        [Button] public void ToggleCanvasOff() { ToggleInternal(true, false); }
        [Button] public void ToggleBackgroundOn() { ToggleInternal(false, true); }
        [Button] public void ToggleBackgroundOff() { ToggleInternal(false, false); }

        private void ToggleInternal(bool isCanvas, bool enable)
        {
            if (isCanvas && loginUiCanvas != null)
            {
                mainMenuIOData.isCanvasEnabled = enable;
                SetCanvasEnabled(enable);
            }
            else if (!isCanvas && backgroundDisplay != null)
            {
                mainMenuIOData.isBackgroundEnabled = enable;
                SetBackgroundEnabled(enable);
            }
            SaveUIState();
        }

        [Button] public void SaveUIState()
        {
            byte[] bytes = SerializationUtility.SerializeValue(mainMenuIOData, DataFormat.Binary);
            File.WriteAllBytes(savePath, bytes);
        }

        private void LoadUIState()
        {
            if (File.Exists(savePath))
            {
                byte[] bytes = File.ReadAllBytes(savePath);
                mainMenuIOData = SerializationUtility.DeserializeValue<MainMenuIOData>(bytes, DataFormat.Binary);
            }
        }
    }
}