using UnityEngine;
using _Project.Scripts.Harbour.Modules;
using System.Linq;                    // ← Make sure this is present

namespace _Project.Scripts.Harbour.ShipBuilder
{
    [CreateAssetMenu(menuName = "Harbour/Hull Data")]
    public class HullData : ScriptableObject
    {
        [Header("Build")]
        public float buildTimeSeconds = 60f;
        
        [Header("Mask System")]
        public HullSlotMask slotMask;
        
        [Header("Hull Info")]
        public string hullName;
        public Sprite hullImage;
        public string description;

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