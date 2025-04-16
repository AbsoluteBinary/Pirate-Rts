using Level_Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Main_Screen
{
    public class SimpleStartGame : MonoBehaviour
    {
        [SerializeField] private GameObject EnterExitPanel;
        //[SerializeField] public GameData _saveLoadSystem;
        public void StartGame()
        {
                //_LevelManager.LoadLevelByNumber(0);
                SceneManager.LoadScene("World Map");
                EnterExitPanel.SetActive(false);
                //EnterExitPanel.SetActive(!EnterExitPanel.activeSelf);
                Debug.Log("Start Game");
            
        }
    }
}
