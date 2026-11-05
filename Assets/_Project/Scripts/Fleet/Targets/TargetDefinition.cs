using UnityEngine;

namespace _Project.Scripts.Fleet.Targets
{
    [CreateAssetMenu(menuName = "Targets/Target Definition", fileName = "New TargetDefinition")]
    public class TargetDefinition : ScriptableObject
    {
        [Tooltip("Matches the targetId on the map object, e.g. Target0A")]
        public string id;

        public string displayName;

        public TargetCategory category = TargetCategory.ResourceNode;

        [Tooltip("Full health, or full hold for a resource node")]
        public float maxCondition = 100f;

        [Tooltip("Which battle scene group this target opens")]
        public int combatSceneGroup = 3;
    }
}