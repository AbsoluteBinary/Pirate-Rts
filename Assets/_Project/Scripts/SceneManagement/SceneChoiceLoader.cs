using UnityEngine;
using Sirenix.OdinInspector; // For Odin attributes

namespace _Project.Scripts.SceneManagement
{
    public class SceneChoiceLoader : MonoBehaviour
    {
        [FoldoutGroup("Scene Group Selection")] // Collapsible group for organization
        [Title("Group Index")] // Custom title for clarity
        [SerializeField] private int groupIndex = 0;

        // Alternative: Use enum for named dropdown (better UX)
         public enum SceneGroupOptions { Boot = 0, GamePlay = 1, Harbour = 2 } // Add your group names
         [SerializeField] private SceneGroupOptions selectedGroup;

        //Public method: Call this from Unity Button's OnClick (or other triggers)
        public void LoadChosenGroup()
        {
            Debug.Log("Button Clicked");
            Debug.Log($"SceneChoiceLoader: Attempting to load group index {groupIndex} from {gameObject.name}");
            
            if (SceneLoader.Instance == null)
            {
                Debug.LogError("SceneLoader instance not found! Ensure it's in the scene.");
                return;
            }
            
            if (SceneLoader.Instance.isLoading)
            {
                SceneLoader.Instance.LoadSpecificSceneGroup(groupIndex);
                Debug.LogWarning("Loading in progress; cannot load new group.");
                return;
            }
            
            
           // Debug.Log($"SceneChoiceLoader: Called LoadSpecificSceneGroup with index {groupIndex}");
        }
    }
}