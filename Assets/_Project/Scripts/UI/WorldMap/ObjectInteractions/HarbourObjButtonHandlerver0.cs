using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.WorldMap.ObjectInteractions
{
    public class HarbourObjButtonHandlerver0 : MonoBehaviour
    {
        
        private void Awake()
        {
            Debug.Log($"BaseButtonHandler: Awake called on {gameObject.name}. InstanceID: {gameObject.GetInstanceID()}");

            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError($"BaseButtonHandler: UIDocument not found on {gameObject.name}.");
                return;
            }
            Debug.Log($"BaseButtonHandler: UIDocument found. Visual Tree Asset: {uiDocument.visualTreeAsset?.name}");

            VisualElement root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("BaseButtonHandler: Root visual element is null.");
                return;
            }

            Button button = root.Q<Button>("PlayerHarbourButton");
            if (button == null)
            {
                var buttons = root.Query<Button>().ToList();
                //Debug.LogWarning($"MarkerButtonHandler: Button 'WorldSpaceButton' not found. Found {buttons.Count} buttons: {string.Join(", ", buttons.Select(b => b.name))}");
                return;
            }

            Debug.Log($"BaseButtonHandler: Button 'PlayerHarbourButton' found. Interactable: {button.enabledSelf}. Position: {button.worldBound}");
            button.clicked += () =>
            {
                Debug.Log("BaseButtonHandler: Player Harbour Button Clicked!");
                WorldSpaceInteractionsEventBus.TriggerBaseButtonClick();
                //Focus here
            };
            //button.RegisterCallback<ClickEvent>(evt => Debug.Log($"BaseButtonHandler: ClickEvent detected on {((VisualElement)evt.target).name}!"));
            //button.RegisterCallback<PointerDownEvent>(evt => Debug.Log($"BaseButtonHandler: PointerDownEvent detected on {((VisualElement)evt.target).name}!"));
            //button.RegisterCallback<PointerEnterEvent>(evt => Debug.Log($"BaseButtonHandler: PointerEnterEvent detected on {((VisualElement)evt.target).name}!"));
        }
    }
}
