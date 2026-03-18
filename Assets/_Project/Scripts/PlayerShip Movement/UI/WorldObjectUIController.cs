using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace _Project.Scripts.PlayerShip_Movement.UI
{
    public class WorldObjectUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private Transform uiTransform;

        [Header("UI Element Names")]
        [SerializeField] private string buttonElementName = "PlayerHarbourButton";

        [Header("Timing & Animation")]
        [SerializeField] [Range(1f, 10f)] private float displayDuration = 3f;
        [SerializeField] private float tweenDuration = 0.5f;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float startY = 2f;
        [SerializeField] private float endY = 10f;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;

        // Cached references
        private VisualElement buttonElement;
        private InputAction clickAction;
        private CancellationTokenSource cts;

        public bool IsButtonVisible { get; private set; }

        private void Awake()
        {
            DOTween.Init();
            ValidateReferences();
            SetupInput();
        }

        private void ValidateReferences()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[WorldObjectUI] UIDocument is not assigned!", this);
                return;
            }

            buttonElement = uiDocument.rootVisualElement.Q<VisualElement>(buttonElementName);
            if (buttonElement == null)
            {
                Debug.LogError($"[WorldObjectUI] Button element '{buttonElementName}' not found in UIDocument!", this);
                return;
            }

            if (uiTransform == null)
            {
                Debug.LogError("[WorldObjectUI] uiTransform is not assigned!", this);
                return;
            }

            // Initial state
            buttonElement.style.display = DisplayStyle.None;
            buttonElement.style.opacity = 0f;
        }

        private void SetupInput()
        {
            if (inputActions == null)
            {
                Debug.LogError("[WorldObjectUI] InputActionAsset is not assigned!", this);
                return;
            }

            var map = inputActions.FindActionMap("BaseUI");
            if (map == null)
            {
                Debug.LogError("[WorldObjectUI] ActionMap 'BaseUI' not found!", this);
                return;
            }

            clickAction = map.FindAction("ClickUI");
            if (clickAction == null)
            {
                Debug.LogError("[WorldObjectUI] Action 'ClickUI' not found in BaseUI map!", this);
                return;
            }

            clickAction.performed += OnClickPerformed;
            clickAction.Enable();
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            if (Camera.main == null) return;

            var mousePos = Mouse.current.position.ReadValue();
            var ray = Camera.main.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    ToggleButton();
                }
            }
        }

        private void ToggleButton()
        {
            if (!IsButtonVisible)
                ShowButton();
            else
                HideButton();
        }

        private void ShowButton()
        {
            if (buttonElement == null || uiTransform == null) return;

            cts?.Cancel();
            cts = new CancellationTokenSource();

            // Reset initial state
            buttonElement.style.display = DisplayStyle.Flex;
            buttonElement.style.opacity = 0f;

            Vector3 startPos = uiTransform.position;
            startPos.y = startY;
            uiTransform.position = startPos;

            // Tween world position + fade in
            Sequence seq = DOTween.Sequence();

            seq.Append(DOTween.To(
                () => uiTransform.position.y,
                y =>
                {
                    Vector3 p = uiTransform.position;
                    p.y = y;
                    uiTransform.position = p;
                },
                endY, tweenDuration
            ).SetEase(Ease.OutQuad));

            seq.Join(DOTween.To(
                () => buttonElement.resolvedStyle.opacity,
                x => buttonElement.style.opacity = x,
                1f, fadeInDuration
            ).SetEase(Ease.OutQuad));

            seq.OnComplete(() => StartAutoHideTimer(cts.Token));

            IsButtonVisible = true;
        }

        private async void StartAutoHideTimer(CancellationToken token)
        {
            try
            {
                await Task.Delay((int)(displayDuration * 1000), token);
                HideButton();
            }
            catch (TaskCanceledException) { /* Timer was cancelled - normal */ }
        }

        private void HideButton()
        {
            if (buttonElement == null || uiTransform == null) return;

            // Cancel any running timer
            cts?.Cancel();

            // Tween world position back down + fade out
            Sequence seq = DOTween.Sequence();

            // Tween position (Y only)
            seq.Append(DOTween.To(
                () => uiTransform.position.y,
                y =>
                {
                    Vector3 p = uiTransform.position;
                    p.y = y;
                    uiTransform.position = p;
                },
                startY, tweenDuration
            ).SetEase(Ease.InQuad));

            // Fade out opacity
            seq.Join(DOTween.To(
                () => buttonElement.resolvedStyle.opacity,
                x => buttonElement.style.opacity = x,
                0f, tweenDuration
            ).SetEase(Ease.InQuad));

            // Hide element when complete
            seq.OnComplete(() =>
            {
                buttonElement.style.display = DisplayStyle.None;
                IsButtonVisible = false;
            });
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;

            if (clickAction != null)
            {
                clickAction.performed -= OnClickPerformed;
                clickAction.Disable();
            }

            DOTween.Kill(this);
        }
    }
}