using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Managers
{
    //public class SceneUIManager : MonoBehaviour
    //{
    //     public static SceneUIManager Instance { get; private set; }
    //     [SerializeField] private GameObject adminCanvasPrefab; // Canvas with Admin panel
    //     [SerializeField] private GameObject[] sceneUiPrefabs; // Array: DefaultUIPrefab, FreeRoamSceneUI, HarbourSceneUI, HarbourSceneBuildModeUI, CombatSceneUI
    //     private GameObject canvasInstance; // Persistent Canvas
    //     private GameObject adminPanel; // Admin panel child
    //     private GameObject currentSceneUi; // Current scene UI instance
    //     private static bool isCanvasInstantiated = false; // Track canvas instantiation
    //
    //     private void Awake()
    //     {
    //         // Strengthen singleton pattern
    //         if (Instance != null && this != Instance)
    //         {
    //             Debug.LogWarning($"Duplicate SceneUIManager on {gameObject.name}, destroying this instance.");
    //             Destroy(gameObject);
    //             return;
    //         }
    //
    //         // Set singleton instance
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //         Debug.Log($"SceneUIManager initialized as singleton on {gameObject.name}.");
    //
    //         // Instantiate the Canvas prefab only once
    //         if (adminCanvasPrefab != null && !isCanvasInstantiated)
    //         {
    //             canvasInstance = Instantiate(adminCanvasPrefab);
    //             canvasInstance.name = "PersistentAdminCanvas";
    //             DontDestroyOnLoad(canvasInstance);
    //             isCanvasInstantiated = true; // Prevent further instantiations
    //             Canvas canvas = canvasInstance.GetComponent<Canvas>();
    //             if (canvas != null)
    //             {
    //                 canvas.sortingOrder = 10; // Ensure UI is on top
    //             }
    //
    //             adminPanel = canvasInstance.transform.Find("Admin")?.gameObject;
    //             if (adminPanel != null)
    //             {
    //                 adminPanel.SetActive(false);
    //             }
    //             else
    //             {
    //                 Debug.LogError("Admin panel not found in Canvas prefab! Check prefab hierarchy.");
    //             }
    //
    //             // Load default UI for initial scene
    //             LoadSceneUI(GetUIIndexForScene(SceneManager.GetActiveScene().name));
    //         }
    //         else if (adminCanvasPrefab == null)
    //         {
    //             Debug.LogError("Admin Canvas prefab is not assigned!");
    //         }
    //         else
    //         {
    //             Debug.LogWarning("Canvas already instantiated, skipping instantiation.");
    //         }
    //     }
    //
    //     private void OnEnable()
    //     {
    //         SceneManager.sceneLoaded += OnSceneLoaded;
    //     }
    //
    //     private void OnDisable()
    //     {
    //         SceneManager.sceneLoaded -= OnSceneLoaded;
    //     }
    //
    //     private void Update()
    //     {
    //         if (Input.GetKeyDown(KeyCode.I) && adminPanel != null)
    //         {
    //             adminPanel.SetActive(!adminPanel.activeSelf);
    //         }
    //     }
    //
    //     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //     {
    //         LoadSceneUI(GetUIIndexForScene(scene.name));
    //     }
    //
    //     private int GetUIIndexForScene(string sceneName)
    //     {
    //         switch (sceneName)
    //         {
    //             case "BootstrapScene":
    //                 return 0; // DefaultUIPrefab (Start/Exit panel)
    //             case "HarbourScene":
    //                 return 1; // FreeRoamSceneUI
    //             case "FreeRoamScene":
    //                 return 2; // HarbourSceneUI
    //             case "CombatScene":
    //                 return 4; // CombatSceneUI
    //             default:
    //                 Debug.LogWarning($"No UI defined for scene: {sceneName}");
    //                 return -1;
    //         }
    //     }
    //
    //     private void LoadSceneUI(int uiIndex)
    //     {
    //         if (currentSceneUi != null)
    //         {
    //             Destroy(currentSceneUi);
    //         }
    //
    //         if (uiIndex >= 0 && uiIndex < sceneUiPrefabs.Length && sceneUiPrefabs[uiIndex] != null)
    //         {
    //             currentSceneUi = Instantiate(sceneUiPrefabs[uiIndex], canvasInstance.transform);
    //             currentSceneUi.name = sceneUiPrefabs[uiIndex].name;
    //         }
    //         else
    //         {
    //             Debug.LogWarning($"No valid UI prefab for index: {uiIndex}");
    //         }
    //     }
    //
    //     public void SetUIMode(string mode)
    //     {
    //         int uiIndex = -1;
    //         switch (mode)
    //         {
    //             case "HarbourNormal":
    //                 uiIndex = 2; // HarbourSceneUI
    //                 break;
    //             case "HarbourBuild":
    //                 uiIndex = 3; // HarbourSceneBuildModeUI
    //                 break;
    //             default:
    //                 Debug.LogWarning($"Unknown UI mode: {mode}");
    //                 break;
    //         }
    //
    //         if (uiIndex >= 0)
    //         {
    //             LoadSceneUI(uiIndex);
    //         }
    //     }
    //     
    //     public GameObject GetAdminPanel()
    //     {
    //         return adminPanel;
    //     }
    // }
}