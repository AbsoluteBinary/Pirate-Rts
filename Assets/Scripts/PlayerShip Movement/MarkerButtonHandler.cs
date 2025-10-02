using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TGS;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayerShip_Movement
{
    public class MarkerButtonHandler : MonoBehaviour
    {
        private void Awake()
        {
            //Debug.Log($"MarkerButtonHandler: Awake called on {gameObject.name}. InstanceID: {gameObject.GetInstanceID()}");

            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError($"MarkerButtonHandler: UIDocument not found on {gameObject.name}.");
                return;
            }
            //Debug.Log($"MarkerButtonHandler: UIDocument found. Visual Tree Asset: {uiDocument.visualTreeAsset?.name}");

            VisualElement root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("MarkerButtonHandler: Root visual element is null.");
                return;
            }

            Button button = root.Q<Button>("WorldSpaceButton");
            if (button == null)
            {
                var buttons = root.Query<Button>().ToList();
                //Debug.LogWarning($"MarkerButtonHandler: Button 'WorldSpaceButton' not found. Found {buttons.Count} buttons: {string.Join(", ", buttons.Select(b => b.name))}");
                return;
            }

            //Debug.Log($"MarkerButtonHandler: Button 'WorldSpaceButton' found. Interactable: {button.enabledSelf}. Position: {button.worldBound}");
            button.clicked += () =>
            {
                Debug.Log("MarkerButtonHandler: World Space Button Clicked!");
                EventBus.TriggerMarkerButtonClick();
            };
            button.RegisterCallback<UnityEngine.UIElements.ClickEvent>(evt =>
                Debug.Log($"MarkerButtonHandler: ClickEvent detected on {((VisualElement)evt.target).name}!"));
            button.RegisterCallback<PointerDownEvent>(evt =>
                Debug.Log($"MarkerButtonHandler: PointerDownEvent detected on {((VisualElement)evt.target).name}!"));
            button.RegisterCallback<PointerEnterEvent>(evt =>
                Debug.Log($"MarkerButtonHandler: PointerEnterEvent detected on {((VisualElement)evt.target).name}!"));
        }
    }
}
