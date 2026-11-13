using System.Collections.Generic;
using _Project.Scripts.Combat;
using _Project.Scripts.PlayerShip_Movement.Combat;
using _Project.Scripts.Fleet;
using _Project.Scripts.Persistence.TempSave;
using _Project.Scripts.UI.WorldMap.HUDInteractions;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.Combat
{
    public class CombatHUDGeneric : MonoBehaviour
    {
        private const int ShipSlotCount = 5;

        [SerializeField] private PanelRenderer panelRenderer;

        private readonly Label[] _shipNames = new Label[ShipSlotCount];
        private readonly VisualElement[] _shipFills = new VisualElement[ShipSlotCount];
        private readonly VisualElement[] _shipRows = new VisualElement[ShipSlotCount];
        private readonly string[] _shipIds = new string[ShipSlotCount];
        private readonly bool[] _selected = new bool[ShipSlotCount];

        private static readonly Color RowBorder = new Color(0.25f, 0.35f, 0.55f);
        private static readonly Color RowSelected = new Color(0.85f, 0.72f, 0.35f);
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
            ActiveFleets.Changed += RefreshShips;

            if (FleetManager.Instance != null)
            {
                FleetManager.Instance.OnFleetChanged += BindFleetHealth;
                FleetManager.Instance.OnFleetChanged += RefreshShips;
            }
        }

        private void OnDisable()
        {
            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReady);
            ActiveFleets.Changed -= RefreshShips;

            if (FleetManager.Instance != null)
            {
                FleetManager.Instance.OnFleetChanged -= BindFleetHealth;
                FleetManager.Instance.OnFleetChanged -= RefreshShips;
            }

            UnbindFleetHealth();
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

            root.Add(CreateShipPanel());
            RefreshShips();
            BindCombatMechanisms();
            BindFleetHealth();
        }

        private VisualElement CreateShipPanel()
        {
            var panel = new VisualElement { name = "FleetInfoPanel" };
            panel.style.position = Position.Absolute;
            panel.style.left = 16;
            panel.style.right = StyleKeyword.Auto;
            panel.style.bottom = 16;
            panel.style.width = 188;
            panel.pickingMode = PickingMode.Ignore;

            for (int i = 0; i < ShipSlotCount; i++)
                panel.Add(CreateShipRow(i));

            return panel;
        }

        private VisualElement CreateShipRow(int index)
        {
            var row = new VisualElement { name = "ShipRow_" + index };
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
            name.style.fontSize = 15;
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

            _shipRows[index] = row;
            _shipNames[index] = name;
            _shipFills[index] = fill;
            return row;
        }

        private void RefreshShips()
        {
            if (_shipNames[0] == null)
                return;

            for (int i = 0; i < ShipSlotCount; i++)
            {
                _shipIds[i] = "";
                _selected[i] = false;
                _shipNames[i].text = "";
                _shipFills[i].style.width = Length.Percent(0);
            }

            if (FillFromLiveShips())
            {
                ApplySelection();
                PaintShipHealth();
                return;
            }

            var fleet = FleetInBattle();
            if (fleet == null)
            {
                ApplySelection();
                return;
            }

            var ships = ShipsInOrder(fleet);
            int count = Mathf.Min(ShipSlotCount, ships.Count);
            for (int i = 0; i < count; i++)
            {
                var ship = ships[i];
                _shipIds[i] = ship.shipId;
                _selected[i] = true;
                _shipNames[i].text = string.IsNullOrEmpty(ship.shipName) ? ship.shipId : ship.shipName;
            }

            ApplySelection();
            PaintShipHealth();
        }

        // Prefer the ships actually spawned in combat. Flagship first, then the rest.
        private bool FillFromLiveShips()
        {
            var manager = FleetManager.Instance;
            if (manager == null || manager.activeShips == null || manager.activeShips.Count == 0)
                return false;

            var ordered = new List<PlayerCombatMovementController>();
            if (manager.flagship != null)
                ordered.Add(manager.flagship);

            foreach (var ship in manager.activeShips)
            {
                if (ship == null || ship == manager.flagship)
                    continue;
                ordered.Add(ship);
            }

            if (ordered.Count == 0)
                return false;

            int count = Mathf.Min(ShipSlotCount, ordered.Count);
            for (int i = 0; i < count; i++)
            {
                var ship = ordered[i];
                var combat = ship.GetComponent<CombatShip>();
                var bp = combat != null ? combat.Blueprint : null;

                _shipIds[i] = bp != null ? bp.saveId : "";
                _selected[i] = true;
                _shipNames[i].text = bp != null && !string.IsNullOrEmpty(bp.shipName)
                    ? bp.shipName
                    : ship.gameObject.name;
            }

            return true;
        }

        private static ActiveFleetRecord FleetInBattle()
        {
            var data = ActiveFleets.Current;
            if (data?.fleets == null)
                return null;

            string preferred = FleetGovernor.Instance != null ? FleetGovernor.Instance.FleetName : "";
            ActiveFleetRecord combat = null;
            ActiveFleetRecord named = null;

            foreach (var fleet in data.fleets)
            {
                if (fleet == null)
                    continue;

                if (!string.IsNullOrEmpty(preferred) && fleet.fleetName == preferred)
                    named = fleet;

                if (fleet.sceneName == ActiveFleets.CombatScene && combat == null)
                    combat = fleet;
            }

            return combat ?? named;
        }

        private static List<ActiveFleetShipRecord> ShipsInOrder(ActiveFleetRecord fleet)
        {
            var ordered = new List<ActiveFleetShipRecord>();
            if (fleet?.ships == null)
                return ordered;

            ActiveFleetShipRecord flagship = null;
            foreach (var ship in fleet.ships)
            {
                if (ship == null || string.IsNullOrEmpty(ship.shipId))
                    continue;

                if (!string.IsNullOrEmpty(fleet.flagshipShipId) && ship.shipId == fleet.flagshipShipId)
                    flagship = ship;
                else
                    ordered.Add(ship);
            }

            if (flagship != null)
                ordered.Insert(0, flagship);

            return ordered;
        }

        private void ApplySelection()
        {
            for (int i = 0; i < ShipSlotCount; i++)
            {
                var border = _selected[i] ? RowSelected : RowBorder;
                var row = _shipRows[i];
                row.style.borderTopColor = border;
                row.style.borderRightColor = border;
                row.style.borderBottomColor = border;
                row.style.borderLeftColor = border;
            }
        }

        private void OnEndBattleClicked()
        {
            HUDMenuButtonsEventBus.TriggerHUDEnterWorldClicked();
        }

        private void BindCombatMechanisms()
        {
        }

        private readonly List<Health> _watched = new List<Health>();

        private void BindFleetHealth()
        {
            UnbindFleetHealth();

            var ships = FleetManager.Instance != null ? FleetManager.Instance.activeShips : null;
            if (ships == null)
                return;

            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship == null)
                    continue;

                var health = ship.GetComponent<Health>();
                if (health == null)
                    continue;

                health.OnHealthChanged.AddListener(OnShipHealthChanged);
                _watched.Add(health);
            }

            PaintShipHealth();
        }

        private void UnbindFleetHealth()
        {
            for (int i = 0; i < _watched.Count; i++)
            {
                if (_watched[i] != null)
                    _watched[i].OnHealthChanged.RemoveListener(OnShipHealthChanged);
            }

            _watched.Clear();
        }

        private void OnShipHealthChanged(float _)
        {
            PaintShipHealth();
        }

        private void PaintShipHealth()
        {
            for (int i = 0; i < ShipSlotCount; i++)
            {
                if (_shipFills[i] == null || string.IsNullOrEmpty(_shipIds[i]))
                    continue;

                float current = 0f;
                float max = 0f;
                if (!ShipHealthFile.TryGet(_shipIds[i], out current, out max) || max <= 0f)
                {
                    _shipFills[i].style.width = Length.Percent(0);
                    continue;
                }

                _shipFills[i].style.width = Length.Percent(current / max * 100f);
            }
        }
    }
}

