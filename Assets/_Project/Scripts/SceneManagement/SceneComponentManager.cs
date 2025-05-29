using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.SceneManagement
{
    public class SceneComponentManager
    {
        private readonly SceneGroupManager _sceneGroupManager;
        
        // Priority order for scenes when selecting primary components
        private readonly SceneType[] _sceneTypePriority = {
            SceneType.ActiveScene,
            SceneType.HUD,
            SceneType.MainMenu,
            SceneType.UserInterface,
            SceneType.Cinematic,
            SceneType.Environment,
            SceneType.Tooling
        };

        public SceneComponentManager(SceneGroupManager sceneGroupManager)
        {
            _sceneGroupManager = sceneGroupManager;
            // Subscribe to scene group loaded event
            _sceneGroupManager.OnSceneGroupLoaded += ManageSceneComponents;
        }

        private void ManageSceneComponents()
        {
            // Manage AudioListeners
            ManageAudioListeners();

            // Manage Cameras
            ManageCameras();

            // Manage EventSystems
            ManageEventSystems();
        }

        private void ManageAudioListeners()
        {
            // Find all AudioListeners in currently loaded scenes
            var audioListeners = FindComponentsInLoadedScenes<AudioListener>();
            if (audioListeners.Count == 0)
            {
                Debug.LogWarning("No AudioListener found in loaded scenes.");
                return;
            }

            // Get the active scene name from the current scene group
            string activeSceneName = _sceneGroupManager.ActiveSceneGroup.FindSceneNameByType(SceneType.ActiveScene);
            AudioListener primaryListener = audioListeners.FirstOrDefault(l => l.gameObject.scene.name == activeSceneName);

            if (primaryListener == null)
            {
                Debug.LogWarning($"No AudioListener found in active scene '{activeSceneName}'. Using fallback.");
                primaryListener = audioListeners.First(); // Fallback to first found
            }

            // Enable the primary AudioListener, disable all others
            foreach (var listener in audioListeners)
            {
                bool isPrimary = listener == primaryListener;
                listener.enabled = isPrimary;
                if (!isPrimary)
                {
                    Debug.Log($"Disabled AudioListener on {listener.gameObject.name} in scene {listener.gameObject.scene.name}");
                }
                else
                {
                    Debug.Log($"Enabled AudioListener on {listener.gameObject.name} in scene {listener.gameObject.scene.name}");
                }
            }
        }
        
        

// Helper method (example implementation)
        private List<T> FindComponentsInLoadedScenes<T>() where T : Component
        {
            List<T> components = new List<T>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    GameObject[] rootObjects = scene.GetRootGameObjects();
                    foreach (var go in rootObjects)
                    {
                        components.AddRange(go.GetComponentsInChildren<T>());
                    }
                }
            }
            return components;
        }

        private void ManageCameras()
        {
            var cameras = FindComponentsInLoadedScenes<Camera>();
            if (cameras.Count == 0)
            {
                Debug.LogWarning("No Camera found in loaded scenes.");
                return;
            }

            var primaryCamera = SelectPrimaryComponent(cameras);
            foreach (var camera in cameras)
            {
                bool isPrimary = camera == primaryCamera;
                camera.enabled = isPrimary || camera.gameObject.activeInHierarchy; // Keep active cameras enabled if needed
                if (!isPrimary && camera.enabled)
                    Debug.Log($"Disabled Camera on {camera.gameObject.name} in scene {camera.gameObject.scene.name}");
            }

            Debug.Log($"Primary Camera: {primaryCamera.gameObject.name} in scene {primaryCamera.gameObject.scene.name}");
        }

        private void ManageEventSystems()
        {
            var eventSystems = FindComponentsInLoadedScenes<EventSystem>();
            if (eventSystems.Count == 0)
            {
                Debug.LogWarning("No EventSystem found in loaded scenes.");
                return;
            }

            var primaryEventSystem = SelectPrimaryComponent(eventSystems);
            if (primaryEventSystem == null)
            {
                Debug.LogWarning("No EventSystem found in priority scene. Using fallback.");
                primaryEventSystem = eventSystems.First();
            }

            foreach (var eventSystem in eventSystems)
            {
                bool isPrimary = eventSystem == primaryEventSystem;
                eventSystem.enabled = isPrimary;
                if (isPrimary)
                {
                    Debug.Log($"Enabled EventSystem on {eventSystem.gameObject.name} in scene {eventSystem.gameObject.scene.name}");
                }
                else
                {
                    Debug.Log($"Disabled EventSystem on {eventSystem.gameObject.name} in scene {eventSystem.gameObject.scene.name}");
                }
            }
        }

        

        private T SelectPrimaryComponent<T>(List<T> components) where T : Component
        {
            if (components.Count == 0) return null;

            // Group components by scene
            var sceneComponents = components
                .GroupBy(c => c.gameObject.scene)
                .Select(g => new { Scene = g.Key, Components = g.ToList() })
                .ToList();

            // Find the scene with highest priority SceneType
            foreach (var priorityType in _sceneTypePriority)
            {
                var matchingScene = sceneComponents.FirstOrDefault(sc =>
                    _sceneGroupManager.ActiveSceneGroup?.Scenes.Any(s => s.Name == sc.Scene.name && s.SceneType == priorityType) == true);
                if (matchingScene != null)
                {
                    // Return the first component from the highest-priority scene
                    return matchingScene.Components.First();
                }
            }

            // Fallback: Return first component from any scene
            return components.First();
        }
    }
}