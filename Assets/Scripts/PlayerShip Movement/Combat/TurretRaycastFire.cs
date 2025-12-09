using UnityEngine;

namespace PlayerShip_Movement.Combat
{
    public class TurretRaycastFire : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float range = 20f;  // Single variable for aiming + firing
        [SerializeField] private LayerMask hitMask = -1;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float fireRate = 1.5f;
        [SerializeField] private GameObject trailPrefab;

        private float nextFireTime;

        private void Awake()
        {
            if (player == null)
                player = GameObject.FindWithTag("Player")?.transform ?? Camera.main.transform;
        }

        private void Update()
        {
            if (player == null || Time.time < nextFireTime) return;

            // 1. Raycast for LOS + range
            Vector3 origin = transform.position;
            Vector3 dir = (player.position - origin).normalized;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, hitMask))  // ← Use range
            {
                if (hit.collider.transform == player)
                {
                    Fire(hit.point);
                    nextFireTime = Time.time + fireRate;
                }
            }
        }

        private void LateUpdate()
        {
            if (player == null) return;

            // ← Only aim when in range
            float sqrDist = (player.position - transform.position).sqrMagnitude;
            if (sqrDist > range * range) return;  // ← Use range

            // Direction to player on horizontal plane only
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0;

            if (toPlayer.sqrMagnitude < 0.001f) return;

            // Desired rotation (Y-axis only)
            Quaternion targetRot = Quaternion.LookRotation(toPlayer, Vector3.up);

            // Apply while preserving the 90° X pitch
            transform.rotation = Quaternion.Euler(90f, targetRot.eulerAngles.y, 0f);
        }

        private void Fire(Vector3 hitPoint)
        {
            // Apply damage instantly
            var health = player.GetComponent<Health>();
            health?.TakeDamage(damage);

            // Spawn visual trail
            if (trailPrefab != null)
            {
                GameObject trail = Instantiate(trailPrefab, transform.position, Quaternion.identity);
                LineRenderer lr = trail.GetComponentInChildren<LineRenderer>();
                if (lr != null)
                {
                    lr.SetPosition(0, transform.position);
                    lr.SetPosition(1, hitPoint);
                }
                Destroy(trail, 0.5f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);  // ← Use range
            Gizmos.DrawRay(transform.position, transform.forward * range);
        }
    }
}