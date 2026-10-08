using _Project.Scripts.PlayerShip_Movement;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public class WorldFleetArrival : MonoBehaviour
    {
        private void Start()
        {
        }
        // private void Start()
        // {
        //     Debug.Log($"<color=cyan>[Fleet] World arrived. Flagship slot {PendingLaunch.FlagShipIndex}</color>");
        //
        //     for (int i = 0; i < PendingLaunch.Size; i++)
        //     {
        //         var ship = PendingLaunch.Ships[i];
        //         string name = ship != null ? ship.shipName : "(empty)";
        //         string mark = i == PendingLaunch.FlagShipIndex ? "  FLAG" : "";
        //         Debug.Log($"[Fleet] Slot {i}: {name}{mark}");
        //     }
        //
        //     var flag = PendingLaunch.Ships[PendingLaunch.FlagShipIndex];
        //     if (flag == null)
        //     {
        //         Debug.LogWarning("[Fleet] No flagship on arrival.");
        //         return;
        //     }
        //
        //     if (flag.shipPrefab == null)
        //     {
        //         Debug.LogWarning($"[Fleet] '{flag.shipName}' has no combat prefab. Assign it on the hull.");
        //         return;
        //     }
        //
        //     var player = FindAnyObjectByType<MapSailController>();
        //     if (player == null)
        //     {
        //         Debug.LogWarning("[Fleet] No MapSailController in this scene.");
        //         return;
        //     }
        //
        //     player.AttachFlagship(flag.shipPrefab, flag.shipName, transform);
        //     Debug.Log($"<color=lime>[Fleet] World ship is now '{flag.shipName}'</color>");
        //
        //     var governor = FleetGovernor.Instance;
        //     if (governor != null)
        //     {
        //         governor.TakeLaunch(PendingLaunch.FleetName, PendingLaunch.Ships, PendingLaunch.FlagShipIndex);
        //         governor.AttachMap(player);
        //     }
        //     
        //     ActiveFleets.SetPosition(PendingLaunch.FleetName, transform.position);
        //
        //     float health01 = 1f;
        //     var health = player.GetComponentInChildren<_Project.Scripts.Combat.Health>(true);
        //     if (health != null && health.MaxHealth > 0f)
        //         health01 = health.CurrentHealth / health.MaxHealth;
        //
        //     string fleetName = string.IsNullOrWhiteSpace(PendingLaunch.FleetName)
        //         ? flag.shipName
        //         : PendingLaunch.FleetName;
        //
        //     MapFleets.Register(fleetName, health01, transform);
        // }
    }
}