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
                ClearSelectedPrefab();

                if (mode == BuilderMode.BuildOnWater || mode == BuilderMode.BuildOnLand)
                {
                    UpdateGridVisibility();
                    SetBuilderInputActive(true);
                }
                else if (mode == BuilderMode.PickUp || mode == BuilderMode.Delete)
                {
                    // Keep grids as they are – do not call UpdateGridVisibility()
                    SetBuilderInputActive(false); // or true if you still want camera locked
                }
                else // Select, etc.
                {
                    UpdateGridVisibility(); // hides both when not building
                    SetBuilderInputActive(false);
                }
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
            _paintAndDrag?.Tick();

            if (ModeSystem.CurrentMode == BuilderMode.PickUp)
            {
                HandlePickUpInput();
                return;
            }

            if (ModeSystem.CurrentMode == BuilderMode.Delete)
            {
                HandleDeleteInput();
                return;
            }

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
            selectedSize = Vector2Int.one;
            isLandObject = true;
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
        
        private void HandlePickUpInput()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;
            if (isPointerOverUI) return;

            Camera cam = buildCamera != null ? buildCamera : Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            int layer = LayerMask.GetMask("PlacedObjects");
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, layer))
                return;

            GameObject target = hit.collider.transform.root.gameObject;

            if (!PlacementService.TryPickUp(target, out var info))
                return;

            // Back into inventory count (will consume again on place)
            RestoreInventory(
                info.IsLandObject ? PlaceableKind.Land : PlaceableKind.Wall,
                info.InventoryIndex
            );

            // Same as clicking an inventory slot → sticks to mouse
            if (info.Prefab != null)
                SelectPrefab(info.Prefab, info.Size, info.IsLandObject, info.InventoryIndex);

            Object.Destroy(info.Instance);

            // Critical: leave PickUp mode so hover + place logic runs
            if (info.IsLandObject)
                ModeSystem.SetMode(BuilderMode.BuildOnWater);
            else
                ModeSystem.SetMode(BuilderMode.BuildOnLand);

            Debug.Log($"<color=cyan>Picked up – now placing {(info.IsLandObject ? "Land" : "Wall")}</color>");
        }
        
        private void HandleDeleteInput()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;
            if (isPointerOverUI) return;

            Camera cam = buildCamera != null ? buildCamera : Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            int layer = LayerMask.GetMask("PlacedObjects");
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, layer))
                return;

            GameObject target = hit.collider.transform.root.gameObject;

            if (!PlacementService.TryPickUp(target, out var info))
                return;
            

            RestoreInventory(info.IsLandObject ? PlaceableKind.Land : PlaceableKind.Wall, info.InventoryIndex);

            Object.Destroy(info.Instance);

            Debug.Log($"<color=orange>Deleted {(info.IsLandObject ? "Land" : "Wall/Building")} index {info.InventoryIndex}</color>");
        }
        
        // private void RestoreInventory(bool isLand, bool isWall, bool isBuilding, bool isWater, int inventoryIndex)
        // {
        //
        //     if (inventoryIndex < 0) return;
        //     //-------Current Logic Used for Land and Wall Inventory Restoration-------
        //     if (isLand && landTileInventory != null)
        //         landTileInventory.Restore(inventoryIndex);
        //     else if (!isLand && wallInventory != null)
        //         wallInventory.Restore(inventoryIndex);
        //     
        //     // ----- New Logic to Restore Inventory for Land, Wall, Building, and Water Objects-----
        //     // if (isLand && landTileInventory != null)
        //     //     landTileInventory.Restore(inventoryIndex);
        //     // if (isWall && wallInventory != null)
        //     //     wallInventory.Restore(inventoryIndex);
        //     // if (isBuilding && buildingInventory != null)
        //     //     buildingInventory.Restore(inventoryIndex);
        //     // if (isWater && waterInventory != null)
        //     //     waterInventory.Restore(inventoryIndex);
        // }
        public enum PlaceableKind
        {
            Land,
            Wall,
            Building,
            Water
        }

        private void RestoreInventory(PlaceableKind kind, int inventoryIndex)
        {
            if (inventoryIndex < 0) return;

            switch (kind)
            {
                case PlaceableKind.Land:
                    if (landTileInventory != null)
                        landTileInventory.Restore(inventoryIndex);
                    break;

                case PlaceableKind.Wall:
                    if (wallInventory != null)
                        wallInventory.Restore(inventoryIndex);
                    break;

                // case PlaceableKind.Building:
                //     if (buildingInventory != null)
                //         buildingInventory.Restore(inventoryIndex);
                //     break;
                //
                // case PlaceableKind.Water:
                //     if (waterInventory != null)
                //         waterInventory.Restore(inventoryIndex);
                //     break;
            }
        }
        
        
        
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