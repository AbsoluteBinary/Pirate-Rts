using System;
using System.Collections.Generic;

namespace _Project.Scripts.LevelEditor.Data
{
    [Serializable]
    public class CombatLayout
    {
        public string LayoutName = "New Layout";
        public List<PlacedObjectData> PlacedObjects = new List<PlacedObjectData>();
    }
}