using _Project.Scripts.Fleet;
using _Project.Scripts.Main_Screen;
using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.IMGUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.WorldMap.HUDInteractions
{
    public class WorldHUD : MonoBehaviour
    {
        private const int FleetSlotCount = 4;

        [SerializeField] private PanelRenderer panelRenderer;

        private readonly VisualElement[] _fleetRows = new VisualElement[FleetSlotCount];
        private readonly Label[] _fleetNames = new Label[FleetSlotCount];
        private readonly VisualElement[] _fleetFills = new VisualElement[FleetSlotCount];
        private int _selectedFleet = -1;

        private static readonly Color RowBorder = new Color(0.25f, 0.35f, 0.55f);
        private static readonly Color RowSelected = new Color(0.85f, 0.72f, 0.35f);
        private static readonly Color RowBackground = new Color(0.06f, 0.08f, 0.14f, 0.92f);

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
            MapFleets.Changed += RefreshFleets;
        }

        private void OnDisable()
        {
            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReady);
            MapFleets.Changed -= RefreshFleets;
        }

        private void OnUIReady(PanelRenderer renderer, VisualElement root)
        {
            root.Clear();
            root.pickingMode = PickingMode.Ignore;
            _selectedFleet = -1;

            var bar = new VisualElement { name = "ButtonHUD" };
            bar.style.position = Position.Absolute;
            bar.style.top = 12;
            bar.style.right = 12;
            bar.style.alignItems = Align.FlexEnd;
            root.Add(bar);

            bar.Add(CreateButton("EnterHarbourButton", "Enter Harbour", new Color(0.18f, 0.22f, 0.38f), OnEnterHarbour));
            bar.Add(CreateButton("ExitToLoginButton", "Exit to Login", new Color(0.22f, 0.22f, 0.26f), OnExitToLogin));

            root.Add(CreateFleetPanel());
            SetFleetSlot(0, "Fleet", 0.75f);
            SetFleetSlot(1, "Empty", 0f);
            SetFleetSlot(2, "Empty", 0f);
            SetFleetSlot(3, "Empty", 0f);
            
            RefreshFleets();
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
            row.pickingMode = PickingMode.Position;
            row.RegisterCallback<ClickEvent>(_ => SelectFleet(index));

            var name = new Label("Empty");
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

            _fleetRows[index] = row;
            _fleetNames[index] = name;
            _fleetFills[index] = fill;
            return row;
        }
        

        private void SetFleetSlot(int index, string fleetName, float health01)
        {
            _fleetNames[index].text = fleetName;
            _fleetFills[index].style.width = Length.Percent(Mathf.Clamp01(health01) * 100f);
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
        
        private void RefreshFleets()
        {
            if (_fleetNames[0] == null)
                return;

            MapFleets.Prune();
            int count = MapFleets.Active.Count;

            for (int i = 0; i < FleetSlotCount; i++)
            {
                bool show = i < count;
                _fleetRows[i].style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                if (!show)
                    continue;

                var fleet = MapFleets.Active[i];
                _fleetNames[i].text = fleet.Name;
                _fleetFills[i].style.width = Length.Percent(fleet.Health01 * 100f);
            }

            ApplySelection();
        }

        private void SelectFleet(int index)
        {
            if (index < 0 || index >= MapFleets.Active.Count)
                return;

            MapFleets.Selected = index;
            ApplySelection();
            Debug.Log($"<color=lime>[World] Selected '{MapFleets.Active[index].Name}'</color>");
        }

        private void ApplySelection()
        {
            for (int i = 0; i < FleetSlotCount; i++)
            {
                var border = i == MapFleets.Selected ? RowSelected : RowBorder;
                var row = _fleetRows[i];
                row.style.borderTopColor = border;
                row.style.borderRightColor = border;
                row.style.borderBottomColor = border;
                row.style.borderLeftColor = border;
            }
        }

        private static void OnExitToLogin()
        {
            if (IMGUILoadingOverlay.Instance != null)
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();

            if (SceneLoader.Instance != null)
                _ = SceneLoader.Instance.BeginSceneTransition(0);
            else
                Debug.LogError("[World] SceneLoader missing.");
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