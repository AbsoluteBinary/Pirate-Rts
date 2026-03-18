using System;
using _Project.Scripts.Inventory.Helpers;
using _Project.Scripts.Persistence;
using UnityEngine;

namespace _Project.Scripts.Inventory {
    [Serializable]
    public class InventoryData : ISaveable {
        [field: SerializeField] public SerializableGuid Id { get; set; }
        public Item[] Items;
        public int Capacity;
        public int Coins;
    }
}