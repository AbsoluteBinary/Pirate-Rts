using System.Threading.Tasks;
using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.WorldMap.HUDInteractions;
using UnityEngine;

namespace _Project.Scripts.UI.Harbour
{
    public class HarbourDisplayHUD : MonoBehaviour
    {
        [Header("Player Profile")]
        [SerializeField] private Texture2D playerPicture;  // Drag your player image here
        [SerializeField] private int playerLevel = 5;
        [SerializeField] private float playerExp = 1200f;
        [SerializeField] private float maxExp = 2000f;

        [Header("Resources")]
        [SerializeField] private float metal = 500f;
        [SerializeField] private float oil = 300f;
        [SerializeField] private float energy = 1000f;
        [SerializeField] private float titanium = 200f;
        [SerializeField] private float maxResource = 1000f;  // Shared max for bars

        [Header("Resource Images")]
        [SerializeField] private Texture2D metalIcon;
        [SerializeField] private Texture2D oilIcon;
        [SerializeField] private Texture2D energyIcon;
        [SerializeField] private Texture2D titaniumIcon;
        [SerializeField] private Texture2D barFill;    // Green/colored fill (1x1 stretchable PNG)
        [SerializeField] private Texture2D barBorder;  // Border frame (e.g., 128x16 PNG)
        private Task _task;
        private Task _task1;

        private void Awake()
        {
            //_task1 = SceneLoader.Instance.BeginSceneTransition(1);
            HUDMenuButtonsEventBus.TriggerHUDEnterWorldClicked();
        }

        private void OnGUI()
        {
            // TOP LEFT: Player Profile
            GUILayout.BeginArea(new Rect(20, 20, 200, 150), GUI.skin.box);
            GUILayout.Label("<b>Player Profile</b>");
            if (playerPicture) GUILayout.Label(playerPicture, GUILayout.Width(80), GUILayout.Height(80));
            GUILayout.Label($"Level: {playerLevel}");
            GUILayout.Label($"Exp: {playerExp:F0} / {maxExp:F0}");
            GUILayout.EndArea();

            // TOP RIGHT: View Map button
            GUILayout.BeginArea(new Rect(Screen.width - 220, 20, 200, 50));
            if (GUILayout.Button("View Map", GUILayout.Height(40)))
            {
                Debug.Log("View Map clicked");
                //_ = _task1;
                _ = SceneLoader.Instance.BeginSceneTransition(1);
            }
            GUILayout.EndArea();

            // CENTER TOP: 4 buttons
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 200, 20, 400, 50));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Build", GUILayout.Height(40))) Debug.Log("Build menu opened");
            if (GUILayout.Button("Research", GUILayout.Height(40))) Debug.Log("Research menu opened");
            if (GUILayout.Button("Fleet", GUILayout.Height(40))) Debug.Log("Fleet menu opened");
            if (GUILayout.Button("Dock", GUILayout.Height(40))) Debug.Log("Dock menu opened");
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // BOTTOM LEFT: Resources with Icons + Bars
            GUILayout.BeginArea(new Rect(20, Screen.height - 220, 300, 200), GUI.skin.box);
            GUILayout.Label("<b>Resources</b>");

            DrawResourceBar(metalIcon, "Metal", metal, maxResource);
            DrawResourceBar(oilIcon, "Oil", oil, maxResource);
            DrawResourceBar(energyIcon, "Energy", energy, maxResource * 2f);  // Example: double max for energy
            DrawResourceBar(titaniumIcon, "Titanium", titanium, maxResource);

            GUILayout.EndArea();
        }

        private void DrawResourceBar(Texture2D icon, string label, float value, float max)
        {
            GUILayout.BeginHorizontal();
            if (icon) GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label($"{label}: {value:F0}");

            // Bar border
            Rect barRect = GUILayoutUtility.GetRect(120, 16);
            if (barBorder) GUI.DrawTexture(barRect, barBorder);

            // Bar fill
            Rect fillRect = new Rect(barRect.x + 2, barRect.y + 2, (barRect.width - 4) * (value / max), barRect.height - 4);
            if (barFill) GUI.DrawTexture(fillRect, barFill);
            GUILayout.EndHorizontal();
        }
    }
}
