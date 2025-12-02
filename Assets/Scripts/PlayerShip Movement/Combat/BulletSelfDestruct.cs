using UnityEngine;

namespace PlayerShip_Movement.Combat
{
    public class BulletSelfDestruct : MonoBehaviour
    {
        private Vector3 velocity;

        public void Initialize(Vector3 vel, float lifetime)
        {
            velocity = vel;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += velocity * Time.deltaTime;
        }
    }
}
