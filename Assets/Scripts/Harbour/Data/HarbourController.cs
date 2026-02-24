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
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (state == null)
            {
                Debug.LogError("HarbourStateSO not assigned!", this);
            }
        }

        private void Start()
        {
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
            switch (state.currentMode)
            {
                case HarbourStateSO.HarbourMode.Idle:
                    SetUI(idleHudDocument, true);
                    SetUI(harbourBuildDocument, false);
                    SetUI(shipBuildDocument, false);

                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);

                    SetTGSGrid(state.tgsGridEnabled); // or force false in Idle
                    break;

                case HarbourStateSO.HarbourMode.HarbourBuild:
                    SetUI(idleHudDocument, false);
                    SetUI(harbourBuildDocument, true);
                    SetUI(shipBuildDocument, false);

                    SetCamera(idleCamera, false);
                    SetCamera(harbourBuildCamera, true);

                    SetTGSGrid(true);
                    break;

                case HarbourStateSO.HarbourMode.ShipBuild:
                    SetUI(idleHudDocument, false);
                    SetUI(harbourBuildDocument, false);
                    SetUI(shipBuildDocument, true);

                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);

                    SetTGSGrid(false);
                    break;
            }
        }

        private void SetUI(UIDocument doc, bool visible)
        {
            if (doc == null || doc.rootVisualElement == null) return;
            doc.rootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
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