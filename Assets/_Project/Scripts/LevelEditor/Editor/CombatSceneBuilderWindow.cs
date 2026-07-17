using System.Collections.Generic;
using _Project.Scripts.LevelEditor.Data;
using TGS;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.LevelEditor.Editor
{
    public class CombatSceneBuilderWindow : EditorWindow
    {
        #region Core References
        private TerrainGridSystem landGrid;
        private TerrainGridSystem objectGrid;
        private TerrainGridSystem activeGrid;
        private CombatLayoutDatabase layoutDatabase;
        #endregion

        #region Move and Rotate Objects
        // === Move / Rotate Selected ===
        private bool isMovingSelection = false;
        private List<GameObject> movingObjects = new List<GameObject>();
        private List<Vector3> originalPositions = new List<Vector3>();
        private List<Quaternion> originalRotations = new List<Quaternion>();
        private Vector3 moveOffset;
        private int currentRotationSteps = 0; // 0, 1, 2, 3 (90 degree steps)
        #endregion

        #region Placement & Tracking
        private GameObject selectedPrefab;
        private GameObject currentPreview;
        private bool isPreviewActive = false;

        private List<GameObject> placedObjects = new List<GameObject>();
        private HashSet<int> occupiedLandCells = new HashSet<int>();
        private HashSet<int> occupiedObjectCells = new HashSet<int>();
        #endregion

        #region Categories
        private bool showLandTiles = true;
        private bool showTurrets = true;
        private bool showWalls = true;
        private bool showBuildings = true;
        private bool showCombatShips = true;

        private List<GameObject> landTiles = new List<GameObject>();
        private List<GameObject> turrets = new List<GameObject>();
        private List<GameObject> walls = new List<GameObject>();
        private List<GameObject> buildings = new List<GameObject>();
        private List<GameObject> combatShips = new List<GameObject>();
        #endregion

        #region Grid & Tools
        private enum GridType { Land, Objects }
        private GridType currentGridType = GridType.Land;
        private int selectedSlot = 0;
        #endregion

        #region Drag Placement
        private bool isDragging = false;
        private Cell dragStartCell = null;
        private List<Cell> dragPreviewCells = new List<Cell>();
        #endregion

        #region Multi Selection
        private List<GameObject> selectedObjects = new List<GameObject>();
        private Color selectionColor = new Color(0.2f, 0.6f, 1f, 0.85f);
        #endregion

        [MenuItem("Tools/Combat Scene Builder")]
        public static void ShowWindow()
        {
            GetWindow<CombatSceneBuilderWindow>("Combat Scene Builder");
        }

        private void OnEnable()
        {
            if (landGrid == null)
                landGrid = Object.FindObjectOfType<TerrainGridSystem>();

            activeGrid = landGrid;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            DestroyPreview();
        }

        #region GUI
        private void OnGUI()
        {
            GUILayout.Label("Combat Scene Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // === Tools ===
            GUILayout.Label("Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = selectedObjects.Count > 0 && !isMovingSelection;

            if (GUILayout.Button($"Pick Up Selected ({selectedObjects.Count})", GUILayout.Height(28)))
            {
                StartMovingSelection();
            }

            GUI.enabled = true;

            // Delete Selected
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button($"Delete Selected ({selectedObjects.Count})", GUILayout.Height(28)))
            {
                DeleteSelectedObjects();
            }
            GUI.backgroundColor = Color.white;

            // Store Selected (placeholder)
            if (GUILayout.Button($"Store Selected ({selectedObjects.Count})", GUILayout.Height(28)))
            {
                // Placeholder for future logic
                Debug.Log($"<color=yellow>Stored {selectedObjects.Count} object(s) — logic coming later</color>");
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Grid References
            GUILayout.Label("Grid References", EditorStyles.boldLabel);
            landGrid = (TerrainGridSystem)EditorGUILayout.ObjectField("Land Grid", landGrid, typeof(TerrainGridSystem), true);
            objectGrid = (TerrainGridSystem)EditorGUILayout.ObjectField("Object Grid", objectGrid, typeof(TerrainGridSystem), true);

            EditorGUILayout.Space(8);

            // Active Grid
            GUILayout.Label("Active Grid for Placement:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(currentGridType == GridType.Land, "Land Grid", "Button"))
            {
                currentGridType = GridType.Land;
                activeGrid = landGrid;
            }

            if (GUILayout.Toggle(currentGridType == GridType.Objects, "Object Grid", "Button"))
            {
                currentGridType = GridType.Objects;
                activeGrid = objectGrid;
            }

            EditorGUILayout.EndHorizontal();

            if (activeGrid == null)
                EditorGUILayout.HelpBox("Please assign the active grid above.", MessageType.Warning);

            EditorGUILayout.Space(10);

            // Layout Database
            layoutDatabase = (CombatLayoutDatabase)EditorGUILayout.ObjectField("Layout Database", layoutDatabase, typeof(CombatLayoutDatabase), false);

            EditorGUILayout.Space(10);

            // Categories
            DrawCategory("Land Tiles", ref showLandTiles, landTiles);
            DrawCategory("Turrets", ref showTurrets, turrets);
            DrawCategory("Walls", ref showWalls, walls);
            DrawCategory("Buildings", ref showBuildings, buildings);
            DrawCategory("Combat Ships", ref showCombatShips, combatShips);

            EditorGUILayout.Space(10);

            // Currently Selected Prefab
            GUILayout.Label("Currently Selected Prefab:", EditorStyles.boldLabel);
            selectedPrefab = (GameObject)EditorGUILayout.ObjectField(selectedPrefab, typeof(GameObject), false);

            if (GUILayout.Button("Cancel Preview / Deselect Prefab"))
            {
                selectedPrefab = null;
                DestroyPreview();
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField($"Selected Objects: {selectedObjects.Count}", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("Clear Selection"))
                selectedObjects.Clear();

            EditorGUILayout.Space(15);

            if (GUILayout.Button("Clear Scene", GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog("Clear Scene",
                    "This will delete all placed objects and reset occupation tracking.\nAre you sure?",
                    "Clear Scene", "Cancel"))
                {
                    ClearAllPlacedObjects();
                }
            }

            // Save / Load
            EditorGUILayout.Space(15);
            GUILayout.Label("Layout Save / Load", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Slot:", GUILayout.Width(40));
            selectedSlot = EditorGUILayout.IntSlider(selectedSlot, 0, CombatLayoutDatabase.MaxSlots - 1, GUILayout.Width(150));
            GUILayout.Label($"Slot {selectedSlot + 1}", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Layout", GUILayout.Height(25))) SaveCurrentLayout();
            if (GUILayout.Button("Load Layout", GUILayout.Height(25))) LoadLayout();
            if (GUILayout.Button("Delete Slot", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Delete Layout", $"Delete layout in Slot {selectedSlot + 1}?", "Delete", "Cancel"))
                    DeleteCurrentSlot();
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Category UI
        private void DrawCategory(string title, ref bool foldout, List<GameObject> prefabs)
        {
            foldout = EditorGUILayout.Foldout(foldout, title, true);
            if (!foldout) return;

            EditorGUI.indentLevel++;
            for (int i = 0; i < prefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);

                if (GUILayout.Button("Select", GUILayout.Width(55)))
                {
                    selectedPrefab = prefabs[i];
                    isPreviewActive = true;
                    selectedObjects.Clear();
                    DestroyPreview();
                    ForceCreatePreviewAtMouse();
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add", GUILayout.Width(50)))
                prefabs.Add(null);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        #endregion

        #region Scene GUI
        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            
            // ===== MOVING SELECTION =====
            if (isMovingSelection)
            {
                UpdateMovingSelection();

                Event e1 = Event.current;

                // Rotate
                if (e1.type == EventType.KeyDown && e1.keyCode == KeyCode.R)
                {
                    RotateMovingSelection();
                    e1.Use();
                }

                // Place
                if (e1.type == EventType.MouseDown && e1.button == 0 && !e1.alt)
                {
                    TryPlaceMovingSelection();
                    e1.Use();
                }

                // Cancel
                if (e1.type == EventType.MouseDown && e1.button == 1 || 
                    (e1.type == EventType.KeyDown && e1.keyCode == KeyCode.Escape))
                {
                    CancelMovingSelection();
                    e1.Use();
                }

                sceneView.Repaint();
                return;
            }
            
            // ===== PLACEMENT + DRAG MODE =====
            if (selectedPrefab != null)
            {
                if (!isDragging)
                    UpdatePreviewPosition();

                if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    Cell cell = GetCellUnderMouse();
                    if (cell != null)
                    {
                        isDragging = true;
                        dragStartCell = cell;
                        dragPreviewCells.Clear();
                        dragPreviewCells.Add(cell);
                        e.Use();
                    }
                }

                if (isDragging && e.type == EventType.MouseDrag)
                {
                    Cell currentCell = GetCellUnderMouse();
                    if (currentCell != null && dragStartCell != null)
                    {
                        dragPreviewCells = GetCellsInLine(dragStartCell, currentCell);
                        e.Use();
                    }
                }

                if (isDragging && e.type == EventType.MouseUp && e.button == 0)
                {
                    PlaceWallsAlongLine();
                    isDragging = false;
                    dragStartCell = null;
                    dragPreviewCells.Clear();
                    e.Use();
                }

                if (e.type == EventType.MouseDown && e.button == 1)
                {
                    isDragging = false;
                    dragStartCell = null;
                    dragPreviewCells.Clear();
                    DestroyPreview();
                    e.Use();
                }

                DrawDragPreview();
                sceneView.Repaint();
                return;
            }

            // ===== SELECTION MODE =====
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                HandleSelectionClick(e.shift);
                e.Use();
            }

            DrawSelectionHighlights();
            sceneView.Repaint();
        }
        #endregion

        #region Helpers
        
        private void DeleteSelectedObjects()
        {
            if (selectedObjects.Count == 0)
            {
                Debug.LogWarning("No objects selected to delete.");
                return;
            }

            // Work on a copy so we can safely modify the list
            List<GameObject> toDelete = new List<GameObject>(selectedObjects);

            foreach (GameObject obj in toDelete)
            {
                if (obj == null) continue;

                // Re-use the same logic we had for single delete
                PlaceableObject placeable = obj.GetComponent<PlaceableObject>();
                Vector2Int size = placeable != null ? placeable.sizeInCells : Vector2Int.one;

                bool isLandObject = landTiles.Contains(PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject);
                TerrainGridSystem targetGrid = isLandObject ? landGrid : objectGrid;
                HashSet<int> targetOccupied = isLandObject ? occupiedLandCells : occupiedObjectCells;

                if (targetGrid != null)
                {
                    float cellSize = targetGrid.cellSize.x;
                    Vector3 objPos = obj.transform.position;

                    Vector3 originPos = objPos;
                    originPos.x -= (size.x - 1) * cellSize * 0.5f;
                    originPos.z -= (size.y - 1) * cellSize * 0.5f;

                    Cell originCell = targetGrid.CellGetAtWorldPosition(originPos, 0);
                    if (originCell != null)
                    {
                        for (int x = 0; x < size.x; x++)
                        {
                            for (int y = 0; y < size.y; y++)
                            {
                                int index = originCell.index + x + (y * targetGrid.columnCount);
                                targetOccupied.Remove(index);
                            }
                        }
                    }
                }

                placedObjects.Remove(obj);
                Undo.DestroyObjectImmediate(obj);
            }

            selectedObjects.Clear();
            Debug.Log($"<color=red>Deleted {toDelete.Count} selected object(s)</color>");
        }
        
        private int GetCurrentLayerMask()
        {
            return currentGridType == GridType.Land
                ? LayerMask.GetMask("LandGrid")
                : LayerMask.GetMask("ObjectGrid");
        }

        private Cell GetCellUnderMouse()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f, GetCurrentLayerMask()))
                return activeGrid.CellGetAtWorldPosition(hit.point, 0);
            return null;
        }

        private Vector2Int GetSelectedObjectSize()
        {
            if (selectedPrefab == null) return Vector2Int.one;
            PlaceableObject placeable = selectedPrefab.GetComponent<PlaceableObject>();
            return placeable != null ? placeable.sizeInCells : Vector2Int.one;
        }

        private bool IsFootprintValid(Cell originCell, Vector2Int size)
        {
            if (originCell == null || activeGrid == null) return false;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int checkIndex = originCell.index + x + (y * activeGrid.columnCount);
                    if (checkIndex < 0 || checkIndex >= activeGrid.numCells) return false;

                    Vector3 cellWorldPos = activeGrid.CellGetPosition(checkIndex);

                    if (currentGridType == GridType.Land)
                    {
                        if (occupiedLandCells.Contains(checkIndex)) return false;
                    }
                    else
                    {
                        if (!HasLandTileUnderneath(cellWorldPos)) return false;
                        if (occupiedObjectCells.Contains(checkIndex)) return false;
                    }
                }
            }
            return true;
        }

        private void MarkFootprintOccupied(Cell originCell, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int index = originCell.index + x + (y * activeGrid.columnCount);
                    if (currentGridType == GridType.Land)
                        occupiedLandCells.Add(index);
                    else
                        occupiedObjectCells.Add(index);
                }
            }
        }

        private bool HasLandTileUnderneath(Vector3 worldPosition)
        {
            if (landGrid == null) return false;

            Vector3 landPos = worldPosition;
            landPos.y = landGrid.transform.position.y + 0.1f;

            Cell landCell = landGrid.CellGetAtWorldPosition(landPos, 0);
            if (landCell == null)
            {
                landPos.y += 0.5f;
                landCell = landGrid.CellGetAtWorldPosition(landPos, 0);
            }

            return landCell != null && occupiedLandCells.Contains(landCell.index);
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
        #endregion

        #region Drag Line Placement
        private List<Cell> GetCellsInLine(Cell start, Cell end)
        {
            List<Cell> cells = new List<Cell>();

            int startX = start.column;
            int startY = start.row;
            int endX = end.column;
            int endY = end.row;

            if (startY == endY) // Horizontal
            {
                int minX = Mathf.Min(startX, endX);
                int maxX = Mathf.Max(startX, endX);
                for (int x = minX; x <= maxX; x++)
                {
                    int index = startY * activeGrid.columnCount + x;
                    if (index >= 0 && index < activeGrid.numCells)
                        cells.Add(activeGrid.cells[index]);
                }
            }
            else if (startX == endX) // Vertical
            {
                int minY = Mathf.Min(startY, endY);
                int maxY = Mathf.Max(startY, endY);
                for (int y = minY; y <= maxY; y++)
                {
                    int index = y * activeGrid.columnCount + startX;
                    if (index >= 0 && index < activeGrid.numCells)
                        cells.Add(activeGrid.cells[index]);
                }
            }
            else
            {
                cells.Add(start);
                cells.Add(end);
            }

            return cells;
        }

        private void PlaceWallsAlongLine()
        {
            if (dragPreviewCells == null || dragPreviewCells.Count == 0) return;

            int placedCount = 0;
            Vector2Int size = GetSelectedObjectSize();

            foreach (Cell cell in dragPreviewCells)
            {
                if (cell == null || !IsFootprintValid(cell, size)) continue;

                Vector3 pos = activeGrid.CellGetPosition(cell.index);

                GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
                placed.transform.position = pos;
                placed.name = selectedPrefab.name;

                int placedLayer = LayerMask.NameToLayer("PlacedObjects");
                if (placedLayer != -1)
                    SetLayerRecursively(placed, placedLayer);

                placedObjects.Add(placed);
                MarkFootprintOccupied(cell, size);
                Undo.RegisterCreatedObjectUndo(placed, "Place Wall Line");
                placedCount++;
            }

            if (placedCount > 0)
                Debug.Log($"<color=green>Placed {placedCount} walls in a line</color>");
        }

        private void DrawDragPreview()
        {
            if (!isDragging || dragPreviewCells == null || dragPreviewCells.Count == 0) return;

            Handles.color = new Color(0.2f, 0.8f, 1f, 0.9f);

            for (int i = 0; i < dragPreviewCells.Count; i++)
            {
                Cell cell = dragPreviewCells[i];
                if (cell == null) continue;

                Vector3 pos = activeGrid.CellGetPosition(cell.index);
                Handles.DrawWireCube(pos + Vector3.up * 0.1f, Vector3.one * activeGrid.cellSize.x * 0.9f);

                if (i > 0 && dragPreviewCells[i - 1] != null)
                {
                    Vector3 prevPos = activeGrid.CellGetPosition(dragPreviewCells[i - 1].index);
                    Handles.DrawLine(prevPos + Vector3.up * 0.15f, pos + Vector3.up * 0.15f, 3f);
                }
            }
        }
        #endregion

        #region Selection
        private void HandleSelectionClick(bool shiftHeld)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            int layerMask = LayerMask.GetMask("PlacedObjects");

            if (Physics.Raycast(ray, out RaycastHit hit, 5000f, layerMask))
            {
                Transform current = hit.collider.transform;
                GameObject found = null;

                while (current != null)
                {
                    if (placedObjects.Contains(current.gameObject))
                    {
                        found = current.gameObject;
                        break;
                    }
                    current = current.parent;
                }

                if (found == null)
                {
                    GameObject root = hit.collider.transform.root.gameObject;
                    if (placedObjects.Contains(root))
                        found = root;
                }

                if (found != null)
                {
                    if (shiftHeld)
                    {
                        if (selectedObjects.Contains(found))
                            selectedObjects.Remove(found);
                        else
                            selectedObjects.Add(found);
                    }
                    else
                    {
                        selectedObjects.Clear();
                        selectedObjects.Add(found);
                    }
                    return;
                }
            }

            if (!shiftHeld)
                selectedObjects.Clear();
        }
        
        private void StartMovingSelection()
        {
            if (selectedObjects.Count == 0) return;

            isMovingSelection = true;
            movingObjects.Clear();
            originalPositions.Clear();
            originalRotations.Clear();
            currentRotationSteps = 0;

            // Record original data and free the cells
            foreach (GameObject obj in selectedObjects)
            {
                if (obj == null) continue;

                movingObjects.Add(obj);
                originalPositions.Add(obj.transform.position);
                originalRotations.Add(obj.transform.rotation);

                FreeObjectCells(obj);
            }

            selectedObjects.Clear();
            Debug.Log($"<color=cyan>Picked up {movingObjects.Count} object(s). Press R to rotate, Left Click to place, Right Click to cancel.</color>");
        }

        private void UpdateMovingSelection()
        {
            if (movingObjects.Count == 0 || activeGrid == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // Prefer hitting the Object Grid
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, LayerMask.GetMask("ObjectGrid", "LandGrid")))
                return;

            // Get the cell under the mouse
            Cell targetCell = activeGrid.CellGetAtWorldPosition(hit.point, 0);
            if (targetCell == null) return;

            // Snapped position = center of the target cell
            Vector3 snappedPos = activeGrid.CellGetPosition(targetCell.index);

            // Calculate current center of the moving group
            Vector3 currentCenter = Vector3.zero;
            int count = 0;
            foreach (GameObject obj in movingObjects)
            {
                if (obj == null) continue;
                currentCenter += obj.transform.position;
                count++;
            }
            if (count == 0) return;
            currentCenter /= count;

            // Keep original height
            snappedPos.y = currentCenter.y;

            // Move the entire group so its center snaps to the cell
            Vector3 delta = snappedPos - currentCenter;

            foreach (GameObject obj in movingObjects)
            {
                if (obj != null)
                    obj.transform.position += delta;
            }
        }

        private void RotateMovingSelection()
        {
            if (movingObjects.Count == 0) return;

            currentRotationSteps = (currentRotationSteps + 1) % 4;

            // Find center of the group
            Vector3 center = Vector3.zero;
            foreach (GameObject obj in movingObjects)
                center += obj.transform.position;
            center /= movingObjects.Count;

            foreach (GameObject obj in movingObjects)
            {
                // Rotate position around center
                Vector3 dir = obj.transform.position - center;
                dir = Quaternion.Euler(0, 90, 0) * dir;
                obj.transform.position = center + dir;

                // Rotate the object itself
                obj.transform.Rotate(0, 90, 0);
            }
        }

        private void TryPlaceMovingSelection()
        {
            if (movingObjects.Count == 0) return;

            foreach (GameObject obj in movingObjects)
            {
                if (obj == null) continue;

                PlaceableObject placeable = obj.GetComponent<PlaceableObject>();
                Vector2Int size = placeable != null ? placeable.sizeInCells : Vector2Int.one;

                bool isLandObject = landTiles.Contains(PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject);
                TerrainGridSystem targetGrid = isLandObject ? landGrid : objectGrid;
                HashSet<int> targetOccupied = isLandObject ? occupiedLandCells : occupiedObjectCells;

                if (targetGrid == null) continue;

                // Find the origin cell for this object (reverse the centering)
                float cellSize = targetGrid.cellSize.x;
                Vector3 objPos = obj.transform.position;

                Vector3 originPos = objPos;
                originPos.x -= (size.x - 1) * cellSize * 0.5f;
                originPos.z -= (size.y - 1) * cellSize * 0.5f;

                Cell originCell = targetGrid.CellGetAtWorldPosition(originPos, 0);
                if (originCell != null)
                {
                    // Mark the full footprint as occupied again
                    for (int x = 0; x < size.x; x++)
                    {
                        for (int y = 0; y < size.y; y++)
                        {
                            int index = originCell.index + x + (y * targetGrid.columnCount);
                            targetOccupied.Add(index);
                        }
                    }
                }
            }

            movingObjects.Clear();
            isMovingSelection = false;
            Debug.Log("<color=green>Placed moved selection (snapped + cells re-occupied)</color>");
        }

        private void CancelMovingSelection()
        {
            // Restore original positions and rotations
            for (int i = 0; i < movingObjects.Count; i++)
            {
                if (movingObjects[i] == null) continue;

                movingObjects[i].transform.position = originalPositions[i];
                movingObjects[i].transform.rotation = originalRotations[i];

                // Re-occupy the original cells
                // (We can call a proper method here later)
            }

            movingObjects.Clear();
            isMovingSelection = false;
            Debug.Log("<color=orange>Move cancelled – objects restored</color>");
        }

        private void FreeObjectCells(GameObject obj)
        {
            PlaceableObject placeable = obj.GetComponent<PlaceableObject>();
            Vector2Int size = placeable != null ? placeable.sizeInCells : Vector2Int.one;

            bool isLandObject = landTiles.Contains(PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject);
            TerrainGridSystem targetGrid = isLandObject ? landGrid : objectGrid;
            HashSet<int> targetOccupied = isLandObject ? occupiedLandCells : occupiedObjectCells;

            if (targetGrid == null) return;

            float cellSize = targetGrid.cellSize.x;
            Vector3 objPos = obj.transform.position;

            Vector3 originPos = objPos;
            originPos.x -= (size.x - 1) * cellSize * 0.5f;
            originPos.z -= (size.y - 1) * cellSize * 0.5f;

            Cell originCell = targetGrid.CellGetAtWorldPosition(originPos, 0);
            if (originCell == null) return;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int index = originCell.index + x + (y * targetGrid.columnCount);
                    targetOccupied.Remove(index);
                }
            }
        }

        private void DrawSelectionHighlights()
        {
            if (selectedObjects.Count == 0) return;

            Handles.color = selectionColor;

            foreach (GameObject obj in selectedObjects)
            {
                if (obj == null) continue;
                Bounds bounds = GetObjectBounds(obj);
                Handles.DrawWireCube(bounds.center, bounds.size * 1.08f);
            }
        }

        private Bounds GetObjectBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(obj.transform.position, Vector3.one);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }
        #endregion

        #region Placement Core
        private void UpdatePreviewPosition()
        {
            if (selectedPrefab == null || activeGrid == null || !isPreviewActive || currentPreview == null)
                return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, GetCurrentLayerMask())) return;

            Cell originCell = activeGrid.CellGetAtWorldPosition(hit.point, 0);
            if (originCell == null) return;

            Vector2Int size = GetSelectedObjectSize();
            Vector3 originPos = activeGrid.CellGetPosition(originCell.index);
            float cellSize = activeGrid.cellSize.x;

            Vector3 footprintCenter = originPos;
            footprintCenter.x += (size.x - 1) * cellSize * 0.5f;
            footprintCenter.z += (size.y - 1) * cellSize * 0.5f;

            currentPreview.transform.position = footprintCenter;

            bool isValid = IsFootprintValid(originCell, size);
            Color targetColor = isValid ? GetPreviewColorForPrefab(selectedPrefab) : new Color(1f, 0.15f, 0.15f);
            targetColor.a = 0.55f;
            SetPreviewColor(targetColor);
        }

        private Vector3 GetSnappedWorldPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f, GetCurrentLayerMask()))
            {
                Cell cell = activeGrid.CellGetAtWorldPosition(hit.point, 0);
                if (cell != null) return activeGrid.CellGetPosition(cell.index);
            }
            return Vector3.zero;
        }

        private void PlaceObjectAtPosition(Vector3 position)
        {
            if (selectedPrefab == null || activeGrid == null) return;

            Cell originCell = activeGrid.CellGetAtWorldPosition(position, 0);
            if (originCell == null) return;

            Vector2Int size = GetSelectedObjectSize();
            if (!IsFootprintValid(originCell, size)) return;

            Vector3 originPos = activeGrid.CellGetPosition(originCell.index);
            float cellSize = activeGrid.cellSize.x;

            Vector3 footprintCenter = originPos;
            footprintCenter.x += (size.x - 1) * cellSize * 0.5f;
            footprintCenter.z += (size.y - 1) * cellSize * 0.5f;

            GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            placed.transform.position = footprintCenter;
            placed.name = selectedPrefab.name;

            int placedLayer = LayerMask.NameToLayer("PlacedObjects");
            if (placedLayer != -1)
                SetLayerRecursively(placed, placedLayer);

            placedObjects.Add(placed);
            MarkFootprintOccupied(originCell, size);
            Undo.RegisterCreatedObjectUndo(placed, "Place Object");
        }
        #endregion

        #region Preview System
        private void CreatePreview(Vector3 position)
        {
            DestroyPreview();
            isPreviewActive = true;

            currentPreview = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            currentPreview.name = "Preview_" + selectedPrefab.name;
            currentPreview.transform.position = position;

            Color c = GetPreviewColorForPrefab(selectedPrefab);
            c.a = 0.55f;
            SetPreviewColor(c);
        }

        private void SetPreviewColor(Color color)
        {
            if (currentPreview == null) return;
            foreach (var rend in currentPreview.GetComponentsInChildren<Renderer>())
            {
                if (rend.sharedMaterial != null)
                {
                    Material mat = new Material(rend.sharedMaterial);
                    mat.color = color;
                    rend.material = mat;
                }
            }
        }

        private Color GetPreviewColorForPrefab(GameObject prefab)
        {
            if (landTiles.Contains(prefab)) return new Color(0.2f, 0.85f, 0.35f);
            if (turrets.Contains(prefab)) return new Color(0.95f, 0.3f, 0.3f);
            if (walls.Contains(prefab)) return new Color(0.35f, 0.55f, 0.95f);
            if (buildings.Contains(prefab)) return new Color(0.95f, 0.65f, 0.2f);
            if (combatShips.Contains(prefab)) return new Color(0.7f, 0.3f, 0.9f);
            return new Color(0.75f, 0.75f, 0.75f);
        }

        private void ForceCreatePreviewAtMouse()
        {
            if (selectedPrefab == null || activeGrid == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Cell cell = null;
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f, GetCurrentLayerMask()))
                cell = activeGrid.CellGetAtWorldPosition(hit.point, 0);

            Vector3 pos = cell != null ? activeGrid.CellGetPosition(cell.index) : Vector3.zero;
            CreatePreview(pos);
        }

        private void DestroyPreview()
        {
            if (currentPreview != null)
            {
                foreach (var rend in currentPreview.GetComponentsInChildren<Renderer>())
                    if (rend.material != null) DestroyImmediate(rend.material);

                DestroyImmediate(currentPreview);
                currentPreview = null;
            }
            isPreviewActive = false;
        }
        #endregion

        #region Delete
        private void TryDeleteObjectAtMouse()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            int layerMask = LayerMask.GetMask("PlacedObjects");

            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, layerMask))
                return;

            Transform current = hit.collider.transform;
            GameObject foundObject = null;

            while (current != null)
            {
                if (placedObjects.Contains(current.gameObject))
                {
                    foundObject = current.gameObject;
                    break;
                }
                current = current.parent;
            }

            if (foundObject == null)
            {
                GameObject root = hit.collider.transform.root.gameObject;
                if (placedObjects.Contains(root))
                    foundObject = root;
            }

            if (foundObject == null) return;

            PlaceableObject placeable = foundObject.GetComponent<PlaceableObject>();
            Vector2Int size = placeable != null ? placeable.sizeInCells : Vector2Int.one;

            bool isLandObject = landTiles.Contains(PrefabUtility.GetCorrespondingObjectFromSource(foundObject) as GameObject);
            TerrainGridSystem targetGrid = isLandObject ? landGrid : objectGrid;
            HashSet<int> targetOccupied = isLandObject ? occupiedLandCells : occupiedObjectCells;

            if (targetGrid == null) return;

            float cellSize = targetGrid.cellSize.x;
            Vector3 objPos = foundObject.transform.position;

            Vector3 originPos = objPos;
            originPos.x -= (size.x - 1) * cellSize * 0.5f;
            originPos.z -= (size.y - 1) * cellSize * 0.5f;

            Cell originCell = targetGrid.CellGetAtWorldPosition(originPos, 0);
            if (originCell != null)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        int index = originCell.index + x + (y * targetGrid.columnCount);
                        targetOccupied.Remove(index);
                    }
                }
            }

            string objectName = foundObject.name;
            placedObjects.Remove(foundObject);
            selectedObjects.Remove(foundObject);
            Undo.DestroyObjectImmediate(foundObject);

            Debug.Log($"<color=red>Deleted {objectName} ({size.x}x{size.y})</color>");
        }
        #endregion

        #region Save / Load / Clear
        private void SaveCurrentLayout()
        {
            if (layoutDatabase == null) return;

            CombatLayout layout = layoutDatabase.GetLayout(selectedSlot);
            layout.PlacedObjects.Clear();

            foreach (GameObject obj in placedObjects)
            {
                if (obj == null) continue;

                PlacedObjectData data = new PlacedObjectData
                {
                    Prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject,
                    Position = obj.transform.position,
                    Rotation = obj.transform.rotation,
                    Scale = obj.transform.localScale
                };

                if (data.Prefab != null)
                    layout.PlacedObjects.Add(data);
            }

            EditorUtility.SetDirty(layoutDatabase);
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>Layout saved to Slot {selectedSlot + 1}</color>");
        }

        private void LoadLayout()
        {
            if (layoutDatabase == null) return;

            CombatLayout layout = layoutDatabase.GetLayout(selectedSlot);
            if (layout == null || layout.PlacedObjects.Count == 0) return;

            ClearAllPlacedObjects();

            foreach (PlacedObjectData data in layout.PlacedObjects)
            {
                if (data.Prefab == null) continue;

                GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(data.Prefab);
                placed.transform.position = data.Position;
                placed.transform.rotation = data.Rotation;
                placed.transform.localScale = data.Scale;
                placed.name = data.Prefab.name;

                int placedLayer = LayerMask.NameToLayer("PlacedObjects");
                if (placedLayer != -1)
                    SetLayerRecursively(placed, placedLayer);

                placedObjects.Add(placed);

                bool isLandTile = landTiles.Contains(data.Prefab);
                TerrainGridSystem targetGrid = isLandTile ? landGrid : objectGrid;

                if (targetGrid != null)
                {
                    Cell cell = targetGrid.CellGetAtWorldPosition(data.Position, 0);
                    if (cell != null)
                    {
                        if (isLandTile) occupiedLandCells.Add(cell.index);
                        else occupiedObjectCells.Add(cell.index);
                    }
                }
            }
        }

        private void DeleteCurrentSlot()
        {
            if (layoutDatabase == null) return;

            CombatLayout layout = layoutDatabase.GetLayout(selectedSlot);
            if (layout != null)
            {
                layout.PlacedObjects.Clear();
                layout.LayoutName = "New Layout";
                EditorUtility.SetDirty(layoutDatabase);
                AssetDatabase.SaveAssets();
            }
        }

        private void ClearAllPlacedObjects()
        {
            foreach (GameObject obj in placedObjects)
                if (obj != null) Undo.DestroyObjectImmediate(obj);

            placedObjects.Clear();
            selectedObjects.Clear();
            occupiedLandCells.Clear();
            occupiedObjectCells.Clear();
            DestroyPreview();
        }
        #endregion
    }
}