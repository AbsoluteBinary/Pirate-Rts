using _Project.Scripts.Harbour.Data;
using _Project.Scripts.Harbour.Data.SO;
using UnityEngine;

namespace _Project.Scripts.Harbour.UIControllers
{
    public class HarbourController : MonoBehaviour
    {
        public static HarbourController Instance { get; private set; }

        [SerializeField] private HarbourStateSO state;

        [SerializeField] private Camera idleCamera;
        [SerializeField] private Camera harbourBuildCamera;

        [SerializeField] private GameObject playerBuildGO;
        [SerializeField] private GameObject tgsGridObject;

        [SerializeField] private HarbourHUD harbourHUD;
        [SerializeField] private HarbourBuilderManager harbourBuilderManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void OnHarbourEntered()
        {
            Debug.Log("<color=lime>HarbourController.OnHarbourEntered() called from SceneLoader</color>");

            if (state != null)
                state.currentMode = HarbourStateSO.HarbourMode.Idle;

            ApplyHarbourState();
        }

        private void Start()
        {
            if (harbourHUD == null)
                harbourHUD = GetComponent<HarbourHUD>();

            if (harbourBuilderManager == null)
                harbourBuilderManager = FindObjectOfType<HarbourBuilderManager>();

            if (harbourHUD != null && harbourBuilderManager != null)
            {
                harbourHUD.OnBuildHarbourBaseClicked += () => SetMode(HarbourStateSO.HarbourMode.HarbourBuild);
                harbourHUD.OnLandTileSlotClicked += (tabName, slotIndex) =>
                {
                    Debug.Log($"<color=orange>HarbourController: Received slot click event from HUD - forwarding to BuilderManager</color>");
                    harbourBuilderManager.StartPreview();
                };

                Debug.Log("<color=lime>✅ All events wired successfully</color>");
            }
            else
            {
                Debug.LogError("HarbourHUD or HarbourBuilderManager reference is missing in Inspector!");
            }

            ApplyHarbourState();
        }

        private void ApplyHarbourState()
        {
            if (playerBuildGO != null)
                playerBuildGO.SetActive(false);

            switch (state.currentMode)
            {
                case HarbourStateSO.HarbourMode.Idle:
                    if (harbourHUD != null) harbourHUD.RefreshUI(HarbourStateSO.HarbourMode.Idle);
                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);
                    SetTGSGrid(false);
                    break;

                case HarbourStateSO.HarbourMode.HarbourBuild:
                    if (harbourHUD != null) harbourHUD.RefreshUI(HarbourStateSO.HarbourMode.HarbourBuild);
                    SetCamera(idleCamera, false);
                    SetCamera(harbourBuildCamera, true);
                    SetTGSGrid(true);
                    if (playerBuildGO != null) playerBuildGO.SetActive(true);
                    break;
            }

            Debug.Log($"<color=lime>Harbour State Applied: {state.currentMode}</color>");
        }

        private void SetCamera(Camera cam, bool active)
        {
            if (cam != null) cam.gameObject.SetActive(active);
        }

        private void SetTGSGrid(bool enabled)
        {
            if (tgsGridObject != null)
            {
                tgsGridObject.SetActive(enabled);
                Debug.Log($"<color=cyan>Grid {(enabled ? "ENABLED" : "DISABLED")}</color>");
            }
        }

        public void SetMode(HarbourStateSO.HarbourMode newMode)
        {
            if (state != null)
                state.currentMode = newMode;

            ApplyHarbourState();
        }

        public void EnterHarbourBuildMode() => SetMode(HarbourStateSO.HarbourMode.HarbourBuild);
        public void ReturnToIdle() => SetMode(HarbourStateSO.HarbourMode.Idle);

        public void ResetToDefaultState()
        {
            SetMode(HarbourStateSO.HarbourMode.Idle);
        }
    }
}