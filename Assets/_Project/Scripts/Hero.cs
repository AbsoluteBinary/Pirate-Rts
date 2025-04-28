using _Project.Scripts.Inventory.Helpers;
using _Project.Scripts.Persistence;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts
{
    public class Hero : MonoBehaviour, IBind<PlayerData>
    {
        [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
    
        [SerializeField] public PlayerData playerData;
        
        public void Bind(PlayerData pData)
        {
            playerData = pData;
            playerData.Id = Id;
        }
    
        private void Update()
        {
            var heroTransform = transform;
            heroTransform.position = playerData.position;
            heroTransform.rotation = playerData.rotation;
        }
        private void LateUpdate()
        {
            
            //Debug.Log(gameData.playerData.Id.ToGuid().ToString());
        }

        // [Button]
        // private void DisplayPlayerID()
        // {
        //     //string playerID;
        //     string playerID = playerData.Id.ToGuid().ToString();
        //     Debug.Log($"The Player ID is : {playerID}");
        // }
        

        
    }
}

