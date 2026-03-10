using _Project.Scripts.SceneManagement;
using UI.IMGUI;
using UI.WorldMap;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Manager
{
    public class LoginScreenController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private Button startGameButton;
        private Button enterHarbourButton;

        [SerializeField] private int targetSceneGroupIndex = 2;
        
        private void Awake()
        {
            WorldSpaceInteractionsEventBus.HarbourButtonClicked += OnEnterHarbourClicked;
            Debug.Log("HUDController: Awake called. Registering event.");
            
            
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
            enterHarbourButton = root.Q<Button>("EnterHarbour");
            if (startGameButton == null && enterHarbourButton == null)
            {
                Debug.LogError("Button named 'StartGame' not found in UXML", this);
                Debug.LogError($"Button named 'EnterHarbour' not found in UXML", this);
                return;
            }

            // Subscribe to click
            startGameButton.clicked += OnStartGameClicked;
            enterHarbourButton.clicked += OnEnterHarbourClicked;
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

        private void OnEnterHarbourClicked()
        {
            if (IMGUILoadingOverlay.Instance != null)
            {
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.ToggleNextSceneGroup();
            }
            
            _ = SceneLoader.Instance.BeginSceneTransition(targetSceneGroupIndex);

            enterHarbourButton.SetEnabled(false);
        }
        //ToggleNextSceneGroup

        private void OnDestroy()
        {
            WorldSpaceInteractionsEventBus.HarbourButtonClicked -= OnEnterHarbourClicked;
            
            
            if (startGameButton != null)
            {
                startGameButton.clicked -= OnStartGameClicked;
            }
        }
    }
}
