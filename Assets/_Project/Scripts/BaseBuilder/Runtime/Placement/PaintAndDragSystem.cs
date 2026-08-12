using System;
using System.Collections.Generic;
using TGS;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.BaseBuilder.Runtime.Placement
{
    /// <summary>
    /// Left Shift + LMB → axis-locked straight line → confirm on next LMB click.
    /// Works for Land tiles and Walls on both grids.
    /// Preview uses cyan 3D wireframe cubes (no cell colour highlights).
    /// </summary>
    public class PaintAndDragSystem
    {
        public enum State
        {
            Idle,
            Active
        }

        // ─────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────
        private readonly CellHighlightService _highlight;
        private readonly PlacementValidator _validator;
        private readonly PlacementService _placementService;

        private readonly Func<GameObject> _getSelectedPrefab;
        private readonly Func<Vector2Int> _getSelectedSize;
        private readonly Func<bool> _getIsLandObject;
        private readonly Func<TerrainGridSystem> _getActiveGrid;
        private readonly Func<int> _getSelectedInventoryIndex;
        private readonly Func<int, int> _getInventoryCount;

        private readonly WireframePreview _wirePreview;

        // ─────────────────────────────────────────────
        // Runtime state
        // ─────────────────────────────────────────────
        public State CurrentState { get; private set; } = State.Idle;
        public bool IsBusy => CurrentState == State.Active;

        private Cell _startCell;
        private TerrainGridSystem _grid;
        private GameObject _prefab;
        private Vector2Int _size;
        private bool _isLand;

        private readonly List<Cell> _previewLine = new List<Cell>(32);
        private bool _lineIsValid;

        // ─────────────────────────────────────────────
        // Events
        // ─────────────────────────────────────────────
        public event Action OnEnteredActive;
        public event Action OnCancelled;
        public event Action OnPlaced;
        public event Action<int> OnLandTilePlaced;
        public event Action<int> OnWallPlaced;

        public PaintAndDragSystem(
            CellHighlightService highlight,
            PlacementValidator validator,
            PlacementService placementService,
            Func<GameObject> getSelectedPrefab,
            Func<Vector2Int> getSelectedSize,
            Func<bool> getIsLandObject,
            Func<int> getSelectedInventoryIndex,
            Func<int, int> getInventoryCount,
            Func<TerrainGridSystem> getActiveGrid)
        {
            _highlight = highlight;
            _validator = validator;
            _placementService = placementService;
            _getSelectedPrefab = getSelectedPrefab;
            _getSelectedSize = getSelectedSize;
            _getIsLandObject = getIsLandObject;
            _getSelectedInventoryIndex = getSelectedInventoryIndex;
            _getInventoryCount = getInventoryCount;
            _getActiveGrid = getActiveGrid;

            _wirePreview = new WireframePreview();
        }

        // ─────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────

        public void Tick()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            if (mouse == null || keyboard == null) return;

            bool shiftHeld = keyboard.leftShiftKey.isPressed;
            bool lmbDown = mouse.leftButton.wasPressedThisFrame;
            bool rmbDown = mouse.rightButton.wasPressedThisFrame;
            bool escapePressed = keyboard.escapeKey.wasPressedThisFrame;

            switch (CurrentState)
            {
                case State.Idle:
                    if (shiftHeld && lmbDown && CanStart())
                        EnterActiveMode();
                    break;

                case State.Active:
                    UpdatePreviewLine();

                    if (lmbDown)
                        TryConfirmOrCancel();

                    if (rmbDown || escapePressed)
                        Cancel();
                    break;
            }
        }

        public void ForceCancel()
        {
            if (CurrentState != State.Idle)
                Cancel();
        }

        // ─────────────────────────────────────────────
        // Internal
        // ─────────────────────────────────────────────

        private bool CanStart()
        {
            var prefab = _getSelectedPrefab();
            if (prefab == null) return false;

            var size = _getSelectedSize();
            return size == Vector2Int.one; // only 1×1
        }

        private void EnterActiveMode()
        {
            _grid = _getActiveGrid();
            _prefab = _getSelectedPrefab();
            _size = _getSelectedSize();
            _isLand = _getIsLandObject();

            if (_grid == null || _prefab == null)
            {
                ResetToIdle();
                return;
            }

            _startCell = GetCellUnderMouse(_grid);
            if (_startCell == null)
            {
                ResetToIdle();
                return;
            }

            CurrentState = State.Active;
            _previewLine.Clear();
            OnEnteredActive?.Invoke();
        }

        private void UpdatePreviewLine()
        {
            if (_startCell == null || _grid == null) return;

            Cell current = GetCellUnderMouse(_grid);
            if (current == null) return;

            BuildAxisAlignedLine(_startCell, current);

            _lineIsValid = true;
            foreach (var cell in _previewLine)
            {
                if (cell == null) continue;

                Vector3 worldPos = _grid.CellGetPosition(cell.index);
                if (!_validator.CanPlace(worldPos, _size, _isLand))
                {
                    _lineIsValid = false;
                    break;
                }
            }

            // Cyan wireframe cubes
            List<Vector3> centers = new List<Vector3>(_previewLine.Count);
            float cellSize = _grid.cellSize.x;

            foreach (var cell in _previewLine)
            {
                if (cell == null) continue;
                Vector3 pos = _grid.CellGetPosition(cell.index);
                pos.y += 0.02f;
                centers.Add(pos);
            }

            _wirePreview.ShowCubes(centers, cellSize);
        }

        private void TryConfirmOrCancel()
        {
            if (_previewLine == null || _previewLine.Count == 0)
            {
                Cancel();
                return;
            }

            if (!_lineIsValid)
            {
                Cancel();
                return;
            }

            if (_grid == null || _prefab == null || _placementService == null)
            {
                Debug.LogError("[PaintAndDrag] Missing reference in TryConfirmOrCancel – cancelling.");
                Cancel();
                return;
            }

            int inventoryIndex = _getSelectedInventoryIndex != null ? _getSelectedInventoryIndex() : -1;

            foreach (var cell in _previewLine)
            {
                if (cell == null) continue;

                if (inventoryIndex >= 0)
                {
                    int remaining = _getInventoryCount != null ? _getInventoryCount(inventoryIndex) : 0;
                    if (remaining <= 0)
                    {
                        Debug.Log("[PaintAndDrag] Inventory empty – stopping line placement");
                        break;
                    }
                }

                Vector3 worldPos = _grid.CellGetPosition(cell.index);
                GameObject instance = _placementService.Place(_prefab, worldPos, _size, _isLand, inventoryIndex);
                if (instance == null)
                    break;

                if (inventoryIndex >= 0)
                {
                    if (_isLand)
                        OnLandTilePlaced?.Invoke(inventoryIndex);
                    else
                        OnWallPlaced?.Invoke(inventoryIndex);
                }
            }

            _wirePreview.Clear();
            _highlight?.Clear();
            CurrentState = State.Idle;
            OnPlaced?.Invoke();
        }

        private void BuildAxisAlignedLine(Cell start, Cell end)
        {
            _previewLine.Clear();
            if (start == null || end == null || _grid == null) return;

            int maxAllowed = 0;

            if (_isLand)
            {
                int inventoryIndex = _getSelectedInventoryIndex != null ? _getSelectedInventoryIndex() : -1;
                if (inventoryIndex >= 0 && _getInventoryCount != null)
                    maxAllowed = _getInventoryCount(inventoryIndex);
            }
            else
            {
                int inventoryIndex = _getSelectedInventoryIndex != null ? _getSelectedInventoryIndex() : -1;
                if (inventoryIndex >= 0 && _getInventoryCount != null)
                    maxAllowed = _getInventoryCount(inventoryIndex);
                else
                    maxAllowed = 14;
            }

            if (maxAllowed <= 0) return;

            int dx = end.column - start.column;
            int dz = end.row - start.row;
            bool useX = Mathf.Abs(dx) >= Mathf.Abs(dz);

            int steps = useX ? Mathf.Abs(dx) : Mathf.Abs(dz);
            steps = Mathf.Min(steps, maxAllowed - 1);

            int stepCol = useX ? (dx >= 0 ? 1 : -1) : 0;
            int stepRow = useX ? 0 : (dz >= 0 ? 1 : -1);

            for (int i = 0; i <= steps; i++)
            {
                int col = start.column + stepCol * i;
                int row = start.row + stepRow * i;

                if (col < 0 || row < 0 || col >= _grid.columnCount || row >= _grid.rowCount)
                    break;

                int index = row * _grid.columnCount + col;
                _previewLine.Add(_grid.cells[index]);
            }
        }

        private void Cancel()
        {
            _wirePreview.Clear();
            _highlight?.Clear();
            ResetToIdle();
            OnCancelled?.Invoke();
        }

        private void ResetToIdle()
        {
            CurrentState = State.Idle;
            _startCell = null;
            _previewLine.Clear();
            _lineIsValid = false;
            _prefab = null;
            _wirePreview.Clear();
        }

        private Cell GetCellUnderMouse(TerrainGridSystem grid)
        {
            if (Camera.main == null || Mouse.current == null) return null;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
                return grid.CellGetAtWorldPosition(hit.point, 0);

            return null;
        }
    }
}