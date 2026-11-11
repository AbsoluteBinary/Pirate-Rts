using _Project.Scripts.Fleet.Targets;
using _Project.Scripts.PlayerShip_Movement;
using _Project.Scripts.PlayerShip_Movement.Combat;
using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.IMGUI;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public class WorldBattleTarget : MonoBehaviour
    {
        [SerializeField] private string targetId = "TargetIron";
        [SerializeField] private TargetCatalog catalog;
        [SerializeField] private int combatSceneGroup = 3;

        private PlayerCombatMovementController _helm;

        public void OrderAttack()
        {
            if (TargetsFile.TryGet(targetId, out var record))
            {
                if (record.condition == TargetCondition.Depleted || record.condition == TargetCondition.Destroyed)
                {
                    Debug.LogWarning($"[Battle] {targetId} is {record.condition}. Sail refused.");
                    return;
                }
            }

            var player = FindAnyObjectByType<MapSailController>();
            if (player == null)
            {
                Debug.LogWarning("[Battle] No MapSailController in this scene.");
                return;
            }

            if (_helm != null)
                _helm.OnArrived -= BeginBattle;

            player.OrderMove(transform.position);
            _helm = player.Helm;

            if (_helm == null)
            {
                Debug.LogWarning("[Battle] MapSailController has no helm.");
                return;
            }

            _helm.OnArrived += BeginBattle;
            Debug.Log($"<color=cyan>[Battle] Sailing to {targetId}</color>");
        }

        private void BeginBattle()
        {
            if (_helm != null)
                _helm.OnArrived -= BeginBattle;

            var player = FindAnyObjectByType<MapSailController>();
            Vector3 here = player != null ? player.transform.position : transform.position;
            if (player != null && player.Helm != null)
                here = player.Helm.transform.position;

            // Catalogue + condition lookup
            TargetDefinition definition = null;
            if (catalog != null)
                definition = catalog.Get(targetId);
            else
                Debug.LogWarning("[Battle] No TargetCatalog assigned.");

            if (definition != null)
            {
                Debug.Log($"<color=cyan>[Battle] Definition: {definition.displayName} | category={definition.category} | max={definition.maxCondition} | group={definition.combatSceneGroup}</color>");
                combatSceneGroup = definition.combatSceneGroup;
            }
            else
            {
                Debug.LogWarning($"[Battle] No definition for {targetId}. Using serialized combatSceneGroup={combatSceneGroup}");
            }

            if (TargetsFile.TryGet(targetId, out var record))
            {
                Debug.Log($"<color=cyan>[Battle] Condition: current={record.current} | condition={record.condition}</color>");
            }
            else
            {
                Debug.LogWarning($"[Battle] No condition record for {targetId}");
            }

            PendingBattle.Engage(targetId, here);

            ActiveFleets.SetScene(PendingLaunch.FleetName, ActiveFleets.CombatScene);

            if (IMGUILoadingOverlay.Instance != null)
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();

            FleetGovernor.Instance?.Commit();
            _ = SceneLoader.Instance.BeginSceneTransition(combatSceneGroup);
        }

        private void OnDestroy()
        {
            if (_helm != null)
                _helm.OnArrived -= BeginBattle;
        }
    }
}