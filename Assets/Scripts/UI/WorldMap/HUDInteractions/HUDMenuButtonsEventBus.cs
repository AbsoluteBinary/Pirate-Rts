using UnityEngine;

namespace UI.WorldMap.HUDInteractions
{
    public class HUDMenuButtonsEventBus : MonoBehaviour
    {
        public static event System.Action HUDEnterBaseClicked;
        public static event System.Action HUDEnterWorldClicked;
        
        // Called by HudController
        public static void TriggerHUDEnterBaseClicked()
        {
            //Debug.Log("EventBus: Triggering HUDEnterHarbourClicked event.");
            Debug.Log("EventBus: Triggering HUDEnterBaseClicked event. Listeners: " + (HUDEnterBaseClicked?.GetInvocationList()?.Length ?? 0));
            HUDEnterBaseClicked?.Invoke();
        }
        
        //Called by IdleHudButtonController
        public static void TriggerHUDEnterWorldClicked()
        {
            //Debug.Log("EventBus: Triggering HUDEnterHarbourClicked event.");
            Debug.Log("EventBus: Triggering HUDEnterWorldClicked event. Listeners: " + (HUDEnterWorldClicked?.GetInvocationList()?.Length ?? 0));
            HUDEnterWorldClicked?.Invoke();
        }
        
    }
}
