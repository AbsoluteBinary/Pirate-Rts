using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement
{
    public class WorldMovementButtonPoolManager : MonoBehaviour
    {
        public static WorldMovementButtonPoolManager instance;
        public GameObject[] buttonPool = {};
        [SerializeField] private GameObject ObjectToPool;
        public int amountToPool = 2;
    
    
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
            }
        }

        public GameObject GetButtonObject()
        {
            return buttonPool[0];

        }

        public GameObject GetPooledObject()
        {
            return null;
        }
    }
}