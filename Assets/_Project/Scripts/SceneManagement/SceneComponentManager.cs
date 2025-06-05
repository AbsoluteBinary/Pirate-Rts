using System;
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
        private readonly Dictionary<Type, Component[]> componentCache = new Dictionary<Type, Component[]>();

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
            _sceneGroupManager.OnSceneGroupLoaded += ManageSceneComponents;
            // Subscribe to scene load/unload events to update cache
            _sceneGroupManager.OnSceneLoaded += (sceneName) => InvalidateCache();
            _sceneGroupManager.OnSceneUnloaded += (sceneName) => InvalidateCache();
        }

        private void InvalidateCache()
        {
            componentCache.Clear();
            Debug.Log("Component cache invalidated due to scene load/unload.");
        }

        private void ManageSceneComponents()
        {
            ManageAudioListeners();
            ManageCameras();
            ManageEventSystems();
        }

        private void ManageAudioListeners()
        {
            var audioListeners = FindComponentsInLoadedScenes<AudioListener>();
            if (audioListeners.Count == 0)
            {
                Debug.LogWarning("No AudioListener found in loaded scenes.");
                return;
            }

            string activeSceneName = _sceneGroupManager.ActiveSceneGroup.FindSceneNameByType(SceneType.ActiveScene);
            AudioListener primaryListener = audioListeners.FirstOrDefault(l => l.gameObject.scene.name == activeSceneName);

            if (primaryListener == null)
            {
                Debug.LogWarning($"No AudioListener found in active scene '{activeSceneName}'. Using fallback.");
                primaryListener = audioListeners.First();
            }

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
                camera.enabled = isPrimary || camera.gameObject.activeInHierarchy;
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

        private List<T> FindComponentsInLoadedScenes<T>() where T : Component
        {
            Type componentType = typeof(T);
            if (componentCache.TryGetValue(componentType, out var cachedComponents))
            {
                Debug.Log($"Using cached {componentType.Name} components.");
                return cachedComponents.Cast<T>().Where(c => c != null).ToList();
            }

            var components = new List<T>();
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

            componentCache[componentType] = components.ToArray();
            Debug.Log($"Cached {components.Count} {componentType.Name} components.");
            return components;
        }

        private T SelectPrimaryComponent<T>(List<T> components) where T : Component
        {
            if (components.Count == 0) return null;

            var sceneComponents = components
                .GroupBy(c => c.gameObject.scene)
                .Select(g => new { Scene = g.Key, Components = g.ToList() })
                .ToList();

            foreach (var priorityType in _sceneTypePriority)
            {
                var matchingScene = sceneComponents.FirstOrDefault(sc =>
                    _sceneGroupManager.ActiveSceneGroup?.Scenes.Any(s => s.Name == sc.Scene.name && s.SceneType == priorityType) == true);
                if (matchingScene != null)
                {
                    return matchingScene.Components.First();
                }
            }

            return components.First();
        }
    }
}