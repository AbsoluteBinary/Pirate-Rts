using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Level_Managers
{

    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelData[] levels; // Array of levels

        // void Start()
        // {
        //     // Example: Initialize manually (optional if set in Inspector)
        //     levels = new LevelData[3];
        //     levels[1] = new LevelData("Level1", 1);
        //     levels[2] = new LevelData("Level2", 2);
        //     levels[3] = new LevelData("Level3", 3);
        //
        //     // Example: Access and log all level numbers
        //     foreach (LevelData level in levels)
        //     {
        //         Debug.Log($"Scene: {level.sceneName}, Level Number: {level.levelNumber}");
        //     }
        // }

        public int GetLevelNumber(string sceneName)
        {
            foreach (LevelData level in levels)
            {
                if (level.sceneName == sceneName)
                {
                    return level.levelNumber;
                }
            }
            Debug.LogWarning($"Scene {sceneName} not found.");
            return -1;
        }

        public void LoadLevelByNumber(int levelNumber)
        {
            foreach (LevelData level in levels)
            {
                if (level.levelNumber == levelNumber)
                {
                    SceneManager.LoadScene(level.sceneName);
                    return;
                }
            }
            Debug.LogWarning($"Level number {levelNumber} not found.");
        }
        
        //TODO:Implement a method to load a level choosen with a clause to cvheck for space withing level population. ie population limit
        //Use this random level picker to set a random level
        // public void LoadRandomLevel()
        // {
        //     if (levels == null || levels.Length == 0)
        //     {
        //         Debug.LogWarning("No levels defined in the array!");
        //         return;
        //     }
        //
        //     int randomIndex = Random.Range(0, levels.Length);
        //     SceneManager.LoadScene(levels[randomIndex].sceneName);
        // }
    }
}