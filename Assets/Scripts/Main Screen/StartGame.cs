using UnityEngine;
using UnityEngine.SceneManagement;

namespace Main_Screen
{
    public class StartGame : MonoBehaviour
    {
        public void StartNewGame()
        {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            Debug.Log("Start a New Game");
            SceneManager.LoadScene("Harbour");
        }
        
        public void ExitGame()
        {
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
        }
    }
}
