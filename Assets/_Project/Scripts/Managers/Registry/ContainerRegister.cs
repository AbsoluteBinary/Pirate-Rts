using UnityEngine;

namespace _Project.Scripts.Managers.Registry
{
    public class ContainerRegister : MonoBehaviour
    {
        private void Awake()
        {
            ContainerRegistry.Register(gameObject);
        }

        private void OnDestroy()
        {
            ContainerRegistry.Unregister(gameObject);
        }
    }
}
