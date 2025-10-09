using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.SceneManagement
{
    public class SceneGroupManager
    {
        public event Action<string> OnSceneLoaded = delegate { };
        public event Action<string> OnSceneUnloaded = delegate { };
        public event Action OnSceneGroupLoaded = delegate { };

        readonly AsyncOperationHandleGroup handleGroup = new AsyncOperationHandleGroup(10);

        public SceneGroup ActiveSceneGroup;

        public async Task LoadScenes(SceneGroup group, IProgress<float> progress, bool reloadDupScenes = false)
        {
            ActiveSceneGroup = group;
            var loadedScenes = new List<string>();

            

            int sceneCount = SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }

            var totalScenesToLoad = ActiveSceneGroup.Scenes.Count;
            var operationGroup = new AsyncOperationGroup(totalScenesToLoad);
            var scenesToProcess = new List<SceneData>();

            for (var i = 0; i < totalScenesToLoad; i++)
            {
                var sceneData = group.Scenes[i];
                if (!reloadDupScenes && loadedScenes.Contains(sceneData.Name)) continue;
                scenesToProcess.Add(sceneData);
            }

            if (scenesToProcess.Count == 0)
            {
                progress?.Report(1f);
                OnSceneGroupLoaded.Invoke();
                return;
            }

            for (var i = 0; i < scenesToProcess.Count; i++)
            {
                var sceneData = scenesToProcess[i];
                if (sceneData.Reference.State == SceneReferenceState.Regular)
                {
                    var operation = SceneManager.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                    operationGroup.Operations.Add(operation);
                }
                else if (sceneData.Reference.State == SceneReferenceState.Addressable)
                {
                    var sceneHandle = Addressables.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                    handleGroup.Handles.Add(sceneHandle);
                }
                OnSceneLoaded.Invoke(sceneData.Name);
            }

            float lastReportedProgress = 0f;
            while (!operationGroup.IsDone || !handleGroup.IsDone)
            {
                float totalProgress = (operationGroup.Progress + handleGroup.Progress) / 2f;
                if (Mathf.Abs(totalProgress - lastReportedProgress) > 0.01f)
                {
                    progress?.Report(totalProgress);
                    lastReportedProgress = totalProgress;
                }
                await Task.Delay(16);
            }

            progress?.Report(1f);

            Scene activeScene = SceneManager.GetSceneByName(ActiveSceneGroup.FindSceneNameByType(SceneType.ActiveScene));
            if (activeScene.IsValid())
            {
                SceneManager.SetActiveScene(activeScene);
                Debug.Log("Set active scene to: " + activeScene.name); // New log to confirm
            }
            else
            {
                Debug.LogWarning("No valid ActiveScene found in group—check SceneType in Inspector.");
            }

            OnSceneGroupLoaded.Invoke();
        }

        public async Task UnloadScenes()
        {
            var scenes = new List<string>();
            var activeScene = SceneManager.GetActiveScene().name;

            int sceneCount = SceneManager.sceneCount;
            Debug.Log($"UnloadScenes: Total loaded scenes: {sceneCount}. Active: {activeScene}");

            for (var i = sceneCount - 1; i > 0; i--)
            {
                var sceneAt = SceneManager.GetSceneAt(i);
                Debug.Log($"Checking scene: {sceneAt.name} (Loaded: {sceneAt.isLoaded}, Active: {sceneAt.name == activeScene})");

                if (!sceneAt.isLoaded) { Debug.Log($"Skip: Not loaded - {sceneAt.name}"); continue; }
                if (sceneAt.name.Equals(activeScene)) { Debug.Log($"Skip: Active - {sceneAt.name}"); continue; }
                if (sceneAt.name == "Bootstrapper") { Debug.Log($"Skip: Bootstrapper - {sceneAt.name}"); continue; }
                if (handleGroup.Handles.Any(h => h.IsValid() && h.Result.Scene.name == sceneAt.name)) { Debug.Log($"Skip: Addressable - {sceneAt.name}"); continue; }

                scenes.Add(sceneAt.name);
                Debug.Log($"Queuing unload: {sceneAt.name}");
            }

            var operationGroup = new AsyncOperationGroup(scenes.Count);

            foreach (var scene in scenes)
            {
                var operation = SceneManager.UnloadSceneAsync(scene);
                if (operation == null)
                {
                    Debug.LogWarning($"Unload failed: Operation null for {scene}. Is it loaded/valid?");
                    continue;
                }
                operationGroup.Operations.Add(operation);
                OnSceneUnloaded.Invoke(scene);
                Debug.Log($"Started unloading: {scene}");
            }

            // Addressables unload (if any—add your existing code here if needed)

            float lastReportedProgress = 0f;
            while (!operationGroup.IsDone || !handleGroup.IsDone)
            {
                float totalProgress = (operationGroup.Progress + handleGroup.Progress) / 2f;
                if (Mathf.Abs(totalProgress - lastReportedProgress) > 0.01f)
                {
                    Debug.Log($"Unload progress: {totalProgress}");
                    lastReportedProgress = totalProgress;
                }
                await Task.Delay(16);
            }

            await Resources.UnloadUnusedAssets(); // If in your original code

            Debug.Log("Unload complete. Remaining scenes: " + SceneManager.sceneCount);
        }
    }

    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> Operations;

        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);
        public bool IsDone => Operations.All(o => o.isDone);

        public AsyncOperationGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperation>(initialCapacity);
        }
    }

    public readonly struct AsyncOperationHandleGroup
    {
        public readonly List<AsyncOperationHandle<SceneInstance>> Handles;

        public float Progress => Handles.Count == 0 ? 0 : Handles.Average(h => h.PercentComplete);
        public bool IsDone => Handles.Count == 0 || Handles.All(o => o.IsDone);

        public AsyncOperationHandleGroup(int initialCapacity)
        {
            Handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }
}