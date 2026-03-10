using UI.Manager;
using UnityEngine;

namespace Managers.Harbour
{
    public class HarbourController01 : MonoBehaviour
    {
        public static HarbourController01 Instance { get; private set; }

        [Header("Default Harbour State")]
        [SerializeField] private bool resetOnEveryEntry = true; // toggle in Inspector if needed

        // Example collections – populate these in Inspector or via code
        [SerializeField] private GameObject[] objectsToEnableOnEnter;
        [SerializeField] private GameObject[] objectsToDisableOnEnter;

        [Header("State Variables (examples)")]
        public HarbourMode currentMode = HarbourMode.Idle; // enum or whatever you use
        public bool isBuildModeActive = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // usually desired for persistent state
        }

        /// <summary>
        /// Call this when the Harbour scene/group becomes the active one
        /// </summary>
        public void OnHarbourEntered()
        {
            if (!resetOnEveryEntry) return;

            ResetToDefaultState();
        }

        public void ResetToDefaultState()
        {
            Debug.Log("[HarbourController] Resetting to default Harbour state");

            // 1. Reset UI visibility / panels
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetPanelVisible("BuildPanel", false);
                UIManager.Instance.SetPanelVisible("SomeOtherHarbourUI", true);
                // ... any other panels you want shown/hidden by default ...
            }

            // 2. Enable / disable GameObjects
            if (objectsToEnableOnEnter != null)
                foreach (var go in objectsToEnableOnEnter)
                    if (go != null) go.SetActive(true);

            if (objectsToDisableOnEnter != null)
                foreach (var go in objectsToDisableOnEnter)
                    if (go != null) go.SetActive(false);

            // 3. Reset variables / modes
            currentMode = HarbourMode.Idle;
            isBuildModeActive = false;

            // 4. Reset any other systems (inventory, resources, camera, etc.)
            // Example:
            // PlayerInventory.Instance?.ResetToDefault();
            // CameraController.Instance?.SetDefaultHarbourView();

            // 5. Optional: broadcast that reset happened
            // EventBus<HarbourResetCompleteEvent>.Raise(new HarbourResetCompleteEvent());
        }

        // Optional: expose methods for build mode toggling later
        public void EnterBuildMode()
        {
            isBuildModeActive = true;
            currentMode = HarbourMode.Build;
            // ... enable build UI, change cursor, etc.
        }

        public void ExitBuildMode()
        {
            isBuildModeActive = false;
            currentMode = HarbourMode.Idle;
            // ... hide build UI, reset cursor, etc.
        }
    }

    public enum HarbourMode
    {
        Idle,
        Build,
        View,
        // etc.
    }
}
