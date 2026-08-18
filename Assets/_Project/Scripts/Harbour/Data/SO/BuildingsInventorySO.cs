using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Harbour.Data.SO
{
    [CreateAssetMenu(fileName = "BuildingsInventory", menuName = "Harbour/Land Buildings Inventory")]
    public class BuildingsInventorySO : ScriptableObject
    {
        [FormerlySerializedAs("tiles")] public List<BuildingsEntry> buildings = new List<BuildingsEntry>();
        
        // === NEW: Event for MVVM View updates ===
        public event Action<int> OnCountChanged;

        [Serializable]
        public class BuildingsEntry
        {
            public string buildingName;
            public GameObject prefab;
            public Sprite icon;
            public int count = 10;   
            public Vector2Int size = new (2, 2);
        }

        // Called by HarbourBuilderManager when a building is placed
        public bool TryConsumeTile(int index)
        {
            if (index < 0 || index >= buildings.Count) return false;
            if (buildings[index].count <= 0) return false;

            buildings[index].count--;
            return true;
        }

        public int GetCount(int index)
        {
            return (index >= 0 && index < buildings.Count) ? buildings[index].count : 0;
        }
        
        /// <summary>
        /// Called by HarbourBuilderManager when a tile is successfully placed.
        /// Decrements the count for the given tile index.
        /// Returns true if the tile was consumed, false if there were none left.
        /// </summary>
        public bool Consume(int index)
        {
            if (index < 0 || index >= buildings.Count) return false;
            if (buildings[index].count <= 0) return false;

            buildings[index].count--;
            
            // Fire the event so the UI knows to update
            Debug.Log($"[Buildings] ConsumeTile {index} → {buildings[index].count}");
            OnCountChanged?.Invoke(index);
            
            return true;
        }
        
        public void Restore(int index, int amount = 1)
        {
            if (index < 0 || index >= buildings.Count) return;
            buildings[index].count += amount;
            OnCountChanged?.Invoke(index);
            
        }
        
        
    }
}