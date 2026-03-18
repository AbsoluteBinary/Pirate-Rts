using _Project.Scripts.Main_Screen;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.WorldMap.HUDInteractions
{
    public class HUDButtonHandler : MonoBehaviour
    {
         
        private void Awake()
        {
            //Debug.Log($"HUDButtonHandler: Awake called on {gameObject.name}");

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
                    LoginMenuManager.Instance?.StartLoadingTransition();
                    //Debug.Log("HUDButtonHandler: Enter Harbour Button Clicked!");
                    HUDMenuButtonsEventBus.TriggerHUDEnterBaseClicked();
                };
            }
        }
    }
}
