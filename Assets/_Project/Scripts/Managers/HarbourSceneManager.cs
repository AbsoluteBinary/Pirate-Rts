using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Managers
{

    public class HarbourSceneManager : MonoBehaviour
    {
        void Start()
        {
            // Optional: Verify SceneUIManager exists
            //if (SceneUIManager.Instance == null)
            {
                Debug.LogError("SceneUIManager instance not found! Ensure it’s initialized.");
            }
        }

        public void LoadFreeRoamScene()
        {
            // Load the FreeRoamScene
            SceneManager.LoadScene("FreeRoamScene");

            // SceneUIManager will automatically load the correct UI via OnSceneLoaded
            //if (SceneUIManager.Instance != null)
            {
                Debug.Log("Loading FreeRoamScene; SceneUIManager will handle UI.");
            }
            //else
            {
                Debug.LogError("Cannot verify UI loading: SceneUIManager not found.");
            }
        }
    }
}
