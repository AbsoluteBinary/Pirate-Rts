using UnityEngine;

namespace PlayerShip_Movement.Combat
{
    [RequireComponent(typeof(Transform))]
    public class BarrelAimer : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float fireRange = 20f;  // ← Match CannonFire range

        private void Awake()
        {
            if (player == null)
                player = GameObject.FindWithTag("Player")?.transform ?? Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (player == null) return;

            // ← NEW: Only aim when in fire range
            float sqrDist = (player.position - transform.position).sqrMagnitude;
            if (sqrDist > fireRange * fireRange) return;

            // Direction to player on horizontal plane only
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0;

            if (toPlayer.sqrMagnitude < 0.001f) return;

            // Desired rotation (Y-axis only)
            Quaternion targetRot = Quaternion.LookRotation(toPlayer, Vector3.up);

            // Apply while preserving the 90° X pitch
            transform.rotation = Quaternion.Euler(90f, targetRot.eulerAngles.y, 0f);
        }
    }
}
