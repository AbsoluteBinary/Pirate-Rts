using UnityEngine;
using DG.Tweening;
using _Project.Scripts.SceneManagement;
using Managers.Registry;
using UI.Manager;
using UnityEngine.EventSystems;

namespace UI.IMGUI
{
    public class IMGUILoadingOverlay : MonoBehaviour
    {
        public static IMGUILoadingOverlay Instance { get; private set; }

        [Header("Startup Behavior")]
        [SerializeField] private bool autoStartInPlayMode = true; // ← DISABLED now

        [Header("Timing & Animation")]
        [SerializeField] private float mainFillDuration    = 6f;
        [SerializeField] private float endBufferDuration   = 3f;
        [SerializeField] private Ease   fillEase           = Ease.OutQuad;
        [SerializeField] private float fadeOutDuration     = 1.0f;

        [Header("Visuals")]
        [SerializeField] private Color backgroundColor     = new Color(0f, 0f, 0f, 0.8f);
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite barFillSprite;
        [SerializeField] private Sprite barBorderSprite;

        // Runtime state
        private float currentProgress = 0f;
        private float guiAlpha        = 1f;
        private bool  isVisible       = false;
        private bool  waitingForRealLoad = false;

        private Sequence loadingSequence;

        public System.Action OnFadeOutFinished { get; set; }
        
        
        private void Awake()
        {
            // Singleton setup first — must be before anything else
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ResetState();
            gameObject.SetActive(true); // Keep alive always
        }

        private void Start()
        {
            // No auto-trigger anymore – called externally from LoginMenuManager
        }

        public void TriggerLoadingScreen()
        {
            Debug.Log("Loading overlay triggered");
            DisableOtherObjects();
            UIManager.Instance.HideBootLoginPanel();
            
            if (loadingSequence != null && loadingSequence.IsActive())
            {
                loadingSequence.Kill();
            }

            ResetStateForNewSequence();

            isVisible = true;
            guiAlpha = 1f;
            currentProgress = 0f;
            waitingForRealLoad = true;

            StartLoadingAnimation();
        }

        private void ResetStateForNewSequence()
        {
            currentProgress = 0f;
            guiAlpha = 1f;
            isVisible = true;
            waitingForRealLoad = true;
        }

        private void ResetState()
        {
            loadingSequence?.Kill();
            currentProgress = 0f;
            guiAlpha = 1f;
            isVisible = false;
            waitingForRealLoad = false;
        }

        private void StartLoadingAnimation()
        {
            loadingSequence = DOTween.Sequence();

            // Fake smooth fill to 100%
            loadingSequence.Append(
                DOTween.To(
                    () => currentProgress,
                    x => currentProgress = x,
                    1f,
                    mainFillDuration
                ).SetEase(fillEase)
            );

            loadingSequence.OnComplete(() =>
            {
                // Fake done → now wait for real loading + buffer
                waitingForRealLoad = true;
            });
        }

        private void Update()
        {
            if (!isVisible) return;

            // Poll SceneLoader only after fake fill complete
            if (waitingForRealLoad && SceneLoader.Instance != null)
            {
                if (!SceneLoader.Instance.isLoading)
                {
                    // Real load done → start buffer countdown
                    if (loadingSequence != null && loadingSequence.IsActive())
                    {
                        loadingSequence.Kill(); // Stop any lingering
                    }

                    // Hold at 100% for buffer, then fade
                    DOTween.Sequence()
                        .AppendInterval(endBufferDuration)
                        .OnComplete(StartFadeOut)
                        .SetUpdate(true);
                    
                    waitingForRealLoad = false; // Prevent re-trigger
                }
            }
        }

        private void StartFadeOut()
        {
            DOTween.To(
                () => guiAlpha,
                x => guiAlpha = x,
                0f,
                fadeOutDuration
            ).SetEase(Ease.InOutQuad)
             .OnComplete(OnFadeOutComplete)
             .SetUpdate(true);
        }

        private void OnFadeOutComplete()
        {
            isVisible = false;
            Debug.Log("Loading overlay faded out – ready for next trigger");

            OnFadeOutFinished?.Invoke();
            //UIManager.HideBootLoginPanel();
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            GUI.color = new Color(1f, 1f, 1f, guiAlpha);

            // Background
            if (backgroundSprite != null)
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundSprite.texture);
            else
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", BackgroundStyle());

            // Loading text
            GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 50), "Loading...", LabelStyle());

            // Bar container
            Rect barRect = new Rect(Screen.width * 0.3f, Screen.height * 0.6f, Screen.width * 0.4f, 30);
            if (barBorderSprite != null)
                GUI.DrawTexture(barRect, barBorderSprite.texture);

            // Fill bar
            if (barFillSprite != null)
            {
                Rect fillRect = new Rect(
                    barRect.x + 2,
                    barRect.y + 2,
                    (barRect.width - 4) * currentProgress,
                    barRect.height - 4
                );
                GUI.DrawTexture(fillRect, barFillSprite.texture);
            }

            // Percentage
            GUI.Label(
                new Rect(0, Screen.height * 0.7f, Screen.width, 50),
                $"{(currentProgress * 100):F0}%",
                LabelStyle()
            );

            GUI.color = Color.white;
        }

        private GUIStyle BackgroundStyle()
        {
            var style = new GUIStyle();
            style.normal.background = MakeTex(1, 1, backgroundColor);
            return style;
        }

        private GUIStyle LabelStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            return style;
        }

        private Texture2D MakeTex(int w, int h, Color col)
        {
            Color[] pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) 
                pix[i] = col;
            
            Texture2D tex = new Texture2D(w, h);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        private void OnDestroy()
        {
            loadingSequence?.Kill();
        }
        
        public void DisableOtherObjects()
        {
            foreach (var container in ContainerRegistry.GetAllContainers())
            {
                if (container == null) continue;

                // Disable the whole container (most important)
                container.SetActive(false);
                Debug.Log($"[Loading] Disabled container: {container.name}");

                // Disable AudioListener
                var listener = container.GetComponentInChildren<AudioListener>(true);
                if (listener != null)
                {
                    listener.enabled = false;
                }

                // Disable EventSystem
                var eventSystem = container.GetComponentInChildren<EventSystem>(true);
                if (eventSystem != null)
                {
                    eventSystem.enabled = false;
                }
            }

            // Optional: Disable BootCamera if tagged
            var bootCamera = GameObject.FindWithTag("BootCamera");
            if (bootCamera != null)
            {
                bootCamera.SetActive(false);
                Debug.Log("[Loading] Disabled BootCamera");
            }
        }
    }
}