using System;
using System.Collections.Generic;
using _Project.Scripts.BaseBuilder.Runtime.Inventory;

namespace _Project.Scripts.Persistence.TempSave
{
    [Serializable]
    public class BuilderSaveData
    {
        public int version = 1;
        public List<SlotCountRecord> counts = new List<SlotCountRecord>();
        public List<PlacedRecord> placed = new List<PlacedRecord>();
        
        public string note;
        public int landPlaced;
        public int wallPlaced;
        public int buildingPlaced;
        public int turretPlaced;
    }

    [Serializable]
    public class SlotCountRecord
    {
        public PlaceableKind kind;
        public int index;
        public int count;
    }

    [Serializable]
    public class PlacedRecord
    {
        public PlaceableKind kind;
        public int inventoryIndex;
        public int originCellIndex;
        public int sizeX;
        public int sizeY;
        public bool isLand;
        public bool isLocked;
        public float yaw;
        
        
    }
}