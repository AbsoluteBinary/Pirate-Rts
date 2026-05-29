using UnityEngine;
using _Project.Scripts.Combat.Data;
using _Project.Scripts.Combat.Managers;

namespace _Project.Scripts.Combat.Projectiles
{
    /// <summary>
    /// Handles bullet movement and returns the bullet to the pool after its lifetime.
    /// </summary>
    public class BulletSelfDestruct : MonoBehaviour
    {
        private Vector3 _velocity;
        private float _lifetime;
        private TurretWeaponData _weaponData;
        private float _timer;

        public void Initialize(Vector3 velocity, float lifetime, TurretWeaponData weaponData)
        {
            _velocity = velocity;
            _lifetime = lifetime;
            _weaponData = weaponData;
            _timer = 0f;
        }

        private void Update()
        {
            transform.position += _velocity * Time.deltaTime;

            _timer += Time.deltaTime;

            if (_timer >= _lifetime)
            {
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (BulletPoolManager.Instance != null)
            {
                BulletPoolManager.Instance.ReturnBullet(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void ForceReturnToPool()
        {
            ReturnToPool();
        }
    }
}
