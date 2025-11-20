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
        [SerializeField] private string buttonElementName = "AttackButtonTypeOA"; // Name of the button VisualElement in UXML
        [SerializeField] [Range(1f, 10f)] private float displayDuration = 3f; // Adjustable timer in Inspector
        [SerializeField] private float tweenDuration = 0.5f; // Duration of position tween
        [SerializeField] private float fadeInDuration = 1f; // Duration of fade-in (longer)
        [SerializeField] private float startY = 2f; // Starting Y position (world units)
        [SerializeField] private float endY = 10f; // Ending Y position (world units)

        private VisualElement _buttonElement; // The button VisualElement (for fading)
        private CancellationTokenSource _cts; // For canceling async timer
        [SerializeField] bool _isButtonVisible = false;

        private void Awake()
        {
            // Initialize DOTween
            DOTween.Init();

            // Find the button VisualElement
            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                if (root == null)
                {
                    Debug.LogError("No rootVisualElement! Check UXML assignment.", this);
                    return;
                }

                // FORCE hide entire UI at start (fixes visible on load)
                root.style.display = DisplayStyle.None;
                root.style.opacity = 0f;
                Debug.Log("✓ Forced hide on rootVisualElement at Awake.", this);

                // List children to spot names (teaching: always inspect hierarchy!)
                Debug.Log("UXML children:");
                foreach (var child in root.Children())
                {
                    Debug.Log("→ " + child.name);
                    // If button is inside a container, list its children too
                    foreach (var grandChild in child.Children())
                    {
                        Debug.Log("  ↳ " + grandChild.name);
                    }
                }

                _buttonElement = root.Q<VisualElement>(buttonElementName);
                if (_buttonElement != null)
                {
                    Debug.Log($"✓ Found button: {buttonElementName}", this);
                    _buttonElement.style.translate = new StyleTranslate(new Translate(0, startY, 0));
                }
                else
                {
                    Debug.LogError($"✗ Button '{buttonElementName}' not found! Use logged names to fix.", this);
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
            Debug.Log($"✓ OnMouseDown CALLED on {gameObject.name}! It WORKS!", this);
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
        
        [ContextMenu("DEBUG: Check Why OnMouseDown Is Not Working")]
        private void DebugWhyOnMouseDownFails()
        {
            Debug.Log("=== ONMOUSEDOWN DIAGNOSTIC FOR: " + gameObject.name + " ===", this);

            // 1. Collider check
            Collider col = GetComponent<Collider>();
            if (col == null)
                Debug.LogError("✗ NO Collider attached! OnMouseDown will NEVER work.");
            else
            {
                Debug.Log($"✓ Collider found: {col.GetType().Name}");
                if (!col.enabled) Debug.LogError("✗ Collider is DISABLED!");
                if (col is BoxCollider box)
                {
                    Debug.Log($"   Size: {box.size}, Center: {box.center}");
                    Debug.Log($"   IsTrigger: {box.isTrigger} (can be true or false — both work for OnMouseDown)");
                }
            }

            // 2. Layer check (HUGE hidden killer!)
            int layer = gameObject.layer;
            string layerName = LayerMask.LayerToName(layer);
            Debug.Log($"Layer: {layer} ({layerName})");
            if (Physics.GetIgnoreLayerCollision(Camera.main.gameObject.layer, layer))
                Debug.LogError("✗ This layer is set to ignore collisions with Main Camera layer!");

            // 3. Camera has PhysicsRaycaster?
            var raycaster = Camera.main.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>();
            if (raycaster == null)
                Debug.LogError("✗ Main Camera has NO PhysicsRaycaster! Mouse clicks won't detect 3D objects!");
            else
                Debug.Log("✓ PhysicsRaycaster found on Main Camera");

            // 4. Is anything blocking the ray? (UI, another object, etc.)
            Debug.Log("Try clicking in Play mode and watch for the log above ^");

            // 5. Is the object visible to camera + not too far?
            if (!GetComponent<Renderer>()?.isVisible ?? true)
                Debug.LogWarning("? Object might be off-screen or culled");

            Debug.Log("=== END DIAGNOSTIC ====");
        }

        private void ShowButton()
        {
            if (_buttonElement == null || uiTransform == null) 
            {
                Debug.LogError("✗ Cannot show — button or uiTransform null! Fix in Awake.", this);
                return;
            }

            Debug.Log("SHOWING BUTTON NOW!", this);

            // ← NEW: Wake up the UI panel!
            uiDocument.enabled = true;

            // Show root and button
            var root = uiDocument.rootVisualElement;
            root.style.display = DisplayStyle.Flex;
            _buttonElement.style.display = DisplayStyle.Flex;
            _buttonElement.style.opacity = 0f;

            // Reset world position
            Vector3 startPos = uiTransform.position;
            startPos.y = startY;
            uiTransform.position = startPos;

            // Tween sequence
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
                1f, fadeInDuration));
            seq.OnStart(() => Debug.Log("✓ Tween STARTED!"));
            seq.OnComplete(() => {
                Debug.Log("✓ Tween COMPLETE! Starting timer.");
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                StartTimer(_cts.Token);
            });

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
                uiDocument.rootVisualElement.style.display = DisplayStyle.None; // Hide root
                _isButtonVisible = false;
                Debug.Log("Hide COMPLETE!");
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
