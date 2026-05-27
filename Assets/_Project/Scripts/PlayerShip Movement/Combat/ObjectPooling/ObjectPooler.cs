using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat.ObjectPooling
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size = 20;
        }

        [SerializeField] private List<Pool> pools = new List<Pool>();
        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Create pools
            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag '{tag}' doesn't exist.");
                return null;
            }

            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            // Reset velocity / state if needed
            if (objectToSpawn.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = Vector3.zero;

            poolDictionary[tag].Enqueue(objectToSpawn); // Re-enqueue for reuse

            return objectToSpawn;
        }

        public void ReturnToPool(string tag, GameObject obj)
        {
            if (string.IsNullOrEmpty(tag) || !poolDictionary.ContainsKey(tag)) return;

            obj.SetActive(false);
            // Do NOT enqueue again here — we already did in Spawn for simplicity
        }
    }
}