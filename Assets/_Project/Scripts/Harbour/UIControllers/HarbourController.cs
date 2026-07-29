using _Project.Scripts.BaseBuilder.Runtime.Core;
using _Project.Scripts.Harbour.Data;
using _Project.Scripts.Harbour.Data.SO;
using _Project.Scripts.Harbour.ShipBuilder;
using TGS;
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
        [SerializeField] private GameObject tgsGridObject;   // old single grid (kept for now)

        [SerializeField] private HarbourHUD harbourHUD;
        [SerializeField] private HarbourBuilderManager harbourBuilderManager;
        [SerializeField] private ShipBuilderHUD shipBuilderHUD;

        #region Base Builder
        [Header("Base Builder")]
        [SerializeField] private BaseBuilderController baseBuilderController;
        [SerializeField] private TerrainGridSystem landGrid;
        [SerializeField] private TerrainGridSystem objectGrid;
        #endregion

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

            if (shipBuilderHUD == null)
                shipBuilderHUD = GetComponent<ShipBuilderHUD>();

            // === EVENT WIRING ===
            if (harbourHUD != null)
            {
                harbourHUD.OnBuildHarbourBaseClicked += () => SetMode(HarbourStateSO.HarbourMode.HarbourBuild);
                harbourHUD.OnBuildShipClicked += () => SetMode(HarbourStateSO.HarbourMode.ShipBuilding);
                harbourHUD.OnExitBuildMode += () => SetMode(HarbourStateSO.HarbourMode.Idle);

                Debug.Log("<color=lime>✅ All events wired (including Build a Ship)</color>");
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
                    if (harbourHUD != null)
                        harbourHUD.RefreshUI(HarbourStateSO.HarbourMode.Idle);

                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);

                    // Deactivate Base Builder + hide both grids
                    baseBuilderController?.SetBuilderActive(false);

                    // Turn off old single grid
                    SetTGSGrid(false);

                    shipBuilderHUD?.CloseShipBuilder();
                    break;

                case HarbourStateSO.HarbourMode.HarbourBuild:
                    if (harbourHUD != null)
                        harbourHUD.RefreshUI(HarbourStateSO.HarbourMode.HarbourBuild);

                    SetCamera(idleCamera, false);
                    SetCamera(harbourBuildCamera, true);

                    // Turn off old single grid
                    SetTGSGrid(false);

                    // Activate Base Builder (starts in Select mode → both grids off)
                    if (baseBuilderController != null)
                    {
                        baseBuilderController.SetGrids(landGrid, objectGrid);
                        baseBuilderController.SetBuilderActive(true);
                    }

                    if (playerBuildGO != null)
                        playerBuildGO.SetActive(true);

                    shipBuilderHUD?.CloseShipBuilder();
                    break;

                case HarbourStateSO.HarbourMode.ShipBuilding:
                    if (harbourHUD != null)
                        harbourHUD.RefreshUI(HarbourStateSO.HarbourMode.ShipBuilding);

                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);

                    // Deactivate Base Builder
                    baseBuilderController?.SetBuilderActive(false);

                    SetTGSGrid(false);
                    break;
            }

            Debug.Log($"<color=lime>Harbour State Applied: {state.currentMode}</color>");
        }

        private void SetCamera(Camera cam, bool active)
        {
            if (cam != null)
                cam.gameObject.SetActive(active);
        }

        private void SetTGSGrid(bool enabled)
        {
            if (tgsGridObject != null)
            {
                tgsGridObject.SetActive(enabled);
                Debug.Log($"<color=cyan>Old TGS Grid {(enabled ? "ENABLED" : "DISABLED")}</color>");
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