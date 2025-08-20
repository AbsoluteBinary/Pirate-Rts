// In your target script (e.g., BootUiControl.cs attached to the scene object)

using UnityEngine;

namespace _Project.Scripts.MainMenu_Controls
{
    public class BootUiControl : MonoBehaviour
    {
        public static BootUiControl Instance { get; private set; } // Global access point

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
            }
            else
            {
                Destroy(gameObject); // Destroy duplicates
            }
        }

        // Example method to access
        public void DoSomething()
        {
            Debug.Log("Accessed from prefab!");
        }
    }
}
