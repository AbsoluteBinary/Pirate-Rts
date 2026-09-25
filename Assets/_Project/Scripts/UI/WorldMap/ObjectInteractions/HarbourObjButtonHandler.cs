using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.WorldMap.ObjectInteractions
{
    public class HarbourObjButtonHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            var panel = GetComponent<PanelRenderer>();
            if (panel == null)
            {
                Debug.LogError("HarbourObjButtonHandler: PanelRenderer not found");
                return;
            }

            panel.RegisterUIReloadCallback(OnUIReady);
        }

        private void OnDisable()
        {
            var panel = GetComponent<PanelRenderer>();
            if (panel != null)
                panel.UnregisterUIReloadCallback(OnUIReady);
        }

        private void OnUIReady(PanelRenderer renderer, VisualElement root)
        {
            var button = root.Q<Button>("PlayerHarbourButton");
            if (button == null) return;

            button.clicked += () =>
            {
                button.clicked += WorldSpaceInteractionsEventBus.TriggerBaseButtonClick;
                
            };
        }
    }
}
