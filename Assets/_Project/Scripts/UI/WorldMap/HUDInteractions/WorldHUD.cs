using _Project.Scripts.Main_Screen;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.WorldMap.HUDInteractions
{
    public class WorldHUD : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;

        private void OnEnable()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            if (panelRenderer == null)
            {
                Debug.LogError("WorldHUD: PanelRenderer is missing.");
                return;
            }

            panelRenderer.RegisterUIReloadCallback(OnUIReady);
        }

        private void OnDisable()
        {
            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReady);
        }

        private void OnUIReady(PanelRenderer renderer, VisualElement root)
        {
            root.Clear();

            var bar = new VisualElement { name = "ButtonHUD" };
            bar.style.position = Position.Absolute;
            bar.style.top = 12;
            bar.style.right = 12;
            bar.style.alignItems = Align.FlexEnd;
            root.Add(bar);

            bar.Add(CreateButton("EnterHarbourButton", "Enter Harbour", new Color(0.18f, 0.22f, 0.38f), OnEnterHarbour));
            //bar.Add(CreateButton("EnterBattleButton", "Enter Battle", new Color(0.45f, 0.16f, 0.16f), OnEnterBattle));
        }

        private static Button CreateButton(string elementName, string text, Color background, System.Action onClick)
        {
            var button = new Button { name = elementName, text = text };
            button.style.width = 166;
            button.style.height = 53;
            button.style.marginBottom = 8;
            button.style.fontSize = 20;
            button.style.color = Color.white;
            button.style.backgroundColor = background;
            button.style.borderTopLeftRadius = 6;
            button.style.borderTopRightRadius = 6;
            button.style.borderBottomLeftRadius = 6;
            button.style.borderBottomRightRadius = 6;
            button.clicked += () => onClick?.Invoke();
            return button;
        }

        private static void OnEnterHarbour()
        {
            LoginMenuManager.Instance?.StartLoadingTransition();
            HUDMenuButtonsEventBus.TriggerHUDEnterBaseClicked();
        }

        private static void OnEnterBattle()
        {
            HUDMenuButtonsEventBus.TriggerHUDEnterBattleClicked();
        }
    }
}