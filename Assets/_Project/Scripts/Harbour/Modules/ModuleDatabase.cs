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
        public List<ArmourData> armours = new();
        public List<EngineData> engines = new();
        public List<ComponentData> components = new();

        // Runtime caches
        private Dictionary<string, ModuleData> _idToModule = new();
        private Dictionary<ModuleType, List<ModuleData>> _modulesByType = new();

        // Category-specific caches
        private Dictionary<WeaponCategory, List<WeaponData>> _weaponsByCategory = new();
        private Dictionary<ArmourCategory, List<ArmourData>> _armoursByCategory = new();
        private Dictionary<EngineCategory, List<EngineData>> _enginesByCategory = new();
        private Dictionary<ComponentCategory, List<ComponentData>> _componentsByCategory = new();

        public void Initialize()
        {
            BuildCaches();
        }

        private void BuildCaches()
        {
            _idToModule.Clear();
            _modulesByType.Clear();
            _weaponsByCategory.Clear();
            _armoursByCategory.Clear();
            _enginesByCategory.Clear();
            _componentsByCategory.Clear();

            // === WEAPONS ===
            foreach (var w in weapons)
            {
                if (w == null) continue;
                _idToModule[w.name] = w;

                if (!_modulesByType.ContainsKey(ModuleType.Weapon))
                    _modulesByType[ModuleType.Weapon] = new List<ModuleData>();
                _modulesByType[ModuleType.Weapon].Add(w);

                if (!_weaponsByCategory.ContainsKey(w.category))
                    _weaponsByCategory[w.category] = new List<WeaponData>();
                _weaponsByCategory[w.category].Add(w);
            }

            // === ARMOUR ===
            foreach (var a in armours)
            {
                if (a == null) continue;
                _idToModule[a.name] = a;

                if (!_modulesByType.ContainsKey(ModuleType.Armour))
                    _modulesByType[ModuleType.Armour] = new List<ModuleData>();
                _modulesByType[ModuleType.Armour].Add(a);

                if (!_armoursByCategory.ContainsKey(a.category))
                    _armoursByCategory[a.category] = new List<ArmourData>();
                _armoursByCategory[a.category].Add(a);
            }

            // === ENGINES ===
            foreach (var e in engines)
            {
                if (e == null) continue;
                _idToModule[e.name] = e;

                if (!_modulesByType.ContainsKey(ModuleType.Engine))
                    _modulesByType[ModuleType.Engine] = new List<ModuleData>();
                _modulesByType[ModuleType.Engine].Add(e);

                if (!_enginesByCategory.ContainsKey(e.category))
                    _enginesByCategory[e.category] = new List<EngineData>();
                _enginesByCategory[e.category].Add(e);
            }

            // === COMPONENTS ===
            foreach (var c in components)
            {
                if (c == null) continue;
                _idToModule[c.name] = c;

                if (!_modulesByType.ContainsKey(ModuleType.Component))
                    _modulesByType[ModuleType.Component] = new List<ModuleData>();
                _modulesByType[ModuleType.Component].Add(c);

                if (!_componentsByCategory.ContainsKey(c.category))
                    _componentsByCategory[c.category] = new List<ComponentData>();
                _componentsByCategory[c.category].Add(c);
            }
        }

        // ==================== Public API ====================

        public ModuleData GetModuleById(string id) => _idToModule.GetValueOrDefault(id);

        public List<ModuleData> GetModulesByType(ModuleType type)
            => _modulesByType.GetValueOrDefault(type) ?? new List<ModuleData>();

        // ====================== WEAPONS ======================
        public List<WeaponData> GetFilteredWeapons(WeaponCategory filter = WeaponCategory.None)
        {
            if (filter == WeaponCategory.None)
                return new List<WeaponData>(weapons);

            return _weaponsByCategory.GetValueOrDefault(filter) ?? new List<WeaponData>();
        }

        // ====================== ARMOUR ======================
        public List<ArmourData> GetFilteredArmour(ArmourCategory filter = ArmourCategory.None)
        {
            if (filter == ArmourCategory.None)
                return new List<ArmourData>(armours);

            return _armoursByCategory.GetValueOrDefault(filter) ?? new List<ArmourData>();
        }

        // ====================== ENGINES ======================
        public List<EngineData> GetFilteredEngines(EngineCategory filter = EngineCategory.None)
        {
            if (filter == EngineCategory.None)
                return new List<EngineData>(engines);

            return _enginesByCategory.GetValueOrDefault(filter) ?? new List<EngineData>();
        }

        // ====================== COMPONENTS ======================
        public List<ComponentData> GetFilteredComponents(ComponentCategory filter = ComponentCategory.None)
        {
            if (filter == ComponentCategory.None)
                return new List<ComponentData>(components);

            return _componentsByCategory.GetValueOrDefault(filter) ?? new List<ComponentData>();
        }

        // ====================== UTILITY ======================
        public List<WeaponData> GetAllWeapons() => new List<WeaponData>(weapons);
        public List<ArmourData> GetAllArmour() => new List<ArmourData>(armours);
        public List<EngineData> GetAllEngines() => new List<EngineData>(engines);
        public List<ComponentData> GetAllComponents() => new List<ComponentData>(components);
    }
}
