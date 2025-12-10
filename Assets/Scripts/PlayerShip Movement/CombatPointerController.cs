using System.Threading;
using DG.Tweening;
using TGS;
using UnityEngine;

namespace PlayerShip_Movement
{
    public class CombatPointerController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private TerrainGridSystem tgs;
        [SerializeField] private GameObject pointerPrefab;
        [SerializeField, Range(0.5f, 10f)] private float fadeDuration = 0.8f;

        [Header("Visual Feedback")]
        [SerializeField] private float dropHeight = 15f;           // Start drop from sky
        private static float finalHeightAboveWater = 1f;  // ← ADJUST THIS: Final Y height on water
        [SerializeField] private float dropDuration = 0.8f;
        [SerializeField] private Ease dropEase = Ease.OutBounce;

        [Header("References")]
        [SerializeField] private PlayerShipController shipController;

        private GameObject currentPointer;
        private CancellationTokenSource cts;

        private void Awake()
        {
            if (pointerPrefab == null)
                Debug.LogError("CombatPointerController: pointerPrefab is not assigned!");
        }

        private void Start()
        {
            tgs = tgs ? tgs : TerrainGridSystem.instance;
            tgs.OnCellClick += OnCellClick;
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
            if (currentPointer) Destroy(currentPointer);
        }

        private void OnCellClick(TerrainGridSystem sender, int cellIndex, int buttonIndex)
        {
            if (buttonIndex != 1) return;  // Right click only

            // Cancel any existing
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            if (currentPointer) Destroy(currentPointer);

            // Get world position
            Vector3 targetPos = tgs.CellGetPosition(cellIndex);

            // Spawn + drop
            Vector3 spawnPos = targetPos + Vector3.up * dropHeight;
            currentPointer = Instantiate(pointerPrefab, spawnPos, Quaternion.identity);
            currentPointer.transform.LookAt(Camera.main.transform.position + Vector3.up * 2f);
            currentPointer.transform.rotation = Quaternion.Euler(0, currentPointer.transform.eulerAngles.y, 0);

            // Drop → land → SHIP MOVES IMMEDIATELY + fade pointer
            currentPointer.transform
                .DOMoveY(targetPos.y + finalHeightAboveWater, dropDuration)  // ← Uses new variable
                .SetEase(dropEase)
                .OnComplete(() =>
                {
                    shipController?.MoveToLocation(targetPos);

                    // Fade/destroy pointer quickly
                    var rend = currentPointer.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material.DOFade(0f, fadeDuration).OnComplete(() => Destroy(currentPointer));
                    }
                    else
                    {
                        Destroy(currentPointer);
                    }
                });
        }
    }
}