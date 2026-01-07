using System;
using _Project.Scripts.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Main_Screen
{
    public class LoginMenuManager : MonoBehaviour
    {
        [SerializeField] Button toggleSceneButton; // Assign in Inspector or find dynamically
        [SerializeField] SceneLoader sceneLoader; // Reference to SceneLoader

        [Obsolete("Obsolete")]
        void Start()
        {
            // Find button dynamically if not assigned
            if (toggleSceneButton == null)
            {
                toggleSceneButton = GameObject.Find("ToggleSceneButton")?.GetComponent<Button>();
                if (toggleSceneButton == null)
                {
                    Debug.LogError("ToggleSceneButton not found in the scene. Please assign it in the Inspector or ensure it exists.");
                }
            }

            // Find SceneLoader dynamically if not assigned
            if (sceneLoader == null)
            {
                sceneLoader = FindObjectOfType<SceneLoader>();
                if (sceneLoader == null)
                {
                    Debug.LogError("SceneLoader not found in the scene. Please assign it in the Inspector or ensure it exists.");
                }
            }

            // Wire the button to toggle the next scene group
            if (toggleSceneButton != null && sceneLoader != null)
            {
                toggleSceneButton.onClick.AddListener(ToggleSceneGroup);
            }
            else
            {
                Debug.LogError("Cannot wire button: ToggleSceneButton or SceneLoader is missing.");
            }
        }

        // Method to toggle the next scene group, inspired by SceneLoaderEditor.cs
        private void ToggleSceneGroup()
        {
            if (sceneLoader != null)
            {
                //TODO: temp block out
                sceneLoader.ToggleNextSceneGroup();
                // Call the new toggle method
            }
            else
            {
                Debug.LogError("SceneLoader is null. Cannot toggle scene group.");
            }
        }

        // Clean up to avoid memory leaks
        void OnDestroy()
        {
            if (toggleSceneButton != null)
            {
                toggleSceneButton.onClick.RemoveListener(ToggleSceneGroup);
            }
        }
    }
}
