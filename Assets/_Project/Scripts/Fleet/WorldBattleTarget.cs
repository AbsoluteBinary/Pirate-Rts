using _Project.Scripts.PlayerShip_Movement;
using _Project.Scripts.PlayerShip_Movement.Combat;
using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.IMGUI;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public class WorldBattleTarget : MonoBehaviour
    {
        [SerializeField] private string targetId = "Target0A";
        [SerializeField] private int combatSceneGroup = 3;

        private PlayerCombatMovementController _helm;

        public void OrderAttack()
        {
            var player = FindAnyObjectByType<PlayerShipController>();
            if (player == null)
            {
                Debug.LogWarning("[Battle] No PlayerShipController.");
                return;
            }

            if (_helm != null)
                _helm.OnArrived -= BeginBattle;

            player.OrderMove(transform.position);
            _helm = player.Helm;

            if (_helm == null)
            {
                Debug.LogWarning("[Battle] Pawn has no helm.");
                return;
            }

            _helm.OnArrived += BeginBattle;
            Debug.Log($"<color=cyan>[Battle] Sailing to {targetId}</color>");
        }

        private void BeginBattle()
        {
            if (_helm != null)
                _helm.OnArrived -= BeginBattle;

            var player = FindAnyObjectByType<PlayerShipController>();
            Vector3 here = player != null ? player.transform.position : transform.position;
            if (player != null && player.Helm != null)
                here = player.Helm.transform.position;

            PendingBattle.Engage(targetId, here);

            if (IMGUILoadingOverlay.Instance != null)
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();

            _ = SceneLoader.Instance.BeginSceneTransition(combatSceneGroup);
        }

        private void OnDestroy()
        {
            if (_helm != null)
                _helm.OnArrived -= BeginBattle;
        }
    }
}