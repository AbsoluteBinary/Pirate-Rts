using _Project.Scripts.Harbour.Data;
using _Project.Scripts.Harbour.Data.SO;
using TGS;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts
{
    public class HarbourBuilderManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera harbourBuildCamera;
        [SerializeField] private TerrainGridSystem tgs;
        [SerializeField] private GameObject testTilePrefab;
        private HarbourHUD _harbourHUD;
        private int _currentSlotIndex = -1;

        [Header("Preview Settings")]
        [SerializeField] private float previewHeight = 0.35f;

        private GameObject currentPreview;
        private bool isPreviewActive = false;
        

        private void Awake()
        {
            if (tgs == null)
                tgs = TerrainGridSystem.instance;
        }
        
        [Header("Tile List - Scriptable Object")]
        [SerializeField] private LandTileInventorySO landTileInventory;

        public void StartPreview(int slotIndex)
        {
            _currentSlotIndex = slotIndex;
            Debug.Log($"<color=magenta>StartPreview() called for slot {slotIndex}</color>");

            if (landTileInventory == null || landTileInventory.tiles == null)
            {
                Debug.LogError("LandTileInventorySO or tiles list is not assigned!");
                return;
            }

            if (slotIndex < 0 || slotIndex >= landTileInventory.tiles.Count)
            {
                Debug.LogError($"Slot index {slotIndex} is out of range in LandTileInventorySO!");
                return;
            }

            LandTileInventorySO.TileEntry entry = landTileInventory.tiles[slotIndex];

            if (entry == null || entry.prefab == null)   // ← Change "prefab" if your field name is different
            {
                Debug.LogError($"No prefab found at index {slotIndex} in TileEntry!");
                return;
            }

            if (currentPreview != null)
                Destroy(currentPreview);

            currentPreview = Instantiate(entry.prefab);
            currentPreview.name = "Preview_Tile";

            // Force visible
            currentPreview.transform.localScale = new Vector3(2.5f, 0.8f, 2.5f);
            currentPreview.transform.position = new Vector3(0, 15, 0);

            var rend = currentPreview.GetComponentInChildren<Renderer>(true);
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 0.9f;
                rend.material.color = new Color(0f, 1f, 0.3f, 0.9f); // bright green for testing
            }

            isPreviewActive = true;
            Debug.Log($"<color=green>✅ Pulled tile from SO index {slotIndex} and attached to cursor</color>");
        }

        // public void StartPreview()
        // {
        //     Debug.Log("<color=magenta>StartPreview() called from slot click</color>");
        //
        //     if (testTilePrefab == null)
        //     {
        //         Debug.LogError("testTilePrefab is NOT assigned!");
        //         return;
        //     }
        //
        //     if (currentPreview != null)
        //         Destroy(currentPreview);
        //
        //     currentPreview = Instantiate(testTilePrefab);
        //     currentPreview.name = "Preview_Tile";
        //
        //     currentPreview.transform.localScale = new Vector3(2.5f, 0.8f, 2.5f);
        //     currentPreview.transform.position = new Vector3(0, 15, 0);
        //
        //     var rend = currentPreview.GetComponentInChildren<Renderer>(true);
        //     if (rend != null)
        //     {
        //         Color c = rend.material.color;
        //         c.a = 0.9f;
        //         rend.material.color = new Color(0f, 1f, 0.3f, 0.9f);
        //     }
        //
        //     isPreviewActive = true;
        //     Debug.Log("<color=green>✅ TestTile instantiated - following mouse + TGS centering</color>");
        // }

        private void Update()
        {
            if (!isPreviewActive || currentPreview == null) return;

            UpdatePreviewPosition();

            if (Mouse.current.rightButton.wasPressedThisFrame)
                PlaceTile();

            if (Mouse.current.leftButton.wasPressedThisFrame)
                CancelPreview();
        }

        private void UpdatePreviewPosition()
        {
            if (harbourBuildCamera == null) return;

            Ray ray = harbourBuildCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Reliable plane for mouse following
            Plane plane = new Plane(Vector3.up, new Vector3(0, previewHeight, 0));
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);

                // Snap to nearest TGS cell center
                if (tgs != null)
                {
                    int cellIndex = tgs.CellGetIndex(worldPos);
                    if (cellIndex >= 0)
                    {
                        Vector3 cellCenter = tgs.CellGetPosition(cellIndex);
                        cellCenter.y = previewHeight;
                        currentPreview.transform.position = cellCenter;
                        return;
                    }
                }

                // Fallback if TGS fails
                currentPreview.transform.position = worldPos;
            }
        }

        private void PlaceTile()
        {
            if (currentPreview == null) return;

            Vector3 placePos = currentPreview.transform.position;

            if (tgs != null)
            {
                int cellIndex = tgs.CellGetIndex(placePos);
                if (cellIndex >= 0)
                {
                    placePos = tgs.CellGetPosition(cellIndex);
                    placePos.y = previewHeight;
                }
            }

            if (testTilePrefab != null)
            {
                GameObject placed = Instantiate(testTilePrefab, placePos, Quaternion.identity);
                placed.name = "Placed_LandTile";

                var rend = placed.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    Color c = rend.material.color;
                    c.a = 1f;
                    rend.material.color = c;
                }

                Debug.Log($"<color=green>Land Tile placed centered on TGS cell at {placePos}</color>");
            }

            // === CORRECTED MVVM LOGIC ===
            if (landTileInventory != null && _currentSlotIndex >= 0)
            {
                if (landTileInventory.ConsumeTile(_currentSlotIndex))
                {
                    if (_harbourHUD != null)
                        _harbourHUD.RefreshSlotCount(_currentSlotIndex);

                    Debug.Log($"<color=green>MVVM: Tile consumed from slot {_currentSlotIndex}. Remaining: {landTileInventory.GetCount(_currentSlotIndex)}</color>");
                }
            }

            Destroy(currentPreview);
            currentPreview = null;
            isPreviewActive = false;
            _currentSlotIndex = -1;   // reset
        }

        private void CancelPreview()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }
            isPreviewActive = false;
            Debug.Log("<color=yellow>Preview cancelled</color>");
        }
    }
}