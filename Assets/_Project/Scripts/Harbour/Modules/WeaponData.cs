using UnityEngine;

namespace _Project.Scripts.Harbour.Modules
{
    [CreateAssetMenu(menuName = "Harbour/Modules/Weapon", fileName = "New Weapon")]
    public class WeaponData : ModuleData
    {
        [Header("Weapon Specific")]
        public WeaponCategory category = WeaponCategory.Cannon;
        
        public int damage = 25;
        public float fireRate = 1.5f;      // shots per second
        public float range = 120f;
        public int ammoPerShot = 1;
        public float accuracy = 0.85f;

        public override string GetStatsSummary()
        {
            return base.GetStatsSummary() + 
                   $"\nDMG: {damage} | RoF: {fireRate:F1} | Range: {range}";
        }
    }
}