using Main_Screen;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.WorldMap.ObjectInteractions
{
    public class Type0AButtonHandler : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log($"Type0AObjButtonHandler: Awake called on {gameObject.name}");

            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("Type0AObjButtonHandler: UIDocument not found");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            Button button = root.Q<Button>("AttackButtonTypeOA");

            if (button != null)
            {
                button.clicked += () =>
                {
                    LoginMenuManager.Instance?.StartLoadingTransition();
                    Debug.Log("Type0AObjButtonHandler: Enter Type0A Button Clicked!");
                    WorldSpaceInteractionsEventBus.TriggerType0AObjButtonClick();
                };
            }
        }
    }
}
