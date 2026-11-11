using _Project.Scripts.Fleet;
using _Project.Scripts.Fleet.Targets;
using UnityEngine;

namespace _Project.Scripts.Combat
{
    public class CombatTargetContext : MonoBehaviour
    {
        [SerializeField] private TargetCatalog catalog;

        public TargetDefinition Definition { get; private set; }
        public TargetConditionRecord Condition { get; private set; }
        public bool HasTarget => Definition != null;

        private void Start()
        {
            if (!PendingBattle.Active || string.IsNullOrEmpty(PendingBattle.TargetId))
            {
                Debug.LogWarning("[CombatTarget] No PendingBattle.TargetId");
                return;
            }

            string id = PendingBattle.TargetId;

            if (catalog == null)
            {
                Debug.LogWarning("[CombatTarget] No TargetCatalog assigned.");
                return;
            }

            Definition = catalog.Get(id);
            if (Definition == null)
            {
                Debug.LogWarning($"[CombatTarget] No definition for {id}");
                return;
            }

            Debug.Log($"<color=cyan>[CombatTarget] {Definition.displayName} | category={Definition.category} | max={Definition.maxCondition} | group={Definition.combatSceneGroup}</color>");

            if (TargetsFile.TryGet(id, out var record))
            {
                Condition = record;
                Debug.Log($"<color=cyan>[CombatTarget] Condition: current={record.current} | condition={record.condition}</color>");
            }
            else
            {
                Debug.LogWarning($"[CombatTarget] No condition record for {id}");
            }
        }
        
        public void Harvest(float amount)
        {
            if (Condition == null || Definition == null || amount <= 0f)
                return;

            Condition.current = Mathf.Max(0f, Condition.current - amount);

            if (Definition.category == TargetCategory.ResourceNode)
                Condition.condition = Condition.current <= 0f ? TargetCondition.Depleted : TargetCondition.Intact;
            else
                Condition.condition = Condition.current <= 0f ? TargetCondition.Destroyed : TargetCondition.Intact;

            Debug.Log($"<color=cyan>[CombatTarget] Harvested {amount} | current={Condition.current} | condition={Condition.condition}</color>");
        }
        // TEMP: right-click the component in Play Mode to drain 100. Remove when a real harvest system drives Harvest().
        [ContextMenu("Harvest 100")]
        private void HarvestTest() => Harvest(100f);
        
        public void Write()
        {
            
            if (Condition == null || Definition == null) return;

            Condition.current = Mathf.Clamp(Condition.current, 0f, Definition.maxCondition);

            if (Definition.category == TargetCategory.ResourceNode)
                Condition.condition = Condition.current <= 0f ? TargetCondition.Depleted : TargetCondition.Intact;
            else
                Condition.condition = Condition.current <= 0f ? TargetCondition.Destroyed : TargetCondition.Intact;

            TargetsFile.WriteRecord(Condition);
            Debug.Log($"<color=cyan>[CombatTarget] Wrote {Condition.targetId} | current={Condition.current} | condition={Condition.condition}</color>");
        }
    }
}