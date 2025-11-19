using PlayerShip_Movement;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.WorldMap.ObjectInteractions
{
    public class HarbourObjButtonHandler : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log($"HarbourObjButtonHandler: Awake called on {gameObject.name}");

            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("HarbourObjButtonHandler: UIDocument not found");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            Button button = root.Q<Button>("PlayerHarbourButton");

            if (button != null)
            {
                button.clicked += () =>
                {
                    Debug.Log("HarbourObjButtonHandler: Enter Harbour Button Clicked!");
                    WorldSpaceInteractionsEventBus.TriggerBaseButtonClick();
                };
            }
        }
    }
}
