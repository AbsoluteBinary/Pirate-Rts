using UnityEngine;

namespace PlayerShip_Movement.Combat
{
    [RequireComponent(typeof(Transform))]
    public class CannonFire : MonoBehaviour
    {
        [Header("Target & Range")]
        [SerializeField] private Transform player;
        [SerializeField] private float fireRange = 200f;

        [Header("Bullet")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float bulletSpeed = 120f;
        [SerializeField] private float bulletLifetime = 2f;

        [Header("Fire Rate")]
        [SerializeField] private float fireRate = 1.5f;
        private float nextFireTime;

        private void Awake()
        {
            if (player == null)
            {
                player = GameObject.FindWithTag("Player")?.transform;
                if (player == null) Debug.LogError("CannonFire: No Player found! Tag your player ship as 'Player'");
            }

            if (bulletPrefab == null)
                Debug.LogError("CannonFire: bulletPrefab not assigned!");
        }

        private void Update()
        {
            if (player == null || bulletPrefab == null) return;
            if (Time.time < nextFireTime) return;

            float distance = Vector3.Distance(transform.position, player.position);
            Debug.Log($"[Cannon] Distance to player: {distance:F1} / {fireRange}");

            if (distance <= fireRange)
            {
                Debug.Log("<color=yellow>[CANNON] IN RANGE — FIRING!</color>");
                Fire();
                nextFireTime = Time.time + fireRate;
            }
        }

        [SerializeField] private Transform fireDirection; // Drag FireDirection here

        private void Fire()
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.linearVelocity = fireDirection.forward * bulletSpeed;
            Destroy(bullet, bulletLifetime);
        }
    }
}