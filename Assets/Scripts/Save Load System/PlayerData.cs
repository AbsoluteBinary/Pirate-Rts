using Save_Load_System.Inventory.Helpers;
using Save_Load_System.Persistence;
using UnityEngine;

namespace Save_Load_System
{
    public class PlayerData : ISaveable
    {
        [field: SerializeField] public SerializableGuid Id { get; set; }
        public Vector3 position;
        public Quaternion rotation;
    }
}
