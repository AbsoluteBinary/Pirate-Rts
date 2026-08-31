using System;
using System.Collections.Generic;
using _Project.Scripts.BaseBuilder.Runtime.Data;
using _Project.Scripts.BaseBuilder.Runtime.Inventory;
using _Project.Scripts.BaseBuilder.Runtime.Modes;
using _Project.Scripts.BaseBuilder.Runtime.Placement;
using _Project.Scripts.BaseBuilder.Runtime.Presentation;
using _Project.Scripts.BaseBuilder.Runtime.Selection;
using _Project.Scripts.BaseBuilder.UI;
using _Project.Scripts.Harbour.Data.SO;
using Packages.ModularStrategyTopDownCameraController.Scripts;
using TGS;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace _Project.Scripts.BaseBuilder.Runtime.Core
{
    public class BaseBuilderController : MonoBehaviour
    {
        #region Inspector Fields
        
        private int _placeYawSteps;

        [Header("Grid References")]
        [SerializeField] private TerrainGridSystem landGrid;
        [SerializeField] private TerrainGridSystem objectGrid;
        
        [Header("Highlight References")]
        private CellHighlightService _highlightService;
        private PaintAndDragSystem _paintAndDrag;
        
        [Header("Camera")]
        [SerializeField] private StrategyCameraController cameraController;

        [Header("Placement")]
        public GameObject selectedPrefab;
        public Vector2Int selectedSize = Vector2Int.one;
        public bool isLandObject = true;
        public int selectedInventoryIndex = -1;
        
        private BuilderInventoryFacade _inventory;
        private bool _clearSelectionAfterPlace;
        private RectangleSelectSystem _rectSelect;
        [SerializeField] private BaseBuilderHUD hud;
        public readonly List<StoredBuilding> BuildingsStorage = new List<StoredBuilding>();

        [SerializeField] private LandTileInventorySO landTileInventory;
        [SerializeField] private WallInventorySO wallInventory;
        [SerializeField] private BuildingsInventorySO  buildingsInventory;
        [SerializeField] private TurretInventorySO turretInventory;
        
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
        
        public PlaceableKind selectedKind = PlaceableKind.Land;

        public event System.Action<int> OnLandTilePlaced;
        public event Action<int> OnTurretPlaced;
        

        #endregion

        #region Private State

        private GameObject currentPreview;
        private Cell currentHoveredCell;
        private int lastHighlightedCell = -1;
        private TerrainGridSystem lastHighlightedGrid;
        private bool isPointerOverUI = false;
        private BuilderInputActions builderInputActions;
        private WallRowHoldSystem _wallRowHold;

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
            _rectSelect = new RectangleSelectSystem(() => isPointerOverUI);
            _rectSelect.OnDragStarted += () => SelectionSystem?.Clear();
            
            _inventory = new BuilderInventoryFacade(landTileInventory, wallInventory, buildingsInventory, turretInventory);
            _rectSelect = new RectangleSelectSystem();
            
            ModeSystem = new BuilderModeSystem();
            OccupationSystem = new OccupationSystem();
            Validator = new PlacementValidator(OccupationSystem);
            PlacementService = new PlacementService(OccupationSystem, Validator);
            SelectionSystem = new SelectionSystem();
            
            SelectionSystem.OnSelectionChanged += RefreshSelectionHighlights;

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
            
            _rectSelect.OnDragUpdated += HandleRectSelectUpdated;
            _rectSelect.OnDragEnded   += HandleRectSelectEnded;

            _paintAndDrag.OnLandTilePlaced += (index) =>
                _inventory.Consume(PlaceableKind.Land, index);

            _paintAndDrag.OnWallPlaced += (index) =>
                _inventory.Consume(PlaceableKind.Wall, index);
            
            inputActions = new BuilderInputActions();
            inputActions.Player.Enable();
            inputActions.Builder.Disable();
            
            inputActions.Builder.RotateHeld.performed += OnRotateHeld;

            if (strategyCamera == null)
                strategyCamera = FindFirstObjectByType<StrategyCameraController>();
            
            ModeSystem.OnModeChanged += (mode) =>
            {
                ClearSelectedPrefab();
                _rectSelect?.Cancel();
                hud?.HideSelectRect();

                if (mode == BuilderMode.BuildOnWater)
                {
                    UpdateGridVisibility();
                    SetBuilderInputActive(true);
                    SetCameraInputEnabled(true);
                    _wallRowHold.DisableInput();
                }
                else if (mode == BuilderMode.BuildOnLand)
                {
                    UpdateGridVisibility();
                    SetBuilderInputActive(true);
                    SetCameraInputEnabled(true);
                    _wallRowHold.EnableInput();
                }
                else if (mode == BuilderMode.Select)
                {
                    SetBuilderInputActive(false);
                    SetCameraInputEnabled(false);
                }
                else if (mode == BuilderMode.Observation)
                {
                    if (landGrid != null) landGrid.gameObject.SetActive(false);
                    if (objectGrid != null) objectGrid.gameObject.SetActive(false);
                    SetBuilderInputActive(false);
                    SetCameraInputEnabled(true);
                }
                else
                {
                    SetBuilderInputActive(false);
                    SetCameraInputEnabled(false);
                }
            };
            
            _wallRowHold = new WallRowHoldSystem();
            _wallRowHold.Bind(
                PlacementService,
                Validator,
                inputActions,
                buildCamera,
                () => selectedPrefab != null
            );
            _wallRowHold.SetGrids(landGrid, objectGrid);

            if (landGrid != null || objectGrid != null)
                PlacementService.SetGrids(landGrid, objectGrid);
        }
        
        

        private void OnDestroy()
        {
            // Restore player/camera input if this object is destroyed mid-build
            SetBuilderInputActive(false);
            builderInputActions?.Dispose();
            builderInputActions = null;
            
            if (inputActions != null)
                inputActions.Builder.RotateHeld.performed -= OnRotateHeld;
            if (SelectionSystem != null)
                SelectionSystem.OnSelectionChanged -= RefreshSelectionHighlights;
        }

        private void Update()
        {
            if (ModeSystem.CurrentMode == BuilderMode.Select)
            {
                _rectSelect.Tick();
            }
            
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
            
            if (ModeSystem.CurrentMode == BuilderMode.Observation)
            {
                currentHoveredCell = null;
                return;
            }

            if (ModeSystem.CurrentMode == BuilderMode.Lock)
            {
                HandleLockInput(true);
                return;
            }

            if (ModeSystem.CurrentMode == BuilderMode.Unlock)
            {
                HandleLockInput(false);
                return;
            }
            if (ModeSystem.CurrentMode == BuilderMode.Store)
            {
                HandleStoreInput();
                return;
            }

            if (!ModeSystem.IsBuildOnWater && !ModeSystem.IsBuildOnLand)
            {
                currentHoveredCell = null;
                return;
            }

            if (ModeSystem.IsBuildOnLand && CurrentMode == BuilderMode.BuildOnLand && _wallRowHold != null)
                _wallRowHold.Tick();

            // do NOT return here — fall through to cell hover + preview follow
            

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
        
        public void ToggleStoreMode()
        {
            ClearSelectedPrefab();
            if (ModeSystem.CurrentMode == BuilderMode.Store)
                SetMode(BuilderMode.Select);
            else
                SetMode(BuilderMode.Store);
        }

        public void SetMode(BuilderMode mode)
        {
            if (mode == BuilderMode.Select && !IsAnyGridActive())
            {
                Debug.LogWarning("Select requires an active grid (Build on Water or Build on Land first)");
                return;
            }
            ModeSystem.SetMode(mode);
        }
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
            _rectSelect?.Cancel();
            hud?.HideSelectRect();
            gameObject.SetActive(active);
            SetCameraInputEnabled(true);

            if (!active)
            {
                if (landGrid != null) landGrid.gameObject.SetActive(false);
                if (objectGrid != null) objectGrid.gameObject.SetActive(false);
                ClearSelectedPrefab();
                isPointerOverUI = false;
                SetBuilderInputActive(false);
                return;
            }

            ModeSystem.SetMode(BuilderMode.Observation);
            UpdateGridVisibility();
            SetBuilderInputActive(false);
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

        public void SelectPrefab(GameObject prefab, Vector2Int size, bool isLand, int inventoryIndex = -1, PlaceableKind kind = PlaceableKind.Land)
        {
            _clearSelectionAfterPlace = false;
            DestroyPreview();

            selectedPrefab = prefab;
            selectedKind = kind;
            selectedSize = size;
            isLandObject = isLand;
            selectedInventoryIndex = inventoryIndex;
            
            _placeYawSteps = 0;
            
            Debug.Log($"[SelectPrefab] Set selectedInventoryIndex = {selectedInventoryIndex}");

            if (prefab != null)
            {
                currentPreview = Instantiate(prefab);
                currentPreview.name = "PlacementPreview";
                SetPreviewStyle(currentPreview);
                ApplyPreviewRotation();
                RefreshPlacementHighlights();
                
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
        
        private void HandleRectSelectUpdated()
        {
            if (hud == null) return;
            hud.ShowSelectRect(_rectSelect.GetScreenRect());
        }

        private void HandleRectSelectEnded()
        {
            if (hud == null) return;
            hud.HideSelectRect();

            if (_rectSelect == null || ModeSystem.CurrentMode != BuilderMode.Select)
                return;

            Rect rect = _rectSelect.GetScreenRect();
            const float MinSize = 4f;
            if (rect.width < MinSize && rect.height < MinSize)
                return;

            Camera cam = buildCamera != null ? buildCamera : Camera.main;
            if (cam == null || PlacementService == null || SelectionSystem == null)
                return;

            var hits = new List<GameObject>();
            var entries = PlacementService.GetPlacedEntries();

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                // Buildings are never marquee-selectable (static structures)
                if (entry.kind == PlaceableKind.Building)
                    continue;

                if (!PassesSelectFilter(entry)) continue;
                if (!IsWorldPointInScreenRect(cam, entry.instance.transform.position, rect))
                    continue;
                hits.Add(entry.instance);
            }

            SelectionSystem.SetSelection(hits);
            

            Debug.Log($"[RectSelect] Selected {hits.Count} object(s) | filter={ModeSystem.CurrentSelectFilter}");
        }

        #endregion
        
        #region Helper's
        
        [Obsolete("Obsolete")]
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
            Debug.Log($"[PickUp] target={target.name} id={target.GetInstanceID()}");

            if (PlacementService.IsLocked(target))
            {
                Debug.LogWarning("[PickUp] BLOCKED – locked");
                return;
            }

            Debug.Log("[PickUp] not locked – continuing");

            if (!PlacementService.TryPickUp(target, out var info))
                return;
            
            RestoreInventory(info.Kind, info.InventoryIndex);

            Object.Destroy(info.Instance);

            if (info.Kind == PlaceableKind.Land)
                ModeSystem.SetMode(BuilderMode.BuildOnWater);
            else
                ModeSystem.SetMode(BuilderMode.BuildOnLand);

            if (info.Prefab != null)
                SelectPrefab(info.Prefab, info.Size, info.IsLandObject, info.InventoryIndex, info.Kind);
            
            if (info.Prefab != null)
                SelectPrefab(info.Prefab, info.Size, info.IsLandObject, info.InventoryIndex, info.Kind);

            _clearSelectionAfterPlace = true;
            
            Debug.Log($"<color=cyan>Picked up – now placing {(info.IsLandObject ? "Land" : "Wall")}</color>");
        }
        
        private void HandleStoreInput()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;
            if (isPointerOverUI) return;

            Camera cam = buildCamera != null ? buildCamera : Camera.main;
            if (cam == null || PlacementService == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            int layer = LayerMask.GetMask("PlacedObjects");
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, layer))
                return;

            GameObject target = hit.collider.transform.root.gameObject;

            if (PlacementService.IsLocked(target))
            {
                Debug.LogWarning("[Store] Blocked – locked");
                return;
            }

            if (!IsPlacedBuilding(target))
            {
                Debug.LogWarning("[Store] Not a building");
                return;
            }

            if (!PlacementService.TryPickUp(target, out var info))
                return;

            // Do NOT RestoreInventory — SO count stays unchanged
            BuildingsStorage.Add(new StoredBuilding(info.Kind, 1, info.Size));

            if (info.Instance != null)
                Object.Destroy(info.Instance);

            Debug.Log($"[Store] Stored {info.Kind} Lv1 size={info.Size} | storage={BuildingsStorage.Count}");
        }
        
        private void HandleDeleteInput()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;
            if (isPointerOverUI) return;

            // ── Multi-delete from marquee selection ──
            if (SelectionSystem != null && SelectionSystem.SelectedObjects.Count > 0)
            {
                DeleteSelectedObjects();
                return;
            }

            // ── Fallback: single-click raycast ──
            Camera cam = buildCamera != null ? buildCamera : Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            int layer = LayerMask.GetMask("PlacedObjects");
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, layer))
                return;

            GameObject target = hit.collider.transform.root.gameObject;
            TryDeleteOne(target);
        }
        
        public void DeleteSelectedOrEnterMode()
        {
            ClearSelectedPrefab();

            if (SelectionSystem != null && SelectionSystem.SelectedObjects.Count > 0)
            {
                DeleteSelectedObjects(); // highlights off here
                return;
            }

            SetMode(BuilderMode.Delete); // no selection → single-click delete mode
        }
        
        /// <summary>
        /// If there is a selection, lock/unlock all of them now.
        /// Otherwise enter Lock or Unlock mode for single-click.
        /// </summary>
        public void LockSelectedOrEnterMode(bool lockIt)
        {
            ClearSelectedPrefab();

            if (SelectionSystem != null && SelectionSystem.SelectedObjects.Count > 0)
            {
                ApplyLockToSelected(lockIt);
                return;
            }

            SetMode(lockIt ? BuilderMode.Lock : BuilderMode.Unlock);
        }

        private void ApplyLockToSelected(bool lockIt)
        {
            var list = new List<GameObject>(SelectionSystem.SelectedObjects);
            int applied = 0;

            foreach (var obj in list)
            {
                if (obj == null) continue;
                if (PlacementService.TrySetLocked(obj, lockIt))
                    applied++;
            }

            Debug.Log(lockIt
                ? $"<color=yellow>Locked {applied} selected object(s)</color>"
                : $"<color=cyan>Unlocked {applied} selected object(s)</color>");
        }

        /// <summary>
        /// Deletes every object currently in SelectionSystem (skips locked).
        /// Restores inventory and clears selection + highlights.
        /// </summary>
        private void DeleteSelectedObjects()
        {
            var toDelete = new List<GameObject>(SelectionSystem.SelectedObjects);
            int deleted = 0;
            int skippedLocked = 0;

            foreach (var obj in toDelete)
            {
                if (obj == null) continue;

                if (PlacementService.IsLocked(obj))
                {
                    skippedLocked++;
                    continue;
                }

                // Turn highlight off BEFORE destroy (Destroy is end-of-frame)
                SetHighlightActive(obj, false);

                if (TryDeleteOne(obj))
                    deleted++;
            }

            // Clear selection list + event (also refreshes any leftovers)
            SelectionSystem.Clear();
            RefreshSelectionHighlights(); // explicit — don’t wait for next input

            Debug.Log($"<color=orange>Deleted {deleted} selected object(s)" +
                      (skippedLocked > 0 ? $", skipped {skippedLocked} locked" : "") +
                      "</color>");
        }

        /// <summary>
        /// Removes one placed object: free occupation, restore inventory, destroy GO.
        /// Returns true if deleted.
        /// </summary>
        private bool TryDeleteOne(GameObject target)
        {
            if (target == null) return false;

            if (PlacementService.IsLocked(target))
            {
                Debug.LogWarning("Object is locked – cannot delete");
                return false;
            }
            // Buildings: never delete — use Pick Up / store only
            if (IsPlacedBuilding(target))
            {
                Debug.LogWarning("Buildings cannot be deleted – use Pick Up");
                return false;
            }

            // Land tiles only: block delete if a wall/building is on top
            if (PlacementService.TryGetIsLand(target, out bool isLand) && isLand)
            {
                if (PlacementService.HasObjectOnTop(target.transform.position))
                {
                    Debug.LogWarning("Cannot delete land tile – object placed on top");
                    return false;
                }
            }

            if (!PlacementService.TryPickUp(target, out var info))
                return false;

            RestoreInventory(info.Kind, info.InventoryIndex);

            if (info.Instance != null)
                Object.Destroy(info.Instance);

            return true;
        }
        
        

        private void RestoreInventory(PlaceableKind kind, int inventoryIndex)
        {
            _inventory?.Restore(kind, inventoryIndex);
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

                currentPreview.transform.SetPositionAndRotation(
                    pos,
                    Quaternion.Euler(0f, _placeYawSteps * 90f, 0f)
                );
                currentPreview.SetActive(true);
                RefreshPlacementHighlights();
                
                bool valid = IsCurrentHoverValid();
                ActiveGrid.CellSetColor(
                    currentHoveredCell.index,
                    valid ? new Color(0.2f, 0.9f, 0.3f, 0.55f) : new Color(0.95f, 0.2f, 0.2f, 0.55f)
                );
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
            if (_wallRowHold != null && _wallRowHold.IsHolding)
                return;
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
            

            // Cache before anything can clear selection
            GameObject prefab = selectedPrefab;
            Vector2Int size = selectedSize;
            bool isLand = isLandObject;
            int invIndex = selectedInventoryIndex;
            bool clearAfter = _clearSelectionAfterPlace;

            PlaceableKind kind = selectedKind;
            if (invIndex >= 0 && _inventory.GetCount(kind, invIndex) <= 0)
            {
                Debug.LogWarning($"No items left in inventory ({kind} index {invIndex})");
                return;
            }

            Vector3 placePos = ActiveGrid.CellGetPosition(currentHoveredCell.index);

            GameObject placed = PlacementService.Place(
                prefab,
                placePos,
                size,
                isLand,
                invIndex,
                selectedKind,
                Quaternion.Euler(0f, _placeYawSteps * 90f, 0f)
            );

            if (placed == null)
            {
                SetNamedHighlight(currentPreview, CorrectHighlightName, false);
                SetNamedHighlight(currentPreview, WrongHighlightName, true);
                ApplyWireFrameColor(currentPreview, new Color(1f, 0.2f, 0.2f, 1f));
                Debug.LogWarning("Placement failed – cell occupied or invalid");
                return;
            }

            if (invIndex >= 0)
                _inventory.Consume(selectedKind, invIndex);

            Debug.Log($"<color=green>Placed {prefab.name} at cell {currentHoveredCell.index}</color>");

            // LAST – after all uses of selection state
            if (clearAfter)
            {
                _clearSelectionAfterPlace = false;
                ClearSelectedPrefab();
            }
            else
            {
                CheckInventoryAndClearIfEmpty();
            }
        }
        
        private void HandleLockInput(bool lockIt)
        {
            //Debug.Log($"[Lock] HandleLockInput lockIt={lockIt} lmb={Mouse.current?.leftButton.wasPressedThisFrame}");
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;
            if (isPointerOverUI) return;

            Camera cam = buildCamera != null ? buildCamera : Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            int layer = LayerMask.GetMask("PlacedObjects");
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, layer))
            {
                //Debug.Log("[Lock] raycast miss – check PlacedObjects layer on instance");
                return;
            }

            GameObject target = hit.collider.transform.root.gameObject;

            //Debug.Log($"[Lock] Click hit={hit.collider.name} root={hit.collider.transform.root.name} lockIt={lockIt}");

            if (PlacementService.TrySetLocked(target, lockIt))
                Debug.Log(lockIt ? "<color=yellow>Locked</color>" : "<color=cyan>Unlocked</color>");
            
            bool ok = PlacementService.TrySetLocked(target, lockIt);
            //Debug.Log($"[Lock] TrySetLocked → {ok}");
        }
        #endregion

        #region Helpers

        private const string SelectionHighlightName = "SelectionHighlight";

        private bool IsPlacedBuilding(GameObject target)
        {
            if (target == null || PlacementService == null) return false;

            var entries = PlacementService.GetPlacedEntries();
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].instance == target)
                    return entries[i].kind == PlaceableKind.Building;
            }
            return false;
        }
        private void RefreshSelectionHighlights()
        {
            if (PlacementService != null)
            {
                foreach (var entry in PlacementService.GetPlacedEntries())
                {
                    if (entry.instance == null) continue;
                    SetHighlightActive(entry.instance, false);
                }
            }

            if (SelectionSystem == null) return;

            foreach (var obj in SelectionSystem.SelectedObjects)
            {
                if (obj == null) continue;
                SetHighlightActive(obj, true);
            }
        }

        private static void SetHighlightActive(GameObject root, bool active)
        {
            if (root == null) return;

            Transform t = root.transform.Find(SelectionHighlightName);
            if (t == null)
                t = FindChildRecursive(root.transform, SelectionHighlightName);

            if (t != null)
                t.gameObject.SetActive(active);
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                    return child;

                Transform nested = FindChildRecursive(child, name);
                if (nested != null)
                    return nested;
            }
            return null;
        }
        
        private static bool IsWorldPointInScreenRect(Camera cam, Vector3 worldPos, Rect screenRect)
        {
            Vector3 sp = cam.WorldToScreenPoint(worldPos);
            if (sp.z < 0f) return false;
            return screenRect.Contains(new Vector2(sp.x, sp.y));
        }

        private bool PassesSelectFilter(PlacedEntry entry)
        {
            if (entry.instance == null) return false;

            switch (ModeSystem.CurrentSelectFilter)
            {
                case SelectFilter.All:
                    return true;

                case SelectFilter.Walls:
                    return entry.kind == PlaceableKind.Wall;

                case SelectFilter.Turrets:
                    return entry.kind == PlaceableKind.Turret;

                case SelectFilter.Land:
                    return entry.kind == PlaceableKind.Land || entry.isLand;

                case SelectFilter.Buildings:
                    return entry.kind == PlaceableKind.Building;

                default:
                    return false;
            }
        }
        
        private void OnRotateHeld(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            if (currentPreview == null) return;
            if (ModeSystem.CurrentMode != BuilderMode.BuildOnLand
                && ModeSystem.CurrentMode != BuilderMode.BuildOnWater)
                return;

            _placeYawSteps = (_placeYawSteps + 1) % 4;

            if (selectedSize.x != selectedSize.y)
                selectedSize = new Vector2Int(selectedSize.y, selectedSize.x);

            ApplyPreviewRotation();
            RefreshPlacementHighlights();
        }

        private void ApplyPreviewRotation()
        {
            if (currentPreview == null) return;
            currentPreview.transform.rotation = Quaternion.Euler(0f, _placeYawSteps * 90f, 0f);
        }

        private const string CorrectHighlightName = "CorrectHighlight";
        private const string WrongHighlightName = "WrongHighlight";

        private void RefreshPlacementHighlights()
        {
            if (currentPreview == null) return;

            bool valid = IsCurrentHoverValid();

            SetNamedHighlight(currentPreview, CorrectHighlightName, valid);
            SetNamedHighlight(currentPreview, WrongHighlightName, !valid);
            SetNamedHighlight(currentPreview, SelectionHighlightName, false);

            ApplyWireFrameColor(currentPreview, valid ? new Color(0.2f, 1f, 0.35f, 1f) : new Color(1f, 0.2f, 0.2f, 1f));
        }
        private bool IsCurrentHoverValid()
        {
            if (currentHoveredCell == null || ActiveGrid == null || Validator == null)
                return false;

            Vector3 worldPos = ActiveGrid.CellGetPosition(currentHoveredCell.index);
            return Validator.CanPlace(worldPos, selectedSize, isLandObject);
        }
        
        private static void ApplyWireFrameColor(GameObject root, Color color)
        {
            if (root == null) return;
            var frames = root.GetComponentsInChildren<StaticWireFrame>(true);
            for (int i = 0; i < frames.Length; i++)
                frames[i].Color = color;
        }

        private static void SetNamedHighlight(GameObject root, string childName, bool active)
        {
            if (root == null) return;
            Transform t = root.transform.Find(childName);
            if (t == null)
                t = FindChildRecursive(root.transform, childName);
            if (t != null)
                t.gameObject.SetActive(active);
        }
        private void SetCameraInputEnabled(bool enabled)
        {
            if (cameraController == null || cameraController.xinputs == null) return;

            if (enabled)
                cameraController.xinputs.EnableInputs();
            else
                cameraController.xinputs.DisableInputs();
        }
        
        public bool IsAnyGridActive()
        {
            bool landOn = landGrid != null && landGrid.gameObject.activeInHierarchy;
            bool objectOn = objectGrid != null && objectGrid.gameObject.activeInHierarchy;
            return landOn || objectOn;
        }

        /// <summary>Land grid, Object grid, or null if none.</summary>
        public TerrainGridSystem GetActiveBuildGrid()
        {
            if (landGrid != null && landGrid.gameObject.activeInHierarchy)
                return landGrid;
            if (objectGrid != null && objectGrid.gameObject.activeInHierarchy)
                return objectGrid;
            return null;
        }

        public bool IsLandGridActive() =>
            landGrid != null && landGrid.gameObject.activeInHierarchy;

        public bool IsObjectGridActive() =>
            objectGrid != null && objectGrid.gameObject.activeInHierarchy;

        #endregion
    }
}          