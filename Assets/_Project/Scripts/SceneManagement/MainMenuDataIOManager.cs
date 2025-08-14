using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.IO;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

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
        [SerializeField] private Canvas loginUiCanvas;
        [SerializeField] private GameObject backgroundDisplay;
        [OdinSerialize, ShowInInspector] private MainMenuIOData mainMenuIOData = new MainMenuIOData { isCanvasEnabled = false, isBackgroundEnabled = false };

        private string savePath;

        void Awake()
        {
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
#if UNITY_EDITOR
            if (!Application.isPlaying) Undo.RecordObject(loginUiCanvas, "Set Canvas Enabled");
#endif
            loginUiCanvas.gameObject.SetActive(enable);
            loginUiCanvas.enabled = enable;
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(loginUiCanvas);
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

        [Button] private void SaveUIState()
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