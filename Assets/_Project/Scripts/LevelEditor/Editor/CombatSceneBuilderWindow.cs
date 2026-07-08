using System.Collections.Generic;
using _Project.Scripts.LevelEditor.Data;
using TGS;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.LevelEditor.Editor
{
    public class CombatSceneBuilderWindow : EditorWindow
    {
        // === Core References ===
        private TerrainGridSystem tgs;
        private CombatLayoutDatabase layoutDatabase;

        // === Placement ===
        private GameObject selectedPrefab;
        private GameObject currentPreview;
        private bool isPreviewActive = false;

        // === Tracking ===
        private List<GameObject> placedObjects = new List<GameObject>();
        private HashSet<int> occupiedCells = new HashSet<int>();

        // === Categories ===
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

        // === Save/Load ===
        private int selectedSlot = 0;

        [MenuItem("Tools/Combat Scene Builder")]
        public static void ShowWindow()
        {
            GetWindow<CombatSceneBuilderWindow>("Combat Scene Builder");
        }

        private void OnEnable()
        {
            if (tgs == null)
                tgs = TerrainGridSystem.instance;

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            DestroyPreview();
        }
        
        private Dictionary<string, int> GetCategoryCounts(CombatLayout layout)
        {
            var counts = new Dictionary<string, int>
            {
                { "Land Tiles", 0 },
                { "Turrets", 0 },
                { "Walls", 0 },
                { "Buildings", 0 },
                { "Combat Ships", 0 }
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

        private void OnGUI()
        {
            GUILayout.Label("Combat Scene Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

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

            // === Save / Load Section ===
            EditorGUILayout.Space(15);
            GUILayout.Label("Layout Save / Load", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Slot:", GUILayout.Width(40));
            selectedSlot = EditorGUILayout.IntSlider(selectedSlot, 0, CombatLayoutDatabase.MaxSlots - 1, GUILayout.Width(150));
            GUILayout.Label($"Slot {selectedSlot + 1}", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

// Show breakdown for selected slot
            if (layoutDatabase != null)
            {
                CombatLayout layout = layoutDatabase.GetLayout(selectedSlot);

                if (layout != null && layout.PlacedObjects.Count > 0)
                {
                    var counts = GetCategoryCounts(layout);
                    int totalObjects = layout.PlacedObjects.Count;

                    EditorGUILayout.LabelField($"Total Objects: {totalObjects}", EditorStyles.miniBoldLabel);

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

            if (GUILayout.Button("Save Layout", GUILayout.Height(25)))
            {
                SaveCurrentLayout();
            }

            if (GUILayout.Button("Load Layout", GUILayout.Height(25)))
            {
                LoadLayout();
            }

            if (GUILayout.Button("Delete Slot", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Delete Layout",
                        $"Are you sure you want to delete the layout in Slot {selectedSlot + 1}?",
                        "Delete", "Cancel"))
                {
                    DeleteCurrentSlot();
                }
            }

            EditorGUILayout.EndHorizontal();
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
                {
                    prefabs.Add(null);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
        }

        // ==================== SCENE GUI ====================
        private void OnSceneGUI(SceneView sceneView)
        {
            if (selectedPrefab == null) return;

            Event e = Event.current;
            UpdatePreviewPosition();

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

            if (e.type == EventType.MouseDown && e.button == 1)
            {
                DestroyPreview();
                e.Use();
            }
        }

        // ==================== PREVIEW & PLACEMENT ====================
        private void UpdatePreviewPosition()
        {
            if (selectedPrefab == null || tgs == null || !isPreviewActive || currentPreview == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
            {
                Cell cell = tgs.CellGetAtWorldPosition(hit.point, 0);
                if (cell != null)
                {
                    Vector3 cellCenter = tgs.CellGetPosition(cell.index);
                    cellCenter.y = 0.35f;
                    currentPreview.transform.position = cellCenter;
                }
            }
        }

        private Vector3 GetSnappedWorldPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
            {
                Cell cell = tgs.CellGetAtWorldPosition(hit.point, 0);
                if (cell != null)
                {
                    Vector3 pos = tgs.CellGetPosition(cell.index);
                    pos.y = 0.35f;
                    return pos;
                }
            }
            return Vector3.zero;
        }

        private void PlaceObjectAtPosition(Vector3 position)
        {
            if (selectedPrefab == null || tgs == null) return;

            Cell cell = tgs.CellGetAtWorldPosition(position, 0);
            if (cell == null || occupiedCells.Contains(cell.index)) return;

            GameObject placed = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            placed.transform.position = position;
            placed.name = selectedPrefab.name;

            placedObjects.Add(placed);
            occupiedCells.Add(cell.index);

            Undo.RegisterCreatedObjectUndo(placed, "Place Object");
        }

        private bool IsCellOccupied(Vector3 position)
        {
            if (tgs == null) return false;
            Cell cell = tgs.CellGetAtWorldPosition(position, 0);
            return cell != null && occupiedCells.Contains(cell.index);
        }

        // ==================== PREVIEW ====================
        private void CreatePreview(Vector3 position)
        {
            DestroyPreview();
            isPreviewActive = true;

            currentPreview = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            currentPreview.name = "Preview_" + selectedPrefab.name;
            currentPreview.transform.position = position;

            foreach (var rend in currentPreview.GetComponentsInChildren<Renderer>())
            {
                if (rend.material != null)
                {
                    Color c = rend.material.color;
                    c.a = 0.5f;
                    rend.material.color = c;
                }
            }
        }

        private void ForceCreatePreviewAtMouse()
        {
            if (selectedPrefab == null || tgs == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Cell cell = null;

            if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
                cell = tgs.CellGetAtWorldPosition(hit.point, 0);

            Vector3 pos = (cell != null) ? tgs.CellGetPosition(cell.index) : new Vector3(0, 0.35f, 0);
            pos.y = 0.35f;

            CreatePreview(pos);
        }

        private void DestroyPreview()
        {
            if (currentPreview != null)
            {
                DestroyImmediate(currentPreview);
                currentPreview = null;
            }
            isPreviewActive = false;
        }

        // ==================== SAVE / LOAD ====================
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

                Cell cell = tgs.CellGetAtWorldPosition(data.Position, 0);
                if (cell != null)
                    occupiedCells.Add(cell.index);
            }

            Debug.Log($"<color=cyan>Loaded layout from Slot {selectedSlot + 1}</color>");
        }

        // ==================== CLEAR ====================
        private void ClearAllPlacedObjects()
        {
            foreach (GameObject obj in placedObjects)
            {
                if (obj != null)
                    Undo.DestroyObjectImmediate(obj);
            }

            placedObjects.Clear();
            occupiedCells.Clear();
            DestroyPreview();
        }
    }
}