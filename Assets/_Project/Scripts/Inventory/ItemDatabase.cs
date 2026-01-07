using System.Collections.Generic;
using _Project.Scripts.Inventory.Helpers;
using UnityEngine;

namespace _Project.Scripts.Inventory {
    public static class ItemDatabase {
        static Dictionary<SerializableGuid, ItemDetails> _itemDetailsDictionary;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Initialize() {
            _itemDetailsDictionary = new Dictionary<SerializableGuid, ItemDetails>();

            var itemDetails = Resources.LoadAll<ItemDetails>("");
            foreach (var item in itemDetails) {
                _itemDetailsDictionary.Add(item.Id, item);
            }
        }

        public static ItemDetails GetDetailsById(SerializableGuid id) {
            try {
                return _itemDetailsDictionary[id];
            } catch {
                Debug.LogError($"Cannot find item details with id {id}");
                return null;
            }
        }
    }
}