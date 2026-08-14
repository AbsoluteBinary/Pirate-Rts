using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Harbour.Data.SO
{
    [CreateAssetMenu(fileName = "WallInventory", menuName = "Harbour/Wall Inventory")]
    public class WallInventorySO : ScriptableObject
    {
        public List<WallEntry> walls = new List<WallEntry>();

        public event Action<int> OnCountChanged;

        [Serializable]
        public class WallEntry
        {
            public string wallName;
            public GameObject prefab;
            public Sprite icon;
            public int count = 100;          // Starting amount
        }

        public bool TryConsume(int index)
        {
            if (index < 0 || index >= walls.Count) return false;
            if (walls[index].count <= 0) return false;

            walls[index].count--;
            return true;
        }

        public int GetCount(int index)
        {
            return (index >= 0 && index < walls.Count) ? walls[index].count : 0;
        }

        public bool Consume(int index)
        {
            if (index < 0 || index >= walls.Count) return false;
            if (walls[index].count <= 0) return false;

            walls[index].count--;
            
            // Fire the event so the UI knows to update
            OnCountChanged?.Invoke(index);
            return true;
        }
        
        public void Restore(int index, int amount = 1)
        {
            if (index < 0 || index >= walls.Count) return;
            walls[index].count += amount;
            OnCountChanged?.Invoke(index);
        }
    }
}