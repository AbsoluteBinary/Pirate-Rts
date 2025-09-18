using System;
using UnityEngine;

namespace PlayerShip_Movement
{
    public static class EventBus
    {
        public static event Action MarkerButtonClicked;

        public static void TriggerMarkerButtonClick()
        {
            Debug.Log("EventBus: Triggering MarkerButtonClicked event.");
            MarkerButtonClicked?.Invoke();
        }
    }
}
