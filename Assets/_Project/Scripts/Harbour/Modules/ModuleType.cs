using UnityEngine;

namespace _Project.Scripts.Harbour.Modules
{
    public enum ModuleType
    {
        Weapon,
        Armour,
        Engine,
        Special,
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

    public enum ModuleRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}