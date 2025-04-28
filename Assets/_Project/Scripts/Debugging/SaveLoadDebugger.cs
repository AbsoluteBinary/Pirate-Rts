using UnityEngine;
using Sirenix.OdinInspector;
using _Project.Scripts.Persistence;
using _Project.Scripts.Inventory.Helpers;

namespace _Project.Scripts.Debugging
{
    public class SaveLoadDebugger : MonoBehaviour
    {
        [SerializeField] private SaveLoadSystem saveLoadSystem;

        [Button("Debug Player ID & Refresh"), ShowInInspector]
        private void DebugPlayerIdAndRefresh()
        {
            if (saveLoadSystem == null)
            {
                saveLoadSystem = FindObjectOfType<SaveLoadSystem>();
                if (saveLoadSystem == null)
                {
                    Debug.LogError("SaveLoadSystem not found in scene!");
                    return;
                }
            }

            var gameData = saveLoadSystem.gameData;
            if (gameData == null)
            {
                Debug.LogError("gameData is null in SaveLoadSystem!");
            }
            else if (gameData.playerData == null)
            {
                Debug.LogError("gameData.playerData is null in SaveLoadSystem!");
            }
            else
            {
                string playerID = gameData.playerData.Id.ToGuid().ToString();
                Debug.Log($"Debug Player ID: {playerID} (Hex: {gameData.playerData.Id.ToHexString()})");
            }

#if UNITY_EDITOR
            if (saveLoadSystem != null)
            {
                UnityEditor.EditorUtility.SetDirty(saveLoadSystem);
                Debug.Log("Inspector refreshed for SaveLoadSystem.");
            }
#endif
        }
    }
}
