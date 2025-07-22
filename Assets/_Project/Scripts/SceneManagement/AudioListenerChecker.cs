using System;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.SceneManagement
{
    public class AudioListenerChecker : MonoBehaviour
    {
        [Obsolete("Obsolete")]
        void Update()
        {
            var activeListeners = FindObjectsOfType<AudioListener>().Where(l => l.enabled).ToArray();
            if (activeListeners.Length > 1)
            {
                Debug.LogError($"Multiple AudioListeners active: {activeListeners.Length}");
                foreach (var listener in activeListeners)
                {
                    Debug.Log($"Active AudioListener on {listener.gameObject.name} in scene {listener.gameObject.scene.name}");
                }
            }
        }
    }
}
