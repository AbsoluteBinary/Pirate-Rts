using _Project.Scripts.UI.WorldMap.HUDInteractions;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.Harbour
{
    public class IdleButtonHandler : MonoBehaviour
    {
        private void Awake()
        {
            //Debug.Log($"HUDButtonHandler: Awake called on {gameObject.name}");

            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                //Debug.LogError("HUDButtonHandler: UIDocument not found");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            Button button = root.Q<Button>("WorldMapLoadButton");

            if (button != null)
            {
                button.clicked += () =>
                {
                    //Debug.Log("HUDButtonHandler: Enter Harbour Button Clicked!");
                    HUDMenuButtonsEventBus.TriggerHUDEnterWorldClicked();
                };
            }
        }
    }
}
