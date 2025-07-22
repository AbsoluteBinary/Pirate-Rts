using System;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;


namespace _Project.Scripts.SceneManagement
{
    public class StateDrivenCameraSetup : MonoBehaviour
    {
        // Reference to the main Unity Camera in the Boot scene
        [SerializeField] private Camera mainCamera;
        
        // Reference to the State Driven Camera component
        [SerializeField] private CinemachineStateDrivenCamera stateDrivenCamera;
        
        // Reference to the Animator controlling scene states
        [SerializeField] private Animator sceneStateAnimator;
        
        // Reference to SceneGroupManager to track scene groups
        [SerializeField] private SceneGroupManager sceneGroupManager;

        [Obsolete("Obsolete")]
        private void Awake()
        {
            // Ensure the main camera persists across scenes
            if (mainCamera != null)
            {
                DontDestroyOnLoad(mainCamera.gameObject);
                Debug.Log("Main Camera set to persist from Boot scene.");
            }
            else
            {
                Debug.LogError("Main Camera reference not set!");
            }

            // Validate components
            if (stateDrivenCamera == null)
            {
                stateDrivenCamera = GetComponent<CinemachineStateDrivenCamera>();
                if (stateDrivenCamera == null)
                {
                    Debug.LogError("CinemachineStateDrivenCamera not found on this GameObject!");
                    return;
                }
            }

            if (sceneStateAnimator == null)
            {
                Debug.LogError("Scene State Animator not assigned!");
                return;
            }

            if (sceneGroupManager == null)
            {
                sceneGroupManager = FindObjectOfType<SceneLoader>()?.manager;
                if (sceneGroupManager == null)
                {
                    Debug.LogError("SceneGroupManager not found or assigned!");
                    return;
                }
            }
        }

        private void Start()
        {
            // Subscribe to scene group loaded event to update Animator state
            sceneGroupManager.OnSceneGroupLoaded += UpdateSceneState;
            // Initial call to set the Animator state
            UpdateSceneState();
        }

        // Updates the Animator state based on the active scene group
        private void UpdateSceneState()
        {
            if (sceneGroupManager?.ActiveSceneGroup == null)
            {
                Debug.LogWarning("No active SceneGroup found. Defaulting Animator state.");
                sceneStateAnimator.SetTrigger("Default");
                return;
            }

            string activeSceneName = sceneGroupManager.ActiveSceneGroup.FindSceneNameByType(SceneType.ActiveScene);
            if (string.IsNullOrEmpty(activeSceneName))
            {
                Debug.LogWarning("No ActiveScene type found. Defaulting Animator state.");
                sceneStateAnimator.SetTrigger("Default");
                return;
            }

            // Example: Set Animator state based on SceneType
            SceneType sceneType = sceneGroupManager.ActiveSceneGroup.Scenes
                .FirstOrDefault(s => s.Name == activeSceneName)?.SceneType ?? SceneType.Environment;
            
            // Trigger Animator states (assumes your Animator Controller has states like "ActiveScene", "MainMenu", etc.)
            sceneStateAnimator.SetTrigger(sceneType.ToString());
            Debug.Log($"Set Animator state to {sceneType} for scene {activeSceneName}");
        }
    }
}
