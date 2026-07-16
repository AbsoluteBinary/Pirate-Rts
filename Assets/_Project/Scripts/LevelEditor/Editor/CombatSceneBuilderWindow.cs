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
        private bool isDeleteMode = false;
        private int selectedSlot = 0;
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

            // Tools
            GUILayout.Label("Tools", EditorStyles.boldLabel);

            Color originalColor = GUI.backgroundColor;
            if (isDeleteMode) GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);

            if (GUILayout.Button(isDeleteMode ? "Delete Mode: ON" : "Delete Mode: OFF", GUILayout.Height(28)))
            {
                isDeleteMode = !isDeleteMode;
                selectedPrefab = null;
                DestroyPreview();
            }

            GUI.backgroundColor = originalColor;

            if (isDeleteMode)
                EditorGUILayout.HelpBox("Delete Mode Active — Left click any placed object to delete it.", MessageType.Warning);

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

            // Currently Selected
            GUILayout.Label("Currently Selected:", EditorStyles.boldLabel);
            selectedPrefab = (GameObject)EditorGUILayout.ObjectField(selectedPrefab, typeof(GameObject), false);

            if (GUILayout.Button("Cancel Preview"))
            {
                selectedPrefab = null;
                DestroyPreview();
            }

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

            if (isDeleteMode)
            {
                if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    TryDeleteObjectAtMouse();
                    e.Use();
                }
                return;
            }

            if (selectedPrefab == null || activeGrid == null) return;

            UpdatePreviewPosition();

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                Vector3 placePos = GetSnappedWorldPosition();
                if (placePos != Vector3.zero)
                {
                    PlaceObjectAtPosition(placePos);
                    e.Use();
                }
            }

            if (e.type == EventType.MouseDown && e.button == 1)
            {
                DestroyPreview();
                e.Use();
            }
        }
        #endregion

        #region Helpers
        private int GetCurrentLayerMask()
        {
            return currentGridType == GridType.Land
                ? LayerMask.GetMask("LandGrid")
                : LayerMask.GetMask("ObjectGrid");
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
        #endregion

        #region Placement
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
            if (!IsFootprintValid(originCell, size))
            {
                Debug.LogWarning("<color=orange>Cannot place — footprint is blocked or missing Land Tile!</color>");
                return;
            }

            Vector3 originPos = activeGrid.CellGetPosition(originCell.index);
            float cellSize = activeGrid.cellSize.x;

            Vector3 footprintCenter = originPos;
            footprintCenter.x += (size.x - 1) * cellSize * 0.5f;
            footprintCenter.z += (size.y - 1) * cellSize * 0.5f;

            GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            placed.transform.position = footprintCenter;
            placed.name = selectedPrefab.name;

            // Important: Put on PlacedObjects layer so Delete Mode can find it
            int placedLayer = LayerMask.NameToLayer("PlacedObjects");
            if (placedLayer != -1)
                SetLayerRecursively(placed, placedLayer);

            placedObjects.Add(placed);
            MarkFootprintOccupied(originCell, size);

            Undo.RegisterCreatedObjectUndo(placed, "Place Object");
            Debug.Log($"<color=green>Placed {selectedPrefab.name} ({size.x}x{size.y})</color>");
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
        #endregion

        #region Preview
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

            // Only hit objects on the PlacedObjects layer
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
                // Fallback to root
                GameObject root = hit.collider.transform.root.gameObject;
                if (placedObjects.Contains(root))
                    foundObject = root;
            }

            if (foundObject == null)
            {
                Debug.Log("Clicked object is not a placed builder object.");
                return;
            }

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

            // Capture the name before destroying
            string objectName = foundObject.name;

            placedObjects.Remove(foundObject);
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

            Debug.Log($"<color=cyan>Loaded layout from Slot {selectedSlot + 1}</color>");
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
            occupiedLandCells.Clear();
            occupiedObjectCells.Clear();
            DestroyPreview();
        }
        #endregion
    }
}
