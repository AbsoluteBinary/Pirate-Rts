using UnityEngine;

namespace _Project.Scripts.Harbour.Modules
{
    [CreateAssetMenu(menuName = "Harbour/Modules/Armour", fileName = "New Armour")]
    public class ArmourData : ModuleData
    {
        [Header("Armour Specific")]
        public ArmourCategory category = ArmourCategory.Light;

        public int armorValue = 120;
        public float damageReduction = 0.25f;   // percentage
        public int weightPenalty = 80;
        public float stealthPenalty = 0.1f;

        public override string GetStatsSummary()
        {
            return base.GetStatsSummary() + 
                   $"\nArmor: {armorValue} | DR: {damageReduction:P0} | Weight: +{weightPenalty}";
        }
    }
}