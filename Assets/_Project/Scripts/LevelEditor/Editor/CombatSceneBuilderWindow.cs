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
        
        private bool needsSceneFocus = false;
        #endregion

        #region Move / Rotate
        private bool isMovingSelection = false;
        private List<GameObject> movingObjects = new List<GameObject>();
        private List<Vector3> originalPositions = new List<Vector3>();
        private List<Quaternion> originalRotations = new List<Quaternion>();
        private int currentRotationSteps = 0;
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        #endregion
        
        #region Minimap
        private bool showMinimap = true;
        private float minimapSize = 220f;          // width & height of the minimap
        private Color landCellColor = new Color(0.2f, 0.7f, 0.3f, 0.85f);
        private Color objectCellColor = new Color(0.3f, 0.55f, 0.95f, 0.9f);
        private Color emptyCellColor = new Color(0.15f, 0.15f, 0.18f, 0.6f);
        private Color borderColor = new Color(0.4f, 0.4f, 0.45f, 1f);
        #endregion

        #region Modes

        private bool isSelectMode = false;

        #endregion

        #region Rectangle Fill
        private bool isDrawingRect = false;
        private Cell rectStartCell = null;
        private Cell rectEndCell = null;
        #endregion

        #region Marquee
        private bool isMarqueeSelecting = false;
        private Vector2 marqueeStartGUI;
        private Vector2 marqueeEndGUI;
        #endregion

        #region Copy / Paste
        private List<GameObject> copiedObjects = new List<GameObject>();
        private List<Vector3> copiedPositions = new List<Vector3>();
        private List<Quaternion> copiedRotations = new List<Quaternion>();
        private Vector3 copyCenter;
        #endregion

        #region Placement & Tracking
        private GameObject selectedPrefab;
        private GameObject currentPreview;
        private bool isPreviewActive = false;
        private bool isPainting = false;
        private Cell lastPaintedCell = null;

        private List<GameObject> placedObjects = new List<GameObject>();
        private HashSet<int> occupiedLandCells = new HashSet<int>();
        private HashSet<int> occupiedObjectCells = new HashSet<int>();
        #endregion

        #region DoubleClick

        private double lastClickTime = 0;
        private GameObject lastClickedObject = null;
        private const float doubleClickThreshold = 0.3f; // seconds

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

        #region Hover Highlight
        private Cell hoveredLandCell = null;
        #endregion
        
        private HashSet<GameObject> lockedObjects = new HashSet<GameObject>();
        private GameObject hoveredObject = null;

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
            UpdateGridVisibility();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            DestroyPreview();
        }
        
        private enum BuilderMode
        {
            Select,
            BuildOnWater,   // Land Grid + landTiles
            BuildOnLand     // Object Grid + walls/buildings/etc.
        }

        private BuilderMode currentMode = BuilderMode.Select;
        
        private enum SelectFilter
        {
            All,
            Walls,
            Turrets,
            Land
        }

        private SelectFilter currentSelectFilter = SelectFilter.All;
        
        #region GUI
        private void OnGUI()
        {
            // ===== Minimap =====
            showMinimap = EditorGUILayout.Foldout(showMinimap, "Minimap", true);
            if (showMinimap)
            {
                Rect minimapRect = GUILayoutUtility.GetRect(minimapSize, minimapSize, GUILayout.ExpandWidth(false));
                DrawMinimap(minimapRect);
                EditorGUILayout.Space(6);
            }
            
            // ===== Tools =====
            GUILayout.Label("Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = selectedObjects.Count > 0;
            if (GUILayout.Button($"Lock Selected ({selectedObjects.Count})", GUILayout.Height(26)))
            {
                foreach (var obj in selectedObjects)
                {
                    if (obj != null)
                        lockedObjects.Add(obj);
                }
                Debug.Log($"<color=orange>Locked {selectedObjects.Count} object(s)</color>");
                selectedObjects.Clear();
            }

            if (GUILayout.Button($"Unlock Selected ({selectedObjects.Count})", GUILayout.Height(26)))
            {
                foreach (var obj in selectedObjects)
                {
                    if (obj != null)
                        lockedObjects.Remove(obj);
                }
                Debug.Log($"<color=cyan>Unlocked {selectedObjects.Count} object(s)</color>");
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            
            // ===== Mode Selection =====
            GUILayout.Label("Mode", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            // Select Mode
            GUI.backgroundColor = currentMode == BuilderMode.Select ? new Color(0.3f, 0.7f, 1f) : Color.white;
            if (GUILayout.Button(currentMode == BuilderMode.Select ? "● Select" : "Select", GUILayout.Height(28)))
            {
                SetMode(BuilderMode.Select);
            }
            GUI.backgroundColor = Color.white;

            // Build on Water (Land Tiles)
            GUI.backgroundColor = currentMode == BuilderMode.BuildOnWater ? new Color(0.2f, 0.8f, 0.9f) : Color.white;
            if (GUILayout.Button(currentMode == BuilderMode.BuildOnWater ? "● Build on Water" : "Build on Water", GUILayout.Height(28)))
            {
                SetMode(BuilderMode.BuildOnWater);
            }
            GUI.backgroundColor = Color.white;

            // Build on Land (Objects)
            GUI.backgroundColor = currentMode == BuilderMode.BuildOnLand ? new Color(0.3f, 0.9f, 0.4f) : Color.white;
            if (GUILayout.Button(currentMode == BuilderMode.BuildOnLand ? "● Build on Land" : "Build on Land", GUILayout.Height(28)))
            {
                SetMode(BuilderMode.BuildOnLand);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
            

            GUI.enabled = selectedObjects.Count > 0 && !isMovingSelection;
            if (GUILayout.Button($"Pick Up Selected ({selectedObjects.Count})", GUILayout.Height(28)))
                StartMovingSelection();
            GUI.enabled = true;

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button($"Delete Selected ({selectedObjects.Count})", GUILayout.Height(28)))
                DeleteSelectedObjects();
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button($"Store Selected ({selectedObjects.Count})", GUILayout.Height(28)))
                Debug.Log($"<color=yellow>Stored {selectedObjects.Count} object(s) — logic coming later</color>");

            EditorGUILayout.EndHorizontal();
            
            // ===== Select Filters (always visible) =====
            // ===== Select Filters (always visible) =====
            GUILayout.Label("Select Filter", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = currentSelectFilter == SelectFilter.All ? new Color(0.3f, 0.7f, 1f) : Color.white;
            if (GUILayout.Button(currentSelectFilter == SelectFilter.All ? "● All" : "All", GUILayout.Height(24)))
            {
                if (currentSelectFilter != SelectFilter.All)
                {
                    currentSelectFilter = SelectFilter.All;
                    selectedObjects.Clear();
                }
            }
            GUI.backgroundColor = Color.white;

            GUI.backgroundColor = currentSelectFilter == SelectFilter.Walls ? new Color(0.3f, 0.7f, 1f) : Color.white;
            if (GUILayout.Button(currentSelectFilter == SelectFilter.Walls ? "● Walls" : "Walls", GUILayout.Height(24)))
            {
                if (currentSelectFilter != SelectFilter.Walls)
                {
                    currentSelectFilter = SelectFilter.Walls;
                    selectedObjects.Clear();
                }
            }
            GUI.backgroundColor = Color.white;

            GUI.backgroundColor = currentSelectFilter == SelectFilter.Turrets ? new Color(0.3f, 0.7f, 1f) : Color.white;
            if (GUILayout.Button(currentSelectFilter == SelectFilter.Turrets ? "● Turrets" : "Turrets", GUILayout.Height(24)))
            {
                if (currentSelectFilter != SelectFilter.Turrets)
                {
                    currentSelectFilter = SelectFilter.Turrets;
                    selectedObjects.Clear();
                }
            }
            GUI.backgroundColor = Color.white;

            GUI.backgroundColor = currentSelectFilter == SelectFilter.Land ? new Color(0.3f, 0.7f, 1f) : Color.white;
            if (GUILayout.Button(currentSelectFilter == SelectFilter.Land ? "● Land" : "Land", GUILayout.Height(24)))
            {
                if (currentSelectFilter != SelectFilter.Land)
                {
                    currentSelectFilter = SelectFilter.Land;
                    selectedObjects.Clear();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);

            GUI.enabled = selectedObjects.Count > 0 && !isMovingSelection;
            if (GUILayout.Button("Select Same Type", GUILayout.Height(26)))
                SelectSameType();
            GUI.enabled = true;

            EditorGUILayout.Space(10);

            // ===== Grid References =====
            GUILayout.Label("Grid References", EditorStyles.boldLabel);
            landGrid = (TerrainGridSystem)EditorGUILayout.ObjectField("Land Grid", landGrid, typeof(TerrainGridSystem), true);
            objectGrid = (TerrainGridSystem)EditorGUILayout.ObjectField("Object Grid", objectGrid, typeof(TerrainGridSystem), true);

            EditorGUILayout.Space(6);

            // ===== Active Grid =====
            GUILayout.Label("Active Grid:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(currentGridType == GridType.Land, "Land Grid", "Button"))
            {
                currentGridType = GridType.Land;
                activeGrid = landGrid;
                UpdateGridVisibility();
            }

            if (GUILayout.Toggle(currentGridType == GridType.Objects, "Object Grid", "Button"))
            {
                currentGridType = GridType.Objects;
                activeGrid = objectGrid;
                UpdateGridVisibility();
            }

            EditorGUILayout.EndHorizontal();

            if (activeGrid == null)
                EditorGUILayout.HelpBox("Please assign the active grid above.", MessageType.Warning);

            EditorGUILayout.Space(10);

            // ===== Copy / Paste =====
            GUI.enabled = selectedObjects.Count > 0;
            if (GUILayout.Button($"Copy Selected ({selectedObjects.Count})", GUILayout.Height(28)))
                CopySelectedObjects();
            GUI.enabled = true;

            GUI.enabled = copiedObjects.Count > 0 && !isMovingSelection;
            if (GUILayout.Button($"Paste ({copiedObjects.Count})", GUILayout.Height(28)))
                StartPasteMode();
            GUI.enabled = true;

            EditorGUILayout.Space(8);
            layoutDatabase = (CombatLayoutDatabase)EditorGUILayout.ObjectField("Layout Database", layoutDatabase, typeof(CombatLayoutDatabase), false);

            EditorGUILayout.Space(10);

            // ===== Categories =====
            EditorGUILayout.Space(10);
            GUILayout.Label("Categories", EditorStyles.boldLabel);

            // Show categories based on current mode
            if (currentMode == BuilderMode.BuildOnWater || currentMode == BuilderMode.Select)
            {
                DrawCategory("Land Tiles", ref showLandTiles, landTiles);
                // Future: DrawCategory("Ships", ref showShips, ships);
                // Future: DrawCategory("Traps", ref showTraps, traps);
            }

            if (currentMode == BuilderMode.BuildOnLand || currentMode == BuilderMode.Select)
            {
                DrawCategory("Walls", ref showWalls, walls);
                DrawCategory("Turrets", ref showTurrets, turrets);
                DrawCategory("Buildings", ref showBuildings, buildings);
                DrawCategory("Combat Ships", ref showCombatShips, combatShips);
            }

            // ===== Currently Selected Prefab =====
            GUILayout.Label("Currently Selected Prefab:", EditorStyles.boldLabel);
            selectedPrefab = (GameObject)EditorGUILayout.ObjectField(selectedPrefab, typeof(GameObject), false);

            if (GUILayout.Button("Cancel Preview / Deselect Prefab"))
            {
                selectedPrefab = null;
                DestroyPreview();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField($"Selected Objects: {selectedObjects.Count}", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("Clear Selection"))
                selectedObjects.Clear();

            EditorGUILayout.Space(12);

            if (GUILayout.Button("Clear Scene", GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog("Clear Scene",
                    "This will delete all placed objects and reset occupation tracking.\nAre you sure?",
                    "Clear Scene", "Cancel"))
                {
                    ClearAllPlacedObjects();
                }
            }

            // ===== Save / Load =====
            EditorGUILayout.Space(12);
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

            // ===== Debug =====
            EditorGUILayout.Space(10);
            GUI.backgroundColor = new Color(1f, 0.6f, 0.1f);
            if (GUILayout.Button("DEBUG: Clear All Occupation Data", GUILayout.Height(22)))
            {
                occupiedLandCells.Clear();
                occupiedObjectCells.Clear();
                Debug.Log("<color=orange>All occupation data cleared</color>");
            }
            GUI.backgroundColor = Color.white;
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
            EditorGUILayout.Space(4);
        }
        #endregion
        
        #region Scene GUI
        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            // Always update hover highlight
            UpdateLandTileHoverHighlight();

            // ===== MOVING SELECTION =====
            if (isMovingSelection)
            {
                if (needsSceneFocus && SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.Focus();
                    needsSceneFocus = false;
                }

                UpdateMovingSelection();
                DrawMovingHighlight();
                DrawLandTileHighlight(true);

                // ----- Keyboard -----
                if (e.type == EventType.KeyDown)
                {
                    Debug.Log($"Key pressed while moving: {e.keyCode}"); // temporary debug

                    if (e.keyCode == KeyCode.R)
                    {
                        RotateMovingSelection();
                        e.Use();
                        sceneView.Repaint();
                        return;
                    }

                    if (e.keyCode == KeyCode.Escape)
                    {
                        CancelMovingSelection();
                        e.Use();
                        return;
                    }
                }

                // ----- Left Click = Place (with debug) -----
                if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    Cell cell = GetCellUnderMouse();
                    Debug.Log($"Left click while moving. Cell found: {(cell != null ? cell.index.ToString() : "NULL")}");

                    if (cell != null)
                    {
                        Vector3 cellPos = activeGrid.CellGetPosition(cell.index);
                        bool hasLand = HasLandTileUnderneath(cellPos);
                        Debug.Log($"Has Land Tile underneath: {hasLand}");

                        if (hasLand)
                        {
                            Debug.Log("Attempting to place...");
                            TryPlaceMovingSelection();
                            e.Use();
                            return;
                        }
                        else
                        {
                            Debug.LogWarning("Place blocked – no Land Tile underneath");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Place blocked – no cell under mouse");
                    }
                }

                // ----- Right Click = Cancel -----
                if (e.type == EventType.MouseDown && e.button == 1)
                {
                    CancelMovingSelection();
                    e.Use();
                    return;
                }

                sceneView.Repaint();
                return;
            }

            // ===== PLACEMENT / PAINT / RECTANGLE / LINE =====
            if (selectedPrefab != null)
            {
                Vector2Int size = GetSelectedObjectSize();
                bool isOneByOne = size == Vector2Int.one;

                if (!isDragging && !isPainting && !isDrawingRect)
                    UpdatePreviewPosition();

                DrawLandTileHighlight(true); // darker cyan while placing

                bool ctrlHeld = e.control;

                // Mouse Down
                if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    Cell cell = GetCellUnderMouse();
                    if (cell != null)
                    {
                        if (ctrlHeld)
                        {
                            isDrawingRect = true;
                            rectStartCell = cell;
                            rectEndCell = cell;
                        }
                        else if (isOneByOne)
                        {
                            isPainting = true;
                            lastPaintedCell = null;
                            TryPaintAtMouse();
                        }
                        else
                        {
                            isDragging = true;
                            dragStartCell = cell;
                            dragPreviewCells.Clear();
                            dragPreviewCells.Add(cell);
                        }
                        e.Use();
                    }
                }

                // Mouse Drag
                if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    if (isDrawingRect)
                    {
                        Cell current = GetCellUnderMouse();
                        if (current != null) rectEndCell = current;
                        e.Use();
                    }
                    else if (isPainting)
                    {
                        TryPaintAtMouse();
                        e.Use();
                    }
                    else if (isDragging)
                    {
                        Cell currentCell = GetCellUnderMouse();
                        if (currentCell != null && dragStartCell != null)
                            dragPreviewCells = GetCellsInLine(dragStartCell, currentCell);
                        e.Use();
                    }
                }

                // Mouse Up
                if (e.type == EventType.MouseUp && e.button == 0)
                {
                    if (isDrawingRect)
                    {
                        FillRectangle();
                        isDrawingRect = false;
                        rectStartCell = null;
                        rectEndCell = null;
                        e.Use();
                    }
                    else if (isPainting)
                    {
                        isPainting = false;
                        lastPaintedCell = null;
                        e.Use();
                    }
                    else if (isDragging)
                    {
                        PlaceWallsAlongLine();
                        isDragging = false;
                        dragStartCell = null;
                        dragPreviewCells.Clear();
                        e.Use();
                    }
                }

                // Right click cancel
                if (e.type == EventType.MouseDown && e.button == 1)
                {
                    isPainting = false;
                    isDragging = false;
                    isDrawingRect = false;
                    lastPaintedCell = null;
                    dragStartCell = null;
                    dragPreviewCells.Clear();
                    rectStartCell = null;
                    rectEndCell = null;
                    DestroyPreview();
                    e.Use();
                }

                if (isDrawingRect) DrawRectanglePreview();
                if (isDragging) DrawDragPreview();

                sceneView.Repaint();
                return;
            }

            // ===== SELECTION MODE =====
            if (currentMode == BuilderMode.Select)
            {
                UpdateHoveredObject();
                DrawHoveredObjectHighlight();

                // Start marquee
                if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    isMarqueeSelecting = true;
                    marqueeStartGUI = e.mousePosition;
                    marqueeEndGUI = e.mousePosition;
                    e.Use();
                }

                if (isMarqueeSelecting)
                {
                    if (e.type == EventType.MouseDrag)
                    {
                        marqueeEndGUI = e.mousePosition;
                        e.Use();
                    }

                    UpdateMarqueeSelection();

                    if (e.type == EventType.MouseUp && e.button == 0)
                    {
                        float dragDistance = Vector2.Distance(marqueeStartGUI, marqueeEndGUI);

                        if (dragDistance > 5f)
                            FinishMarqueeSelection(e.shift);
                        else
                        {
                            isMarqueeSelecting = false;
                            HandleSelectionClick(e.shift);
                        }
                        e.Use();
                    }

                    sceneView.Repaint();
                }

                DrawSelectionHighlights();
            }
            else
            {
                // Make sure marquee is cancelled if we left Select Mode
                isMarqueeSelecting = false;
            }
        }
        #endregion

        #region SetMode

        private void SetMode(BuilderMode newMode)
        {
            currentMode = newMode;

            // Clear placement state when changing mode
            selectedPrefab = null;
            DestroyPreview();
            isPainting = false;
            isDragging = false;
            isDrawingRect = false;
            isMovingSelection = false;

            switch (newMode)
            {
                case BuilderMode.Select:
                    // No grid forced
                    Debug.Log("<color=cyan>Mode → Select</color>");
                    break;

                case BuilderMode.BuildOnWater:
                    currentGridType = GridType.Land;
                    activeGrid = landGrid;
                    UpdateGridVisibility();
                    Debug.Log("<color=cyan>Mode → Build on Water (Land Grid)</color>");
                    break;

                case BuilderMode.BuildOnLand:
                    currentGridType = GridType.Objects;
                    activeGrid = objectGrid;
                    UpdateGridVisibility();
                    Debug.Log("<color=cyan>Mode → Build on Land (Object Grid)</color>");
                    break;
            }
        }

        #endregion
        
        #region Grid Visibility & Highlight
        private void UpdateHoveredObject()
        {
            hoveredObject = null;

            if (currentMode != BuilderMode.Select) return;

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

                // Respect filters + locked
                if (found != null && !IsLocked(found) && MatchesSelectFilter(found))
                {
                    hoveredObject = found;
                }
            }
        }

        private void DrawHoveredObjectHighlight()
        {
            if (hoveredObject == null) return;

            // Don’t draw hover if it’s already selected
            if (selectedObjects.Contains(hoveredObject)) return;

            Bounds bounds = GetObjectBounds(hoveredObject);
            Vector3 size = bounds.size * 1.08f;

            // Softer orange / yellow hover colour so it’s distinct from selection cyan
            Handles.color = new Color(1f, 0.8f, 0.2f, 0.85f);
            Handles.DrawWireCube(bounds.center, size);
        }

        private void UpdateGridVisibility()
        {
            if (landGrid != null)
                landGrid.gameObject.SetActive(currentGridType == GridType.Land);

            if (objectGrid != null)
                objectGrid.gameObject.SetActive(currentGridType == GridType.Objects);
        }

        private void UpdateLandTileHoverHighlight()
        {
            hoveredLandCell = null;
            if (objectGrid == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f, LayerMask.GetMask("ObjectGrid", "LandGrid", "Default")))
            {
                hoveredLandCell = objectGrid.CellGetAtWorldPosition(hit.point, 0);
            }
        }

        private void DrawLandTileHighlight(bool objectAttached)
        {
            if (objectGrid == null) return;

            // When objects are attached to the mouse → highlight under every object
            if (objectAttached && isMovingSelection && movingObjects.Count > 0)
            {
                foreach (GameObject obj in movingObjects)
                {
                    if (obj == null) continue;

                    Cell cell = objectGrid.CellGetAtWorldPosition(obj.transform.position, 0);
                    if (cell == null) continue;

                    Vector3 cellPos = objectGrid.CellGetPosition(cell.index);
                    bool hasLand = HasLandTileUnderneath(cellPos);

                    Color col = hasLand
                        ? new Color(0.1f, 0.75f, 1f, 0.65f)   // Cyan = valid (on land)
                        : new Color(1f, 0.2f, 0.2f, 0.65f);   // Red = invalid (water)

                    DrawSingleCellHighlight(cell, col);
                }
                return;
            }

            // Normal hover (single cell)
            if (hoveredLandCell != null)
            {
                Vector3 cellPos = objectGrid.CellGetPosition(hoveredLandCell.index);
                bool hasLand = HasLandTileUnderneath(cellPos);

                Color col = hasLand
                    ? new Color(0.3f, 0.85f, 1f, 0.4f)     // Light cyan
                    : new Color(1f, 0.25f, 0.25f, 0.4f);   // Light red (water)

                DrawSingleCellHighlight(hoveredLandCell, col);
            }
        }

        private void DrawSingleCellHighlight(Cell cell, Color col)
        {
            if (cell == null || objectGrid == null) return;

            Vector3 pos = objectGrid.CellGetPosition(cell.index);
            float size = objectGrid.cellSize.x * 0.92f;
            pos.y += 0.15f;

            Handles.color = col;

            Vector3[] corners = new Vector3[]
            {
                pos + new Vector3(-size, 0f, -size) * 0.5f,
                pos + new Vector3( size, 0f, -size) * 0.5f,
                pos + new Vector3( size, 0f,  size) * 0.5f,
                pos + new Vector3(-size, 0f,  size) * 0.5f
            };

            Handles.DrawSolidRectangleWithOutline(corners, col, new Color(col.r, col.g, col.b, 0.95f));
        }
        #endregion
        
        #region Helpers
        
        private bool IsLocked(GameObject obj)
        {
            return obj != null && lockedObjects.Contains(obj);
        }
        private Vector3 GetFootprintCenter(Cell originCell, Vector2Int size)
        {
            Vector3 originPos = activeGrid.CellGetPosition(originCell.index);
            float cellSize = activeGrid.cellSize.x;

            Vector3 center = originPos;
            center.x += (size.x - 1) * cellSize * 0.5f;
            center.z += (size.y - 1) * cellSize * 0.5f;
            return center;
        }
        
        private bool IsWall(GameObject obj)
        {
            if (obj == null) return false;
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject;
            if (prefab == null) prefab = obj;
            return walls.Contains(prefab) || walls.Contains(obj);
        }

        private void SelectConnectedWalls(GameObject startWall)
        {
            
            if (startWall == null || objectGrid == null) return;

            selectedObjects.Clear();

            // Get starting cell
            Cell startCell = objectGrid.CellGetAtWorldPosition(startWall.transform.position, 0);
            if (startCell == null) return;

            HashSet<int> visited = new HashSet<int>();
            Queue<Cell> queue = new Queue<Cell>();

            queue.Enqueue(startCell);
            visited.Add(startCell.index);

            // 4-directional offsets (N/S/E/W)
            int[] dCol = { 0, 0, 1, -1 };
            int[] dRow = { 1, -1, 0, 0 };

            while (queue.Count > 0)
            {
                Cell current = queue.Dequeue();

                // Find wall object on this cell
                GameObject wallOnCell = FindWallOnCell(current);
                if (wallOnCell != null && !selectedObjects.Contains(wallOnCell))
                    selectedObjects.Add(wallOnCell);
                
                if (IsLocked(wallOnCell))
                    continue;   // or continue

                // Check 4 neighbours
                for (int i = 0; i < 4; i++)
                {
                    int newCol = current.column + dCol[i];
                    int newRow = current.row + dRow[i];

                    if (newCol < 0 || newRow < 0 || newCol >= objectGrid.columnCount || newRow >= objectGrid.rowCount)
                        continue;

                    int neighbourIndex = newRow * objectGrid.columnCount + newCol;
                    if (visited.Contains(neighbourIndex)) continue;

                    Cell neighbour = objectGrid.cells[neighbourIndex];
                    GameObject neighbourWall = FindWallOnCell(neighbour);

                    if (neighbourWall != null)
                    {
                        visited.Add(neighbourIndex);
                        queue.Enqueue(neighbour);
                    }
                }
            }

            Debug.Log($"<color=cyan>Select Connected → {selectedObjects.Count} wall(s)</color>");
        }

        private GameObject FindWallOnCell(Cell cell)
        {
            if (cell == null) return null;

            Vector3 cellPos = objectGrid.CellGetPosition(cell.index);

            foreach (GameObject obj in placedObjects)
            {
                if (obj == null || !IsWall(obj)) continue;

                // Simple distance check (works well for 1x1 walls)
                if (Vector3.Distance(obj.transform.position, cellPos) < objectGrid.cellSize.x * 0.6f)
                    return obj;
            }
            return null;
        }
        
        private bool MatchesSelectFilter(GameObject obj)
        {
            if (obj == null) return false;

            // All → always true
            if (currentSelectFilter == SelectFilter.All)
                return true;

            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject;
            if (prefab == null) prefab = obj;

            switch (currentSelectFilter)
            {
                case SelectFilter.Walls:
                    return walls.Contains(prefab) || walls.Contains(obj);

                case SelectFilter.Turrets:
                    return turrets.Contains(prefab) || turrets.Contains(obj);

                case SelectFilter.Land:
                    // More robust check for land tiles
                    bool match = landTiles.Contains(prefab) || landTiles.Contains(obj);
                    if (!match)
                    {
                        // Fallback: check by name (helps when prefab references differ)
                        string objName = prefab != null ? prefab.name : obj.name;
                        foreach (var tile in landTiles)
                        {
                            if (tile != null && tile.name == objName)
                                return true;
                        }
                    }
                    return match;

                default:
                    return true;
            }
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

        private bool IsLandObject(GameObject obj)
        {
            if (obj == null) return false;
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject;
            return landTiles.Contains(prefab) || landTiles.Contains(obj);
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
                    if (IsLocked(found))
                        return;   
                    GameObject root = hit.collider.transform.root.gameObject;
                    if (placedObjects.Contains(root))
                        found = root;
                }
                

                if (found != null)
                {
                    // Mode + filter checks (keep your existing ones)
                    if (currentMode != BuilderMode.Select)
                    {
                        bool isLand = IsLandObject(found);
                        if ((currentGridType == GridType.Land && !isLand) ||
                            (currentGridType == GridType.Objects && isLand))
                            return;
                    }

                    if (!MatchesSelectFilter(found))
                        return;

                    // ===== Double-click detection for Select Connected =====
                    double timeSinceLast = EditorApplication.timeSinceStartup - lastClickTime;
                    bool isDoubleClick = timeSinceLast < doubleClickThreshold && lastClickedObject == found;

                    lastClickTime = EditorApplication.timeSinceStartup;
                    lastClickedObject = found;

                    if (isDoubleClick && IsWall(found))
                    {
                        SelectConnectedWalls(found);
                        return;
                    }

                    // Normal single-click selection
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

        private void SelectSameType()
        {
            if (selectedObjects.Count == 0) return;

            GameObject reference = selectedObjects[0];
            if (reference == null) return;

            GameObject referencePrefab = PrefabUtility.GetCorrespondingObjectFromSource(reference) as GameObject;
            if (referencePrefab == null) referencePrefab = reference;

            selectedObjects.Clear();

            foreach (GameObject obj in placedObjects)
            {
                if (IsLocked(obj))
                    continue;
                if (obj == null) continue;

                GameObject objPrefab = PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject;
                if (objPrefab == null) objPrefab = obj;

                if (objPrefab == referencePrefab)
                    selectedObjects.Add(obj);
            }

            Debug.Log($"<color=cyan>Selected {selectedObjects.Count} object(s) of the same type</color>");
        }

        private void DrawSelectionHighlights()
        {
            if (selectedObjects.Count == 0) return;

            // Bright cyan with good opacity
            Color lineColor = new Color(0.1f, 0.85f, 1f, 0.95f);
            Color fillColor = new Color(0.1f, 0.75f, 1f, 0.12f);

            foreach (GameObject obj in selectedObjects)
            {
                if (obj == null) continue;

                Bounds bounds = GetObjectBounds(obj);

                // Slightly expand so it’s clearly outside the object
                Vector3 size = bounds.size * 1.12f;
                Vector3 center = bounds.center;

                // Soft fill
                Handles.color = fillColor;
                Handles.DrawSolidRectangleWithOutline(
                    new Vector3[]
                    {
                        center + new Vector3(-size.x, 0, -size.z) * 0.5f,
                        center + new Vector3( size.x, 0, -size.z) * 0.5f,
                        center + new Vector3( size.x, 0,  size.z) * 0.5f,
                        center + new Vector3(-size.x, 0,  size.z) * 0.5f
                    },
                    fillColor,
                    Color.clear
                );

                // Strong outline
                Handles.color = lineColor;
                Handles.DrawWireCube(center, size);

                // Extra thick top outline for better visibility
                Handles.DrawAAPolyLine(5f,
                    center + new Vector3(-size.x, size.y, -size.z) * 0.5f,
                    center + new Vector3( size.x, size.y, -size.z) * 0.5f,
                    center + new Vector3( size.x, size.y,  size.z) * 0.5f,
                    center + new Vector3(-size.x, size.y,  size.z) * 0.5f,
                    center + new Vector3(-size.x, size.y, -size.z) * 0.5f
                );
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

        #region Move / Rotate System
        private void StartMovingSelection()
        {
            if (selectedObjects.Count == 0) return;

            isMovingSelection = true;
            movingObjects.Clear();
            originalPositions.Clear();
            originalRotations.Clear();
            currentRotationSteps = 0;

            foreach (GameObject obj in selectedObjects)
            {
                if (obj == null) continue;

                movingObjects.Add(obj);
                originalPositions.Add(obj.transform.position);
                originalRotations.Add(obj.transform.rotation);
                FreeObjectCells(obj);
            }

            MakeObjectsTransparent(movingObjects);
            selectedObjects.Clear();
            
            // Force Scene view to take keyboard focus so R works immediately
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.Focus();
            }

            needsSceneFocus = true;
            
            Debug.Log($"<color=cyan>Picked up {movingObjects.Count} object(s). R = Rotate | Space = Place | Right Click / Esc = Cancel</color>");
        }

        private void UpdateMovingSelection()
        {
            if (movingObjects.Count == 0 || activeGrid == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, LayerMask.GetMask("ObjectGrid", "LandGrid")))
                return;

            Cell targetCell = activeGrid.CellGetAtWorldPosition(hit.point, 0);
            if (targetCell == null) return;

            Vector3 targetCenter = activeGrid.CellGetPosition(targetCell.index);

            // Use the first object as the stable reference
            GameObject reference = null;
            foreach (var obj in movingObjects)
            {
                if (obj != null)
                {
                    reference = obj;
                    break;
                }
            }
            if (reference == null) return;

            Vector3 referencePos = reference.transform.position;
            targetCenter.y = referencePos.y;

            // Delta from reference to target cell
            Vector3 delta = targetCenter - referencePos;

            // Snap the delta to whole cells so the whole group stays aligned
            float cellSize = activeGrid.cellSize.x;
            delta.x = Mathf.Round(delta.x / cellSize) * cellSize;
            delta.z = Mathf.Round(delta.z / cellSize) * cellSize;

            // Apply the same delta to every object (preserves relative spacing)
            foreach (var obj in movingObjects)
            {
                if (obj != null)
                    obj.transform.position += delta;
            }
        }

        private void RotateMovingSelection()
        {
            if (movingObjects.Count == 0) return;

            currentRotationSteps = (currentRotationSteps + 1) % 4;

            Vector3 center = Vector3.zero;
            foreach (GameObject obj in movingObjects)
                center += obj.transform.position;
            center /= movingObjects.Count;

            foreach (GameObject obj in movingObjects)
            {
                Vector3 dir = obj.transform.position - center;
                dir = Quaternion.Euler(0, 90, 0) * dir;
                obj.transform.position = center + dir;
                obj.transform.Rotate(0, 90, 0);
            }
        }

        private void TryPlaceMovingSelection()
        {
            if (movingObjects.Count == 0) return;

            // First pass – check if EVERY object is valid
            foreach (GameObject obj in movingObjects)
            {
                if (obj == null) continue;

                Cell cell = activeGrid.CellGetAtWorldPosition(obj.transform.position, 0);
                if (cell == null)
                {
                    Debug.LogWarning("Place cancelled – one or more objects are not on a valid cell.");
                    return;
                }

                Vector3 cellPos = activeGrid.CellGetPosition(cell.index);
                if (!HasLandTileUnderneath(cellPos))
                {
                    Debug.LogWarning("Place cancelled – one or more objects are over water.");
                    return;
                }
            }

            // Second pass – all objects are valid, so place them
            foreach (GameObject obj in movingObjects)
            {
                if (obj == null) continue;

                PlaceableObject placeable = obj.GetComponent<PlaceableObject>();
                Vector2Int size = placeable != null ? placeable.sizeInCells : Vector2Int.one;

                bool isLand = IsLandObject(obj);
                TerrainGridSystem targetGrid = isLand ? landGrid : objectGrid;
                HashSet<int> targetOccupied = isLand ? occupiedLandCells : occupiedObjectCells;

                if (targetGrid == null) continue;

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
                            targetOccupied.Add(index);
                        }
                    }
                }
            }

            RestoreOriginalMaterials();
            movingObjects.Clear();
            isMovingSelection = false;
            Debug.Log("<color=green>Placed moved selection</color>");
        }

        private void CancelMovingSelection()
        {
            RestoreOriginalMaterials();
            UpdateLandTileHoverHighlight();
            if (movingObjects.Count == 0)
            {
                isMovingSelection = false;
                return;
            }

            bool cameFromPaste = movingObjects[0] != null && movingObjects[0].name.Contains("_Pasted");

            if (cameFromPaste)
            {
                foreach (GameObject obj in movingObjects)
                {
                    if (obj != null)
                    {
                        placedObjects.Remove(obj);
                        Undo.DestroyObjectImmediate(obj);
                    }
                }
                Debug.Log("<color=orange>Paste cancelled — temporary objects destroyed</color>");
            }
            else
            {
                for (int i = 0; i < movingObjects.Count; i++)
                {
                    if (movingObjects[i] == null) continue;
                    movingObjects[i].transform.position = originalPositions[i];
                    movingObjects[i].transform.rotation = originalRotations[i];
                }
                Debug.Log("<color=orange>Move cancelled — objects restored</color>");
            }

            movingObjects.Clear();
            originalPositions.Clear();
            originalRotations.Clear();
            isMovingSelection = false;
        }

        private void FreeObjectCells(GameObject obj)
        {
            PlaceableObject placeable = obj.GetComponent<PlaceableObject>();
            Vector2Int size = placeable != null ? placeable.sizeInCells : Vector2Int.one;

            bool isLand = IsLandObject(obj);
            TerrainGridSystem targetGrid = isLand ? landGrid : objectGrid;
            HashSet<int> targetOccupied = isLand ? occupiedLandCells : occupiedObjectCells;

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

        private void MakeObjectsTransparent(List<GameObject> objects)
        {
            originalMaterials.Clear(); // we reuse this dictionary just to track which renderers we disabled

            foreach (GameObject obj in objects)
            {
                if (obj == null) continue;

                foreach (Renderer rend in obj.GetComponentsInChildren<Renderer>())
                {
                    if (rend == null) continue;

                    // Store that this renderer was enabled
                    originalMaterials[rend] = null; // we only need the key
                    rend.enabled = false;           // fully hide it
                }
            }
        }

        private void RestoreOriginalMaterials()
        {
            foreach (var kvp in originalMaterials)
            {
                if (kvp.Key != null)
                    kvp.Key.enabled = true; // turn renderers back on
            }
            originalMaterials.Clear();
        }

        private void DrawMovingHighlight()
        {
            if (!isMovingSelection || movingObjects.Count == 0) return;

            Handles.color = new Color(0.2f, 0.9f, 1f, 0.9f);

            foreach (GameObject obj in movingObjects)
            {
                if (obj == null) continue;
                Bounds bounds = GetObjectBounds(obj);
                Handles.DrawWireCube(bounds.center, bounds.size * 1.08f);
            }
        }
        #endregion
        
        #region Paint / Continuous Place
        private void TryPaintAtMouse()
        {
            if (selectedPrefab == null || activeGrid == null) return;

            Vector2Int size = GetSelectedObjectSize();
            if (size != Vector2Int.one) return; // paint is only for 1x1

            Cell cell = GetCellUnderMouse();
            if (cell == null) return;

            if (lastPaintedCell != null && lastPaintedCell.index == cell.index)
                return;

            if (!IsFootprintValid(cell, size)) return;

            Vector3 pos = GetFootprintCenter(cell, size);

            GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            placed.transform.position = pos;
            placed.name = selectedPrefab.name;

            int placedLayer = LayerMask.NameToLayer("PlacedObjects");
            if (placedLayer != -1)
                SetLayerRecursively(placed, placedLayer);

            placedObjects.Add(placed);
            MarkFootprintOccupied(cell, size);
            Undo.RegisterCreatedObjectUndo(placed, "Paint Object");

            lastPaintedCell = cell;
        }
        #endregion

        #region Rectangle / Box Fill
        private List<Cell> GetCellsInRectangle(Cell start, Cell end)
        {
            List<Cell> cells = new List<Cell>();
            if (start == null || end == null || activeGrid == null) return cells;

            int minCol = Mathf.Min(start.column, end.column);
            int maxCol = Mathf.Max(start.column, end.column);
            int minRow = Mathf.Min(start.row, end.row);
            int maxRow = Mathf.Max(start.row, end.row);

            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    int index = row * activeGrid.columnCount + col;
                    if (index >= 0 && index < activeGrid.numCells)
                        cells.Add(activeGrid.cells[index]);
                }
            }
            return cells;
        }

        private void DrawRectanglePreview()
        {
            if (!isDrawingRect || rectStartCell == null || rectEndCell == null || activeGrid == null)
                return;

            Vector3 startPos = activeGrid.CellGetPosition(rectStartCell.index);
            Vector3 endPos = activeGrid.CellGetPosition(rectEndCell.index);
            float cellSize = activeGrid.cellSize.x * 0.5f;

            float minX = Mathf.Min(startPos.x, endPos.x) - cellSize;
            float maxX = Mathf.Max(startPos.x, endPos.x) + cellSize;
            float minZ = Mathf.Min(startPos.z, endPos.z) - cellSize;
            float maxZ = Mathf.Max(startPos.z, endPos.z) + cellSize;
            float y = startPos.y + 0.2f;

            Vector3[] corners = new Vector3[]
            {
                new Vector3(minX, y, minZ),
                new Vector3(maxX, y, minZ),
                new Vector3(maxX, y, maxZ),
                new Vector3(minX, y, maxZ)
            };

            Handles.color = new Color(0.1f, 1f, 0.3f, 1f);
            Handles.DrawAAPolyLine(6f, corners[0], corners[1], corners[2], corners[3], corners[0]);
            Handles.color = new Color(0.1f, 1f, 0.3f, 0.15f);
            Handles.DrawSolidRectangleWithOutline(corners, new Color(0.1f, 1f, 0.3f, 0.15f), new Color(0.1f, 1f, 0.3f, 0.8f));
        }

        private void FillRectangle()
        {
            if (rectStartCell == null || rectEndCell == null || selectedPrefab == null) return;

            List<Cell> cells = GetCellsInRectangle(rectStartCell, rectEndCell);
            Vector2Int size = GetSelectedObjectSize();
            int placedCount = 0;

            foreach (Cell cell in cells)
            {
                if (cell == null || !IsFootprintValid(cell, size)) continue;

                Vector3 pos = GetFootprintCenter(cell, size);

                GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
                placed.transform.position = pos;
                placed.name = selectedPrefab.name;

                int placedLayer = LayerMask.NameToLayer("PlacedObjects");
                if (placedLayer != -1)
                    SetLayerRecursively(placed, placedLayer);

                placedObjects.Add(placed);
                MarkFootprintOccupied(cell, size);
                Undo.RegisterCreatedObjectUndo(placed, "Fill Rectangle");
                placedCount++;
            }

            if (placedCount > 0)
                Debug.Log($"<color=green>Filled rectangle with {placedCount} objects</color>");
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

                Vector3 pos = GetFootprintCenter(cell, size);

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
                Debug.Log($"<color=green>Placed {placedCount} objects in a line</color>");
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

        #region Marquee
        private void UpdateMarqueeSelection()
        {
            Event e = Event.current;
            marqueeEndGUI = e.mousePosition;

            // Only draw during Repaint
            if (e.type != EventType.Repaint) return;

            Rect rect = GetMarqueeRect();

            Handles.BeginGUI();

            // Semi-transparent fill
            Color oldColor = GUI.color;
            GUI.color = new Color(0.2f, 0.6f, 1f, 0.25f);
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);

            // Border
            GUI.color = new Color(0.2f, 0.6f, 1f, 0.95f);
            Handles.DrawSolidRectangleWithOutline(rect, Color.clear, new Color(0.2f, 0.6f, 1f, 0.95f));

            GUI.color = oldColor;
            Handles.EndGUI();
        }

        private Rect GetMarqueeRect()
        {
            float xMin = Mathf.Min(marqueeStartGUI.x, marqueeEndGUI.x);
            float yMin = Mathf.Min(marqueeStartGUI.y, marqueeEndGUI.y);
            float width = Mathf.Abs(marqueeEndGUI.x - marqueeStartGUI.x);
            float height = Mathf.Abs(marqueeEndGUI.y - marqueeStartGUI.y);

            return new Rect(xMin, yMin, width, height);
        }

        private void FinishMarqueeSelection(bool addToSelection)
        {
            Rect guiRect = GetMarqueeRect();

            if (guiRect.width < 5f && guiRect.height < 5f)
            {
                isMarqueeSelecting = false;
                return;
            }

            if (!addToSelection)
                selectedObjects.Clear();

            Camera cam = SceneView.lastActiveSceneView?.camera;
            if (cam == null)
            {
                isMarqueeSelecting = false;
                return;
            }

            foreach (GameObject obj in placedObjects)
            {
                if (IsLocked(obj))
                    continue;
                if (obj == null) continue;

                // Only apply grid filter when NOT in Select Mode
                if (currentMode != BuilderMode.Select)
                {
                    bool isLand = IsLandObject(obj);
                    if ((currentGridType == GridType.Land && !isLand) ||
                        (currentGridType == GridType.Objects && isLand))
                        continue;
                }
                
                // NEW: Select Filter
                if (!MatchesSelectFilter(obj))
                    continue;

                Vector3 screenPos = cam.WorldToScreenPoint(obj.transform.position);
                Vector2 guiPos = new Vector2(screenPos.x, cam.pixelHeight - screenPos.y);

                if (guiRect.Contains(guiPos))
                {
                    if (!selectedObjects.Contains(obj))
                        selectedObjects.Add(obj);
                }
            }

            isMarqueeSelecting = false;
            Debug.Log($"<color=cyan>Marquee selected {selectedObjects.Count} object(s)</color>");
        }
        #endregion

        #region Copy / Paste
        private void CopySelectedObjects()
        {
            if (selectedObjects.Count == 0)
            {
                Debug.LogWarning("No objects selected to copy.");
                return;
            }

            copiedObjects.Clear();
            copiedPositions.Clear();
            copiedRotations.Clear();

            copyCenter = Vector3.zero;
            int count = 0;

            foreach (GameObject obj in selectedObjects)
            {
                if (obj == null) continue;

                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject;
                if (prefab == null) prefab = obj;

                copiedObjects.Add(prefab);
                copiedPositions.Add(obj.transform.position);
                copiedRotations.Add(obj.transform.rotation);

                copyCenter += obj.transform.position;
                count++;
            }

            if (count > 0)
                copyCenter /= count;

            Debug.Log($"<color=cyan>Copied {copiedObjects.Count} object(s)</color>");
        }

        private void StartPasteMode()
        {
            if (copiedObjects.Count == 0) return;

            List<GameObject> newObjects = new List<GameObject>();

            for (int i = 0; i < copiedObjects.Count; i++)
            {
                GameObject prefab = copiedObjects[i];
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = prefab.name + "_Pasted";
                instance.transform.position = copiedPositions[i];
                instance.transform.rotation = copiedRotations[i];

                int placedLayer = LayerMask.NameToLayer("PlacedObjects");
                if (placedLayer != -1)
                    SetLayerRecursively(instance, placedLayer);

                newObjects.Add(instance);
                placedObjects.Add(instance);
            }

            if (newObjects.Count == 0) return;

            isMovingSelection = true;
            movingObjects.Clear();
            originalPositions.Clear();
            originalRotations.Clear();
            currentRotationSteps = 0;

            foreach (GameObject obj in newObjects)
            {
                movingObjects.Add(obj);
                originalPositions.Add(obj.transform.position);
                originalRotations.Add(obj.transform.rotation);
            }

            copiedObjects.Clear();
            copiedPositions.Clear();
            copiedRotations.Clear();
            
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.Focus();
            
            needsSceneFocus = true;

            Debug.Log($"<color=cyan>Paste → moved into Pick Up mode ({movingObjects.Count} objects). R = Rotate | Space = Place | Right Click / Esc = Cancel</color>");
        }
        #endregion

        #region Preview System
        private void UpdatePreviewPosition()
        {
            if (selectedPrefab == null || activeGrid == null || !isPreviewActive || currentPreview == null)
                return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            int mask = LayerMask.GetMask("LandGrid", "ObjectGrid", "Default");
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f, mask))
                return;

            Cell originCell = activeGrid.CellGetAtWorldPosition(hit.point, 0);
            if (originCell == null) return;

            Vector2Int size = GetSelectedObjectSize();
            float cellSize = activeGrid.cellSize.x;

            // Bottom-left (origin) cell position
            Vector3 originPos = activeGrid.CellGetPosition(originCell.index);

            // Correct center of the whole footprint
            Vector3 footprintCenter = originPos;
            footprintCenter.x += (size.x - 1) * cellSize * 0.5f;
            footprintCenter.z += (size.y - 1) * cellSize * 0.5f;
            footprintCenter.y = originPos.y + 0.1f;

            currentPreview.transform.position = footprintCenter;

            bool isValid = IsFootprintValid(originCell, size);
            Color targetColor = isValid ? GetPreviewColorForPrefab(selectedPrefab) : new Color(1f, 0.15f, 0.15f);
            targetColor.a = 0.55f;
            SetPreviewColor(targetColor);
        }

        private void CreatePreview(Vector3 pos)
        {
            DestroyPreview();
            isPreviewActive = true;

            currentPreview = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            currentPreview.name = "Preview_" + selectedPrefab.name;
            currentPreview.transform.position = pos;

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

        #region MiniMap

        private void DrawMinimap(Rect rect)
        {
            if (landGrid == null && objectGrid == null) return;

            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.14f, 1f));

            List<Vector3> landWorld = new List<Vector3>();
            List<Vector3> objectWorld = new List<Vector3>();

            if (landGrid != null)
            {
                foreach (int index in occupiedLandCells)
                    landWorld.Add(landGrid.CellGetPosition(index));
            }

            if (objectGrid != null)
            {
                foreach (int index in occupiedObjectCells)
                    objectWorld.Add(objectGrid.CellGetPosition(index));
            }

            if (landWorld.Count == 0 && objectWorld.Count == 0)
            {
                GUI.Label(rect, "No objects placed", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            // World bounds
            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            void Encapsulate(Vector3 p)
            {
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                minZ = Mathf.Min(minZ, p.z);
                maxZ = Mathf.Max(maxZ, p.z);
            }

            foreach (var p in landWorld) Encapsulate(p);
            foreach (var p in objectWorld) Encapsulate(p);

            // Padding so content isn’t stuck to the edge
            float pad = 1.2f;
            minX -= pad; maxX += pad;
            minZ -= pad; maxZ += pad;

            float worldW = Mathf.Max(0.01f, maxX - minX);
            float worldH = Mathf.Max(0.01f, maxZ - minZ);

            float cellPixelSize = Mathf.Min(rect.width / worldW, rect.height / worldH);
            cellPixelSize = Mathf.Clamp(cellPixelSize * 1.05f, 5f, 20f);

// Helper
            Vector2 WorldToMinimap(Vector3 p)
            {
                float nx = (p.x - minX) / worldW;          // X stays X
                float ny = (p.z - minZ) / worldH;          // Z becomes Y

                float x = rect.x + (1f - nx) * rect.width;
                float y = rect.y + ny * rect.height;        // no flip    // keep your current Y flip

                return new Vector2(x, y);
            }

// Draw land (slightly larger so gaps are smaller)
            float landSize = cellPixelSize * 1.05f;
            foreach (var p in landWorld)
            {
                Vector2 mp = WorldToMinimap(p);
                EditorGUI.DrawRect(new Rect(mp.x - landSize * 0.5f, mp.y - landSize * 0.5f, landSize, landSize), landCellColor);
            }

// Draw objects
            float objSize = cellPixelSize * 0.82f;
            foreach (var p in objectWorld)
            {
                Vector2 mp = WorldToMinimap(p);
                EditorGUI.DrawRect(new Rect(mp.x - objSize * 0.5f, mp.y - objSize * 0.5f, objSize, objSize), objectCellColor);
            }

// Thin grid overlay
            Handles.BeginGUI();
            Handles.color = new Color(1f, 1f, 1f, 0.08f);
            int gridLines = 12;
            for (int i = 0; i <= gridLines; i++)
            {
                float t = i / (float)gridLines;
                // vertical
                float x = rect.x + t * rect.width;
                Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.yMax));
                // horizontal
                float y = rect.y + t * rect.height;
                Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.xMax, y));
            }

// Border
            Handles.color = borderColor;
            Handles.DrawSolidRectangleWithOutline(rect, Color.clear, borderColor);
            Handles.EndGUI();
        }

        #endregion

        #region Delete
        private void DeleteSelectedObjects()
        {
            if (selectedObjects.Count == 0)
            {
                Debug.LogWarning("No objects selected to delete.");
                return;
            }

            List<GameObject> toDelete = new List<GameObject>(selectedObjects);

            foreach (GameObject obj in toDelete)
            {
                if (obj == null) continue;

                PlaceableObject placeable = obj.GetComponent<PlaceableObject>();
                Vector2Int size = placeable != null ? placeable.sizeInCells : Vector2Int.one;

                bool isLand = IsLandObject(obj);
                TerrainGridSystem targetGrid = isLand ? landGrid : objectGrid;
                HashSet<int> targetOccupied = isLand ? occupiedLandCells : occupiedObjectCells;

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