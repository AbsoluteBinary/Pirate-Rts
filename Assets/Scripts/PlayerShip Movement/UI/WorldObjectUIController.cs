using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace PlayerShip_Movement.UI
{
    public class WorldObjectUIController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument; // Reference to the UIDocument
        [SerializeField] private Transform uiTransform; // Transform of the GameObject with UIDocument (for world position tween)
        [SerializeField] private string buttonElementName = "PlayerHarbourButton"; // Name of the button VisualElement in UXML
        [SerializeField] [Range(1f, 10f)] private float displayDuration = 3f; // Adjustable timer in Inspector
        [SerializeField] private float tweenDuration = 0.5f; // Duration of position tween
        [SerializeField] private float fadeInDuration = 1f; // Duration of fade-in (longer)
        [SerializeField] private float startY = 2f; // Starting Y position (world units)
        [SerializeField] private float endY = 10f; // Ending Y position (world units)

        private VisualElement buttonElement; // The button VisualElement (for fading)
        private CancellationTokenSource cts; // For canceling async timer
        public bool isButtonVisible = false;
        
        // NEW: Input Action Asset (drag in Inspector)
        [SerializeField] private InputActionAsset inputActions;
        private InputAction clickAction;

        private void Awake()
        {
            DOTween.Init();

            if (uiDocument != null)
            {
                buttonElement = uiDocument.rootVisualElement.Q<VisualElement>(buttonElementName);
                if (buttonElement != null)
                {
                    buttonElement.style.display = DisplayStyle.None;
                    buttonElement.style.opacity = 0f;
                    buttonElement.style.translate = new StyleTranslate(new Translate(0, startY, 0));
                    Debug.Log($"[BaseUI] Found button element: {buttonElementName}");
                }
                else
                {
                    Debug.LogError($"[BaseUI] Button '{buttonElementName}' not found in UIDocument", this);
                }
            }
            else
            {
                Debug.LogError("[BaseUI] UIDocument not assigned!", this);
            }

            if (uiTransform != null)
            {
                Vector3 pos = uiTransform.position;
                pos.y = startY;
                uiTransform.position = pos;
                Debug.Log($"[BaseUI] uiTransform set to startY = {startY}");
            }
            else
            {
                Debug.LogError("[BaseUI] uiTransform not assigned!", this);
            }

            // Input setup
            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("BaseUI");
                if (map == null)
                {
                    Debug.LogError("[BaseUI] ActionMap 'BaseUI' not found in InputActions asset", this);
                    return;
                }

                clickAction = map.FindAction("ClickUI");
                if (clickAction == null)
                {
                    Debug.LogError("[BaseUI] Action 'ClickUI' not found in 'BaseUI' map", this);
                    return;
                }

                Debug.Log("[BaseUI] ClickUI action found – subscribing");
                clickAction.performed += OnClickPerformed;
                clickAction.Enable();
            }
            else
            {
                Debug.LogError("[BaseUI] InputActionAsset not assigned in Inspector!", this);
            }
        }
        
        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            Debug.Log("[BaseUI] Click action performed – raw input detected");

            var mousePos = Mouse.current.position.ReadValue();
            Debug.Log($"[BaseUI] Mouse position: {mousePos}");

            var ray = Camera.main.ScreenPointToRay(mousePos);
            Debug.Log($"[BaseUI] Ray origin: {ray.origin} direction: {ray.direction}");

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"[BaseUI] Raycast hit object: {hit.transform.name} (expected: {gameObject.name})");

                if (hit.transform == transform)
                {
                    Debug.Log("[BaseUI] Raycast success – this object clicked → toggling button");
                    ToggleButton();
                }
                else
                {
                    Debug.LogWarning($"[BaseUI] Raycast hit wrong object: {hit.transform.name}");
                }
            }
            else
            {
                Debug.LogWarning("[BaseUI] Raycast hit nothing – no collider detected");
            }
        }

        private void ToggleButton()
        {
            if (!isButtonVisible)
            {
                ShowButton();
            }
            else
            {
                HideButton();
            }
        }

        private void ShowButton()
        {
            if (buttonElement == null || uiTransform == null) return;

            // Cancel any existing timer
            cts?.Cancel();
            cts = new CancellationTokenSource();

            // Set initial state
            buttonElement.style.display = DisplayStyle.Flex;
            buttonElement.style.opacity = 0f;

            // Set initial world Y position
            Vector3 startPos = uiTransform.position;
            startPos.y = startY;
            uiTransform.position = startPos;

            // Tween world position and fade in
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(
                () => uiTransform.position.y,
                y => {
                    Vector3 pos = uiTransform.position;
                    pos.y = y;
                    uiTransform.position = pos;
                },
                endY, tweenDuration).SetEase(Ease.OutQuad));
            seq.Join(DOTween.To(
                () => buttonElement.style.opacity.value,
                x => buttonElement.style.opacity = x,
                1f, fadeInDuration)); // Longer fade-in
            seq.OnComplete(() => StartTimer(cts.Token));

            isButtonVisible = true;
        }

        private async void StartTimer(CancellationToken token)
        {
            try
            {
                // Wait for the specified duration
                await Task.Delay((int)(displayDuration * 1000), token);
                HideButton();
            }
            catch (TaskCanceledException)
            {
                // Task was canceled (e.g., on hide or destroy)
                Debug.Log("Timer canceled.");
            }
        }

        private void HideButton()
        {
            if (buttonElement == null || uiTransform == null) return;

            // Cancel timer
            cts?.Cancel();

            // Tween world position back and fade out
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(
                () => uiTransform.position.y,
                y => {
                    Vector3 pos = uiTransform.position;
                    pos.y = y;
                    uiTransform.position = pos;
                },
                startY, tweenDuration).SetEase(Ease.InQuad));
            seq.Join(DOTween.To(
                () => buttonElement.style.opacity.value,
                x => buttonElement.style.opacity = x,
                0f, tweenDuration)); // Fade-out uses tweenDuration
            seq.OnComplete(() =>
            {
                buttonElement.style.display = DisplayStyle.None;
                isButtonVisible = false;
            });
        }

        private void OnDisable()
        {
            // Cancel timer and cleanup tweens when disabled
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            DOTween.Kill(this);
        }

        private void OnDestroy()
        {
            // Ensure cleanup on destroy
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            DOTween.Kill(this);
        }
    }
}
