using System;
using _Project.Scripts.Inventory.Helpers;
using _Project.Scripts.Persistence;
using UnityEngine;

namespace _Project.Scripts.Player
{
    namespace _Project.Scripts.Persistence
    {
        [Serializable]
        public class PlayerData : ISaveable 
        {
            [field: SerializeField] public SerializableGuid Id { get; set; }
            public Vector3 position;
            public Quaternion rotation;
            public String playerUserName;
            [SerializeField] private int health = 100; // Serialized for Inspector
            [SerializeField] private int mana = 50; 
            [SerializeField] private int maxHealth = 100;
            [SerializeField] private int maxMana = 50;
            
            
            public int MaxHealth
            {
                get => maxHealth;
                set => maxHealth = Mathf.Max(value, 1);
            }
            public int MaxMana
            {
                get => maxMana;
                set => maxMana = Mathf.Max(value, 1);
            }// Serialized for Inspector

            // Properties with clamping
            public int Health
            {
                get => health;
                set => health = Mathf.Clamp(value, 0, 100);
            }
            public int Mana
            {
                get => mana;
                set => mana = Mathf.Clamp(value, 0, 50);
            }
        }
    }
    
    
}
