using _Project.Scripts.UI.WorldMap.HUDInteractions;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.Combat
{
    public class CombatHUDGeneric : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;

        private void OnEnable()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            if (panelRenderer == null)
            {
                Debug.LogError("CombatHUDGeneric: PanelRenderer is missing.");
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

            var bar = new VisualElement { name = "CombatButtonHUD" };
            bar.style.position = Position.Absolute;
            bar.style.top = 12;
            bar.style.right = 12;
            bar.style.alignItems = Align.FlexEnd;
            root.Add(bar);

            var endBattle = new Button { name = "EndBattleButton", text = "End Battle" };
            endBattle.style.width = 166;
            endBattle.style.height = 53;
            endBattle.style.fontSize = 20;
            endBattle.style.color = Color.white;
            endBattle.style.backgroundColor = new Color(0.45f, 0.16f, 0.16f);
            endBattle.style.borderTopLeftRadius = 6;
            endBattle.style.borderTopRightRadius = 6;
            endBattle.style.borderBottomLeftRadius = 6;
            endBattle.style.borderBottomRightRadius = 6;
            endBattle.clicked += OnEndBattleClicked;
            bar.Add(endBattle);

            BindCombatMechanisms();
        }

        private void OnEndBattleClicked()
        {
            HUDMenuButtonsEventBus.TriggerHUDEnterWorldClicked();
        }

        /// <summary>
        /// Later: shooting, turret input, hit VFX, damage numbers.
        /// Called once when the combat panel is ready.
        /// </summary>
        private void BindCombatMechanisms()
        {
        }
    }
}