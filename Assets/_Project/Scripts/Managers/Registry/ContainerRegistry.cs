using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Managers.Registry
{
    public static class ContainerRegistry
    {
        private static readonly List<GameObject> containers = new();

        public static void Register(GameObject container)
        {
            if (container != null && !containers.Contains(container))
                containers.Add(container);
        }

        public static void Unregister(GameObject container)
        {
            containers.Remove(container);
        }

        public static IReadOnlyList<GameObject> GetAllContainers() => containers;

        public static void Clear() => containers.Clear();
    }
}