using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Harbour.Data.SO
{
    [CreateAssetMenu(fileName = "LandTileInventory", menuName = "Harbour/Land Tile Inventory")]
    public class LandTileInventorySO : ScriptableObject
    {
        public List<TileEntry> tiles = new List<TileEntry>();

        [Serializable]
        public class TileEntry
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
    }
}
