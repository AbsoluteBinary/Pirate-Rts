using _Project.Scripts.Harbour.Data;
using _Project.Scripts.Harbour.Data.SO;
using TGS;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts
{
    public class HarbourBuilderManager : MonoBehaviour
    {
    //     [Header("References")]
    //     [SerializeField] private Camera harbourBuildCamera;
    //     [SerializeField] private TerrainGridSystem tgs;
    //     [SerializeField] private GameObject testTilePrefab;
    //     private HarbourHUD _harbourHUD;
    //     private int _currentSlotIndex = -1;
    //
    //     [Header("Preview Settings")]
    //     [SerializeField] private float previewHeight = 0.35f;
    //
    //     private GameObject currentPreview;
    //     private bool isPreviewActive = false;
    //     
    //
    //     private void Awake()
    //     {
    //         if (tgs == null)
    //             tgs = TerrainGridSystem.instance;
    //     }
    //     
    //     [Header("Tile List - Scriptable Object")]
    //     [SerializeField] private LandTileInventorySO landTileInventory;
    //
    //     public void StartPreview(int slotIndex)
    //     {
    //         _currentSlotIndex = slotIndex;
    //
    //         Debug.Log($"<color=magenta>StartPreview() called for slot {slotIndex}</color>");
    //
    //         if (testTilePrefab == null)
    //         {
    //             Debug.LogError("testTilePrefab is NOT assigned!");
    //             return;
    //         }
    //
    //         if (currentPreview != null)
    //             Destroy(currentPreview);
    //
    //         currentPreview = Instantiate(testTilePrefab);
    //         currentPreview.name = "Preview_Tile";
    //
    //         currentPreview.transform.localScale = new Vector3(2.5f, 0.8f, 2.5f);
    //         currentPreview.transform.position = new Vector3(0, 15, 0);
    //
    //         var rend = currentPreview.GetComponentInChildren<Renderer>(true);
    //         if (rend != null)
    //         {
    //             Color c = rend.material.color;
    //             c.a = 0.9f;
    //             rend.material.color = new Color(0f, 1f, 0.3f, 0.9f);
    //         }
    //
    //         isPreviewActive = true;
    //         Debug.Log($"<color=green>✅ Preview attached to cursor for slot {slotIndex}</color>");
    //     }
    //
    //     private void Update()
    //     {
    //         if (!isPreviewActive || currentPreview == null) return;
    //
    //         UpdatePreviewPosition();     // <--- This must be called
    //
    //         if (Mouse.current.rightButton.wasPressedThisFrame)
    //             PlaceTile();
    //
    //         if (Mouse.current.leftButton.wasPressedThisFrame)
    //             CancelPreview();
    //     }
    //
    //     private void UpdatePreviewPosition()
    //     {
    //         if (harbourBuildCamera == null || tgs == null) return;
    //
    //         Ray ray = harbourBuildCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
    //
    //         if (Physics.Raycast(ray, out RaycastHit hit, 3000f))
    //         {
    //             Cell cell = tgs.CellGetAtPosition(hit.point, true);
    //
    //             if (cell != null)
    //             {
    //                 Vector3 cellCenter = tgs.CellGetPosition(cell.index);
    //                 cellCenter.y = previewHeight;
    //                 currentPreview.transform.position = cellCenter;
    //                 return;
    //             }
    //         }
    //
    //         // Fallback to flat plane
    //         Plane plane = new Plane(Vector3.up, previewHeight);
    //         if (plane.Raycast(ray, out float distance))
    //         {
    //             currentPreview.transform.position = ray.GetPoint(distance);
    //         }
    //     }
    //
    //     private void PlaceTile()
    //     {
    //         if (currentPreview == null) return;
    //
    //         Vector3 placePos = currentPreview.transform.position;
    //
    //         if (tgs != null)
    //         {
    //             Cell cell = tgs.CellGetAtPosition(placePos, true);
    //             if (cell != null)
    //             {
    //                 placePos = tgs.CellGetPosition(cell.index);
    //                 placePos.y = previewHeight;
    //             }
    //         }
    //
    //         if (testTilePrefab != null)
    //         {
    //             GameObject placed = Instantiate(testTilePrefab, placePos, Quaternion.identity);
    //             placed.name = "Placed_LandTile";
    //
    //             var rend = placed.GetComponentInChildren<Renderer>();
    //             if (rend != null)
    //             {
    //                 Color c = rend.material.color;
    //                 c.a = 1f;
    //                 rend.material.color = c;
    //             }
    //         }
    //
    //         // MVVM Counter Update
    //         if (landTileInventory != null && _currentSlotIndex >= 0)
    //         {
    //             if (landTileInventory.ConsumeTile(_currentSlotIndex))
    //             {
    //                 if (_harbourHUD != null)
    //                     _harbourHUD.RefreshSlotCount(_currentSlotIndex);
    //             }
    //         }
    //
    //         Destroy(currentPreview);
    //         currentPreview = null;
    //         isPreviewActive = false;
    //         _currentSlotIndex = -1;
    //     }
    //
    //     private void CancelPreview()
    //     {
    //         if (currentPreview != null)
    //         {
    //             Destroy(currentPreview);
    //             currentPreview = null;
    //         }
    //         isPreviewActive = false;
    //         Debug.Log("<color=yellow>Preview cancelled</color>");
    //     }
     }
}