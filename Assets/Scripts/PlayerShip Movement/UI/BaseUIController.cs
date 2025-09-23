using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayerShip_Movement.UI
{
    public class BaseUIController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument; // Reference to the UIDocument
        [SerializeField] private Transform uiTransform; // Transform of the GameObject with UIDocument (for world position tween)
        [SerializeField] private string buttonElementName = "BaseButton"; // Name of the button VisualElement in UXML
        [SerializeField] [Range(1f, 10f)] private float displayDuration = 3f; // Adjustable timer in Inspector
        [SerializeField] private float tweenDuration = 0.5f; // Duration of position tween
        [SerializeField] private float fadeInDuration = 1f; // Duration of fade-in (longer)
        [SerializeField] private float startY = 2f; // Starting Y position (world units)
        [SerializeField] private float endY = 10f; // Ending Y position (world units)

        private VisualElement buttonElement; // The button VisualElement (for fading)
        private CancellationTokenSource cts; // For canceling async timer
        private bool isButtonVisible = false;

        private void Awake()
        {
            // Initialize DOTween
            DOTween.Init();

            // Find the button VisualElement
            if (uiDocument != null)
            {
                buttonElement = uiDocument.rootVisualElement.Q<VisualElement>(buttonElementName);
                if (buttonElement != null)
                {
                    // Ensure button is hidden and positioned at startY
                    buttonElement.style.display = DisplayStyle.None;
                    buttonElement.style.opacity = 0f;
                    buttonElement.style.translate = new StyleTranslate(new Translate(0, startY, 0));
                    // Fix reversed Y by rotating 180 degrees around Z
                    //buttonElement.style.rotate = new StyleRotate(new Rotate(-180f));
                }
                else
                {
                    Debug.LogWarning($"Button element '{buttonElementName}' not found in UIDocument!", this);
                }
            }
            else
            {
                Debug.LogWarning("UIDocument not assigned!", this);
            }

            // Set initial world position if uiTransform is assigned
            if (uiTransform != null)
            {
                Vector3 pos = uiTransform.position;
                pos.y = startY;
                uiTransform.position = pos;
            }
            else
            {
                Debug.LogWarning("uiTransform not assigned!", this);
            }
        }

        private void OnMouseDown()
        {
            // Toggle UI on click
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
