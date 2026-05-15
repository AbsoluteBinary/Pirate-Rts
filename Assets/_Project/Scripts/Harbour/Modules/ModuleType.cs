using UnityEngine;

namespace _Project.Scripts.Harbour.Modules
{
    public enum ModuleType
    {
        Weapon,
        Armour,
        Engine,
        Component,
        Utility
    }

    public enum WeaponCategory
    {
        Cannon,
        Missile,
        Laser,
        Torpedo,
        Railgun,
        None
    }
    
    public enum ArmourCategory
    {
        Light,
        Medium,
        Heavy,
        None
    }
    
    public enum EngineCategory
    {
        Standard,
        Advanced,
        None
    }
    
    public enum ComponentCategory
    {
        Utility,
        None
    }

    public enum ModuleRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}