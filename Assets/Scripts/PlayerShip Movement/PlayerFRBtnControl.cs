using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TGS;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayerShip_Movement
{
    public class PlayerFRBtnControl : MonoBehaviour
    {
        [SerializeField] private TerrainGridSystem tgs;
        [SerializeField] private GameObject markerPrefab; // Prefab for placement
        [SerializeField, Range(1, 10)] private int timeRemaining = 2;
        [SerializeField] private PlayerShipController shipController; // Reference to PlayerShipController

        private GameObject activeMarker; // Instance of the prefab
        private Tween moveTween;
        private CancellationTokenSource cts;

        private void Awake()
        {
            cts = new CancellationTokenSource();
            if (markerPrefab == null)
            {
                Debug.LogError("PlayerFRBtnControl: markerPrefab is not assigned!");
                return;
            }
            Debug.Log($"PlayerFRBtnControl: markerPrefab ({markerPrefab.name}) has UIDocument: {markerPrefab.GetComponent<UIDocument>() != null}, Canvas: {markerPrefab.GetComponent<Canvas>() != null}, BoxCollider: {markerPrefab.GetComponent<BoxCollider>() != null}");
            if (markerPrefab.GetComponent<UIDocument>() != null)
            {
                Debug.Log($"PlayerFRBtnControl: markerPrefab UIDocument Visual Tree Asset: {markerPrefab.GetComponent<UIDocument>().visualTreeAsset?.name}");
            }
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
                Debug.Log($"PlayerFRBtnControl: Destroying previous marker (InstanceID: {activeMarker.GetInstanceID()})");
                Destroy(activeMarker);
                activeMarker = null;
            }

            // Instantiate new marker
            activeMarker = Instantiate(markerPrefab);
            Debug.Log($"PlayerFRBtnControl: Spawned new marker at cell {cellIndex}. InstanceID: {activeMarker.GetInstanceID()}");

            // Update PlayerShipController's frButton
            if (shipController != null)
            {
                shipController.SetFrButton(activeMarker);
            }
            else
            {
                Debug.LogWarning("PlayerFRBtnControl: PlayerShipController is not assigned.");
            }

            Vector3 cellPos = tgs.CellGetPosition(cellIndex, true, 0);
            activeMarker.transform.position = cellPos + Vector3.up * 10;

            moveTween?.Kill();
            moveTween = activeMarker.transform.DOMove(cellPos + Vector3.up * 0.5f, 1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => StartUniTaskCountdown().Forget());
        }

        private async UniTask StartUniTaskCountdown()
        {
            if (!activeMarker)
            {
                Debug.LogWarning("PlayerFRBtnControl: No active marker during countdown.");
                return;
            }

            Debug.Log($"PlayerFRBtnControl: Starting countdown for {timeRemaining} seconds");
            await UniTask.Delay(timeRemaining * 1000, cancellationToken: cts.Token);

            if (activeMarker)
            {
                activeMarker.SetActive(false);
                Debug.Log("PlayerFRBtnControl: Marker deactivated after countdown");
            }
        }
    }
}