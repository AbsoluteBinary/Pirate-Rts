// File: LevelData.cs

using UnityEditor;
using UnityEngine;

// Required for SceneAsset

namespace _Project.Scripts.Level_Managers
{
    [System.Serializable]
    public class LevelData
    {
#if UNITY_EDITOR
        [SerializeField] private SceneAsset scene; // Assign scenes in Inspector
#endif
        [SerializeField] public string sceneName; // Runtime-accessible scene name
        public int levelNumber;

        // Constructor
        public LevelData(string name, int number)
        {
            sceneName = name;
            levelNumber = number;
        }

        public LevelData() { }

#if UNITY_EDITOR
        // Ensure sceneName stays in sync with the SceneAsset
        public void OnValidate()
        {
            if (scene != null && sceneName != scene.name)
            {
                sceneName = scene.name;
            }
        }
#endif
    }
}