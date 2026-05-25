using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class BulletSelfDestruct : MonoBehaviour
    {
        private Vector3 velocity;

        public void Initialize(Vector3 vel, float lifetime)
        {
            velocity = vel;
            Debug.Log($"Bullet Initialized → Speed: {vel.magnitude:F1} | Lifetime: {lifetime}s");
            
            // Make sure we don't destroy too fast
            Destroy(gameObject, Mathf.Max(lifetime, 3f)); // minimum 3 seconds
        }

        private void Update()
        {
            if (velocity != Vector3.zero)
            {
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                Debug.LogWarning("Bullet has zero velocity!");
            }
        }

        private void OnDestroy()
        {
            Debug.Log($"Bullet destroyed at position: {transform.position}");
        }
    }
}
