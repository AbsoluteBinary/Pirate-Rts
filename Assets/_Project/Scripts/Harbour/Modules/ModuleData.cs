using _Project.Scripts.Harbour.ShipBuilder;
using UnityEngine;

namespace _Project.Scripts.Harbour.Modules
{
    public abstract class ModuleData : ScriptableObject
    {
        [Header("Core Module Info")]
        public string moduleName = "New Module";
        public Sprite icon;
        public string description;
        public ModuleSlot[] moduleSlots;
        
        public ModuleType type;
        public ModuleRarity rarity = ModuleRarity.Common;
        
        [Header("Stats")]
        public int cost = 100;
        public float weight = 10f;
        public float powerDraw = 5f;           
        
        [Header("Build Time")]
        public float buildTimeSeconds = 15f;

        [TextArea(3, 6)]
        public string tooltip;

        public virtual string GetStatsSummary()
        {
            return $"Cost: {cost} | Weight: {weight} | Power: {powerDraw}";
        }
        
    }
}
