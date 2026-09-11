using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.SceneManagement
{
    public class AudioListenerChecker : MonoBehaviour
    {
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnforceSingleListener();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnforceSingleListener();
        }

        public static void EnforceSingleListener()
        {
            var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (listeners.Length == 0) return;

            var active = SceneManager.GetActiveScene();
            AudioListener keep = null;

            for (int i = 0; i < listeners.Length; i++)
            {
                if (listeners[i].gameObject.scene == active)
                {
                    keep = listeners[i];
                    break;
                }
            }

            if (keep == null)
                keep = listeners[0];

            int disabled = 0;
            for (int i = 0; i < listeners.Length; i++)
            {
                bool on = listeners[i] == keep;
                if (listeners[i].enabled != on)
                    listeners[i].enabled = on;
                if (!on) disabled++;
            }

            if (disabled > 0)
                Debug.Log($"AudioListenerChecker: kept '{keep.gameObject.name}' ({keep.gameObject.scene.name}), disabled {disabled}.");
        }
    }
}
