using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Main_Screen
{
    public class MainMenuController : MonoBehaviour
    {
        
        public void OnStartButtonClicked()
        {
            Debug.Log("Start button clicked, loading FreeRoamScene.");
            SceneManager.LoadScene("FreeRoamScene"); // Adjust scene name
        }

        public void LoadHarbour()
        {
            SceneManager.LoadScene("HarbourScene");
        }

        public void OnExitButtonClicked()
        {
            Debug.Log("Exit button clicked, quitting game.");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stop play in Editor
#endif
        }
    }
}
