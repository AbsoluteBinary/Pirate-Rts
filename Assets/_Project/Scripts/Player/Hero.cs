using _Project.Scripts.EventBus;
using _Project.Scripts.Inventory.Helpers;
using _Project.Scripts.Persistence;
using _Project.Scripts.Player._Project.Scripts.Persistence;
using UnityEngine;

namespace _Project.Scripts.Player
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
            if (health == null) health = GetComponent<HealthComponent>();
            if (mana == null)   mana   = GetComponent<ManaComponent>();
            
            //if (health == null || mana == null)
                //Debug.LogWarning("HealthComponent or ManaComponent missing on Hero", this);
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
            // Debug.Log("Test Event Received");   ← commented or removed
        }

        private void HandlePlayerEvent(PlayerEvent playerEvent)
        {
            // Debug.Log($"Player Event Received - Health: {playerEvent.health}, Mana: {playerEvent.mana}");  ← removed
        }

        public void Bind(PlayerData pData)
        {
            playerData = pData;
            playerData.Id = Id;

            if (health != null) health.SetHealth(playerData.Health);
            if (mana != null)   mana.SetMana(playerData.Mana);
        }

        public void UpdatePlayerDataForSave()
        {
            if (health != null) playerData.Health = health.GetHealth();
            if (mana != null)   playerData.Mana   = mana.GetMana();
            playerData.position = transform.position;
            playerData.rotation = transform.rotation;
        }
    
        private void Update()
        {
            var heroTransform = transform;
            heroTransform.position = playerData.position;
            heroTransform.rotation = playerData.rotation;
        }
    }
}