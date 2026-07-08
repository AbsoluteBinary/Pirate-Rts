using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.LevelEditor.Data
{
    [CreateAssetMenu(fileName = "CombatLayoutDatabase", menuName = "Combat/Combat Layout Database")]
    public class CombatLayoutDatabase : ScriptableObject
    {
        public const int MaxSlots = 12;

        [SerializeField]
        private List<CombatLayout> layouts = new List<CombatLayout>();

        public List<CombatLayout> Layouts
        {
            get
            {
                // Ensure we always have 12 slots
                while (layouts.Count < MaxSlots)
                {
                    layouts.Add(new CombatLayout());
                }
                return layouts;
            }
        }

        public CombatLayout GetLayout(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots)
                return null;

            return Layouts[slotIndex];
        }
    }
}