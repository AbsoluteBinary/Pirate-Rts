using System;
using System.Collections.Generic;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Persistence.TempSave
{
    [Serializable]
    public class ShipInventorySaveData
    {
        public int version = 1;
        public List<ShipBuildRecord> ships = new List<ShipBuildRecord>();
    }

    [Serializable]
    public class ShipBuildRecord
    {
        public string id;
        public string shipName;
        public string hullId;
        public List<EquippedSlotRecord> slots = new List<EquippedSlotRecord>();
        public float totalWeight;
        public int totalCost;
        public float totalPowerDraw;
        public float totalDamageOutput;
    }

    [Serializable]
    public class EquippedSlotRecord
    {
        public string slotId;
        public string moduleId;
        public ModuleType moduleType;
    }
}