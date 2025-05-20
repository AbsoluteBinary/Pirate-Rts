using _Project.Scripts.Utility;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _Project.Scripts.SceneManagement
{
    public class Bootstrapper : PersistentSingleton<Bootstrapper>
    {
        // NOTE: This script is intended to be placed in your first scene included in the build settings.
        private static readonly int sceneIndex = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Debug.Log("Bootstrapper initializing...");
#if UNITY_EDITOR
            // Set the bootstrapper scene to be the play mode start scene when running in the editor
            if (!EditorApplication.isPlaying) return; // Ensure this runs only in play mode
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[sceneIndex].path);
#endif
        }
    }
}
