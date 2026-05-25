using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
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

        private void LateUpdate()
        {
            if (!facePlayer || playerTarget == null) return;

            Vector3 toTarget = playerTarget.position - transform.position;
    
            transform.rotation = Quaternion.LookRotation(toTarget, Vector3.up) 
                                 * Quaternion.Euler(90f, 0f, 0f);  // ← Compensation for downward forward

            if (keepUpright)
            {
                Vector3 euler = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(0, euler.y, 0); // or remove this if you want pitch
            }
        }
    }
}
