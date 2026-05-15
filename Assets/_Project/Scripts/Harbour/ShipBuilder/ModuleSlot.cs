using UnityEngine;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    [System.Serializable]
    public class ModuleSlot
    {
        public string slotId = "Slot_1";
        public ModuleType acceptedType = ModuleType.Weapon;
        [Header("Visual")]
        public Sprite slotIcon;                    // ← Assign per-slot icon in Inspector
        public Color iconTint = Color.white;       // Optional tint
        
        [Header("Pixel Position (from top-left of sprite)")]
        public Vector2Int pixelPosition;     // ← This is what we'll fill from GIMP
        
        public int maxCapacity = 1;

        [HideInInspector] public ModuleData equippedModule;
    }
}
