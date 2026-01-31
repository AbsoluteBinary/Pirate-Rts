using DG.Tweening;
using UnityEngine;

namespace Main_Screen
{
    public class BootSplashIMGUI : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Sprite backgroundSprite;      // Full-screen bg (black/naval themed)
        [SerializeField] private Sprite logoSprite;            // Centered logo
        [SerializeField] private Color backgroundFallback = new Color(0f, 0f, 0f, 1f);

        [Header("Timing")]
        [SerializeField] private float minDisplayTime = 5f;
        [SerializeField] private float fadeOutDuration = 1.2f;
        [SerializeField] private float delayBeforeLoginFadeIn = 0.6f;

        private bool isVisible = true;
        private float guiAlpha = 1f;

        private void Start()
        {
            // Start timer immediately
        }

        private void Update()
        {
            if (!isVisible) return;

            if (Time.unscaledTime >= minDisplayTime)
            {
                StartFadeOut();
            }
        }

        private void StartFadeOut()
        {
            DOTween.To(() => guiAlpha, x => guiAlpha = x, 0f, fadeOutDuration)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    isVisible = false;
                    // Hook into login fade-in with delay
                    DOVirtual.DelayedCall(delayBeforeLoginFadeIn, () =>
                    {
                        var manager = FindObjectOfType<LoginMenuManager>();
                        if (manager != null)
                        {
                            manager.FadeInLoginMenu();
                        }
                        else
                        {
                            Debug.LogWarning("LoginMenuManager not found for splash complete callback.");
                        }
                    });

                    // Optional: disable or destroy self after fade
                    gameObject.SetActive(false);
                });
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            GUI.color = new Color(1f, 1f, 1f, guiAlpha);

            // Background
            if (backgroundSprite != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundSprite.texture);
            }
            else
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", GetBackgroundStyle());
            }

            // Logo (centered)
            if (logoSprite != null)
            {
                float maxWidth  = Screen.width  * 0.70f;
                float maxHeight = Screen.height * 0.50f;

                float aspect = logoSprite.rect.width / logoSprite.rect.height;
                float drawWidth  = Mathf.Min(maxWidth, maxHeight * aspect);
                float drawHeight = drawWidth / aspect;

                Rect logoRect = new Rect(
                    (Screen.width  - drawWidth)  * 0.5f,
                    (Screen.height - drawHeight) * 0.40f,   // Slightly above center
                    drawWidth,
                    drawHeight
                );

                GUI.DrawTexture(logoRect, logoSprite.texture);
            }

            // Text - Legend Nation Gaming
            // GUI.Label(
            //     new Rect(0, Screen.height * 0.68f, Screen.width, 80),
            //     "Legend Nation Gaming",
            //     GetTitleStyle()
            // );

            // Text - Developed by Conchobar
            GUI.Label(
                new Rect(0, Screen.height * 0.94f, Screen.width, 50),
                "Developed by Conchobar",
                GetSubtitleStyle()
            );

            GUI.color = Color.white;
        }

        private GUIStyle GetBackgroundStyle()
        {
            var style = new GUIStyle { normal = { background = MakeTex(2, 2, backgroundFallback) } };
            return style;
        }

        private GUIStyle GetTitleStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 64,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold
            };
            return style;
        }

        private GUIStyle GetSubtitleStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f, 1f) }
            };
            return style;
        }

        private Texture2D MakeTex(int w, int h, Color col)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var tex = new Texture2D(w, h);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}