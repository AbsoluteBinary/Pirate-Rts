using System.Collections.Generic;
using UnityEngine;

namespace Managers.Registry
{
    public static class ContainerRegistry
    {
        // All containers that need to be disabled during loading
        private static readonly List<GameObject> containers = new();

        public static void Register(GameObject container)
        {
            if (container != null && !containers.Contains(container))
            {
                containers.Add(container);
                Debug.Log($"[ContainerRegistry] Registered: {container.name}");
            }
        }

        public static void Unregister(GameObject container)
        {
            containers.Remove(container);
        }

        public static IReadOnlyList<GameObject> GetAllContainers() => containers;

        // Optional: Clear when changing scenes
        public static void Clear() => containers.Clear();
    }
}
