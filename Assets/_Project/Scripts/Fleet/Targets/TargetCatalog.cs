using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Fleet.Targets
{
    [CreateAssetMenu(menuName = "Targets/Target Catalog", fileName = "TargetCatalog")]
    public class TargetCatalog : ScriptableObject
    {
        public List<TargetDefinition> targets = new List<TargetDefinition>();

        public TargetDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id) || targets == null) return null;
            foreach (var t in targets)
                if (t != null && t.id == id) return t;
            return null;
        }

        public IEnumerable<TargetDefinition> ByCategory(TargetCategory category)
        {
            if (targets == null) yield break;
            foreach (var t in targets)
                if (t != null && t.category == category) yield return t;
        }
    }
}