using UnityEngine;
using Sirenix.OdinInspector; // For Odin attributes

namespace _Project.Scripts.SceneManagement
{
    public class SceneChoiceLoader : MonoBehaviour
    {
        // Removed the serialized groupIndex and selectedGroup fields, as we'll now pass the index as a parameter
        // from each button's OnClick event in the Inspector.

        // Public method: Call this from Unity Button's OnClick (or other triggers)
        // Now takes an int parameter for the group index, allowing each button to specify its own value
        public void LoadChosenGroup(int groupIndex)
        {
            Debug.Log("Button Clicked");
            Debug.Log($"SceneChoiceLoader: Attempting to load group index {groupIndex} from {gameObject.name}");
            
            if (SceneLoader.Instance == null)
            {
                Debug.LogError("SceneLoader instance not found! Ensure it's in the scene.");
                return;
            }
            
            // Fixed the logic here: Previously, it was loading when isLoading was true, which was likely a mistake.
            // Now, we check if NOT loading before proceeding.
            if (SceneLoader.Instance.isLoading)
            {
                Debug.LogWarning("Loading in progress; cannot load new group.");
                return;
            }
            
            SceneLoader.Instance.LoadSpecificSceneGroup(groupIndex);
            // Debug.Log($"SceneChoiceLoader: Called LoadSpecificSceneGroup with index {groupIndex}");
        }
    }
}