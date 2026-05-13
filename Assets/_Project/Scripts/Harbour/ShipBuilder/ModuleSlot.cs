using UnityEngine;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    [System.Serializable]
    public class ModuleSlot
    {
        public string slotId = "Slot_1";
        public ModuleType acceptedType = ModuleType.Weapon;
        
        [Header("Pixel Position (from top-left of sprite)")]
        public Vector2Int pixelPosition;     // ← This is what we'll fill from GIMP
        
        public int maxCapacity = 1;

        [HideInInspector] public ModuleData equippedModule;
    }
}
