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

        #region Grid Selection
        private enum GridType { Land, Objects }
        private GridType currentGridType = GridType.Land;
        #endregion

        #region Save / Load
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

            // === Grid Assignment ===
            GUILayout.Label("Grid References", EditorStyles.boldLabel);

            landGrid = (TerrainGridSystem)EditorGUILayout.ObjectField(
                "Land Grid", landGrid, typeof(TerrainGridSystem), true);

            objectGrid = (TerrainGridSystem)EditorGUILayout.ObjectField(
                "Object Grid", objectGrid, typeof(TerrainGridSystem), true);

            EditorGUILayout.Space(8);

            // === Active Grid Toggle ===
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

            // === Layout Database ===
            layoutDatabase = (CombatLayoutDatabase)EditorGUILayout.ObjectField(
                "Layout Database", layoutDatabase, typeof(CombatLayoutDatabase), false);

            EditorGUILayout.Space(10);

            // === Categories ===
            DrawCategory("Land Tiles", ref showLandTiles, landTiles);
            DrawCategory("Turrets", ref showTurrets, turrets);
            DrawCategory("Walls", ref showWalls, walls);
            DrawCategory("Buildings", ref showBuildings, buildings);
            DrawCategory("Combat Ships", ref showCombatShips, combatShips);

            EditorGUILayout.Space(10);

            // === Currently Selected ===
            GUILayout.Label("Currently Selected:", EditorStyles.boldLabel);
            selectedPrefab = (GameObject)EditorGUILayout.ObjectField(selectedPrefab, typeof(GameObject), false);

            EditorGUILayout.Space();

            if (GUILayout.Button("Cancel Preview"))
            {
                selectedPrefab = null;
                DestroyPreview();
            }

            // === Clear Scene ===
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

            // === Save / Load Section ===
            EditorGUILayout.Space(15);
            GUILayout.Label("Layout Save / Load", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Slot:", GUILayout.Width(40));
            selectedSlot = EditorGUILayout.IntSlider(selectedSlot, 0, CombatLayoutDatabase.MaxSlots - 1, GUILayout.Width(150));
            GUILayout.Label($"Slot {selectedSlot + 1}", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            if (layoutDatabase != null)
            {
                CombatLayout layout = layoutDatabase.GetLayout(selectedSlot);
                if (layout != null && layout.PlacedObjects.Count > 0)
                {
                    var counts = GetCategoryCounts(layout);
                    EditorGUILayout.LabelField($"Total Objects: {layout.PlacedObjects.Count}", EditorStyles.miniBoldLabel);
                    foreach (var kvp in counts)
                    {
                        if (kvp.Value > 0)
                            EditorGUILayout.LabelField($"   {kvp.Key}: {kvp.Value}");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Empty slot", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Layout", GUILayout.Height(25))) SaveCurrentLayout();
            if (GUILayout.Button("Load Layout", GUILayout.Height(25))) LoadLayout();
            if (GUILayout.Button("Delete Slot", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Delete Layout",
                    $"Delete layout in Slot {selectedSlot + 1}?", "Delete", "Cancel"))
                {
                    DeleteCurrentSlot();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Category UI
        private void DrawCategory(string title, ref bool foldout, List<GameObject> prefabs)
        {
            foldout = EditorGUILayout.Foldout(foldout, title, true);

            if (foldout)
            {
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
        }
        #endregion

        #region Scene GUI & Placement
        private void OnSceneGUI(SceneView sceneView)
        {
            if (selectedPrefab == null || activeGrid == null) return;

            Event e = Event.current;
            UpdatePreviewPosition();

            // Left Click = Place
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                Vector3 placePos = GetSnappedWorldPosition();

                if (placePos != Vector3.zero && !IsCellOccupied(placePos))
                {
                    PlaceObjectAtPosition(placePos);
                    e.Use();
                }
                else if (IsCellOccupied(placePos))
                {
                    Debug.LogWarning("<color=orange>Cell is occupied!</color>");
                }
            }

            // Right Click = Cancel Preview
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                DestroyPreview();
                e.Use();
            }
        }

        private void UpdatePreviewPosition()
        {
            if (selectedPrefab == null || activeGrid == null || !isPreviewActive || currentPreview == null)
                return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 5000f)) return;

            Cell cell = activeGrid.CellGetAtWorldPosition(hit.point, 0);
            if (cell == null) return;

            // Move preview
            Vector3 cellCenter = activeGrid.CellGetPosition(cell.index);
            currentPreview.transform.position = cellCenter;

            // === Visual Feedback ===
            bool isValid = IsPlacementValid(cellCenter);
            Color targetColor = isValid
                ? GetPreviewColorForPrefab(selectedPrefab)
                : new Color(1f, 0.15f, 0.15f); // Bright red

            targetColor.a = 0.55f;
            SetPreviewColor(targetColor);
        }

        private Vector3 GetSnappedWorldPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
            {
                Cell cell = activeGrid.CellGetAtWorldPosition(hit.point, 0);
                if (cell != null)
                {
                    return activeGrid.CellGetPosition(cell.index);
                }
            }
            return Vector3.zero;
        }

        private void PlaceObjectAtPosition(Vector3 position)
        {
            if (selectedPrefab == null || activeGrid == null) return;

            Cell cell = activeGrid.CellGetAtWorldPosition(position, 0);
            if (cell == null) return;

            if (currentGridType == GridType.Objects)
            {
                if (!HasLandTileUnderneath(position))
                {
                    Debug.LogWarning("<color=orange>Cannot place object — no Land Tile underneath!</color>");
                    return;
                }

                if (occupiedObjectCells.Contains(cell.index))
                {
                    Debug.LogWarning("<color=orange>This object cell is already occupied!</color>");
                    return;
                }
            }
            else // Land Grid
            {
                if (occupiedLandCells.Contains(cell.index))
                {
                    Debug.LogWarning("<color=orange>Land cell is already occupied!</color>");
                    return;
                }
            }

            // Place
            GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            placed.transform.position = position;
            placed.name = selectedPrefab.name;

            placedObjects.Add(placed);

            if (currentGridType == GridType.Land)
                occupiedLandCells.Add(cell.index);
            else
                occupiedObjectCells.Add(cell.index);

            Undo.RegisterCreatedObjectUndo(placed, "Place Object");
            Debug.Log($"<color=green>Placed {selectedPrefab.name} on {currentGridType} Grid - Cell {cell.index}</color>");
        }

        private bool IsCellOccupied(Vector3 position)
        {
            if (activeGrid == null) return true;

            Cell cell = activeGrid.CellGetAtWorldPosition(position, 0);
            if (cell == null) return true;

            if (currentGridType == GridType.Land)
                return occupiedLandCells.Contains(cell.index);

            // Object Grid
            if (!HasLandTileUnderneath(position)) return true;
            return occupiedObjectCells.Contains(cell.index);
        }

        private bool IsPlacementValid(Vector3 position)
        {
            if (activeGrid == null) return false;

            Cell cell = activeGrid.CellGetAtWorldPosition(position, 0);
            if (cell == null) return false;

            if (currentGridType == GridType.Land)
                return !occupiedLandCells.Contains(cell.index);

            // Object Grid
            return HasLandTileUnderneath(position) && !occupiedObjectCells.Contains(cell.index);
        }

        /// <summary>
        /// Checks if there is a Land Tile under the given world position.
        /// Projects the position down to the Land Grid height.
        /// </summary>
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

            if (landCell == null) return false;

            return occupiedLandCells.Contains(landCell.index);
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

            Color previewColor = GetPreviewColorForPrefab(selectedPrefab);
            previewColor.a = 0.55f;
            SetPreviewColor(previewColor);
        }

        private void SetPreviewColor(Color color)
        {
            if (currentPreview == null) return;

            foreach (var rend in currentPreview.GetComponentsInChildren<Renderer>())
            {
                if (rend.sharedMaterial != null)
                {
                    Material tempMat = new Material(rend.sharedMaterial);
                    tempMat.color = color;
                    rend.material = tempMat;
                }
            }
        }

        private Color GetPreviewColorForPrefab(GameObject prefab)
        {
            if (landTiles.Contains(prefab)) return new Color(0.2f, 0.85f, 0.35f);     // Green
            if (turrets.Contains(prefab)) return new Color(0.95f, 0.3f, 0.3f);        // Red
            if (walls.Contains(prefab)) return new Color(0.35f, 0.55f, 0.95f);        // Blue
            if (buildings.Contains(prefab)) return new Color(0.95f, 0.65f, 0.2f);     // Orange
            if (combatShips.Contains(prefab)) return new Color(0.7f, 0.3f, 0.9f);     // Purple

            return new Color(0.75f, 0.75f, 0.75f); // Default gray
        }

        private void ForceCreatePreviewAtMouse()
        {
            if (selectedPrefab == null || activeGrid == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Cell cell = null;

            if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
                cell = activeGrid.CellGetAtWorldPosition(hit.point, 0);

            Vector3 pos = (cell != null)
                ? activeGrid.CellGetPosition(cell.index)
                : Vector3.zero;

            CreatePreview(pos);
        }

        private void DestroyPreview()
        {
            if (currentPreview != null)
            {
                foreach (var rend in currentPreview.GetComponentsInChildren<Renderer>())
                {
                    if (rend.material != null)
                        DestroyImmediate(rend.material);
                }

                DestroyImmediate(currentPreview);
                currentPreview = null;
            }
            isPreviewActive = false;
        }
        #endregion

        #region Save / Load
        private void SaveCurrentLayout()
        {
            if (layoutDatabase == null)
            {
                Debug.LogError("No Layout Database assigned!");
                return;
            }

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
            if (layout == null || layout.PlacedObjects.Count == 0)
            {
                Debug.LogWarning($"Slot {selectedSlot + 1} is empty.");
                return;
            }

            ClearAllPlacedObjects();

            foreach (PlacedObjectData data in layout.PlacedObjects)
            {
                if (data.Prefab == null) continue;

                GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(data.Prefab);
                placed.transform.position = data.Position;
                placed.transform.rotation = data.Rotation;
                placed.transform.localScale = data.Scale;
                placed.name = data.Prefab.name;

                placedObjects.Add(placed);

                bool isLandTile = landTiles.Contains(data.Prefab);
                TerrainGridSystem targetGrid = isLandTile ? landGrid : objectGrid;

                if (targetGrid != null)
                {
                    Cell cell = targetGrid.CellGetAtWorldPosition(data.Position, 0);
                    if (cell != null)
                    {
                        if (isLandTile)
                            occupiedLandCells.Add(cell.index);
                        else
                            occupiedObjectCells.Add(cell.index);
                    }
                }
            }

            Debug.Log($"<color=cyan>Loaded layout from Slot {selectedSlot + 1} " +
                      $"(Land: {occupiedLandCells.Count}, Objects: {occupiedObjectCells.Count})</color>");
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
                Debug.Log($"<color=red>Deleted layout in Slot {selectedSlot + 1}</color>");
            }
        }

        private Dictionary<string, int> GetCategoryCounts(CombatLayout layout)
        {
            var counts = new Dictionary<string, int>
            {
                { "Land Tiles", 0 }, { "Turrets", 0 }, { "Walls", 0 },
                { "Buildings", 0 }, { "Combat Ships", 0 }
            };

            if (layout == null) return counts;

            foreach (var data in layout.PlacedObjects)
            {
                if (data.Prefab == null) continue;

                if (landTiles.Contains(data.Prefab)) counts["Land Tiles"]++;
                else if (turrets.Contains(data.Prefab)) counts["Turrets"]++;
                else if (walls.Contains(data.Prefab)) counts["Walls"]++;
                else if (buildings.Contains(data.Prefab)) counts["Buildings"]++;
                else if (combatShips.Contains(data.Prefab)) counts["Combat Ships"]++;
            }
            return counts;
        }
        #endregion

        #region Clear Scene
        private void ClearAllPlacedObjects()
        {
            foreach (GameObject obj in placedObjects)
            {
                if (obj != null)
                    Undo.DestroyObjectImmediate(obj);
            }

            placedObjects.Clear();
            occupiedLandCells.Clear();
            occupiedObjectCells.Clear();
            DestroyPreview();

            Debug.Log("<color=yellow>Scene cleared. Both grids reset.</color>");
        }
        #endregion
    }
}
