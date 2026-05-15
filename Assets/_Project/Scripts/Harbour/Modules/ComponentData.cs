using UnityEngine;

namespace _Project.Scripts.Harbour.Modules
{
    [CreateAssetMenu(menuName = "Harbour/Modules/Component", fileName = "New Component")]
    public class ComponentData : ModuleData
    {
        [Header("Component Specific")]
        public ComponentCategory category = ComponentCategory.Utility;

        public int powerDraw = 25;
        public int weight = 40;
        public float specialBonus = 1.0f;       // e.g. radar range, repair rate, etc.

        [TextArea(3, 6)]
        public string specialEffectDescription = "Increases scan range by 30%";

        public override string GetStatsSummary()
        {
            return base.GetStatsSummary() + 
                   $"\nPower: -{powerDraw} | Weight: {weight} | Bonus: {specialBonus:F1}x";
        }
    }
}