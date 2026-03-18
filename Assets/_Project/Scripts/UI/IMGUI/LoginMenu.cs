using System.Threading.Tasks;
using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.Manager;
using UnityEngine;

namespace _Project.Scripts.UI.IMGUI
{
    public class LoginMenu : MonoBehaviour
    {
        private string username = "";
        private string password = "";
        private bool showError = false;

        private void OnGUI()
        {
            var io = BootIODataManager.Instance;
            if (io == null) return;  // Safety

            // Background overlay
            if (io.ShowBackground)
            {
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            }

            // Main menu content
            if (io.ShowMainMenu)
            {
                // Your full menu drawing code here
                DrawMainMenu();
            }
        
        
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void DrawMainMenu()
        {
            // Menu always draws if manager says so (or fallback true)
            if (!BootIODataManager.Instance.ShowMainMenu) return;
        
            // Centered rect: Calculate position based on screen size
            Rect menuRect = new Rect(
                (Screen.width * 0.5f) - 200f,  // Center X (width/2)
                (Screen.height * 0.5f) - 300f, // Center Y (height/2)
                400f,  // Width
                600f   // Height
            );

            // Draw box for menu border
            GUI.Box(menuRect, "Login");

            // Begin layout inside rect (offsets from top-left)
            GUILayout.BeginArea(menuRect);

            GUILayout.Space(100f);  // Padding from top

            // Username field
            GUILayout.BeginHorizontal();
            GUILayout.Label("Username:", GUILayout.Width(100f));
            username = GUILayout.TextField(username);
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            // Password field
            GUILayout.BeginHorizontal();
            GUILayout.Label("Password:", GUILayout.Width(100f));
            password = GUILayout.PasswordField(password, '*');
            GUILayout.EndHorizontal();

            GUILayout.Space(50f);

            // 4 Buttons (centered horizontally)
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();  // Center buttons
            if (GUILayout.Button("Login", GUILayout.Width(150f)))
            {
                OnLoginClicked();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Register", GUILayout.Width(150f)))
            {
                Debug.Log("Register clicked – implement registration logic");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Guest Login", GUILayout.Width(150f)))
            {
                Debug.Log("Guest login clicked – load as guest");
                OnGuestLoginClicked();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Quit", GUILayout.Width(150f)))
            {
                Application.Quit();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Error message
            if (showError)
            {
                GUILayout.Space(50f);
                GUILayout.Label("<color=red>Invalid credentials!</color>", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
            }

            GUILayout.EndArea();
        }

        private void OnLoginClicked()
        {
            // if (string.IsNullOrEmpty(username) || string(password))
            // {
            //     showError = true;
            //     return;
            // }
            // ... validation ...
            BootIODataManager.Instance.ToggleMainMenu(false);
            BootIODataManager.Instance.DisableBootComponentIOBox();

            showError = false;
            Debug.Log($"Login with user: {username}, pass: {password} – implement auth logic");
            // Example: Load next scene on success
            // SceneManager.LoadScene("Harbour");
        }
    
        private void OnGuestLoginClicked()
        {
            Debug.Log("Guest login – bypassing auth");
            BootIODataManager.Instance.ToggleMainMenu(false);  // Hide IMGUI menu
            BootIODataManager.Instance.DisableBootComponentIOBox();  // Disable boot IO box
            LoadNextScene();
        }

        private void LoadNextScene()
        {
            // Hide + save menu state
            BootIODataManager.Instance.ToggleMainMenu(false);
            // Replace 1 with your Harbour/WorldMap group index
            _ = SceneLoader.Instance.BeginSceneTransition(1);
        }
    }
}
