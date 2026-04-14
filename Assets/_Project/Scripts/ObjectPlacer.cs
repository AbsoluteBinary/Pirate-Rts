using TGS;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Scripts
{
    public class ObjectPlacer : MonoBehaviour
    {
        public GameObject prefabToInstantiate;
        public Button spawnButton;
        public LayerMask terrainLayer;

        private GameObject instantiatedObject;
        private bool isAttached = false;

        [SerializeField] public Camera buildCamera;
        [SerializeField] TMP_Text ValueText;

        public int tempPlacementcnt;
        public int placementcnt;
        public int buildLimtcnt = 5;

        private TerrainGridSystem tgs;

        void Start()
        {
            ValueText.text = buildLimtcnt.ToString();
            tgs = TerrainGridSystem.instance;
            tgs.OnCellClick += PlaceObject;

            if (spawnButton != null)
                spawnButton.onClick.AddListener(OnSpawnButtonClick);
        }

        public void StartTilePreview(GameObject tilePrefab)
        {
            if (tilePrefab == null) return;

            if (instantiatedObject != null)
                Destroy(instantiatedObject);

            Vector3 spawnPos = GetMouseWorldPosition();
            if (spawnPos == Vector3.zero)
                spawnPos = Vector3.up * 5f;

            instantiatedObject = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
            isAttached = true;

            var rend = instantiatedObject.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 0.65f;
                rend.material.color = c;
            }

            Debug.Log("<color=cyan>ObjectPlacer: Preview started - snapping to TGS cells</color>");
        }

        void Update()
        {
            if (isAttached && instantiatedObject != null)
            {
                MoveObjectWithMouse();
            }

            if (isAttached && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelPreview();
            }
        }

        Vector3 GetMouseWorldPosition()
        {
            if (buildCamera == null || Mouse.current == null) return Vector3.zero;

            Ray ray = buildCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                return hit.point;
            }
            return Vector3.zero;
        }

        void MoveObjectWithMouse()
        {
            if (tgs == null) return;

            Vector3 hitPoint = GetMouseWorldPosition();
            if (hitPoint == Vector3.zero) return;

            int cellIndex = tgs.CellGetIndex(hitPoint);
            if (cellIndex >= 0)
            {
                Vector3 cellCenter = tgs.CellGetPosition(cellIndex);
                cellCenter.y = 0.2f;
                instantiatedObject.transform.position = cellCenter;
            }
        }

        public void ConfirmCurrentPlacement()
        {
            if (!isAttached || instantiatedObject == null || tgs == null) return;

            int cell = tgs.CellGetIndex(instantiatedObject.transform.position);
            if (cell >= 0)
            {
                PlaceObject(tgs, cell, 0);
            }

            isAttached = false;
            instantiatedObject = null;
        }

        public void CancelPreview()
        {
            if (instantiatedObject != null)
            {
                Destroy(instantiatedObject);
                instantiatedObject = null;
            }
            isAttached = false;
            Debug.Log("<color=yellow>Preview cancelled</color>");
        }

        // Your existing methods (unchanged)
        void OnSpawnButtonClick()
        {
            // ... your original spawn button logic if still needed ...
        }

        public void PlaceObject(TerrainGridSystem grid, int cell, int button)
        {
            if (isAttached)
            {
                if (button == 0 || button == 1) // left or right click handling
                {
                    placementcnt += 1;
                    instantiatedObject.transform.position = tgs.CellGetPosition(cell);
                    instantiatedObject = null;
                    tempPlacementcnt -= 1;
                    buildLimtcnt -= 1;

                    if (buildLimtcnt == 0 && spawnButton != null)
                        spawnButton.interactable = false;

                    ValueText.text = buildLimtcnt.ToString();
                }
            }
        }
    }
}