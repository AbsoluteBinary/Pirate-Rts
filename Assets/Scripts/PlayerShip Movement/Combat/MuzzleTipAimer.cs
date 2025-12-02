using UnityEngine;

namespace PlayerShip_Movement.Combat
{
    public class MuzzleTipAimer : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform playerTarget;  // Drag Player Ship or Camera here

        [Header("LookAt Settings")]
        [SerializeField] private bool facePlayer = true;
        [SerializeField] private bool keepUpright = true;  // Prevents flipping/tilting

        private void Start()
        {
            // Auto-find player if not assigned (teaching: flexible!)
            if (playerTarget == null)
            {
                playerTarget = GameObject.FindWithTag("Player")?.transform ?? Camera.main.transform;
                Debug.Log($"MuzzleTipAimer: Auto-found target: {playerTarget?.name}");
            }
        }

        private void LateUpdate()  // Beats all other rotations!
        {
            if (!facePlayer || playerTarget == null) return;

            // Face player
            transform.LookAt(playerTarget);

            // Optional: Keep upright (no roll/tilt — perfect for muzzles)
            if (keepUpright)
            {
                Vector3 euler = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(0, euler.y, 0);
            }
        }
    }
}
