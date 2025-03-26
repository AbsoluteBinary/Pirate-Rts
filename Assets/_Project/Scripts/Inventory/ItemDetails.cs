using System;
using _Project.Scripts.Inventory.Helpers;
using Sirenix.OdinInspector;
using Systems.Inventory;
using UnityEngine;

namespace _Project.Scripts.Inventory {
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    [Serializable]
    public class ItemDetails : ScriptableObject {

        [HorizontalGroup("ItemSplit", 0.75f), VerticalGroup("ItemSplit/Left")]
        public string Name;

        [HorizontalGroup("ItemSplit", 0.75f), VerticalGroup("ItemSplit/Left")]
        public int maxStack = 1;
            
        [HorizontalGroup("ItemSplit", 0.75f), VerticalGroup("ItemSplit/Left")]
        public SerializableGuid Id = SerializableGuid.NewGuid();

        [HorizontalGroup("ItemSplit", 0.75f), VerticalGroup("ItemSplit/Left"), Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
        private void AssignNewGuid() {
            Id = SerializableGuid.NewGuid();
        }
        
        [HorizontalGroup("ItemSplit", 0.25f), VerticalGroup("ItemSplit/Right"), HideLabel, PreviewField(100)]
        public Sprite Icon;
        
        [TextArea, HideLabel]
        public string Description;
        
        public Item Create(int quantity) {
            return new Item(this, quantity);
        }
    }
}