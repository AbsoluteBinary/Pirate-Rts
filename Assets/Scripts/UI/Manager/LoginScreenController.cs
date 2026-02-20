using _Project.Scripts.SceneManagement;
using UI.IMGUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Manager
{
    public class LoginScreenController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private Button startGameButton;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("No UIDocument on this GameObject", this);
                return;
            }

            // Wait for next frame so rootVisualElement is ready
            Invoke(nameof(SetupButtons), 0f);
        }

        private void SetupButtons()
        {
            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("rootVisualElement is null – UIDocument not initialized yet");
                return;
            }

            startGameButton = root.Q<Button>("StartGame");
            if (startGameButton == null)
            {
                Debug.LogError("Button named 'StartGame' not found in UXML", this);
                return;
            }

            // Subscribe to click
            startGameButton.clicked += OnStartGameClicked;
            Debug.Log("StartGame button wired up successfully");
        }

        private void OnStartGameClicked()
        {
            //Debug.Log("StartGame button clicked – triggering scene transition");
            //Debug.Log("StartGame button clicked");
            
            //1. Hide LoginPanel via UIManager (robust & saved)
            //UIManager.Instance.SetPanelVisible("LoginPanel", false, isGlobal: true);
            
            // 2. Trigger IMGUI loading overlay
            if (IMGUILoadingOverlay.Instance != null)
            {
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.ToggleNextSceneGroup();
            }
            else
            {
                Debug.LogError("SceneLoader.Instance is null");
            }

            startGameButton.SetEnabled(false); // prevent spam

        }
        //ToggleNextSceneGroup

        private void OnDestroy()
        {
            if (startGameButton != null)
            {
                startGameButton.clicked -= OnStartGameClicked;
            }
        }
    }
}
