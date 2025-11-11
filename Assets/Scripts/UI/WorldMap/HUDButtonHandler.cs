using Managers.World_Map.HUDInteractions;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.WorldMap
{
    public class HUDButtonHandler : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log($"HUDButtonHandler: Awake called on {gameObject.name}");

            UIDocument _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("HUDButtonHandler: UIDocument not found");
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;
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
