using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    [System.Serializable]
    public class ShipLoadout
    {
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
        }
    }

    [System.Serializable]
    public class EquippedSlot
    {
        public string slotId;
        public ModuleData module;
    }
}
