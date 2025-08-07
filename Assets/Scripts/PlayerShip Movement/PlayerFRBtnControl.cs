using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TGS;
using UnityEngine;

namespace PlayerShip_Movement
{
    public class PlayerFRBtnControl : MonoBehaviour
    {
        [SerializeField] private TerrainGridSystem tgs;
        [SerializeField] private GameObject markerPrefab; // Prefab for placement
        [SerializeField, Range(1, 10)] private int timeRemaining = 2;

        private GameObject activeMarker; // Instance of the prefab
        private Tween moveTween;
        private CancellationTokenSource cts;

        private void Awake()
        {
            cts = new CancellationTokenSource();
        }

        private void Start()
        {
            tgs = tgs ? tgs : TerrainGridSystem.instance;
            tgs.OnCellClick += OnCellClick;
        }

        private void OnDestroy()
        {
            moveTween?.Kill();
            cts?.Cancel();
            cts?.Dispose();
            if (activeMarker) Destroy(activeMarker);
        }

        private void OnCellClick(TerrainGridSystem sender, int cellIndex, int buttonIndex)
        {
            if (buttonIndex != 1) return;

            // Cancel previous countdown and reset CTS
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            // Destroy previous marker
            if (activeMarker)
            {
                Destroy(activeMarker);
                activeMarker = null;
            }

            // Instantiate new marker
            activeMarker = Instantiate(markerPrefab);
            Debug.Log($"Spawned new marker at cell {cellIndex}");

            Vector3 cellPos = tgs.CellGetPosition(cellIndex, true, 0);
            activeMarker.transform.position = cellPos + Vector3.up * 10;

            moveTween?.Kill();
            moveTween = activeMarker.transform.DOMove(cellPos + Vector3.up * 0.5f, 1f) // Above water
                .SetEase(Ease.OutQuad)
                .OnComplete(() => StartUniTaskCountdown().Forget());
        }

        private async UniTask StartUniTaskCountdown()
        {
            if (!activeMarker)
            {
                Debug.LogWarning("No active marker during countdown.");
                return;
            }

            Debug.Log($"Starting countdown for {timeRemaining} seconds");
            await UniTask.Delay(timeRemaining * 1000, cancellationToken: cts.Token);

            if (activeMarker)
            {
                activeMarker.SetActive(false);
                Debug.Log("Marker deactivated after countdown");
            }
        }
    }
}