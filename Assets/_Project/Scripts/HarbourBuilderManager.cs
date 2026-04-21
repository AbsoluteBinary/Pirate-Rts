using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts
{
    public class HarbourBuilderManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera harbourBuildCamera;

        private GameObject currentPreview;
        private bool isPreviewActive = false;
        [SerializeField] private GameObject testTilePrefab;

        public void StartPreview()
        {
            Debug.Log("<color=magenta>HarbourBuilderManager.StartPreview() CALLED</color>");

            if (testTilePrefab == null)
            {
                Debug.LogError("testTilePrefab is NOT assigned in HarbourBuilderManager Inspector!");
                return;
            }

            if (currentPreview != null)
                Destroy(currentPreview);

            currentPreview = Instantiate(testTilePrefab);
            currentPreview.name = "Preview_Tile";

            // Force visible scale and starting position
            currentPreview.transform.localScale = new Vector3(2.5f, 0.8f, 2.5f);
            currentPreview.transform.position = new Vector3(0, 15, 0);

            // Make it bright green for testing
            var rend = currentPreview.GetComponentInChildren<Renderer>(true);
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 0.9f;
                rend.material.color = new Color(0f, 1f, 0.3f, 0.9f);
            }

            isPreviewActive = true;
            Debug.Log("<color=green>✅ TestTile instantiated - should now follow mouse cursor</color>");
        }

        private void Update()
        {
            if (!isPreviewActive || currentPreview == null) return;

            FollowMouseCursor();
        }

        private void FollowMouseCursor()
        {
            if (harbourBuildCamera == null || Mouse.current == null) return;

            Ray ray = harbourBuildCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Plane at fixed height above water (very reliable)
            Plane plane = new Plane(Vector3.up, new Vector3(0, 0.4f, 0));

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 pos = ray.GetPoint(distance);
                currentPreview.transform.position = pos;
            }
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