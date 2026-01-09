using UnityEngine;
using UnityEngine.UI;

namespace UI.IMGUI
{
    public class IMGUILoadingOverlay : MonoBehaviour
    {
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.8f);
        [SerializeField] private Sprite backgroundSprite;     // Optional full-screen sprite
        [SerializeField] private Sprite barFillSprite;        // Fill sprite (stretchable)
        [SerializeField] private Sprite barBorderSprite;      // Border sprite

        private float currentProgress;
        private bool isVisible;

        public void Show() { isVisible = true; currentProgress = 0f; }
        public void Hide() { isVisible = false; }
        public void SetProgress(float value) { currentProgress = Mathf.Clamp01(value); }

        private void OnGUI()
        {
            if (!isVisible) return;

            // Background
            if (backgroundSprite)
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundSprite.texture);
            else
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", BackgroundStyle());

            // Loading text
            GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 50), "Loading...", LabelStyle());

            // Bar
            Rect barRect = new Rect(Screen.width * 0.3f, Screen.height * 0.6f, Screen.width * 0.4f, 30);
            if (barBorderSprite) GUI.DrawTexture(barRect, barBorderSprite.texture);

            Rect fillRect = new Rect(barRect.x + 2, barRect.y + 2, (barRect.width - 4) * currentProgress, barRect.height - 4);
            if (barFillSprite) GUI.DrawTexture(fillRect, barFillSprite.texture);

            // Percentage
            GUI.Label(new Rect(0, Screen.height * 0.7f, Screen.width, 50), $"{currentProgress * 100:F0}%", LabelStyle());
        }

        private GUIStyle BackgroundStyle()
        {
            var style = new GUIStyle();
            style.normal.background = MakeTex(1, 1, backgroundColor);
            return style;
        }

        private GUIStyle LabelStyle()
        {
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 32;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            return style;
        }

        private Texture2D MakeTex(int w, int h, Color col)
        {
            Color[] pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D tex = new Texture2D(w, h);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}