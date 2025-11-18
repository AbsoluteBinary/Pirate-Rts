using UnityEngine;
using UnityEngine.UIElements;

namespace UI.WorldMap.HUDInteractions
{
    public class HUDButtonHandler : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log($"HUDButtonHandler: Awake called on {gameObject.name}");

            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("HUDButtonHandler: UIDocument not found");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            Button button = root.Q<Button>("EnterHarbourButton");

            if (button != null)
            {
                button.clicked += () =>
                {
                    Debug.Log("HUDButtonHandler: Enter Harbour Button Clicked!");
                    HUDMenuButtonsEventBus.TriggerHUDEnterBaseClicked();
                };
            }
        }
    }
}
