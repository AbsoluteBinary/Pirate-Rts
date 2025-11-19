using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.WorldMap.ObjectInteractions
{
    public class EnemyType0A : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument; // Reference to the UIDocument
        [SerializeField] private Transform uiTransform; // Transform of the GameObject with UIDocument (for world position tween)
        [SerializeField] private string buttonElementName = "EnemyUIType0A"; // Name of the button VisualElement in UXML
        [SerializeField] [Range(1f, 10f)] private float displayDuration = 3f; // Adjustable timer in Inspector
        [SerializeField] private float tweenDuration = 0.5f; // Duration of position tween
        [SerializeField] private float fadeInDuration = 1f; // Duration of fade-in (longer)
        [SerializeField] private float startY = 2f; // Starting Y position (world units)
        [SerializeField] private float endY = 10f; // Ending Y position (world units)

        private VisualElement _buttonElement; // The button VisualElement (for fading)
        private CancellationTokenSource _cts; // For canceling async timer
        private bool _isButtonVisible = false;

        private void Awake()
        {
            // Initialize DOTween
            DOTween.Init();

            // Find the button VisualElement
            if (uiDocument != null)
            {
                _buttonElement = uiDocument.rootVisualElement.Q<VisualElement>(buttonElementName);
                if (_buttonElement != null)
                {
                    // Ensure button is hidden and positioned at startY
                    _buttonElement.style.display = DisplayStyle.None;
                    _buttonElement.style.opacity = 0f;
                    _buttonElement.style.translate = new StyleTranslate(new Translate(0, startY, 0));
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
            if (!_isButtonVisible)
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
            if (_buttonElement == null || uiTransform == null) return;

            // Cancel any existing timer
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            // Set initial state
            _buttonElement.style.display = DisplayStyle.Flex;
            _buttonElement.style.opacity = 0f;

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
                () => _buttonElement.style.opacity.value,
                x => _buttonElement.style.opacity = x,
                1f, fadeInDuration)); // Longer fade-in
            seq.OnComplete(() => StartTimer(_cts.Token));

            _isButtonVisible = true;
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
            if (_buttonElement == null || uiTransform == null) return;

            // Cancel timer
            _cts?.Cancel();

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
                () => _buttonElement.style.opacity.value,
                x => _buttonElement.style.opacity = x,
                0f, tweenDuration)); // Fade-out uses tweenDuration
            seq.OnComplete(() =>
            {
                _buttonElement.style.display = DisplayStyle.None;
                _isButtonVisible = false;
            });
        }

        private void OnDisable()
        {
            // Cancel timer and cleanup tweens when disabled
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            DOTween.Kill(this);
        }

        private void OnDestroy()
        {
            // Ensure cleanup on destroy
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            DOTween.Kill(this);
        }
    }
    
}
