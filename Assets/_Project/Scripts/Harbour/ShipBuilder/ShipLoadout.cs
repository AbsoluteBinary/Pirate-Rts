using System.Collections.Generic;
using _Project.Scripts.Harbour.Economy;
using UnityEngine;
using _Project.Scripts.Harbour.Modules;



namespace _Project.Scripts.Harbour.ShipBuilder
{
    
    [System.Serializable]
    public class ShipLoadout
    {
        public List<ResourceCost> totalResourceCosts = new List<ResourceCost>();
        
        
        public HullData hull;
        public List<EquippedSlot> equippedSlots = new();

        [Header("Cached Stats")]
        public float totalWeight;
        public int totalCost;
        public float totalPowerDraw;
        public float totalDamageOutput;

        public void EquipModule(ModuleSlot slot, ModuleData module)
        {
            var existing = equippedSlots.Find(e => e.slotId == slot.slotId);
            if (existing != null)
            {
                existing.module = module;
            }
            else
            {
                equippedSlots.Add(new EquippedSlot
                {
                    slotId = slot.slotId,
                    module = module
                });
            }

            slot.equippedModule = module;
            RecalculateStats();
        }
        
        public void UnequipSlot(ModuleSlot slot)
        {
            if (slot == null) return;
            equippedSlots.RemoveAll(e => e != null && e.slotId == slot.slotId);
            slot.equippedModule = null;
            RecalculateStats();
        }

        public float totalBuildTime;

        public void RecalculateStats()
        {
            totalWeight = 0;
            totalCost = 0;
            totalPowerDraw = 0;
            totalDamageOutput = 0;
            totalBuildTime = hull != null ? hull.buildTimeSeconds : 0f;

            foreach (var eq in equippedSlots)
            {
                if (eq.module == null) continue;

                totalWeight += eq.module.weight;
                totalCost += eq.module.cost;
                totalPowerDraw += eq.module.powerDraw;
                totalBuildTime += eq.module.buildTimeSeconds;

                if (eq.module is WeaponData weapon)
                    totalDamageOutput += weapon.damage * weapon.fireRate;
            }
            
            var map = new Dictionary<string, int>();
            AddCosts(map, hull != null ? hull.resourceCosts : null);
            foreach (var eq in equippedSlots)
            {
                if (eq?.module == null) continue;
                AddCosts(map, eq.module.resourceCosts);
            }

            totalResourceCosts.Clear();
            foreach (var kv in map)
                totalResourceCosts.Add(new ResourceCost { id = kv.Key, amount = kv.Value });
        }
        
        private static void AddCosts(Dictionary<string, int> map, List<ResourceCost> src)
        {
            if (src == null) return;
            foreach (var c in src)
            {
                if (c == null || string.IsNullOrEmpty(c.id) || c.amount <= 0) continue;
                map[c.id] = (map.TryGetValue(c.id, out int n) ? n : 0) + c.amount;
            }
        }
        
        public bool AllSlotsFilled()
        {
            if (hull?.moduleSlots == null || hull.moduleSlots.Length == 0)
                return true;

            foreach (var slot in hull.moduleSlots)
            {
                if (slot == null) continue;

                var eq = equippedSlots.Find(e => e != null && e.slotId == slot.slotId);
                if (eq == null || eq.module == null)
                    return false;
            }

            return true;
        }
    }

    
    
    [System.Serializable]
    public class EquippedSlot
    {
        public string slotId;
        public ModuleData module;
    }
}
