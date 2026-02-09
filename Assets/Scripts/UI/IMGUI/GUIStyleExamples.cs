using UnityEngine;

namespace UI
{
    public class GUIStyleExamples : MonoBehaviour
    {
        [Header("Styles")]
        [SerializeField] private GUIStyle healthLabelStyle;  // Customize in Inspector
        [SerializeField] private GUIStyle fireButtonStyle;
        [SerializeField] private GUIStyle resourceBoxStyle;

        private void Awake()
        {
            // Health label: Big, red, centered, with background
            healthLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                normal = { textColor = Color.red, background = MakeTex(1, 1, new Color(0, 0, 0, 0.5f)) },
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 5, 5),  // Inner spacing
                border = new RectOffset(4, 4, 4, 4)     // Border width
            };

            // Fire button: Bold, hover effect, custom border
            fireButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white, background = MakeTex(1, 1, Color.gray) },
                hover = { textColor = Color.yellow, background = MakeTex(1, 1, Color.blue) },
                padding = new RectOffset(20, 20, 10, 10),
                border = new RectOffset(8, 8, 8, 8)
            };

            // Resource box: Rounded border, gradient background, custom font
            resourceBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeGradientTex(1, 1, Color.black, Color.gray) },
                border = new RectOffset(12, 12, 12, 12),  // Rounded feel with texture
                alignment = TextAnchor.UpperLeft
            };
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 300, 200));

            // Example 1: Health label
            GUILayout.Label("Health: 100/100", healthLabelStyle, GUILayout.Height(40));

            // Example 2: Fire button
            if (GUILayout.Button("Fire Cannon", fireButtonStyle, GUILayout.Height(50)))
            {
                Debug.Log("Cannon fired!");
            }

            // Example 3: Resource box
            GUILayout.BeginVertical(resourceBoxStyle);
            GUILayout.Label("Metal: 500");
            GUILayout.Label("Oil: 300");
            GUILayout.EndVertical();

            GUILayout.EndArea();
        }

        // Helper: Solid color texture
        private Texture2D MakeTex(int w, int h, Color col)
        {
            Texture2D tex = new Texture2D(w, h);
            Color[] pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        // Helper: Gradient texture
        private Texture2D MakeGradientTex(int w, int h, Color start, Color end)
        {
            Texture2D tex = new Texture2D(w, h);
            Color[] pix = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                Color col = Color.Lerp(start, end, (float)y / h);
                for (int x = 0; x < w; x++) pix[y * w + x] = col;
            }
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}
