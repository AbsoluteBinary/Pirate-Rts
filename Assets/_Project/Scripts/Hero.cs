using System;
using _Project.Scripts.EventBus;
using _Project.Scripts.Inventory.Helpers;
using _Project.Scripts.Persistence;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts
{
    public class Hero : MonoBehaviour, IBind<PlayerData>
    {
        [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
    
        [SerializeField] public PlayerData playerData;

        //HealthComponent health;
        //ManaComponent mana;
        
        EventBinding<TestEvent> testEventBinding;
        EventBinding<PlayerEvent>  playerEventBinding;

        private void OnEnable()
        {
            testEventBinding = new EventBinding<TestEvent>(HandleTestEvent);
            EventBus<TestEvent>.Register(testEventBinding);
            
            playerEventBinding = new EventBinding<PlayerEvent>(HandlePlayerEvent);
            EventBus<PlayerEvent>.Register(playerEventBinding);
        }

        private void OnDisable()
        {
            EventBus<TestEvent>.Deregister(testEventBinding);
            EventBus<PlayerEvent>.Deregister(playerEventBinding);
        }

        private void HandleTestEvent(TestEvent testEvent)
        {
            Debug.Log("Test Event Received");
        }
        private void HandlePlayerEvent(PlayerEvent playerEvent)
        {
            Debug.Log("Player Event Received");
        }

        public void Bind(PlayerData pData)
        {
            playerData = pData;
            playerData.Id = Id;
        }
    
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                EventBus<TestEvent>.Raise(new TestEvent());
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                EventBus<PlayerEvent>.Raise(new PlayerEvent
                {
                    //health = health.GetHealth(),
                    //mana = mana.GetMana()
                });
            }
            
            
            var heroTransform = transform;
            heroTransform.position = playerData.position;
            heroTransform.rotation = playerData.rotation;
        }
        private void LateUpdate()
        {
            
            //Debug.Log(gameData.playerData.Id.ToGuid().ToString());
        }

        // [Button]
        // private void DisplayPlayerID()
        // {
        //     //string playerID;
        //     string playerID = playerData.Id.ToGuid().ToString();
        //     Debug.Log($"The Player ID is : {playerID}");
        // }
        

        
    }
}

