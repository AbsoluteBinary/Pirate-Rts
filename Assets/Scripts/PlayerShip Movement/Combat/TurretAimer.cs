using UnityEngine;

namespace PlayerShip_Movement.Combat
{
    public class TurretAimer : MonoBehaviour
    {
        [Header("Turret Parts (Drag from Hierarchy)")]
        [SerializeField] private Transform barrel;      // Barrel Transform
        [SerializeField] private float rotationSpeed = 2f;  // Smoothness (higher = faster)

        [Header("Detection")]
        [SerializeField] private float playerRange = 100f;  // Only aim if close
        [SerializeField] private LayerMask playerLayer = 1 << 9;  // "Player" layer

        private Transform playerShip;
        private Collider[] _scanBuffer = new Collider[1];  // Fast player check

        private void Start()
        {
            // Auto-find Barrel if not assigned (teaching: flexible!)
            if (barrel == null)
            {
                barrel = transform.Find("TurretMount/Barrel");  // Matches your hierarchy
            }

            // Tag player ship "PlayerShip" for easy find
            playerShip = GameObject.FindWithTag("PlayerShip")?.transform;
        }

        private void LateUpdate()  // After player/camera moves
        {
            if (playerShip == null || barrel == null) return;

            // Quick range check (perf!)
            int hits = Physics.OverlapSphereNonAlloc(transform.position, playerRange, _scanBuffer, playerLayer);
            if (hits == 0) return;

            // Get direction to player (ignore Y for flat aiming)
            Vector3 direction = playerShip.position - barrel.position;
            direction.y = 0;  // Horizontal only
            if (direction == Vector3.zero) return;

            // Smooth rotate Barrel to face player
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            barrel.rotation = Quaternion.Slerp(barrel.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            Debug.Log("Turret locked on player!");  // Remove later
        }

        // Gizmo for editor viz
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, playerRange);
            if (barrel != null && playerShip != null)
            {
                Gizmos.DrawLine(barrel.position, playerShip.position);
            }
        }
    }
}
