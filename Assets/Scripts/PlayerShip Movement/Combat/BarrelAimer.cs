using UnityEngine;

namespace PlayerShip_Movement.Combat
{
    [RequireComponent(typeof(Transform))]
    public class BarrelAimer : MonoBehaviour
    {
        [SerializeField] private Transform player;

        private void Awake()
        {
            if (player == null)
                player = GameObject.FindWithTag("Player")?.transform ?? Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (player == null) return;

            // Direction to player on horizontal plane only
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0;

            if (toPlayer.sqrMagnitude < 0.001f) return;

            // Desired rotation (Y-axis only)
            Quaternion targetRot = Quaternion.LookRotation(toPlayer, Vector3.up);

            // Apply while preserving the 90° X pitch you set in the prefab
            transform.rotation = Quaternion.Euler(90f, targetRot.eulerAngles.y, 0f);
        }
    }
}
