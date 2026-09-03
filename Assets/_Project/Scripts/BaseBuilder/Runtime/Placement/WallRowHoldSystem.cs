using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TGS;
using _Project.Scripts.BaseBuilder.Runtime.Inventory;
using Object = UnityEngine.Object;

namespace _Project.Scripts.BaseBuilder.Runtime.Placement
{
    public class WallRowHoldSystem
    {
        public bool IsHolding => _held.Count > 0;
        
        Func<bool> _isInventoryPlacing;
        bool _ignorePlaceUntilRelease;

        struct HeldPiece
        {
            public GameObject prefab;
            public Vector2Int size;
            public int inventoryIndex;
            public PlaceableKind kind;
            public Vector2Int offset; // cells from pivot (object grid)
            public GameObject ghost;
            public Vector3 originalWorld;
            public int originalCell;
        }

        struct OriginalRecord
        {
            public GameObject prefab;
            public Vector2Int size;
            public int inventoryIndex;
            public PlaceableKind kind;
            public Vector3 worldPos;
            public bool isLand;
        }

        readonly List<HeldPiece> _held = new();
        readonly List<OriginalRecord> _restore = new();
        Vector2Int _pivotCell;
        TerrainGridSystem _objectGrid;
        TerrainGridSystem _landGrid;

        PlacementService _placement;
        PlacementValidator _validator;
        BuilderInputActions _input; // rename to your generated class
        Camera _cam;

        public void Bind(
            PlacementService placement,
            PlacementValidator validator,
            BuilderInputActions input,
            Camera cam,
            Func<bool> isInventoryPlacing)
        {
            _placement = placement;
            _validator = validator;
            _input = input;
            _cam = cam;
            _isInventoryPlacing = isInventoryPlacing;
        }

        public void SetGrids(TerrainGridSystem land, TerrainGridSystem objects)
        {
            _landGrid = land;
            _objectGrid = objects;
        }

        public void EnableInput()
        {
            if (_input == null) return;
            _input.Builder.PickUpRow.performed += OnPickUpRow;
            _input.Builder.CancelHeld.performed += OnCancel;
            _input.Builder.RotateHeld.performed += OnRotate;
        }

        public void DisableInput()
        {
            if (_input == null) return;
            _input.Builder.PickUpRow.performed -= OnPickUpRow;
            _input.Builder.CancelHeld.performed -= OnCancel;
            _input.Builder.RotateHeld.performed -= OnRotate;
        }

        

        public void Tick()
        {
            if (!IsHolding || _objectGrid == null) return;

            if (_ignorePlaceUntilRelease)
            {
                if (Mouse.current == null || Mouse.current.leftButton.isPressed)
                    return; // still holding the pickup click
                _ignorePlaceUntilRelease = false;
            }

            Cell under = GetCellUnderMouse(_objectGrid);
            if (under == null) return;

            MoveGhosts(under);

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                TryPlaceGroup(under);
        }

        void OnPickUpRow(InputAction.CallbackContext _)
        {
            if (_objectGrid == null || IsHolding) return;
            if (_isInventoryPlacing != null && _isInventoryPlacing()) return;

            Cell start = GetCellUnderMouse(_objectGrid);
            if (start == null) return;

            if (!_placement.TryGetAtCell(start.index, isLand: false, out var origin) ||
                origin.kind != PlaceableKind.Wall)
                return;

            if (_placement.IsLocked(origin.instance))
            {
                Debug.LogWarning("Row pick up: wall is locked");
                return;
            }
            
            _ignorePlaceUntilRelease = true;

            var row = CollectRow(start.index);
            if (row.Count == 0) return;

            PickupRow(row, start.index);
        }

        List<int> CollectRow(int startIndex)
        {
            var alongX = Walk(startIndex, 1);
            var alongZ = Walk(startIndex, _objectGrid.columnCount);
            return alongX.Count >= alongZ.Count ? alongX : alongZ;
        }

        List<int> Walk(int start, int step)
        {
            var list = new List<int> { start };
            list.AddRange(WalkDir(start, step));
            list.AddRange(WalkDir(start, -step));
            list.Sort();
            return list;
        }

        List<int> WalkDir(int start, int step)
        {
            var found = new List<int>();
            int cols = _objectGrid.columnCount;
            int max = _objectGrid.numCells;
            int i = start + step;
            while (i >= 0 && i < max)
            {
                // stay on same row when stepping by 1
                if (Mathf.Abs(step) == 1)
                {
                    if (i / cols != start / cols) break;
                }

                if (!_placement.TryGetAtCell(i, false, out var e)) break;
                if (e.kind != PlaceableKind.Wall) break;
                if (_placement.IsLocked(e.instance)) break;
                found.Add(i);
                i += step;
            }
            return found;
        }

