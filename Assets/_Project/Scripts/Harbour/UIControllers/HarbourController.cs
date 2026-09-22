using _Project.Scripts.BaseBuilder.Runtime.Core;
using _Project.Scripts.BaseBuilder.UI;
using _Project.Scripts.Harbour.Data;
using _Project.Scripts.Harbour.Data.HUDData;
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

        [SerializeField] private BaseBuilderHUD baseBuilderHUD;
        [SerializeField] private HarbourHUD harbourHUD;
        [SerializeField] private HarbourBuilderManager harbourBuilderManager;
        [SerializeField] private ShipBuilderHUD shipBuilderHUD;
        [SerializeField] private DockHUD dockHUD;
        [SerializeField] private ResourceHUD resourceHUD;

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

            if (dockHUD == null)
                dockHUD = GetComponent<DockHUD>();

            if (harbourHUD != null)
            {
                harbourHUD.OnBuildHarbourBaseClicked += () => SetMode(HarbourStateSO.HarbourMode.HarbourBuild);
                harbourHUD.OnBuildShipClicked += () => SetMode(HarbourStateSO.HarbourMode.ShipBuilding);
                harbourHUD.OnExitBuildMode += () => SetMode(HarbourStateSO.HarbourMode.Idle);
                harbourHUD.OnDockClicked += ToggleDockMode;
                //harbourHUD.OnResourcesClicked += () => SetMode(HarbourStateSO.HarbourMode.Resources);
                if (resourceHUD != null)
                    resourceHUD.OnCloseRequested += ReturnToIdle;
            }

            if (dockHUD != null)
                dockHUD.OnCloseRequested += ReturnToIdle;

            if (baseBuilderHUD != null)
            {
                baseBuilderHUD.OnExitClicked += () =>
                {
                    Debug.Log("<color=cyan>Exit Build Mode clicked</color>");
                    SetMode(HarbourStateSO.HarbourMode.Idle);
                };
            }

            ApplyHarbourState();
        }
        
        private void ToggleDockMode()
        {
            SetMode(HarbourStateSO.HarbourMode.Dock);
        }

        private void ApplyHarbourState()
        {
            if (playerBuildGO != null)
                playerBuildGO.SetActive(false);

            switch (state.currentMode)
            {
                case HarbourStateSO.HarbourMode.Idle:
                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);

                    if (harbourHUD != null)
                        harbourHUD.gameObject.SetActive(true);

                    if (baseBuilderHUD != null)
                        baseBuilderHUD.gameObject.SetActive(false);

                    if (baseBuilderController != null)
                        baseBuilderController.SetBuilderActive(false);
                    shipBuilderHUD?.CloseShipBuilder();
                    dockHUD?.CloseDock();
                    harbourHUD?.RefreshUI(HarbourStateSO.HarbourMode.Idle);
                    break;

                case HarbourStateSO.HarbourMode.HarbourBuild:
                    SetCamera(idleCamera, false);
                    SetCamera(harbourBuildCamera, true);

                    if (harbourHUD != null)
                        harbourHUD.gameObject.SetActive(false);

                    if (baseBuilderHUD != null)
                        baseBuilderHUD.gameObject.SetActive(true);

                    if (baseBuilderController != null)
                    {
                        baseBuilderController.SetGrids(landGrid, objectGrid);
                        baseBuilderController.SetBuilderActive(true);
                    }

                    if (playerBuildGO != null)
                        playerBuildGO.SetActive(true);

                    shipBuilderHUD?.CloseShipBuilder();
                    dockHUD?.CloseDock();
                    break;

                case HarbourStateSO.HarbourMode.ShipBuilding:
                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);

                    if (harbourHUD != null)
                        harbourHUD.gameObject.SetActive(true);

                    if (baseBuilderHUD != null)
                        baseBuilderHUD.gameObject.SetActive(false);

                    if (baseBuilderController != null)
                        baseBuilderController.SetBuilderActive(false);
                    SetTGSGrid(false);
                    dockHUD?.CloseDock();
                    harbourHUD?.RefreshUI(HarbourStateSO.HarbourMode.ShipBuilding);
                    break;

                case HarbourStateSO.HarbourMode.Dock:
                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);

                    if (harbourHUD != null)
                        harbourHUD.gameObject.SetActive(true);

                    if (baseBuilderHUD != null)
                        baseBuilderHUD.gameObject.SetActive(false);

                    if (baseBuilderController != null)
                        baseBuilderController.SetBuilderActive(false);
                    shipBuilderHUD?.CloseShipBuilder();
                    harbourHUD?.RefreshUI(HarbourStateSO.HarbourMode.Dock);
                    break;
                
                // case HarbourStateSO.HarbourMode.Resources:
                //     SetCamera(idleCamera, true);
                //     SetCamera(harbourBuildCamera, false);
                //     if (harbourHUD != null) harbourHUD.gameObject.SetActive(true);
                //     if (baseBuilderHUD != null) baseBuilderHUD.gameObject.SetActive(false);
                //     baseBuilderController?.SetBuilderActive(false);
                //     shipBuilderHUD?.CloseShipBuilder();
                //     dockHUD?.CloseDock();
                //     harbourHUD?.RefreshUI(HarbourStateSO.HarbourMode.Resources);
                //     break;
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