using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class SceneUIManager : MonoBehaviour
    {
        public static SceneUIManager Instance { get; private set; }
        [SerializeField] private GameObject adminCanvasPrefab; // Canvas with Admin panel
        [SerializeField] private GameObject[] sceneUiPrefabs; // Array: DefaultUIPrefab, FreeRoamSceneUI, HarbourSceneUI, HarbourSceneBuildModeUI, CombatSceneUI
        private GameObject canvasInstance; // Persistent Canvas
        private GameObject adminPanel; // Admin panel child
        private GameObject currentSceneUi; // Current scene UI instance

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("SceneUIManager initialized as singleton.");
            }
            else
            {
                Debug.LogWarning("Duplicate SceneUIManager found, destroying this instance.");
                Destroy(gameObject);
                return;
            }

            // Instantiate the Canvas prefab
            if (adminCanvasPrefab != null)
            {
                canvasInstance = Instantiate(adminCanvasPrefab);
                canvasInstance.name = "PersistentAdminCanvas";
                DontDestroyOnLoad(canvasInstance);
                Canvas canvas = canvasInstance.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 10; // Ensure UI is on top
                    Debug.Log("PersistentAdminCanvas sorting order set to 10.");
                }

                adminPanel = canvasInstance.transform.Find("Admin")?.gameObject;
                if (adminPanel != null)
                {
                    adminPanel.SetActive(false);
                    Debug.Log("Admin panel found and set to inactive.");
                }
                else
                {
                    Debug.LogError("Admin panel not found in Canvas prefab! Check prefab hierarchy.");
                    foreach (Transform child in canvasInstance.transform)
                    {
                        Debug.Log("Child found: " + child.name);
                    }
                }

                // Load default UI for initial scene
                LoadSceneUI(GetUIIndexForScene(SceneManager.GetActiveScene().name));
            }
            else
            {
                Debug.LogError("Admin Canvas prefab is not assigned!");
            }
        }

        private void OnEnable()
        {
            // Subscribe to scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            // Unsubscribe
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            // Toggle Admin panel with 'I' key
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("I key pressed, toggling Admin panel!");
                if (adminPanel != null)
                {
                    adminPanel.SetActive(!adminPanel.activeSelf);
                    Debug.Log("Admin panel toggled to: " + adminPanel.activeSelf);
                }
                else
                {
                    Debug.LogWarning("Admin panel reference is missing!");
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Update CameraManager for scene-specific cameras
            // Temp comment out
            // if (CameraManager.Instance != null)
            // {
            //     CameraManager.Instance.UpdateCamerasForScene(scene.name);
            // }
            LoadSceneUI(GetUIIndexForScene(scene.name));
        }

        private int GetUIIndexForScene(string sceneName)
        {
            switch (sceneName)
            {
                case "BootstrapScene":
                    return 0; // DefaultUIPrefab (Start/Exit panel)
                case "HarbourScene":
                    return 1; // FreeRoamSceneUI
                case "FreeRoamScene":
                    return 2; // HarbourSceneUI (default)
                case "CombatScene":
                    return 4; // CombatSceneUI
                default:
                    Debug.LogWarning($"No UI defined for scene: {sceneName}");
                    return -1;
            }
        }

        // Load UI prefab by index
        private void LoadSceneUI(int uiIndex)
        {
            // Destroy current scene UI
            if (currentSceneUi != null)
            {
                Destroy(currentSceneUi);
                Debug.Log("Destroyed previous scene UI.");
            }

            // Instantiate new scene UI
            if (uiIndex >= 0 && uiIndex < sceneUiPrefabs.Length && sceneUiPrefabs[uiIndex] != null)
            {
                currentSceneUi = Instantiate(sceneUiPrefabs[uiIndex], canvasInstance.transform);
                currentSceneUi.name = sceneUiPrefabs[uiIndex].name;
                Debug.Log($"Loaded scene UI: {currentSceneUi.name}");
            }
            else
            {
                Debug.LogWarning($"No valid UI prefab for index: {uiIndex}");
            }
        }

        // Public method to switch UI mode (e.g., Harbour build mode)
        public void SetUIMode(string mode)
        {
            int uiIndex = -1;
            switch (mode)
            {
                case "HarbourNormal":
                    uiIndex = 2; // HarbourSceneUI
                    break;
                case "HarbourBuild":
                    uiIndex = 3; // HarbourSceneBuildModeUI
                    break;
                default:
                    Debug.LogWarning($"Unknown UI mode: {mode}");
                    break;
            }

            if (uiIndex >= 0)
            {
                LoadSceneUI(uiIndex);
            }
        }
    }
}