        void PickupRow(List<int> cells, int pivotIndex)
        {
            ClearHeld(destroyGhosts: true);
            _pivotCell = CellToXY(pivotIndex);

            foreach (int idx in cells)
            {
                if (!_placement.TryGetAtCell(idx, false, out var e)) continue;

                _restore.Add(new OriginalRecord
                {
                    prefab = e.prefab,
                    size = e.size,
                    inventoryIndex = e.inventoryIndex,
                    kind = e.kind,
                    worldPos = e.instance.transform.position,
                    isLand = false
                });

                Vector2Int xy = CellToXY(idx);
                var ghost = Object.Instantiate(e.prefab);
                ghost.name = "HeldWall";
                SetGhostStyle(ghost);

                _held.Add(new HeldPiece
                {
                    prefab = e.prefab,
                    size = e.size,
                    inventoryIndex = e.inventoryIndex,
                    kind = e.kind,
                    offset = xy - _pivotCell,
                    ghost = ghost,
                    originalWorld = e.instance.transform.position,
                    originalCell = idx
                });

                _placement.TryPickUp(e.instance, out _);
                Object.Destroy(e.instance);
            }

            Debug.Log($"<color=cyan>Picked up wall row ({_held.Count})</color>");
        }

        void MoveGhosts(Cell pivot)
        {
            Vector2Int p = CellToXY(pivot.index);
            for (int i = 0; i < _held.Count; i++)
            {
                var h = _held[i];
                Vector2Int c = p + h.offset;
                int idx = XYToCell(c);
                if (idx < 0 || h.ghost == null) continue;
                h.ghost.transform.position = _objectGrid.CellGetPosition(idx);
            }
        }

        void TryPlaceGroup(Cell pivot)
        {
            Vector2Int p = CellToXY(pivot.index);

            foreach (var h in _held)
            {
                int idx = XYToCell(p + h.offset);
                if (idx < 0) return;
                Vector3 world = _objectGrid.CellGetPosition(idx);
                if (!_validator.CanPlace(world, h.size, isLandObject: false))
                {
                    Debug.LogWarning("Group place blocked – invalid / occupied / no land");
                    return;
                }
            }

            foreach (var h in _held)
            {
                int idx = XYToCell(p + h.offset);
                Vector3 world = _objectGrid.CellGetPosition(idx);
                _placement.Place(h.prefab, world, h.size, false, h.inventoryIndex, h.kind);
            }

            ClearHeld(destroyGhosts: true);
            Debug.Log("<color=green>Placed wall row</color>");
        }

        void OnCancel(InputAction.CallbackContext _)
        {
            if (!IsHolding) return;

            foreach (var rec in _restore)
            {
                _placement.Place(rec.prefab, rec.worldPos, rec.size, rec.isLand,
                    rec.inventoryIndex, rec.kind);
            }

            ClearHeld(destroyGhosts: true);
            Debug.Log("<color=cyan>Cancelled wall row – restored</color>");
        }

        void OnRotate(InputAction.CallbackContext _)
        {
            if (!IsHolding) return;

            for (int i = 0; i < _held.Count; i++)
            {
                var h = _held[i];
                // 90° CW in XZ grid: (x,z) -> (z, -x)
                h.offset = new Vector2Int(h.offset.y, -h.offset.x);
                if (h.ghost != null)
                    h.ghost.transform.Rotate(0f, 90f, 0f, Space.World);
                _held[i] = h;
            }
        }

        public void ClearHeld(bool destroyGhosts)
        {
            if (destroyGhosts)
            {
                foreach (var h in _held)
                    if (h.ghost != null) Object.Destroy(h.ghost);
            }
            _held.Clear();
            _restore.Clear();
        }

        Vector2Int CellToXY(int index)
        {
            int cols = _objectGrid.columnCount;
            return new Vector2Int(index % cols, index / cols);
        }

        int XYToCell(Vector2Int xy)
        {
            if (xy.x < 0 || xy.y < 0) return -1;
            int cols = _objectGrid.columnCount;
            if (xy.x >= cols) return -1;
            int idx = xy.x + xy.y * cols;
            return idx < _objectGrid.numCells ? idx : -1;
        }

        Cell GetCellUnderMouse(TerrainGridSystem grid)
        {
            Camera cam = _cam != null ? _cam : Camera.main;
            if (cam == null || grid == null) return null;
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f)) return null;
            return grid.CellGetAtWorldPosition(hit.point, 0);
        }

        static void SetGhostStyle(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>())
                c.enabled = false;
            // optional: tint / transparent later
        }
        
        
        
    }
}