using UnityEngine;

namespace PlayerShip_Movement.Combat
{
    public class PlayerTurretDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private LayerMask turretLayer = 1 << 10; // "Turret" layer
        [SerializeField] private float scanInterval = 0.5f;       // Not every frame = perf!

        [Header("Output")]
        public bool turretInRange;  // Use for UI/firing

        private Collider[] _resultsBuffer = new Collider[16]; // Pre-alloc (max turrets expected)
        private float _scanTimer;

        private void Start()
        {
            InvokeRepeating(nameof(ScanForTurrets), 0f, scanInterval);
        }

        private void ScanForTurrets()
        {
            // Query all turrets in sphere (NonAlloc = zero GC!)
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,      // Ship center
                detectionRange,         // 50 units
                _resultsBuffer,         // Reuse buffer
                turretLayer             // Only "Turret" layer
            );

            turretInRange = hitCount > 0;

            if (turretInRange)
            {
                // Get closest turret (teaching: always prioritize!)
                Collider closest = _resultsBuffer[0];
                float closestDist = Vector3.Distance(transform.position, closest.transform.position);
                for (int i = 1; i < hitCount; i++)
                {
                    float dist = Vector3.Distance(transform.position, _resultsBuffer[i].transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = _resultsBuffer[i];
                    }
                }
                Debug.Log($"Turret '{closest.name}' in range ({closestDist:F1}m)!");
            }
        }

        // Editor visualization (super helpful!)
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = turretInRange ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}