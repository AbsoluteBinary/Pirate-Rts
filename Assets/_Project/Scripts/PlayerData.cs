using System;
using _Project.Scripts.Inventory.Helpers;
using _Project.Scripts.Persistence;
using UnityEngine;

namespace _Project.Scripts
{
    [Serializable]
    public class PlayerData : ISaveable {
        [field: SerializeField] public SerializableGuid Id { get; set; }
        public Vector3 position;
        public Quaternion rotation;
    }
}
