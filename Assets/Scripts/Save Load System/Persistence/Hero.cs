using Save_Load_System.Inventory.Helpers;
using UnityEngine;

namespace Save_Load_System.Persistence
{
    public class Hero : MonoBehaviour, IBind<PlayerData>
    {
        [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
    
        [SerializeField] public PlayerData playerdata;
        
        public void Bind(PlayerData pData)
        {
            playerdata = pData;
            playerdata.Id = Id;
        }
    
        private void Update()
        {
            var heroTransform = transform;
            heroTransform.position = playerdata.position;
            heroTransform.rotation = playerdata.rotation;
        }
    }
}