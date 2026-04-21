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

        [Header("Preview Settings")]
        [SerializeField] private float previewHeight = 0.4f;

        private GameObject currentPreview;
        private bool isPreviewActive = false;

        private void Awake()
        {
            if (tgs == null)
                tgs = TerrainGridSystem.instance;

            Debug.Log("<color=cyan>HarbourBuilderManager Awake - TGS: " + (tgs != null) + "</color>");
        }

        public void StartPreview()
        {
            Debug.Log("<color=yellow>StartPreview() called from slot click</color>");

            if (testTilePrefab == null)
            {
                Debug.LogError("testTilePrefab is NOT assigned in HarbourBuilderManager Inspector!");
                return;
            }

            if (currentPreview != null)
                Destroy(currentPreview);

            currentPreview = Instantiate(testTilePrefab);
            currentPreview.name = "Preview_Tile";

            // Force very visible
            currentPreview.transform.localScale = new Vector3(2.5f, 0.8f, 2.5f);
            currentPreview.transform.position = new Vector3(0, 15, 0); // start high

            var rend = currentPreview.GetComponentInChildren<Renderer>(true);
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 0.9f;
                rend.material.color = new Color(0f, 1f, 0.3f, 0.9f); // bright green for testing
            }
            else
            {
                Debug.LogWarning("Preview tile has no Renderer!");
            }

            isPreviewActive = true;
            Debug.Log("<color=green>✅ Preview instantiated - should now follow mouse</color>");
        }

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
            if (harbourBuildCamera == null || tgs == null || currentPreview == null)
            {
                Debug.LogWarning("UpdatePreviewPosition: Missing camera or tgs");
                return;
            }

            Ray ray = harbourBuildCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
            {
                int cellIndex = tgs.CellGetIndex(hit.point);

                if (cellIndex >= 0)
                {
                    Vector3 cellCenter = tgs.CellGetPosition(cellIndex);
                    cellCenter.y = previewHeight;
                    currentPreview.transform.position = cellCenter;
                }
            }
        }

        private void PlaceTile()
        {
            // ... (same as before)
            if (currentPreview == null || tgs == null) return;

            int cellIndex = tgs.CellGetIndex(currentPreview.transform.position);
            Vector3 placePos = (cellIndex >= 0) ? tgs.CellGetPosition(cellIndex) : currentPreview.transform.position;
            placePos.y = previewHeight;

            if (testTilePrefab != null)
            {
                GameObject placed = Instantiate(testTilePrefab, placePos, Quaternion.identity);
                placed.name = "Placed_LandTile";
                Debug.Log($"<color=green>Tile placed at {placePos}</color>");
            }

            Destroy(currentPreview);
            currentPreview = null;
            isPreviewActive = false;
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