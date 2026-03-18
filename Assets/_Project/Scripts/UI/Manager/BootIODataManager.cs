using System.IO;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.UI.Manager
{
    [System.Serializable]
    public class BootIOData
    {
        public bool showMainMenu = false;
        public bool showBackground = false;
    }

    public class BootIODataManager : SerializedMonoBehaviour
    {
        public static BootIODataManager Instance { get; private set; }

        private GameObject bootComponentIOBox;
        private GameObject harbourComponentIOBox;

        [OdinSerialize, ShowInInspector] private BootIOData bootIOData = new BootIOData();

        private string savePath;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            savePath = Application.persistentDataPath + "/bootIOState.dat";
            LoadIOState();

            bootComponentIOBox = GameObject.FindWithTag("BootComponentBoxIO");
            if (bootComponentIOBox == null)
            {
                Debug.LogWarning("[BootIO] BootComponentBoxIO not found – add tag 'BootComponentBoxIO'");
            }

            // Delay harbour lookup (additive scene may not be loaded yet)
            //Invoke(nameof(FindHarbourBox), 1f);
        }

        // private void FindHarbourBox()
        // {
        //     harbourComponentIOBox = GameObject.FindWithTag("HarbourIOBox");
        //     if (harbourComponentIOBox == null)
        //     {
        //         Debug.LogWarning("[BootIO] HarbourIOBox not found – add tag 'HarbourIOBox' or check scene load order");
        //     }
        //
        //     DisableComponentBoxes();
        // }

        private void Start()
        {
            ApplyIOState();
        }

        private void ApplyIOState()
        {
            // Expose via public getters for IMGUI or other systems
            // No log needed here unless debugging specific issues
        }

        private void DisableComponentBoxes()
        {
            DisableBox(bootComponentIOBox, "Boot");
            DisableBox(harbourComponentIOBox, "Harbour");
        }

        public void DisableBootComponentIOBox()
        {
            if (bootComponentIOBox == null)
            {
                bootComponentIOBox = GameObject.FindWithTag("BootComponentBoxIO");
                if (bootComponentIOBox == null)
                {
                    Debug.LogError("[BootIO] BootComponentBoxIO not found – check tag 'BootComponentBoxIO'");
                    return;
                }
            }

            DisableBox(bootComponentIOBox, "Boot");
        }

        private void DisableBox(GameObject box, string name)
        {
            if (box == null)
            {
                return;  // Silent fail – already warned earlier
            }

            // Disable AudioListener if present
            var listener = box.GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = false;

            // Disable EventSystem if present
            var eventSystem = box.GetComponentInChildren<EventSystem>();
            if (eventSystem != null) eventSystem.enabled = false;

            // Disable whole box
            box.SetActive(false);
        }

        public void SaveIOState(bool mainMenuVisible, bool backgroundVisible)
        {
            bootIOData.showMainMenu = mainMenuVisible;
            bootIOData.showBackground = backgroundVisible;

            byte[] data = SerializationUtility.SerializeValue(bootIOData, DataFormat.Binary);
            File.WriteAllBytes(savePath, data);
            // No success log – silent unless error
        }

        private void LoadIOState()
        {
            if (File.Exists(savePath))
            {
                byte[] data = File.ReadAllBytes(savePath);
                bootIOData = SerializationUtility.DeserializeValue<BootIOData>(data, DataFormat.Binary);
                // No success log
            }
            // No "no file" log – defaults are fine
        }

        // Public getters for IMGUI / other systems
        public bool ShowMainMenu => bootIOData.showMainMenu;
        public bool ShowBackground => bootIOData.showBackground;

        // Public setters with save
        public void ToggleMainMenu(bool visible)
        {
            bootIOData.showMainMenu = visible;
            SaveIOState(bootIOData.showMainMenu, bootIOData.showBackground);
        }

        public void ToggleBackground(bool visible)
        {
            bootIOData.showBackground = visible;
            SaveIOState(bootIOData.showMainMenu, bootIOData.showBackground);
        }
    }
}