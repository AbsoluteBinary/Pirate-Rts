using UnityEngine;

namespace Managers.Registry
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
