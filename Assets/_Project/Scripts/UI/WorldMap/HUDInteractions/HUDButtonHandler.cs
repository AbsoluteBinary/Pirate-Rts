using _Project.Scripts.Main_Screen;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.WorldMap.HUDInteractions
{
    public class HUDButtonHandler : MonoBehaviour
    {
         
        private void OnEnable()
        {
            var panel = GetComponent<PanelRenderer>();
            if (panel == null)
            {
                Debug.LogError("HUDButtonHandler: PanelRenderer not found");
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
            var button = root.Q<Button>("EnterHarbourButton");
            if (button == null) return;

            button.clicked += () =>
            {
                LoginMenuManager.Instance?.StartLoadingTransition();
                HUDMenuButtonsEventBus.TriggerHUDEnterBaseClicked();
            };
        }
    }
}
