using _Project.Scripts.BaseBuilder.Runtime.Data;
using _Project.Scripts.BaseBuilder.Runtime.Modes;
using _Project.Scripts.BaseBuilder.Runtime.Placement;
using _Project.Scripts.BaseBuilder.Runtime.Selection;
using _Project.Scripts.Harbour.Data.SO;
using Packages.ModularStrategyTopDownCameraController.Scripts;
using TGS;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.BaseBuilder.Runtime.Core
{
    public class BaseBuilderController : MonoBehaviour
    {
        #region Inspector Fields
        
        

        [Header("Grid References")]
        [SerializeField] private TerrainGridSystem landGrid;
        [SerializeField] private TerrainGridSystem objectGrid;
        
        [Header("Highlight References")]
        private CellHighlightService _highlightService;
        private PaintAndDragSystem _paintAndDrag;

        [Header("Placement")]
        public GameObject selectedPrefab;
        public Vector2Int selectedSize = Vector2Int.one;
        public bool isLandObject = true;
        public int selectedInventoryIndex = -1;

        [SerializeField] private LandTileInventorySO landTileInventory;
        [SerializeField] private WallInventorySO wallInventory;

        [Header("Input")]
        [SerializeField] private Camera buildCamera;
        [SerializeField] private StrategyCameraController strategyCamera;
        
        private BuilderInputActions inputActions;

        #endregion

        #region Systems

        public BuilderModeSystem ModeSystem { get; private set; }
        public OccupationSystem OccupationSystem { get; private set; }
        public PlacementValidator Validator { get; private set; }
        public PlacementService PlacementService { get; private set; }
        public SelectionSystem SelectionSystem { get; private set; }

        public BuilderMode CurrentMode => ModeSystem.CurrentMode;
        public SelectFilter CurrentSelectFilter => ModeSystem.CurrentSelectFilter;

        public event System.Action<int> OnLandTilePlaced;

        #endregion

        #region Private State

        private GameObject currentPreview;
        private Cell currentHoveredCell;
        private int lastHighlightedCell = -1;
        private TerrainGridSystem lastHighlightedGrid;
        private bool isPointerOverUI = false;
        private BuilderInputActions builderInputActions;

        private TerrainGridSystem ActiveGrid
        {
            get
            {
                if (ModeSystem.IsBuildOnWater) return landGrid;
                if (ModeSystem.IsBuildOnLand) return objectGrid;
                return null;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ModeSystem = new BuilderModeSystem();
            OccupationSystem = new OccupationSystem();
            Validator = new PlacementValidator(OccupationSystem);
            PlacementService = new PlacementService(OccupationSystem, Validator);
            SelectionSystem = new SelectionSystem();

            // Input maps + strategy camera (prevent pan/zoom while placing)
            builderInputActions = new BuilderInputActions();
            if (strategyCamera == null)
                strategyCamera = FindFirstObjectByType<StrategyCameraController>();

            // New systems
            _highlightService = new CellHighlightService();
            _paintAndDrag = new PaintAndDragSystem(
                _highlightService,
                Validator,
                PlacementService,
                () => selectedPrefab,
                () => selectedSize,
                () => isLandObject,
                () => selectedInventoryIndex,
                (index) => landTileInventory != null ? landTileInventory.GetCount(index) : 0,  // ← NEW
                () => ActiveGrid
            );

            _paintAndDrag.OnLandTilePlaced += (index) => OnLandTilePlaced?.Invoke(index);
            
            _paintAndDrag.OnWallPlaced += (index) =>
            {
                if (wallInventory != null)
                    wallInventory.Consume(index);
            };
            
            inputActions = new BuilderInputActions();
            inputActions.Player.Enable();
            inputActions.Builder.Disable();

            if (strategyCamera == null)
                strategyCamera = FindFirstObjectByType<StrategyCameraController>();
            
            ModeSystem.OnModeChanged += (mode) =>
            {
                UpdateGridVisibility();
                ClearSelectedPrefab();

                // Lock strategy camera while in placement modes
                bool inPlacementMode = mode == BuilderMode.BuildOnWater || mode == BuilderMode.BuildOnLand;
                bool isBuilding = mode == BuilderMode.BuildOnWater || mode == BuilderMode.BuildOnLand;
                SetBuilderInputActive(isBuilding);
                SetBuilderInputActive(inPlacementMode);
            };

            if (landGrid != null || objectGrid != null)
                PlacementService.SetGrids(landGrid, objectGrid);
        }

        private void OnDestroy()
        {
            // Restore player/camera input if this object is destroyed mid-build
            SetBuilderInputActive(false);
            builderInputActions?.Dispose();
            builderInputActions = null;
        }

        private void Update()
        {
            _paintAndDrag.Tick();
            
            if (!ModeSystem.IsBuildOnWater && !ModeSystem.IsBuildOnLand)
            {
                currentHoveredCell = null;
                return;
            }

            UpdateHoveredCell();
            HandlePlacementInput();
        }

        #endregion

        #region Public API

        public void SetGrids(TerrainGridSystem land, TerrainGridSystem objects)
        {
            landGrid = land;
            objectGrid = objects;
            PlacementService.SetGrids(land, objects);
        }

        public void SetMode(BuilderMode mode) => ModeSystem.SetMode(mode);
        public void SetSelectFilter(SelectFilter filter) => ModeSystem.SetSelectFilter(filter);

        public void SetPointerOverUI(bool over)
        {
            isPointerOverUI = over;
        }

        /// <summary>
        /// Switches Input Action maps and strategy-camera input.
        /// active true  → Builder map on, Player map off, camera inputs disabled.
        /// active false → Player map on, Builder map off, camera inputs enabled.
        /// </summary>
        private void SetBuilderInputActive(bool active)
        {
            Debug.Log($"[Input] SetBuilderInputActive({active}) | strategyCamera={(strategyCamera != null)}");

            if (inputActions != null)
            {
                if (active)
                {
                    inputActions.Player.Disable();
                    inputActions.Builder.Enable();
                }
                else
                {
                    inputActions.Builder.Disable();
                    inputActions.Player.Enable();
                }
            }

            if (strategyCamera != null)
            {
                if (active)
                    strategyCamera.xinputs.DisableInputs();
                else
                    strategyCamera.xinputs.EnableInputs();
            }
        }

        public void SetBuilderActive(bool active)
        {
            gameObject.SetActive(active);

            if (!active)
            {
                if (landGrid != null) landGrid.gameObject.SetActive(false);
                if (objectGrid != null) objectGrid.gameObject.SetActive(false);
                ClearSelectedPrefab();
                isPointerOverUI = false;
                SetBuilderInputActive(false);
                return;
            }

            ModeSystem.SetMode(BuilderMode.Select);
            UpdateGridVisibility();
            SetBuilderInputActive(true);
        }

        public void UpdateGridVisibility()
        {
            bool showLand = ModeSystem.IsBuildOnWater;
            bool showObject = ModeSystem.IsBuildOnLand;

            if (landGrid != null)
                landGrid.gameObject.SetActive(showLand);

            if (objectGrid != null)
                objectGrid.gameObject.SetActive(showObject);

            Debug.Log($"<color=cyan>Grid Visibility → Land: {showLand} | Object: {showObject} | Mode: {ModeSystem.CurrentMode}</color>");
        }

        #endregion

        #region Selection & Preview

        public void SelectPrefab(GameObject prefab, Vector2Int size, bool isLand, int inventoryIndex = -1)
        {
            DestroyPreview();

            selectedPrefab = prefab;
            selectedSize = size;
            isLandObject = isLand;
            selectedInventoryIndex = inventoryIndex;
            
            Debug.Log($"[SelectPrefab] Set selectedInventoryIndex = {selectedInventoryIndex}");

            if (prefab != null)
            {
                currentPreview = Instantiate(prefab);
                currentPreview.name = "PlacementPreview";
                SetPreviewStyle(currentPreview);
                Debug.Log($"<color=cyan>Selected: {prefab.name} (index {inventoryIndex})</color>");
            }
        }

        public void ClearSelectedPrefab()
        {
            selectedPrefab = null;
            selectedInventoryIndex = -1;
            DestroyPreview();
        }

        private void DestroyPreview()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }
        }

        private void SetPreviewStyle(GameObject preview)
        {
            foreach (var col in preview.GetComponentsInChildren<Collider>())
                col.enabled = false;

            foreach (var r in preview.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = 0.5f;
                        mat.color = c;
                    }
                }
            }
        }

        #endregion
        
        #region Helper's
        
        private void CheckInventoryAndClearIfEmpty()
        {
            if (selectedInventoryIndex < 0) return;

            int remaining = 0;

            if (isLandObject)
            {
                if (landTileInventory == null) return;
                remaining = landTileInventory.GetCount(selectedInventoryIndex);
            }
            else
            {
                // Walls
                // We need access to the WallInventorySO – see note below
                if (wallInventory == null) return;
                remaining = wallInventory.GetCount(selectedInventoryIndex);
            }

            if (remaining <= 0)
            {
                ClearSelectedPrefab();
                Debug.Log("<color=orange>Inventory empty – preview cleared</color>");
            }
        }
        
        #endregion

        #region Hover & Highlight

        private void UpdateHoveredCell()
        {
            ClearHighlight();

            // Don't highlight / move preview while over UI
            if (isPointerOverUI)
            {
                if (currentPreview != null)
                    currentPreview.SetActive(false);
                return;
            }

            Camera cam = buildCamera != null ? buildCamera : Camera.main;
            if (cam == null || ActiveGrid == null) return;
            if (Mouse.current == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            float gridY = ActiveGrid.transform.position.y;

            Ray ray = cam.ScreenPointToRay(mousePos);
            Plane gridPlane = new Plane(Vector3.up, new Vector3(0f, gridY, 0f));

            if (!gridPlane.Raycast(ray, out float enter)) return;

            Vector3 hitPoint = ray.GetPoint(enter);
            currentHoveredCell = ActiveGrid.CellGetAtWorldPosition(hitPoint, 0);
            if (currentHoveredCell == null) return;

            if (currentPreview != null)
            {
                Vector3 pos = ActiveGrid.CellGetPosition(currentHoveredCell.index);
                float cellSize = ActiveGrid.cellSize.x;
                pos.x += (selectedSize.x - 1) * cellSize * 0.5f;
                pos.z += (selectedSize.y - 1) * cellSize * 0.5f;
                pos.y += 0.1f;

                currentPreview.transform.position = pos;
                currentPreview.SetActive(true);
            }

            ActiveGrid.CellSetColor(currentHoveredCell.index, new Color(0.2f, 0.9f, 0.3f, 0.55f));
            lastHighlightedCell = currentHoveredCell.index;
            lastHighlightedGrid = ActiveGrid;
        }

        private void ClearHighlight()
        {
            if (lastHighlightedGrid != null && lastHighlightedCell >= 0)
            {
                lastHighlightedGrid.CellSetColor(lastHighlightedCell, Color.clear);
                lastHighlightedCell = -1;
                lastHighlightedGrid = null;
            }
        }

        #endregion

        #region Placement

        private void HandlePlacementInput()
        {
            // Prevent normal single-click placement while PaintAndDrag is active
            if (_paintAndDrag != null && _paintAndDrag.IsBusy)
                return;
            
            if (selectedPrefab == null || currentHoveredCell == null) return;
            if (Mouse.current == null) return;

            // Block placement while pointer is over UI
            if (isPointerOverUI)
                return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
                TryPlaceSelected();
        }

        private void TryPlaceSelected()
        {
            if (selectedPrefab == null || currentHoveredCell == null || ActiveGrid == null)
                return;

            // Block placement when inventory is empty
            if (isLandObject && selectedInventoryIndex >= 0 && landTileInventory != null)
            {
                if (landTileInventory.GetCount(selectedInventoryIndex) <= 0)
                {
                    Debug.LogWarning("No tiles left in inventory for this slot");
                    return;
                }
            }

            Vector3 placePos = ActiveGrid.CellGetPosition(currentHoveredCell.index);

            GameObject placed = PlacementService.Place(
                selectedPrefab,
                placePos,
                selectedSize,
                isLandObject,
                selectedInventoryIndex
            );

            if (placed == null)
            {
                Debug.LogWarning("Placement failed – cell occupied or invalid");
                return;
            }

            if (isLandObject && selectedInventoryIndex >= 0)
            {
                OnLandTilePlaced?.Invoke(selectedInventoryIndex);
            }
            else if (!isLandObject && selectedInventoryIndex >= 0 && wallInventory != null)
            {
                // Consume wall
                bool consumed = wallInventory.Consume(selectedInventoryIndex);
                if (!consumed)
                    Debug.LogWarning($"Could not consume wall at index {selectedInventoryIndex}");
            }

            CheckInventoryAndClearIfEmpty();

            Debug.Log($"<color=green>Placed {selectedPrefab.name} at cell {currentHoveredCell.index}</color>");
        }

        #endregion
    }
}          