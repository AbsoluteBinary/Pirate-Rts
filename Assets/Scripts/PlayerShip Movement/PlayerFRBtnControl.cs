using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TGS;
using UnityEngine;
using UnityEngine.UIElements; // For UI Toolkit Button

namespace PlayerShip_Movement
{
    public class PlayerFRBtnControl : MonoBehaviour
    {
        [SerializeField] private TerrainGridSystem tgs;
        [SerializeField] private GameObject markerPrefab; // Prefab with cube and ButtonQuad+UIDocument
        [SerializeField, Range(1, 10)] private int timeRemaining = 2;

        private GameObject activeMarker; // Instance of the prefab
        private Tween moveTween;
        private CancellationTokenSource cts;
        private Button uiButton; // UI Toolkit button reference

        private void Awake()
        {
            cts = new CancellationTokenSource();
        }

        private void Start()
        {
            tgs = tgs ? tgs : TerrainGridSystem.instance;
            tgs.OnCellClick += OnCellClick;
        }

        private void Update()
        {
            // Make button quad face camera
            if (activeMarker != null && activeMarker.activeSelf)
            {
                var buttonQuad = activeMarker.transform.Find("ButtonQuad");
                if (buttonQuad != null)
                {
                    buttonQuad.LookAt(buttonQuad.position + Camera.main.transform.forward);
                    buttonQuad.Rotate(0, 180, 0); // Flip for correct UI orientation
                }
            }
        }

        private void OnDestroy()
        {
            moveTween?.Kill();
            cts?.Cancel();
            cts?.Dispose();
            tgs.OnCellClick -= OnCellClick;
            if (activeMarker) Destroy(activeMarker);
        }

        private void OnCellClick(TerrainGridSystem sender, int cellIndex, int buttonIndex)
        {
            if (buttonIndex != 1) return;

            // Instantiate new marker
            if (activeMarker) Destroy(activeMarker);
            activeMarker = Instantiate(markerPrefab);

            // Validate setup
            var uiDoc = activeMarker.GetComponent<UIDocument>();
            if (uiDoc == null || uiDoc.panelSettings == null)
            {
                Debug.LogError("UIDocument misconfigured: Check Panel Settings assignment.");
                return;
            }

            var buttonQuad = activeMarker.transform.Find("ButtonQuad");
            if (buttonQuad != null)
            {
                var renderer = buttonQuad.GetComponent<MeshRenderer>();
                if (renderer == null || renderer.material == null || renderer.material.mainTexture == null)
                {
                    Debug.LogError("ButtonQuad material missing or no RenderTexture assigned. Ensure UIMaterial has ButtonRenderTexture in Main Tex/Base Map.");
                    return;
                }
            }
            else
            {
                Debug.LogError("ButtonQuad not found in prefab hierarchy.");
                return;
            }

            Vector3 cellPos = tgs.CellGetPosition(cellIndex, true, 0);
            activeMarker.transform.position = cellPos + Vector3.up * 10;

            // Hide UI during tween
            uiDoc.enabled = false;

            moveTween?.Kill();
            moveTween = activeMarker.transform.DOMove(cellPos + Vector3.up * 0.5f, 1f) // Above water
                .SetEase(Ease.OutQuad)
                .OnComplete(() => StartUniTaskCountdown().Forget());
        }

        private async UniTask StartUniTaskCountdown()
        {
            // Enable UI Toolkit button
            var uiDoc = activeMarker.GetComponent<UIDocument>();
            if (uiDoc)
            {
                uiDoc.enabled = true;
                uiButton = uiDoc.rootVisualElement.Q<Button>("ActivateButton"); // Query by name
                if (uiButton != null)
                {
                    uiButton.text = "Activate Ship";
                    uiButton.clicked += OnButtonClicked;
                }
                else
                {
                    Debug.LogError("Button 'ActivateButton' not found in UXML. Check WorldButton.uxml name field.");
                }
            }

            // Animate button quad
            var buttonQuad = activeMarker.transform.Find("ButtonQuad");
            if (buttonQuad) buttonQuad.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack).OnComplete(() => buttonQuad.DOScale(1f, 0.5f));

            await UniTask.Delay(timeRemaining * 1000, cancellationToken: cts.Token);

            // Cleanup
            if (uiButton != null) uiButton.clicked -= OnButtonClicked;
            activeMarker.SetActive(false); // Or Destroy(activeMarker);
        }

        private void OnButtonClicked()
        {
            Debug.Log("Button clicked! Ready for ship movement logic.");
            // Add ship movement logic here
        }
    }
}