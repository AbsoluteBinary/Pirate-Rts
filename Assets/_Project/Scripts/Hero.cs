using System;
using _Project.Scripts._Project.Scripts.Persistence;
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
        [SerializeField] private HealthComponent health;
        [SerializeField] private ManaComponent mana;
        
        EventBinding<TestEvent> testEventBinding;
        EventBinding<PlayerEvent> playerEventBinding;

        private void Awake()
        {
            if (health == null)
                health = GetComponent<HealthComponent>();
            if (mana == null)
                mana = GetComponent<ManaComponent>();
            if (health == null || mana == null)
                Debug.LogWarning("HealthComponent or ManaComponent missing on Hero", this);
        }

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
            Debug.Log($"Player Event Received - Health: {playerEvent.health}, Mana: {playerEvent.mana}");
        }

        public void Bind(PlayerData pData)
        {
            playerData = pData;
            playerData.Id = Id;
            if (health != null)
                health.SetHealth(playerData.Health); // Use property
            if (mana != null)
                mana.SetMana(playerData.Mana);     // Use property
            Debug.Log($"Hero Bound: Health={playerData.Health}, Mana={playerData.Mana}");
        }

        public void UpdatePlayerDataForSave()
        {
            if (health != null)
                playerData.Health = health.GetHealth(); // Use property
            if (mana != null)
                playerData.Mana = mana.GetMana();      // Use property
            playerData.position = transform.position;
            playerData.rotation = transform.rotation;
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
                    health = health.GetHealth(),
                    mana = mana.GetMana()
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
    }
}
