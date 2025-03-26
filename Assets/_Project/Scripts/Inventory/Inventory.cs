using System.Collections.Generic;
using _Project.Scripts.Inventory.Helpers;
using _Project.Scripts.Persistence;
using Systems.Inventory;
using UnityEngine;

namespace _Project.Scripts.Inventory {
    public class Inventory : MonoBehaviour, IBind<InventoryData> {
        [SerializeField] InventoryView view;
        [SerializeField] int capacity = 20;
        [SerializeField] List<ItemDetails> startingItems = new List<ItemDetails>();
        [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();

        InventoryController controller;

        void Awake() {
            controller = new InventoryController.Builder(view)
                .WithStartingItems(startingItems)
                .WithCapacity(capacity)
                .Build();
        }
         
        public void Bind(InventoryData data) {
            controller.Bind(data);
            data.Id = Id;
        }
    }
}