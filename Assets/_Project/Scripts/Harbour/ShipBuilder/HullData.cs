using UnityEngine;
using _Project.Scripts.Harbour.Modules;
using System.Linq;
using System.Collections.Generic;
using _Project.Scripts.Harbour.Economy;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    [CreateAssetMenu(menuName = "Harbour/Hull Data")]
    public class HullData : ScriptableObject
    {
        [Header("Build")]
        public float buildTimeSeconds = 60f;
        
        [Header("Build Cost")]
        public List<ResourceCost> resourceCosts = new List<ResourceCost>();
        
        [Header("Mask System")]
        public HullSlotMask slotMask;
        
        [Header("Hull Info")]
        public string hullName;
        public Sprite hullImage;
        public string description;
        
        [Header("Combat")]
        public GameObject combatPrefab;
        
        public enum HullClass
        {
            Generic,
            Advanced,
            Legendary,
            Garrison
        }

        public HullClass hullClass = HullClass.Generic;

        [Header("Module Slots")]
        public ModuleSlot[] moduleSlots;

        public string slotInfo => GetSlotSummary();

        private string GetSlotSummary()
        {
            if (moduleSlots == null || moduleSlots.Length == 0)
                return "No slots";

            // Fixed version - cast to IEnumerable to avoid explicit ICollection.Count conflict
            int weaponCount = moduleSlots.Count(s => s.acceptedType == ModuleType.Weapon);
            int armourCount = moduleSlots.Count(s => s.acceptedType == ModuleType.Armour);
            int engineCount = moduleSlots.Count(s => s.acceptedType == ModuleType.Engine);

            return $"{weaponCount} Weapon • {armourCount} Armour • {engineCount} Engine";
        }
    }
}