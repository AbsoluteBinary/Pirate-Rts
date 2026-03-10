using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace Harbour.Data
{
    public class HarbourController : MonoBehaviour
    {
        public static HarbourController Instance { get; private set; }

        [SerializeField] private HarbourStateSO state; // drag the SO here

        // UIDocument references (drag in Inspector or find by name/tag)
        [SerializeField] private UIDocument idleHudDocument;        // LoginScreen / HarbourScreenSpaceIdleHud
        [SerializeField] private UIDocument harbourBuildDocument;   // HarbourBuildHud
        [SerializeField] private UIDocument shipBuildDocument;      // ShipBuildHud

        // Camera references (drag or tag)
        [SerializeField] private Camera idleCamera;
        [SerializeField] private Camera harbourBuildCamera;

        // TGS grid reference (your TGS component or GameObject)
        [SerializeField] private GameObject tgsGridObject;

        private void Awake()
        {
            Debug.Log($"[HarbourController] Awake() started on {gameObject.name} in scene {gameObject.scene.name}");

            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Duplicate HarbourController on {gameObject.name} – destroying self", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[HarbourController] Singleton set successfully");
        }

        private void Start()
        {
            Debug.Log($"[HarbourController] Start() ran in scene {gameObject.scene.name} at frame {Time.frameCount}");
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
            SetUIDocumentActive(harbourBuildDocument, false);
            SetUIDocumentActive(shipBuildDocument, false);

            switch (state.currentMode)
            {
                case HarbourStateSO.HarbourMode.Idle:
                    SetUIDocumentActive(idleHudDocument, true);
                    if (idleHudDocument != null && idleHudDocument.rootVisualElement != null)
                    {
                        var root = idleHudDocument.rootVisualElement;
                        root.style.display = DisplayStyle.Flex;
                        root.style.opacity = 1f;
                        Debug.Log($"Forced Idle HUD root visible: display={root.style.display}, opacity={root.style.opacity}");
                    }
                    SetUIDocumentActive(idleHudDocument, true);
                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);
                    SetTGSGrid(false); // or state.tgsGridEnabled if you want to persist it
                    break;

                case HarbourStateSO.HarbourMode.HarbourBuild:
                    SetUIDocumentActive(harbourBuildDocument, true);
                    SetCamera(idleCamera, false);
                    SetCamera(harbourBuildCamera, true);
                    SetTGSGrid(true);
                    break;

                case HarbourStateSO.HarbourMode.ShipBuild:
                    SetUIDocumentActive(shipBuildDocument, true);
                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);
                    SetTGSGrid(false);
                    break;
            }
        }

        private void SetUIDocumentActive(UIDocument doc, bool active)
        {
            if (doc == null || doc.gameObject == null) return;
            doc.gameObject.SetActive(active);
            Debug.Log($"Set UIDocument '{doc.gameObject.name}' active = {active}");
        }

        // private void SetUI(UIDocument doc, bool visible)
        // {
        //     if (doc == null || doc.rootVisualElement == null) return;
        //     doc.rootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        // }

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