using Managers.World_Map.HUDInteractions;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class HUDButtonHandler : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log($"HUDButtonHandler: Awake called on {gameObject.name}. InstanceID: {gameObject.GetInstanceID()}");

            UIDocument _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.Log("HUDButtonHandler: UIDocument not found on " + gameObject.name);
                return;
            }
            Debug.Log($"HUDButtonHandler: UIDocument found. Visual Tree Asset: {_uiDocument.visualTreeAsset?.name}");
            
            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("HUDButtonHandler: Root visual element is null.");
            }
            
            Button button = root.Q<Button>("EnterHarbourButton");
            if (button == null)
            {
                Debug.LogWarning("HUDButtonHandler: Button 'HUDButton' not found.");
            }

            if (button != null)
            {
                Debug.Log(
                    $"HUDButtonHandler: Button 'EnterHarbourButton' found. Interactable: {button.enabledSelf}. Position: {button.clickable}");

                button.clicked += () =>
                {
                    Debug.Log("HUDButtonHandler: Enter Harbour Button Clicked!");
                    HUDMenuButtonsEventBus.TriggerHUDEnterBaseClicked();
                };

                button.RegisterCallback<ClickEvent>(evt =>
                    Debug.Log($"HUDButtonHandler: ClickEvent detected on {((VisualElement)evt.target).name}!"));
                button.RegisterCallback<PointerDownEvent>(evt =>
                    Debug.Log($"HUDButtonHandler: PointerDownEvent detected on {((VisualElement)evt.target).name}!"));
                button.RegisterCallback<PointerEnterEvent>(evt =>
                    Debug.Log($"HUDButtonHandler: PointerEnterEvent detected on {((VisualElement)evt.target).name}!"));
            }
        }
}
}
