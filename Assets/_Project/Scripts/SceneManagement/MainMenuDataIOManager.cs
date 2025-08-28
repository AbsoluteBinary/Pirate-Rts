using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.IO;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;
using DG.Tweening;
using UnityEngine.EventSystems;

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
        
        private GameObject componentIOBox; // Changed: Not serialized, find at runtime
        [SerializeField] private GameObject bootComponentIOBox; // Keep serialized for Boot
        [SerializeField] private Canvas loginUiCanvas;
        [SerializeField] private CanvasGroup loginCanvasGroup;
        [SerializeField] private GameObject backgroundDisplay;
        [OdinSerialize, ShowInInspector] private MainMenuIOData mainMenuIOData = new MainMenuIOData { isCanvasEnabled = false, isBackgroundEnabled = false };

        private string savePath;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            savePath = Application.persistentDataPath + "/uiState.dat";
            LoadUIState();
        }

        void Start()
        {
            // New: Find componentIOBox at start
            AssignComponentIOBox();
            ApplyUIState();
        }

        // New: Find componentIOBox at runtime
        private void AssignComponentIOBox()
        {
            componentIOBox = GameObject.FindWithTag("ComponentBoxIO"); // Assumes tag on container
            if (componentIOBox == null)
            {
                Debug.LogWarning("componentIOBox not found with tag ComponentBoxIO.");
            }
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
            if (loginUiCanvas == null || loginCanvasGroup == null) return;

            DOTween.Kill(loginCanvasGroup);

            if (enable)
            {
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

        public void DisableComponentIOBox()
        {
            // New: Ensure componentIOBox is assigned before disabling
            if (componentIOBox == null)
            {
                AssignComponentIOBox();
            }

            AudioListener listener = FindAudioListener(componentIOBox);
            if (listener != null)
            {
                listener.enabled = false;
                Debug.Log("Disabled AudioListener on componentIOBox.");
            }
            else
            {
                Debug.LogWarning("No AudioListener found on componentIOBox.");
            }

            EventSystem eventSystem = FindEventSystem(componentIOBox);
            if (eventSystem != null)
            {
                eventSystem.enabled = false;
                Debug.Log("Disabled EventSystem on componentIOBox.");
            }
            else
            {
                Debug.LogWarning("No EventSystem found on componentIOBox.");
            }
        }

        public void DisableBootComponentIOBox()
        {
            AudioListener listener = FindAudioListener(bootComponentIOBox);
            if (listener != null)
            {
                listener.enabled = false;
                Debug.Log("Disabled AudioListener on bootComponentIOBox.");
            }
            else
            {
                Debug.LogWarning("No AudioListener found on bootComponentIOBox.");
            }

            EventSystem eventSystem = FindEventSystem(bootComponentIOBox);
            if (eventSystem != null)
            {
                eventSystem.enabled = false;
                Debug.Log("Disabled EventSystem on bootComponentIOBox.");
            }
            else
            {
                Debug.LogWarning("No EventSystem found on bootComponentIOBox.");
            }
        }

        private AudioListener FindAudioListener(GameObject container)
        {
            if (container == null) return null;
            AudioListener listener = container.GetComponent<AudioListener>();
            if (listener != null) return listener;
            return container.GetComponentInChildren<AudioListener>();
        }

        private EventSystem FindEventSystem(GameObject container)
        {
            if (container == null) return null;
            EventSystem eventSystem = container.GetComponent<EventSystem>();
            if (eventSystem != null) return eventSystem;
            return container.GetComponentInChildren<EventSystem>();
        }
    }
}