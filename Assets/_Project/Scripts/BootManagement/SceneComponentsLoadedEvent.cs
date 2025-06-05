using _Project.Scripts.EventBus;

namespace _Project.Scripts.BootManagement
{
    public struct SceneComponentsLoadedEvent : IEvent
    {
        public SceneComponentsData components;
    }
}