using UnityEngine;

namespace UI.Harbour
{
    public class HarbourHUD : MonoBehaviour
    {
        // Player profile (top left)
        public Texture2D playerPicture;  // Drag your image here
        public int playerLevel = 5;
        public float playerExp = 1200f;
        public float maxExp = 2000f;

        // Resources (bottom left)
        public float metal = 500f;
        public float oil = 300f;
        public float energy = 1000f;
        public float titanium = 200f;
        
        [Header("Resource Icons (Drag Textures Here)")]
        public Texture2D metalIcon;
        public Texture2D oilIcon;
        public Texture2D energyIcon;
        public Texture2D titaniumIcon;
        public Texture2D barFill;  // Green fill texture
        public Texture2D barBorder; // Border texture



        private void OnGUI()
        {
            // TOP LEFT: Player profile
            GUILayout.BeginArea(new Rect(20, 20, 200, 150), GUI.skin.box);
            GUILayout.Label("<b>Player Profile</b>");

            // Picture
            if (playerPicture)
                GUILayout.Label(playerPicture, GUILayout.Width(100), GUILayout.Height(100));

            GUILayout.Label($"Level: {playerLevel}");
            GUILayout.Label($"Exp: {playerExp:F0} / {maxExp:F0}");
            GUILayout.EndArea();

            // TOP RIGHT: View Map button
            GUILayout.BeginArea(new Rect(Screen.width - 220, 20, 200, 50));
            if (GUILayout.Button("View Map", GUILayout.Height(40)))
            {
                Debug.Log("View Map clicked – open map logic here");
            }
            GUILayout.EndArea();

            // CENTER TOP: 4 buttons
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 200, 20, 400, 50));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Build", GUILayout.Height(40)))
                Debug.Log("Build menu opened");
            if (GUILayout.Button("Research", GUILayout.Height(40)))
                Debug.Log("Research menu opened");
            if (GUILayout.Button("Fleet", GUILayout.Height(40)))
                Debug.Log("Fleet menu opened");
            if (GUILayout.Button("Dock", GUILayout.Height(40)))
                Debug.Log("Dock menu opened");
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // BOTTOM LEFT: Resources (sliders)
            GUILayout.BeginArea(new Rect(20, Screen.height - 220, 300, 200), GUI.skin.box);
            GUILayout.Label("<b>Resources</b>");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Metal: " + metal.ToString("F0"));
            GUILayout.HorizontalSlider(metal, 0f, 1000f, GUILayout.Width(150));  // Read-only slider
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Oil: " + oil.ToString("F0"));
            GUILayout.HorizontalSlider(oil, 0f, 1000f, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Energy: " + energy.ToString("F0"));
            GUILayout.HorizontalSlider(energy, 0f, 2000f, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Titanium: " + titanium.ToString("F0"));
            GUILayout.HorizontalSlider(titanium, 0f, 1000f, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            
            // Metal
            GUILayout.BeginHorizontal();
            if (metalIcon) GUILayout.Label(metalIcon, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label("Metal: " + metal.ToString("F0"));
            DrawImageBar(metal / 1000f, barFill, barBorder);
            GUILayout.EndHorizontal();

            // Oil
            GUILayout.BeginHorizontal();
            if (oilIcon) GUILayout.Label(oilIcon, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label("Oil: " + oil.ToString("F0"));
            DrawImageBar(oil / 1000f, barFill, barBorder);
            GUILayout.EndHorizontal();

            // Energy
            GUILayout.BeginHorizontal();
            if (energyIcon) GUILayout.Label(energyIcon, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label("Energy: " + energy.ToString("F0"));
            DrawImageBar(energy / 2000f, barFill, barBorder);
            GUILayout.EndHorizontal();

            // Titanium
            GUILayout.BeginHorizontal();
            if (titaniumIcon) GUILayout.Label(titaniumIcon, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label("Titanium: " + titanium.ToString("F0"));
            DrawImageBar(titanium / 1000f, barFill, barBorder);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
        private void DrawImageBar(float fillAmount, Texture2D fillTex, Texture2D borderTex)
        {
            const float barWidth = 120f;
            const float barHeight = 16f;

            Rect borderRect = GUILayoutUtility.GetRect(barWidth, barHeight);
            if (borderTex) GUI.DrawTexture(borderRect, borderTex);

            Rect fillRect = new Rect(borderRect.x + 2, borderRect.y + 2, barWidth * fillAmount - 4, barHeight - 4);
            if (fillTex) GUI.DrawTexture(fillRect, fillTex);
        }
    }
}
