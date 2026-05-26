using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class NavalCombatManager : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private TurretController[] turrets;

        private void Awake()
        {
            if (playerTransform == null)
                playerTransform = GameObject.FindWithTag("Player")?.transform;
        }

        private void Update()
        {
            if (playerTransform == null) return;

            foreach (var turret in turrets)
            {
                if (turret != null)
                    turret.UpdateTurret(playerTransform);
            }
        }
    }
}