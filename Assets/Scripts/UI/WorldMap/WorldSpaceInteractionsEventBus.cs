using System;
using UnityEngine;

namespace UI.WorldMap
{
    public static class WorldSpaceInteractionsEventBus
    {
        public static event Action MarkerButtonClicked;
        public static event Action HarbourButtonClicked;
        public static event Action Type0AObjButtonClicked;

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

        public static void TriggerType0AObjButtonClick()
        {
            Debug.Log("EventBus: Triggering Type0AObjButtonClicked event.");
            Type0AObjButtonClicked?.Invoke();
        }
    }
}
