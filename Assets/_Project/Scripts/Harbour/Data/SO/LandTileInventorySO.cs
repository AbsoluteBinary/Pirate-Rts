using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Harbour.Data.SO
{
    [CreateAssetMenu(fileName = "LandTileInventory", menuName = "Harbour/Land Tile Inventory")]
    public class LandTileInventorySO : ScriptableObject
    {
        public List<LandTileEntry> tiles = new List<LandTileEntry>();
        
        // === NEW: Event for MVVM View updates ===
        public event Action<int> OnCountChanged;

        [Serializable]
        public class LandTileEntry
        {
            public string tileName;
            public GameObject prefab;
            public Sprite icon;
            public int count = 10;           // Starting amount
        }

        // Called by HarbourBuilderManager when a tile is placed
        public bool TryConsumeTile(int index)
        {
            if (index < 0 || index >= tiles.Count) return false;
            if (tiles[index].count <= 0) return false;

            tiles[index].count--;
            return true;
        }

        public int GetCount(int index)
        {
            return (index >= 0 && index < tiles.Count) ? tiles[index].count : 0;
        }
        
        /// <summary>
        /// Called by HarbourBuilderManager when a tile is successfully placed.
        /// Decrements the count for the given tile index.
        /// Returns true if the tile was consumed, false if there were none left.
        /// </summary>
        public bool ConsumeTile(int index)
        {
            if (index < 0 || index >= tiles.Count) return false;
            if (tiles[index].count <= 0) return false;

            tiles[index].count--;
            
            // Fire the event so the UI knows to update
            OnCountChanged?.Invoke(index);
            
            return true;
        }
        
        public void Restore(int index, int amount = 1)
        {
            if (index < 0 || index >= tiles.Count) return;
            tiles[index].count += amount;
            OnCountChanged?.Invoke(index);
            
        }
        
        
    }
}
