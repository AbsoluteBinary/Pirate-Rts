using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Combat.Data;           // For TurretWeaponData etc.
using _Project.Scripts.Fleet.Data;
using _Project.Scripts.Harbour.Modules; // New namespace

namespace _Project.Scripts.Harbour.ShipBuilder.Data
{
    [CreateAssetMenu(menuName = "ShipBuilder/Ship Blueprint", fileName = "New ShipBlueprint")]
    public class ShipBlueprint : ScriptableObject
    {
        [Header("Identity")]
        public string shipName = "Unnamed Ship";
        public HullData hull;

        [Header("Equipped Modules")]
        public List<WeaponData> equippedWeapons = new List<WeaponData>();
        public List<ArmourData> equippedArmour = new List<ArmourData>();
        public List<EngineData> equippedEngines = new List<EngineData>();
        public List<ComponentData> equippedComponents = new List<ComponentData>();

        [Header("Combat Settings")]
        public VFXProfile vfxProfile;
        public GameObject shipPrefab;               // The actual 3D model used in combat

        [Header("Calculated Stats")]
        public float totalHealth = 100f;
        public float totalSpeed = 15f;
        public float totalWeight = 0f;

        public void CalculateStats()
        {
            // TODO: Aggregate stats from hull + all modules
            // This can be expanded later
        }
    }
}