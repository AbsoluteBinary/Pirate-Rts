using _Project.Scripts.UI.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.Data
{
    public class HarbourController : MonoBehaviour
    {
        [SerializeField] private GameObject bootComponentIOBox;
        [SerializeField] private GameObject harbourComponentIOBox;
        public static HarbourController Instance { get; private set; }

        [SerializeField] private HarbourStateSO state; // drag the SO here

        // UIDocument references (drag in Inspector or find by name/tag)
        [SerializeField] private UIDocument idleHudDocument;        // LoginScreen / HarbourScreenSpaceIdleHud
        //[SerializeField] private UIDocument harbourBuildDocument;   // HarbourBuildHud
        //[SerializeField] private UIDocument shipBuildDocument;      // ShipBuildHud

        // Camera references (drag or tag)
        [SerializeField] private Camera idleCamera;
        [SerializeField] private Camera harbourBuildCamera;

        // TGS grid reference (your TGS component or GameObject)
        [SerializeField] private GameObject tgsGridObject;
        
        [Header("Reset / Default State")]
        [SerializeField] private bool resetOnEveryHarbourEntry = true;

        // Optional: if you have more panels or objects to reset that aren't mode-specific
        [SerializeField] private string[] panelsToHideOnReset = { "BuildPanel", "ExtraHarbourUI" };
        [SerializeField] private string[] panelsToShowOnReset = { "IdleHUD", "MainHarbourInfo" };

        // If you have extra GameObjects to reset (not already handled by mode)
        [SerializeField] private GameObject[] objectsToEnableOnReset;
        [SerializeField] private GameObject[] objectsToDisableOnReset;
        
        //Playerbuild GO holding camera controls and camera
        [SerializeField] private GameObject playerBuildGO;

        private void Awake()
        {
            //Debug.Log($"[HarbourController] Awake() started on {gameObject.name} in scene {gameObject.scene.name}");

            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Duplicate HarbourController on {gameObject.name} – destroying self", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            //Debug.Log("[HarbourController] Singleton set successfully");
        }
        
        public void OnHarbourEntered()
        {
            if (resetOnEveryHarbourEntry)
            {
                ResetToDefaultState();
            }
            else
            {
                // Optional: only reset if not already in a valid state
                if (state.currentMode != HarbourStateSO.HarbourMode.Idle)
                {
                    ResetToDefaultState();
                }
            }

            // You can also force-apply state here if needed
            ApplyHarbourState();
        }

        private void Start()
        {
            //Debug.Log($"[HarbourController] Start() ran in scene {gameObject.scene.name} at frame {Time.frameCount}");
            ApplyHarbourState();
        }

        public void SetMode(HarbourStateSO.HarbourMode newMode)
        {
            state.currentMode = newMode;
            ApplyHarbourState();
            // Optional: save here if you want mode persistence
        }

        private void ApplyHarbourState()
        {
            // Hide ALL first (prevents overlap)
            SetUIDocumentActive(idleHudDocument, false);
            //SetUIDocumentActive(harbourBuildDocument, false);
            //SetUIDocumentActive(shipBuildDocument, false);

            switch (state.currentMode)
            {
                case HarbourStateSO.HarbourMode.Idle:
                    SetUIDocumentActive(idleHudDocument, true);
                    if (idleHudDocument != null && idleHudDocument.rootVisualElement != null)
                    {
                        var root = idleHudDocument.rootVisualElement;
                        root.style.display = DisplayStyle.Flex;
                        root.style.opacity = 1f;
                        //Debug.Log($"Forced Idle HUD root visible: display={root.style.display}, opacity={root.style.opacity}");
                    }
                    SetUIDocumentActive(idleHudDocument, true);
                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);
                    SetTGSGrid(false); // or state.tgsGridEnabled if you want to persist it
                    break;

                case HarbourStateSO.HarbourMode.HarbourBuild:
                    //SetUIDocumentActive(harbourBuildDocument, true);
                    SetCamera(idleCamera, false);
                    SetCamera(harbourBuildCamera, true);
                    SetTGSGrid(true);
                    playerBuildGO.SetActive(true);
                    break;

                case HarbourStateSO.HarbourMode.ShipBuild:
                    //SetUIDocumentActive(shipBuildDocument, true);
                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);
                    SetTGSGrid(false);
                    break;
            }
        }
        
        public void ResetToDefaultState()
        {
            //Debug.Log("[HarbourController] Resetting Harbour to default Idle state");

            // 1. Force Idle mode first (this already handles most UI/camera/TGS via ApplyHarbourState)
            SetMode(HarbourStateSO.HarbourMode.Idle);

            // 2. Extra UI panel visibility via UIManager (from HarbourController01 logic)
            if (UIManager.Instance != null)
            {
                foreach (var panelName in panelsToHideOnReset)
                {
                    UIManager.Instance.SetPanelVisible(panelName, false);
                }

                foreach (var panelName in panelsToShowOnReset)
                {
                    UIManager.Instance.SetPanelVisible(panelName, true);
                }
            }
            else
            {
                //Debug.LogWarning("UIManager.Instance is null – skipping panel reset");
            }

            // 3. Extra GameObject resets (if any not covered by mode)
            if (objectsToEnableOnReset != null)
                foreach (var go in objectsToEnableOnReset)
                    if (go != null) go.SetActive(true);

            if (objectsToDisableOnReset != null)
                foreach (var go in objectsToDisableOnReset)
                    if (go != null) go.SetActive(false);

            // 4. Any other resets (inventory, resources, player position, etc.)
            // Example placeholders:
            // if (InventorySystem.Instance) InventorySystem.Instance.ResetToDefault();
            // if (PlayerController.Instance) PlayerController.Instance.TeleportToHarbourSpawn();

            //Debug.Log("[HarbourController] Reset complete");
        }
        

        private void SetUIDocumentActive(UIDocument doc, bool active)
        {
            if (doc == null || doc.gameObject == null) return;
            doc.gameObject.SetActive(active);
            //Debug.Log($"Set UIDocument '{doc.gameObject.name}' active = {active}");
        }

        // private void SetUI(UIDocument doc, bool visible)
        // {
        //     if (doc == null || doc.rootVisualElement == null) return;
        //     doc.rootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        // }
        
        
        public void DisableBootComponents()
        {
            DisableComponentsOnContainer(bootComponentIOBox);
        }

        public void DisableHarbourComponents()
        {
            DisableComponentsOnContainer(harbourComponentIOBox);
        }

        private void DisableComponentsOnContainer(GameObject container)
        {
            if (container == null)
            {
                Debug.LogWarning("Container GameObject is null – cannot disable components");
                return;
            }

            // Disable whole container (most important)
            container.SetActive(false);

            // Disable AudioListener
            var listener = container.GetComponentInChildren<AudioListener>(true);
            if (listener != null)
                listener.enabled = false;
            else
                Debug.LogWarning($"No AudioListener found on {container.name}");

            // Disable EventSystem
            var eventSystem = container.GetComponentInChildren<EventSystem>(true);
            if (eventSystem != null)
                eventSystem.enabled = false;
            else
                Debug.LogWarning($"No EventSystem found on {container.name}");
        }

        private void SetCamera(Camera cam, bool active)
        {
            if (cam != null) cam.gameObject.SetActive(active);
        }

        private void SetTGSGrid(bool enabled)
        {
            if (tgsGridObject != null) tgsGridObject.SetActive(enabled);
            // or call your TGS enable/disable method
        }

        // Call this from buttons / logic
        public void EnterHarbourBuildMode() => SetMode(HarbourStateSO.HarbourMode.HarbourBuild);
        public void EnterShipBuildMode() => SetMode(HarbourStateSO.HarbourMode.ShipBuild);
        public void ReturnToIdle() => SetMode(HarbourStateSO.HarbourMode.Idle);
        
        
    }
}