using _Project.Scripts;
using _Project.Scripts._Project.Scripts.Persistence;
using _Project.Scripts.Persistence;
using Level_Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Main_Screen
{
    public class StartGame : MonoBehaviour
    {
        [ShowInInspector, PropertyTooltip("This is Username Variables.")]
        //public TMP_InputField usernameInputField;
        public TMP_InputField gameNameInputField;
        private string username;
        private string gamename;
        
        [SerializeField] public PlayerData _playerData;
        [SerializeField] public LevelManager _LevelManager;

        //[SerializeField] public GameData _saveLoadSystem;
        public void StartNewGame()
        {
            //if (username != null && gamename != null)
            if(gamename != null)
            {
                _LevelManager.LoadLevelByNumber(0);
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                Debug.Log("Start a New Game");
            }
            else
            {
                Debug.Log("Username is null, cannot start a new game.");
            }
        }
        
        void Start()
        {
            //if (usernameInputField != null && gameNameInputField != null) 
            if (gameNameInputField != null)
            {
                //usernameInputField.onEndEdit.AddListener(SetUsername);
                gameNameInputField.onEndEdit.AddListener(SetGameName);
            }
            else
            {
                //Debug.LogError("usernameInputField is not assigned in the Inspector!");
                Debug.LogError("gameNameInputField is not assigned in the Inspector!");
            }
        }
        
        // This method is called when the user finishes editing (presses Enter or clicks away)
        private void SetUsername(string inputText)
        {
            username = inputText;
            if (_playerData != null)
            {
                _playerData.playerUserName = username; // Sync with PlayerData
                Debug.Log("Updated Username to: " + username + _playerData.playerUserName);
            }
            else
            {
                Debug.LogWarning("_playerData is null, cannot update playerUserName.");
            }
        }
        // This method is called when the user finishes editing (presses Enter or clicks away)
        private void SetGameName(string inputText)
        {
            gamename = inputText;
            if (SaveLoadSystem.Instance != null)
            {
                //_saveLoadSystem.gameName = gamename;
                SaveLoadSystem.Instance.SetGameName(inputText);  // Sync with SaveLoadSystem
                Debug.Log("Updated game name: " + SaveLoadSystem.Instance.GetGameName());
            }
            else
            {
                Debug.LogWarning("_saveLoadSystem is null, cannot update gameName.");
            }
        }

        // Optional: Method to access the username later
        public string GetUsername()
        {
            return username;
        }
        // Optional: Method to access the username later
        public string GetGameName()
        {
            return gamename;
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
