using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.SceneManagement
{
    public class EventSystemChecker : MonoBehaviour
    {
        void Update()
        {
            var activeEventSystems = FindObjectsOfType<EventSystem>().Where(es => es.enabled).ToArray();
            if (activeEventSystems.Length > 1)
            {
                Debug.LogError($"Multiple EventSystems active: {activeEventSystems.Length}");
                foreach (var es in activeEventSystems)
                {
                    Debug.Log($"Active EventSystem: {es.gameObject.name} in {es.gameObject.scene.name}");
                }
            }
        }
    }
}