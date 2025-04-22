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
        [SerializeField] private GameObject activeObject;
        [SerializeField, Range(1, 10)] private int timeRemaining = 2;

        private Tween moveTween;
        private CancellationTokenSource cts;

        private void Awake()
        {
            cts = new CancellationTokenSource();
            activeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            activeObject.layer = LayerMask.NameToLayer("UI");
            activeObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = Color.green, renderQueue = 3200 };
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
            tgs.OnCellClick -= OnCellClick;
            if (activeObject) Destroy(activeObject);
        }

        private void OnCellClick(TerrainGridSystem sender, int cellIndex, int buttonIndex)
        {
            if (buttonIndex != 1) return;

            Vector3 cellPos = tgs.CellGetPosition(cellIndex, true, 0);
            activeObject.transform.position = cellPos + Vector3.up * 10;

            moveTween?.Kill();
            moveTween = activeObject.transform.DOMove(cellPos, 1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => StartUniTaskCountdown().Forget());
        }

        private async UniTask StartUniTaskCountdown()
        {
            await UniTask.Delay(timeRemaining * 1000, cancellationToken: cts.Token);
            activeObject.transform.position = new Vector3(0, 110, 0);
        }
    }
}