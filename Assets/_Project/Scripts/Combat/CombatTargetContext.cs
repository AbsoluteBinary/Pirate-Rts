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
    }
}