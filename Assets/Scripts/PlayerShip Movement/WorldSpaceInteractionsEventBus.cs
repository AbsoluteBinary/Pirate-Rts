using System;
using UnityEngine;

namespace PlayerShip_Movement
{
    public static class WorldSpaceInteractionsEventBus
    {
        public static event Action MarkerButtonClicked;
        public static event Action BaseButtonClicked;

        public static void TriggerMarkerButtonClick()
        {
            Debug.Log("EventBus: Triggering MarkerButtonClicked event.");
            MarkerButtonClicked?.Invoke();
        }
        public static void TriggerBaseButtonClick()
        {
            Debug.Log("EventBus: Triggering BaseButtonClicked event.");
            BaseButtonClicked?.Invoke();
        }
    }
}
