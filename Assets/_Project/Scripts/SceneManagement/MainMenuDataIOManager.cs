using System;
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
        
        private GameObject componentIOBox; // Runtime find
        [SerializeField] private GameObject bootComponentIOBox; // Boot container
        [SerializeField] private GameObject harbourComponentIOBox; // ← NEW LINE
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
            AssignComponentIOBox();
            ApplyUIState();
        }

        private void AssignComponentIOBox()
        {
            componentIOBox = GameObject.FindWithTag("ComponentBoxIO");
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
        
        [Button("Disable Harbour Component Box")]
        private void Editor_DisableHarbourBox()
        {
            if (!Application.isPlaying) return;
            DisableHarbourComponentIOBox();
        }

        public void DisableComponentIOBox()
        {
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

            // New: Disable the entire bootComponentIOBox GameObject
            if (bootComponentIOBox != null)
            {
                bootComponentIOBox.SetActive(false);
                Debug.Log("Disabled bootComponentIOBox GameObject.");
            }
            else
            {
                Debug.LogWarning("bootComponentIOBox not assigned.");
            }
        }
        
        public void DisableHarbourComponentIOBox()
        {
            AudioListener listener = FindAudioListener(harbourComponentIOBox);
            if (listener != null)
            {
                listener.enabled = false;
                Debug.Log("Disabled AudioListener on harbourComponentIOBox.");
            }
            else
            {
                Debug.LogWarning("No AudioListener found on harbourComponentIOBox.");
            }

            EventSystem eventSystem = FindEventSystem(harbourComponentIOBox);
            if (eventSystem != null)
            {
                eventSystem.enabled = false;
                Debug.Log("Disabled EventSystem on harbourComponentIOBox.");
            }
            else
            {
                Debug.LogWarning("No EventSystem found on harbourComponentIOBox.");
            }

            // Disable the whole container (most important!)
            if (harbourComponentIOBox != null)
            {
                harbourComponentIOBox.SetActive(false);
                Debug.Log("Disabled harbourComponentIOBox GameObject.");
            }
            else
            {
                Debug.LogWarning("harbourComponentIOBox not assigned.");
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