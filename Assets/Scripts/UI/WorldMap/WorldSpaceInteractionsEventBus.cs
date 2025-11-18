using System;
using UnityEngine;

namespace UI.WorldMap
{
    public static class WorldSpaceInteractionsEventBus
    {
        public static event Action MarkerButtonClicked;
        public static event Action HarbourButtonClicked;

        public static void TriggerMarkerButtonClick()
        {
            Debug.Log("EventBus: Triggering MarkerButtonClicked event.");
            MarkerButtonClicked?.Invoke();
        }
        public static void TriggerBaseButtonClick()
        {
            Debug.Log("EventBus: Triggering HarbourButtonClicked event.");
            HarbourButtonClicked?.Invoke();
        }
    }
}
