using UnityEngine;

namespace Managers.World_Map.HUDInteractions
{
    public class HUDMenuButtonsEventBus : MonoBehaviour
    {
        public static event System.Action HUDEnterBaseClicked;
        
        public static void TriggerHUDEnterBaseClicked()
        {
            //Debug.Log("EventBus: Triggering HUDEnterHarbourClicked event.");
            Debug.Log("EventBus: Triggering HUDEnterBaseClicked event. Listeners: " + (HUDEnterBaseClicked?.GetInvocationList()?.Length ?? 0));
            HUDEnterBaseClicked?.Invoke();
        }
        
    }
}
