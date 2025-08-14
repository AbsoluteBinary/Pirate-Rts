namespace _Project.Scripts.EventBus
{
    public interface IEvent { }

    public struct TestEvent : IEvent { }

    public struct PlayerEvent : IEvent {
        public int health;
        public int mana;
    }
    // New event for scene group loading
    public struct LoadSceneGroupEvent : IEvent
    {
        public int groupIndex;
    }
}