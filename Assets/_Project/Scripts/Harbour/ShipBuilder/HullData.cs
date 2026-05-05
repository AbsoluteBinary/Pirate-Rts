using UnityEngine;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    [CreateAssetMenu(menuName = "Harbour/Hull Data")]
    public class HullData : ScriptableObject
    {
        public string hullName;
        public Sprite hullImage;           // ← Your baked sprite (with slot visuals drawn on it)
        public string slotInfo;
        public string description;

        [Header("Interactive Slot Positions")]
        public ModuleSlot[] slots;

        [System.Serializable]
        public class ModuleSlot
        {
            public string type = "Weapon";        // Weapon, Armour, Engine, Special
            [Range(0f, 1f)] public float xPercent = 0.5f;
            [Range(0f, 1f)] public float yPercent = 0.5f;
            public int capacity = 1;

            public string GetShortLabel() => type.Length > 0 ? type.Substring(0, 1) : "?";
        }
    }
}