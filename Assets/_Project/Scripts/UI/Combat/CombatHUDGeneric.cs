using _Project.Scripts.Fleet;
using _Project.Scripts.UI.WorldMap.HUDInteractions;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.Combat
{
    public class CombatHUDGeneric : MonoBehaviour
    {
        private const int FleetSlotCount = 4;

        [SerializeField] private PanelRenderer panelRenderer;

        private readonly Label[] _fleetNames = new Label[FleetSlotCount];
        private readonly VisualElement[] _fleetFills = new VisualElement[FleetSlotCount];

        private static readonly Color RowBorder = new Color(0.25f, 0.35f, 0.55f);
        private static readonly Color RowBackground = new Color(0.06f, 0.08f, 0.14f, 0.92f);

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
            ActiveFleets.Changed += RefreshFleets;
        }

        private void OnDisable()
        {
            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReady);
            ActiveFleets.Changed -= RefreshFleets;
        }

        private void OnUIReady(PanelRenderer renderer, VisualElement root)
        {
            root.Clear();
            root.pickingMode = PickingMode.Ignore;

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

            root.Add(CreateFleetPanel());
            RefreshFleets();
            BindCombatMechanisms();
        }

        private VisualElement CreateFleetPanel()
        {
            var panel = new VisualElement { name = "FleetInfoPanel" };
            panel.style.position = Position.Absolute;
            panel.style.right = 16;
            panel.style.bottom = 16;
            panel.style.width = 168;
            panel.pickingMode = PickingMode.Ignore;

            for (int i = 0; i < FleetSlotCount; i++)
                panel.Add(CreateFleetRow(i));

            return panel;
        }

        private VisualElement CreateFleetRow(int index)
        {
            var row = new VisualElement { name = "FleetRow_" + index };
            row.style.marginTop = 4;
            row.style.paddingLeft = 6;
            row.style.paddingRight = 6;
            row.style.paddingTop = 3;
            row.style.paddingBottom = 3;
            row.style.backgroundColor = RowBackground;
            row.style.borderTopWidth = 1;
            row.style.borderRightWidth = 1;
            row.style.borderBottomWidth = 1;
            row.style.borderLeftWidth = 1;
            row.style.borderTopColor = RowBorder;
            row.style.borderRightColor = RowBorder;
            row.style.borderBottomColor = RowBorder;
            row.style.borderLeftColor = RowBorder;
            row.style.borderTopLeftRadius = 4;
            row.style.borderTopRightRadius = 4;
            row.style.borderBottomLeftRadius = 4;
            row.style.borderBottomRightRadius = 4;
            row.pickingMode = PickingMode.Ignore;

            var name = new Label("");
            name.style.unityTextAlign = TextAnchor.MiddleLeft;
            name.style.fontSize = 16;
            name.style.color = Color.white;
            name.style.height = 22;
            name.pickingMode = PickingMode.Ignore;
            row.Add(name);

            var track = new VisualElement();
            track.style.height = 4;
            track.style.marginTop = 4;
            track.style.backgroundColor = new Color(0.15f, 0.18f, 0.24f, 1f);
            track.pickingMode = PickingMode.Ignore;

            var fill = new VisualElement();
            fill.style.height = Length.Percent(100);
            fill.style.width = Length.Percent(0);
            fill.style.backgroundColor = new Color(0.45f, 0.72f, 0.42f, 1f);
            fill.pickingMode = PickingMode.Ignore;
            track.Add(fill);
            row.Add(track);

            _fleetNames[index] = name;
            _fleetFills[index] = fill;
            return row;
        }

        private void RefreshFleets()
        {
            if (_fleetNames[0] == null)
                return;

            for (int i = 0; i < FleetSlotCount; i++)
            {
                var fleet = ActiveFleets.AtSlot(i);
                if (fleet == null)
                {
                    _fleetNames[i].text = "";
                    _fleetFills[i].style.width = Length.Percent(0);
                    continue;
                }

                _fleetNames[i].text = fleet.fleetName;
                _fleetFills[i].style.width = Length.Percent(ActiveFleets.Health01(fleet) * 100f);
            }
        }

        private void OnEndBattleClicked()
        {
            HUDMenuButtonsEventBus.TriggerHUDEnterWorldClicked();
        }

        private void BindCombatMechanisms()
        {
        }
    }
}