using UnityEngine;

namespace _Project.Scripts.Fleet.Targets
{
    [CreateAssetMenu(menuName = "Targets/Resource Node Definition", fileName = "New ResourceNode")]
    public class ResourceNodeDefinition : TargetDefinition
    {
        [Header("Resource Node")]
        [Tooltip("Id matching a ResourceDef in ResourceCatalog (e.g. Iron, Oil)")]
        public string resourceId;

        [Tooltip("How much a full node contains")]
        public float fullAmount = 100f;

        private void OnValidate()
        {
            category = TargetCategory.ResourceNode;
            maxCondition = fullAmount;
        }
    }
}