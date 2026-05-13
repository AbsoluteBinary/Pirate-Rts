using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Harbour.Modules
{
    [CreateAssetMenu(menuName = "Harbour/Module Database", fileName = "ModuleDatabase")]
    public class ModuleDatabase : ScriptableObject
    {
        [Header("All Modules")]
        public List<WeaponData> weapons = new();
        public List<ModuleData> armours = new();
        public List<ModuleData> engines = new();
        public List<ModuleData> specials = new();

        // Runtime caches (built once)
        private Dictionary<string, ModuleData> _idToModule = new();
        private Dictionary<ModuleType, List<ModuleData>> _modulesByType = new();
        private Dictionary<WeaponCategory, List<WeaponData>> _weaponsByCategory = new();

        public void Initialize()
        {
            BuildCaches();
        }

        private void BuildCaches()
        {
            _idToModule.Clear();
            _modulesByType.Clear();
            _weaponsByCategory.Clear();

            // Weapons
            foreach (var w in weapons)
            {
                if (w != null)
                {
                    _idToModule[w.name] = w;
                    if (!_modulesByType.ContainsKey(ModuleType.Weapon))
                        _modulesByType[ModuleType.Weapon] = new List<ModuleData>();
                    _modulesByType[ModuleType.Weapon].Add(w);

                    if (!_weaponsByCategory.ContainsKey(w.category))
                        _weaponsByCategory[w.category] = new List<WeaponData>();
                    _weaponsByCategory[w.category].Add(w);
                }
            }

            // TODO: Add similar loops for armours, engines, specials later
        }

        // ==================== Public API ====================

        public ModuleData GetModuleById(string id)
        {
            return _idToModule.GetValueOrDefault(id);
        }

        public List<ModuleData> GetModulesByType(ModuleType type)
        {
            return _modulesByType.GetValueOrDefault(type) ?? new List<ModuleData>();
        }

        public List<WeaponData> GetWeaponsByCategory(WeaponCategory category)
        {
            return _weaponsByCategory.GetValueOrDefault(category) ?? new List<WeaponData>();
        }

        public List<WeaponData> GetAllWeapons() => weapons;

        // Helper for Weapon Selection HUD (All / Cannons / Missiles etc.)
        public List<WeaponData> GetFilteredWeapons(WeaponCategory filter = WeaponCategory.None)
        {
            if (filter == WeaponCategory.None)
                return new List<WeaponData>(weapons);
            
            return GetWeaponsByCategory(filter);
        }
    }
}
