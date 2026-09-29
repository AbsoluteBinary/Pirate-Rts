using UnityEngine;

namespace _Project.Scripts.SceneManagement
{
    public class PersistentRig : MonoBehaviour
    {
        private static PersistentRig _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}