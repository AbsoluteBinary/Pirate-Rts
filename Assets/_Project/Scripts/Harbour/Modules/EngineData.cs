using UnityEngine;

namespace _Project.Scripts.Harbour.Modules
{
    [CreateAssetMenu(menuName = "Harbour/Modules/Engine", fileName = "New Engine")]
    public class EngineData : ModuleData
    {
        [Header("Engine Specific")]
        public EngineCategory category = EngineCategory.Standard;

        public int thrustPower = 450;
        public float fuelEfficiency = 1.8f;     // higher = better
        public int maxSpeedBonus = 35;
        public float acceleration = 2.4f;

        public override string GetStatsSummary()
        {
            return base.GetStatsSummary() + 
                   $"\nThrust: {thrustPower} | Speed: +{maxSpeedBonus} | Efficiency: {fuelEfficiency:F1}";
        }
    }
}
