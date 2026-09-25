using _Project.Scripts.Main_Screen;
using _Project.Scripts.UI.WorldMap.HUDInteractions;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.WorldMap.ObjectInteractions
{
    public class Type0AButtonHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            var panel = GetComponent<PanelRenderer>();
            if (panel == null)
            {
                Debug.LogError("Type0AObjButtonHandler: PanelRenderer not found");
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
            var button = root.Q<Button>("AttackButtonTypeOA");
            if (button == null) return;

            button.clicked += () =>
            {
                LoginMenuManager.Instance?.StartLoadingTransition();
                WorldSpaceInteractionsEventBus.TriggerType0AObjButtonClick();
            };
        }
        
    }
}
