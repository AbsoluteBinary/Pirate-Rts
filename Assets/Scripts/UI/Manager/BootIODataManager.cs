using System.IO;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Manager
{
    [System.Serializable]
    public class BootIOData
    {
        public bool showMainMenu = false;     // Controls IMGUI main menu drawing
        public bool showBackground = false;   // Controls IMGUI background overlay
    }

    public class BootIODataManager : SerializedMonoBehaviour
    {
        public static BootIODataManager Instance { get; private set; }

        // Component boxes to disable on load (audio/event system conflicts)
        private GameObject bootComponentIOBox;
        private GameObject harbourComponentIOBox;

        [OdinSerialize, ShowInInspector] private BootIOData bootIOData = new BootIOData();

        private string savePath;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);  // Persistent across scenes
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            savePath = Application.persistentDataPath + "/bootIOState.dat";
            LoadIOState();
            
            
            // Runtime find – tag your IO boxes uniquely
            bootComponentIOBox = GameObject.FindWithTag("BootComponentBoxIO");
            if (bootComponentIOBox == null) Debug.LogWarning("[BootComponentBoxIO] BootComponentBoxIO not found – add tag 'BootComponentBoxIO'");

            // Harbour loads later – find on enable or delay
            Invoke(nameof(FindHarbourBox), 1f);  // 1s delay safe for additive load
        
        }
        
        private void FindHarbourBox()
        {
            harbourComponentIOBox = GameObject.FindWithTag("HarbourIOBox");
            if (harbourComponentIOBox == null) Debug.LogWarning("[BootIO] HarbourIOBox not found – add tag 'HarbourIOBox' or load scene");

            DisableComponentBoxes();  // Run after both found
        }

        private void Start()
        {
            ApplyIOState();  // Apply on start (e.g., hide menus if saved off)
            //DisableComponentBoxes();  // Clean up audio/event system
        }

        private void ApplyIOState()
        {
            // No uGUI – expose public getters for your IMGUI HUD script
            // e.g., HarbourIMGUI.ShowMainMenu = bootIOData.showMainMenu;
            Debug.Log($"[BootIO] Applied state – MainMenu: {bootIOData.showMainMenu}, Background: {bootIOData.showBackground}");
        }

        private void DisableComponentBoxes()
        {
            DisableBox(bootComponentIOBox, "boot");
            DisableBox(harbourComponentIOBox, "harbour");
        }
        
        public void DisableBootComponentIOBox()
        {
            if (bootComponentIOBox == null)
            {
                bootComponentIOBox = GameObject.FindWithTag("BootComponentBoxIO");
                if (bootComponentIOBox == null)
                {
                    Debug.LogError("[BootIO] BootIOBox not found – check tag 'BootIOBox'");
                    return;
                }
            }

            DisableBox(bootComponentIOBox, "boot");
        }

        private void DisableBox(GameObject box, string name)
        {
            if (box == null)
            {
                Debug.LogWarning($"[BootIO] {name}ComponentIOBox not assigned.");
                return;
            }

            // Disable AudioListener
            var listener = box.GetComponentInChildren<AudioListener>();
            if (listener) listener.enabled = false;

            // Disable EventSystem
            var eventSystem = box.GetComponentInChildren<EventSystem>();
            if (eventSystem) eventSystem.enabled = false;

            // Disable whole box
            box.SetActive(false);
            Debug.Log($"[BootIO] Disabled {name}ComponentIOBox (audio, event system, active)");
        }

        public void SaveIOState(bool mainMenuVisible, bool backgroundVisible)
        {
            bootIOData.showMainMenu = mainMenuVisible;
            bootIOData.showBackground = backgroundVisible;

            byte[] data = SerializationUtility.SerializeValue(bootIOData, DataFormat.Binary);
            File.WriteAllBytes(savePath, data);
            Debug.Log("[BootIO] State saved");
        }

        private void LoadIOState()
        {
            if (File.Exists(savePath))
            {
                byte[] data = File.ReadAllBytes(savePath);
                bootIOData = SerializationUtility.DeserializeValue<BootIOData>(data, DataFormat.Binary);
                Debug.Log("[BootIO] State loaded");
            }
            else
            {
                Debug.Log("[BootIO] No save file – using defaults");
            }
        }

        // Public getters for your IMGUI HUD
        public bool ShowMainMenu => bootIOData.showMainMenu;
        public bool ShowBackground => bootIOData.showBackground;

        // Public setter for runtime changes (e.g., toggle menu)
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
