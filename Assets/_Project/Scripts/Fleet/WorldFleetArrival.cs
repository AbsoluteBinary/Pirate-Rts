using _Project.Scripts.PlayerShip_Movement;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public class WorldFleetArrival : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log($"<color=cyan>[Fleet] World arrived. Flagship slot {PendingLaunch.FlagShipIndex}</color>");


            for (int i = 0; i < PendingLaunch.Size; i++)
            {
                var ship = PendingLaunch.Ships[i];
                string name = ship != null ? ship.shipName : "(empty)";
                string mark = i == PendingLaunch.FlagShipIndex ? "  FLAG" : "";
                Debug.Log($"[Fleet] Slot {i}: {name}{mark}");
            }

            var flag = PendingLaunch.Ships[PendingLaunch.FlagShipIndex];
            if (flag == null)
            {
                Debug.LogWarning("[Fleet] No flagship on arrival.");
                return;
            }

            if (flag.shipPrefab == null)
            {
                Debug.LogWarning($"[Fleet] '{flag.shipName}' has no combat prefab. Assign it on the hull.");
                return;
            }

            var player = FindAnyObjectByType<PlayerShipController>();
            if (player == null)
            {
                Debug.LogWarning("[Fleet] No PlayerShipController in this scene.");
                return;
            }

            player.ReplaceVisual(flag.shipPrefab, flag.shipName);
            Debug.Log($"<color=lime>[Fleet] World ship is now '{flag.shipName}'</color>");
        }
    }
}