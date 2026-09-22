using System;
using System.Collections.Generic;
using _Project.Scripts.Fleet;
using _Project.Scripts.Harbour.Economy;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using _Project.Scripts.SceneManagement;
using _Project.Scripts.UI.IMGUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.Data.HUDData
{
    public class DockHUD : MonoBehaviour
    {
        [SerializeField] private int worldSceneGroupIndex = 1;
        
        private const int FleetSize = 5;
        private const int FlagShipIndex = 2;
        
        [Header("Wallet")]
        [SerializeField] private ResourceWalletHolder walletHolder;

        [Header("Slot Visuals (assign later)")]
        [SerializeField] private Sprite hexSlotSprite;

        [Header("Repair (assign later)")]
        [SerializeField] private Sprite coinSprite;

        [SerializeField] private BuiltShipInventory builtShipInventory;
        private readonly ShipBlueprint[] _slotShips = new ShipBlueprint[FleetSize];
        public event Action OnCloseRequested;

        private VisualElement _root;
        private VisualElement _dockRoot;
        private VisualElement _dockPanel;
        private VisualElement _shipsOverlay;
        private int _pendingDockSlot = -1;
        private int _highlightedShipIndex = -1;
        private Label _inspectHealthValue;
        private Label _inspectShipName;
        private Label _inspectWeight;
        private Label _inspectFuel;
        private Label _inspectCargo;
        private readonly List<VisualElement> _shipListRows = new();

        
        
        private readonly string[] _placeholderShipNames =
        {
            "Gun Boat Alpha",
            "Skirmisher Tide",
            "HammerHead Vale"
        };
        private readonly bool[] _slotOccupied = new bool[FleetSize];
        private readonly VisualElement[] _slotFrames = new VisualElement[FleetSize];
        private readonly Button[] _slotButtons = new Button[FleetSize];
        private Label _selectedHealthLabel;
        private Label _selectedHealthValue;
        private Label _selectedShipName;
        private Label _selectedWeight;
        private Label _selectedFuel;
        private Label _selectedCargo;

        private static readonly string[] MaterialNames =
        {
            "Oil", "Iron", "Steel",
            "Energy", "Aluminium", "Lumber",
            "Alloy", "Cloth", "Uranium"
        };

        public bool IsOpen => _dockRoot != null && _dockRoot.parent != null;

        
        
        private void RefreshWalletNumbers(VisualElement root, string labelPrefix)
        {
            if (root == null || walletHolder?.Wallet == null) return;

            string[] ids =
            {
                "Oil", "Iron", "Steel",
                "Energy", "Aluminium", "Lumber",
                "Alloy", "Cloth", "Uranium"
            };

            foreach (var id in ids)
            {
                var lab = root.Q<Label>($"{labelPrefix}{id}");
                if (lab != null)
                    lab.text = walletHolder.Wallet.Get(id).ToString();
            }
        }

        public void OpenDock(VisualElement root)
        {
            if (root == null)
            {
                Debug.LogError("DockHUD.OpenDock: root is null.");
                return;
            }

            _root = root;
            CloseDock();

            _dockRoot = new VisualElement { name = "DockRoot" };
            _dockRoot.style.position = Position.Absolute;
            _dockRoot.style.top = 0;
            _dockRoot.style.left = 0;
            _dockRoot.style.right = 240;
            _dockRoot.style.bottom = 12;
            _dockRoot.style.justifyContent = Justify.Center;
            _dockRoot.style.alignItems = Align.Center;
            _dockRoot.RegisterCallback<GeometryChangedEvent>(OnDockRootResized);

            _dockPanel = new VisualElement { name = "DockPanel" };
            _dockPanel.style.flexDirection = FlexDirection.Row;
            _dockPanel.style.alignItems = Align.Stretch;
            _dockPanel.style.flexShrink = 0;
            _dockPanel.style.paddingTop = 12;
            _dockPanel.style.paddingBottom = 12;
            _dockPanel.style.paddingLeft = 16;
            _dockPanel.style.paddingRight = 16;
            _dockPanel.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50));

            _dockPanel.Add(CreateSideFrame("DockLeftFrame"));

            var main = CreateMainFrame();
            main.style.flexGrow = 1;
            main.style.flexShrink = 1;
            main.style.marginBottom = 0;
            main.style.marginLeft = 8;
            main.style.marginRight = 8;
            _dockPanel.Add(main);

            _dockPanel.Add(CreateFleetStoragePanel());

            _dockRoot.Add(_dockPanel);
            _root.Add(_dockRoot);
            
            
            RefreshAllSlotButtons();

            _dockPanel.schedule.Execute(FitDockToScreen);
        }

        public void CloseDock()
        {
            CloseMyShipsPanel();
            if (_dockRoot == null) return;
            _dockRoot.UnregisterCallback<GeometryChangedEvent>(OnDockRootResized);
            _dockRoot.RemoveFromHierarchy();
            _dockRoot = null;
            _dockPanel = null;
        }
        
        private void OpenMyShipsPanel()
        {
            if (_dockRoot == null) return;

            CloseMyShipsPanel();
            _highlightedShipIndex = -1;
            _shipListRows.Clear();
            
            // if (builtShipInventory != null && builtShipInventory.ships.Count > 0)
            //     HighlightShip(0);

            _shipsOverlay = new VisualElement { name = "MyShipsOverlay" };
            _shipsOverlay.style.position = Position.Absolute;
            _shipsOverlay.style.top = 0;
            _shipsOverlay.style.left = 0;
            _shipsOverlay.style.right = 0;
            _shipsOverlay.style.bottom = 0;
            _shipsOverlay.pickingMode = PickingMode.Position;

            var dimmer = new VisualElement { name = "MyShipsDimmer" };
            dimmer.style.position = Position.Absolute;
            dimmer.style.top = 0;
            dimmer.style.left = 0;
            dimmer.style.right = 0;
            dimmer.style.bottom = 0;
            dimmer.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            dimmer.pickingMode = PickingMode.Position;
            _shipsOverlay.Add(dimmer);

            var card = new VisualElement { name = "MyShipsPanel" };
            card.style.position = Position.Absolute;
            card.style.left = Length.Percent(12f);
            card.style.right = Length.Percent(12f);
            card.style.top = Length.Percent(8f);
            card.style.bottom = Length.Percent(8f);
            card.style.backgroundColor = new Color(0.06f, 0.10f, 0.22f, 0.98f);
            card.style.borderTopWidth = 2;
            card.style.borderRightWidth = 2;
            card.style.borderBottomWidth = 2;
            card.style.borderLeftWidth = 2;
            card.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
            card.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
            card.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            card.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
            card.style.borderTopLeftRadius = 10;
            card.style.borderTopRightRadius = 10;
            card.style.borderBottomLeftRadius = 10;
            card.style.borderBottomRightRadius = 10;
            card.style.paddingTop = 12;
            card.style.paddingBottom = 16;
            card.style.paddingLeft = 16;
            card.style.paddingRight = 16;
            card.style.flexDirection = FlexDirection.Column;
            card.pickingMode = PickingMode.Position;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 12;

            var title = new Label("My Ships");
            title.style.fontSize = 22;
            title.style.color = Color.cyan;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.width = 36;
            closeBtn.style.height = 36;
            closeBtn.style.fontSize = 18;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.style.borderTopLeftRadius = 18;
            closeBtn.style.borderTopRightRadius = 18;
            closeBtn.style.borderBottomLeftRadius = 18;
            closeBtn.style.borderBottomRightRadius = 18;
            closeBtn.clicked += CloseMyShipsPanel;
            header.Add(closeBtn);
            card.Add(header);

            var body = new VisualElement { name = "MyShipsBody" };
            body.style.flexDirection = FlexDirection.Row;
            body.style.flexGrow = 1;
            body.style.minHeight = 280;
            body.Add(CreateMyShipsList());
            body.Add(CreateMyShipsInspectPane());
            card.Add(body);

            var actions = new VisualElement { name = "MyShipsActions" };
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.justifyContent = Justify.FlexEnd;
            actions.style.marginTop = 14;

            var cancelBtn = new Button { text = "Cancel" };
            StyleMyShipsActionButton(cancelBtn, new Color(0.18f, 0.22f, 0.38f));
            cancelBtn.clicked += CloseMyShipsPanel;
            actions.Add(cancelBtn);

            var confirmBtn = new Button { text = "Confirm" };
            StyleMyShipsActionButton(confirmBtn, new Color(0.12f, 0.42f, 0.38f));
            confirmBtn.style.marginLeft = 10;
            confirmBtn.clicked += ConfirmHighlightedShip;
            actions.Add(confirmBtn);

            card.Add(actions);
            _shipsOverlay.Add(card);
            _dockRoot.Add(_shipsOverlay);

            if (_placeholderShipNames.Length > 0)
                HighlightShip(0);
        }

        private VisualElement CreateMyShipsList()
        {
            var list = new VisualElement { name = "MyShipsList" };
            list.style.width = Length.Percent(42f);
            list.style.flexShrink = 0;
            list.style.marginRight = 12;
            list.style.paddingTop = 8;
            list.style.paddingBottom = 8;
            list.style.paddingLeft = 8;
            list.style.paddingRight = 8;
            list.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            list.style.borderTopWidth = 1;
            list.style.borderRightWidth = 1;
            list.style.borderBottomWidth = 1;
            list.style.borderLeftWidth = 1;
            list.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            list.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            list.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            list.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            list.style.borderTopLeftRadius = 8;
            list.style.borderTopRightRadius = 8;
            list.style.borderBottomLeftRadius = 8;
            list.style.borderBottomRightRadius = 8;

            var listTitle = new Label("Built Ships");
            listTitle.style.fontSize = 14;
            listTitle.style.color = new Color(0.7f, 0.85f, 1f);
            listTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
            listTitle.style.marginBottom = 8;
            list.Add(listTitle);

            if (builtShipInventory == null || builtShipInventory.ships.Count == 0)
            {
                var empty = new Label("No ships built");
                empty.style.color = Color.white;
                empty.style.unityTextAlign = TextAnchor.MiddleCenter;
                empty.style.marginTop = 20;
                list.Add(empty);
                return list;
            }

            for (int i = 0; i < builtShipInventory.ships.Count; i++)
            {
                int captured = i;
                var ship = builtShipInventory.ships[i];

                var row = new Button();
                row.style.height = 72;
                row.style.marginBottom = 6;
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.backgroundColor = new Color(0.14f, 0.18f, 0.34f);
                row.style.borderTopLeftRadius = 6;
                row.style.borderTopRightRadius = 6;
                row.style.borderBottomLeftRadius = 6;
                row.style.borderBottomRightRadius = 6;
                row.clicked += () => HighlightShip(captured);

                var hex = new VisualElement();
                hex.style.width = 56;
                hex.style.height = 56;
                hex.style.marginLeft = 6;
                hex.style.marginRight = 10;
                hex.style.flexShrink = 0;
                hex.style.alignItems = Align.Center;
                hex.style.justifyContent = Justify.Center;
                ApplyHexSprite(hex);

                var portrait = ship != null && ship.storageImage != null
                    ? ship.storageImage
                    : ship?.hull?.hullImage;
                if (portrait != null)
                {
                    var img = new VisualElement();
                    img.style.width = 36;
                    img.style.height = 36;
                    img.style.backgroundImage = new StyleBackground(portrait);
                    img.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    hex.Add(img);
                }

                row.Add(hex);

                var nameLabel = new Label(ship != null ? ship.shipName : "Unknown");
                nameLabel.style.fontSize = 14;
                nameLabel.style.color = Color.white;
                nameLabel.style.flexGrow = 1;
                row.Add(nameLabel);

                list.Add(row);
                _shipListRows.Add(row);
            }

            return list;
        }

        private VisualElement CreateMyShipsInspectPane()
        {
            var pane = new VisualElement { name = "MyShipsInspect" };
            pane.style.flexGrow = 1;
            pane.style.alignItems = Align.Center;
            pane.style.justifyContent = Justify.FlexStart;
            pane.style.paddingTop = 16;
            pane.style.paddingBottom = 16;
            pane.style.paddingLeft = 16;
            pane.style.paddingRight = 16;
            pane.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            pane.style.borderTopWidth = 2;
            pane.style.borderRightWidth = 2;
            pane.style.borderBottomWidth = 2;
            pane.style.borderLeftWidth = 2;
            pane.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
            pane.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
            pane.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            pane.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
            pane.style.borderTopLeftRadius = 8;
            pane.style.borderTopRightRadius = 8;
            pane.style.borderBottomLeftRadius = 8;
            pane.style.borderBottomRightRadius = 8;

            var healthHeader = new Label("Health");
            healthHeader.style.fontSize = 13;
            healthHeader.style.color = new Color(0.7f, 0.85f, 1f);
            healthHeader.style.unityTextAlign = TextAnchor.MiddleCenter;
            healthHeader.style.marginBottom = 4;
            pane.Add(healthHeader);

            _inspectHealthValue = new Label("3000 / 5000");
            _inspectHealthValue.style.fontSize = 20;
            _inspectHealthValue.style.color = Color.white;
            _inspectHealthValue.style.unityFontStyleAndWeight = FontStyle.Bold;
            _inspectHealthValue.style.unityTextAlign = TextAnchor.MiddleCenter;
            _inspectHealthValue.style.marginBottom = 14;
            pane.Add(_inspectHealthValue);

            _inspectShipName = new Label("No Ship Selected");
            _inspectShipName.style.fontSize = 18;
            _inspectShipName.style.color = Color.cyan;
            _inspectShipName.style.unityFontStyleAndWeight = FontStyle.Bold;
            _inspectShipName.style.unityTextAlign = TextAnchor.MiddleCenter;
            _inspectShipName.style.marginBottom = 12;
            pane.Add(_inspectShipName);

            _inspectWeight = AddInspectLine(pane, "Weight", "0 t");
            _inspectFuel = AddInspectLine(pane, "Fuel", "0");
            _inspectCargo = AddInspectLine(pane, "Cargo Hold", "0 / 0");

            return pane;
        }

        private static Label AddInspectLine(VisualElement parent, string title, string value)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.marginBottom = 6;

            var label = new Label($"{title}:  {value}");
            label.style.fontSize = 14;
            label.style.color = Color.white;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            row.Add(label);
            parent.Add(row);
            return label;
        }

        private static void StyleMyShipsActionButton(Button btn, Color bg)
        {
            btn.style.minWidth = 120;
            btn.style.height = 40;
            btn.style.fontSize = 15;
            btn.style.color = Color.white;
            btn.style.backgroundColor = bg;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }

        private void HighlightShip(int index)
        {
            if (builtShipInventory?.ships == null) return;
            if (index < 0 || index >= builtShipInventory.ships.Count) return;
            if (_inspectShipName == null || _inspectHealthValue == null) return;

            var ship = builtShipInventory.ships[index];
            if (ship == null) return;

            _highlightedShipIndex = index;

            for (int i = 0; i < _shipListRows.Count; i++)
                _shipListRows[i].style.backgroundColor = i == index
                    ? new Color(0.20f, 0.38f, 0.62f)
                    : new Color(0.14f, 0.18f, 0.34f);

            _inspectShipName.text = ship.shipName;
            _inspectHealthValue.text = $"{ship.totalHealth:0} / {ship.totalHealth:0}";
            if (_inspectWeight != null) _inspectWeight.text = $"Weight:  {ship.totalWeight:0} t";
            if (_inspectFuel != null) _inspectFuel.text = "Fuel:  0";
            if (_inspectCargo != null) _inspectCargo.text = "Cargo Hold:  0 / 0";
        }

        private void ConfirmHighlightedShip()
        {
            if (_highlightedShipIndex < 0 || _pendingDockSlot < 0)
            {
                Debug.Log("My Ships: nothing to confirm.");
                return;
            }

            if (builtShipInventory == null ||
                _highlightedShipIndex >= builtShipInventory.ships.Count)
                return;

            var ship = builtShipInventory.ships[_highlightedShipIndex];
            if (ship == null) return;

            int slot = _pendingDockSlot;
            _slotOccupied[slot] = true;
            _slotShips[slot] = ship;
            RefreshSlotButton(slot);
            ApplyDockSlotPortrait(slot);

            Debug.Log($"My Ships: '{ship.shipName}' -> Dock slot {slot}");
            CloseMyShipsPanel();
        }
        
        private void ApplyDockSlotPortrait(int index)
        {
            var frame = _slotFrames[index];
            if (frame == null) return;

            const string childName = "SlotPortrait";
            var portrait = frame.Q(childName);
            if (portrait == null)
            {
                portrait = new VisualElement { name = childName };
                portrait.style.position = Position.Absolute;
                portrait.style.left = Length.Percent(18);
                portrait.style.right = Length.Percent(18);
                portrait.style.top = Length.Percent(18);
                portrait.style.bottom = Length.Percent(18);
                portrait.pickingMode = PickingMode.Ignore;
                frame.Add(portrait);
            }

            var ship = _slotShips[index];
            var sprite = ship != null
                ? (ship.storageImage != null ? ship.storageImage : ship.hull?.hullImage)
                : null;

            if (_slotOccupied[index] && sprite != null)
            {
                portrait.style.backgroundImage = new StyleBackground(sprite);
                portrait.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                portrait.style.display = DisplayStyle.Flex;
            }
            else
            {
                portrait.style.backgroundImage = StyleKeyword.None;
                portrait.style.display = DisplayStyle.None;
            }
        }

        private void CloseMyShipsPanel()
        {
            if (_shipsOverlay == null) return;
            _shipsOverlay.RemoveFromHierarchy();
            _shipsOverlay = null;
            _pendingDockSlot = -1;
        }

        private void OnDockRootResized(GeometryChangedEvent evt)
        {
            if (Mathf.Approximately(evt.newRect.width, evt.oldRect.width) &&
                Mathf.Approximately(evt.newRect.height, evt.oldRect.height))
                return;

            FitDockToScreen();
        }

        private void FitDockToScreen()
        {
            if (_dockRoot == null || _dockPanel == null) return;

            var host = _dockRoot.contentRect;
            var content = _dockPanel.layout;
            if (host.width < 8f || host.height < 8f) return;
            if (content.width < 8f || content.height < 8f) return;

            float scale = Mathf.Min(
                (host.width * 0.88f) / content.width,
                (host.height * 0.88f) / content.height);

            scale = Mathf.Clamp(scale, 0.4f, 1f);
            _dockPanel.style.scale = new Scale(new Vector3(scale, scale, 1f));
        }

        private void RequestClose()
        {
            CloseDock();
            OnCloseRequested?.Invoke();
        }

        private VisualElement CreateOuterFrame(string name, Length height)
        {
            var frame = new VisualElement { name = name };
            frame.style.height = height;
            frame.style.flexShrink = 0;
            frame.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            frame.style.borderTopWidth = 1;
            frame.style.borderRightWidth = 1;
            frame.style.borderBottomWidth = 1;
            frame.style.borderLeftWidth = 1;
            frame.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderTopLeftRadius = 8;
            frame.style.borderTopRightRadius = 8;
            frame.style.borderBottomLeftRadius = 8;
            frame.style.borderBottomRightRadius = 8;
            frame.style.marginBottom = 8;
            return frame;
        }
        
        private const int StorageSlotCount = 12;

        private VisualElement CreateSideFrame(string name)
        {
            var frame = new VisualElement { name = name };
            frame.style.width = 330;
            frame.style.flexShrink = 0;
            frame.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            frame.style.borderTopWidth = 1;
            frame.style.borderRightWidth = 1;
            frame.style.borderBottomWidth = 1;
            frame.style.borderLeftWidth = 1;
            frame.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderTopLeftRadius = 8;
            frame.style.borderTopRightRadius = 8;
            frame.style.borderBottomLeftRadius = 8;
            frame.style.borderBottomRightRadius = 8;
            frame.style.marginRight = 8;
            return frame;
        }

        private VisualElement CreateFleetStoragePanel()
        {
            var panel = new VisualElement { name = "FleetStoragePanel" };
            panel.style.width = 330;
            panel.style.flexShrink = 0;
            panel.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            panel.style.borderTopWidth = 1;
            panel.style.borderRightWidth = 1;
            panel.style.borderBottomWidth = 1;
            panel.style.borderLeftWidth = 1;
            panel.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            panel.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            panel.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            panel.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            panel.style.borderTopLeftRadius = 8;
            panel.style.borderTopRightRadius = 8;
            panel.style.borderBottomLeftRadius = 8;
            panel.style.borderBottomRightRadius = 8;
            panel.style.paddingTop = 10;
            panel.style.paddingBottom = 10;
            panel.style.paddingLeft = 10;
            panel.style.paddingRight = 10;
            panel.style.flexDirection = FlexDirection.Column;

            var header = new Label("Fleet Storage");
            header.style.width = Length.Percent(100f);
            header.style.fontSize = 16;
            header.style.color = Color.cyan;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.alignSelf = Align.Center;
            header.style.marginBottom = 10;
            panel.Add(header);

            var grid = new VisualElement { name = "FleetStorageGrid" };
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.Center;
            grid.style.flexGrow = 1;

            for (int i = 0; i < StorageSlotCount; i++)
                grid.Add(CreateStorageSlot(i));

            panel.Add(grid);
            return panel;
        }

        private VisualElement CreateStorageSlot(int index)
        {
            var col = new VisualElement { name = $"StorageSlot_{index}" };
            col.style.alignItems = Align.Center;
            col.style.width = Length.Percent(31f);
            col.style.marginBottom = 10;
            col.style.marginLeft = 4;
            col.style.marginRight = 4;
            

            var frame = new VisualElement { name = $"StorageFrame_{index}" };
            frame.style.width = 72;
            frame.style.height = 72;
            frame.style.backgroundColor = new Color(0.12f, 0.16f, 0.30f, 0.95f);
            frame.style.borderTopWidth = 2;
            frame.style.borderRightWidth = 2;
            frame.style.borderBottomWidth = 2;
            frame.style.borderLeftWidth = 2;
            frame.style.borderTopColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.borderRightColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.borderBottomColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.borderLeftColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            col.Add(frame);

            var nameField = new TextField { name = $"StorageName_{index}", value = "" };
            nameField.SetEnabled(false);
            nameField.style.width = 88;
            nameField.style.marginTop = 6;
            nameField.style.fontSize = 11;
            col.Add(nameField);

            return col;
        }

        private VisualElement CreateMainFrame()
        {
            var main = new VisualElement { name = "DockMainFrame" };
            main.style.flexGrow = 1;
            main.style.flexShrink = 0;
            main.style.backgroundColor = new Color(0.06f, 0.10f, 0.22f, 0.96f);
            main.style.borderTopWidth = 2;
            main.style.borderRightWidth = 2;
            main.style.borderBottomWidth = 2;
            main.style.borderLeftWidth = 2;
            main.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
            main.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
            main.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            main.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
            main.style.borderTopLeftRadius = 10;
            main.style.borderTopRightRadius = 10;
            main.style.borderBottomLeftRadius = 10;
            main.style.borderBottomRightRadius = 10;
            main.style.paddingTop = 12;
            main.style.paddingBottom = 16;
            main.style.paddingLeft = 16;
            main.style.paddingRight = 16;
            main.style.flexDirection = FlexDirection.Column;
            main.style.justifyContent = Justify.FlexStart;
            main.style.marginBottom = 8;

            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 8;

            var title = new Label("Dock");
            title.style.fontSize = 22;
            title.style.color = Color.cyan;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerRow.Add(title);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.width = 36;
            closeBtn.style.height = 36;
            closeBtn.style.fontSize = 18;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.style.borderTopLeftRadius = 18;
            closeBtn.style.borderTopRightRadius = 18;
            closeBtn.style.borderBottomLeftRadius = 18;
            closeBtn.style.borderBottomRightRadius = 18;
            closeBtn.clicked += RequestClose;
            headerRow.Add(closeBtn);
            main.Add(headerRow);
            

            main.Add(CreateLaunchButton());
            main.Add(CreateFleetRow());
            main.Add(CreateCostsBlock());
            main.Add(CreateRepairBlock());
            main.Add(CreateSelectedShipInfoPanel());
            return main;
        }
        private VisualElement CreateLaunchButton()
        {
            var btn = new Button { text = "Launch Fleet" };
            btn.style.height = 48;
            btn.style.marginTop = 4;
            btn.style.marginBottom = 12;
            btn.style.fontSize = 18;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.color = Color.white;
            btn.style.backgroundColor = new Color(0.18f, 0.42f, 0.22f);
            btn.style.borderTopLeftRadius = 8;
            btn.style.borderTopRightRadius = 8;
            btn.style.borderBottomLeftRadius = 8;
            btn.style.borderBottomRightRadius = 8;
            btn.clicked += LaunchFleet;
            return btn;
        }

        private void LaunchFleet()
        {
            if (_slotShips[FlagShipIndex] == null)
            {
                Debug.LogWarning("[Dock] Put a ship in the Flag Ship slot first.");
                return;
            }

            PendingLaunch.Capture(_slotShips, FlagShipIndex);

            CloseDock();
            GetComponent<HarbourHUD>()?.HideAllUI();

            if (IMGUILoadingOverlay.Instance != null)
                IMGUILoadingOverlay.Instance.TriggerLoadingScreen();

            if (SceneLoader.Instance != null)
                _ = SceneLoader.Instance.BeginSceneTransition(worldSceneGroupIndex);
            else
                Debug.LogError("[Dock] SceneLoader missing.");
        }
        
        private VisualElement CreateSelectedShipInfoPanel()
        {
            var frame = new VisualElement { name = "SelectedShipInfoFrame" };
            frame.style.flexGrow = 1;
            frame.style.flexShrink = 0;
            frame.style.minHeight = 140;
            frame.style.marginTop = 16;
            frame.style.alignItems = Align.Center;
            frame.style.justifyContent = Justify.FlexStart;
            frame.style.paddingTop = 16;
            frame.style.paddingBottom = 16;
            frame.style.paddingLeft = 20;
            frame.style.paddingRight = 20;
            frame.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            frame.style.borderTopWidth = 2;
            frame.style.borderRightWidth = 2;
            frame.style.borderBottomWidth = 2;
            frame.style.borderLeftWidth = 2;
            frame.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
            frame.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
            frame.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            frame.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
            frame.style.borderTopLeftRadius = 8;
            frame.style.borderTopRightRadius = 8;
            frame.style.borderBottomLeftRadius = 8;
            frame.style.borderBottomRightRadius = 8;

            _selectedHealthLabel = new Label("Health");
            _selectedHealthLabel.style.fontSize = 13;
            _selectedHealthLabel.style.color = new Color(0.7f, 0.85f, 1f);
            _selectedHealthLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _selectedHealthLabel.style.marginBottom = 4;
            frame.Add(_selectedHealthLabel);

            _selectedHealthValue = new Label("3000 / 5000");
            _selectedHealthValue.style.fontSize = 20;
            _selectedHealthValue.style.color = Color.white;
            _selectedHealthValue.style.unityFontStyleAndWeight = FontStyle.Bold;
            _selectedHealthValue.style.unityTextAlign = TextAnchor.MiddleCenter;
            _selectedHealthValue.style.marginBottom = 14;
            frame.Add(_selectedHealthValue);

            _selectedShipName = new Label("No Ship Selected");
            _selectedShipName.style.fontSize = 18;
            _selectedShipName.style.color = Color.cyan;
            _selectedShipName.style.unityFontStyleAndWeight = FontStyle.Bold;
            _selectedShipName.style.unityTextAlign = TextAnchor.MiddleCenter;
            _selectedShipName.style.marginBottom = 12;
            frame.Add(_selectedShipName);

            _selectedWeight = AddInfoLine(frame, "Weight", "0 t");
            _selectedFuel = AddInfoLine(frame, "Fuel", "0");
            _selectedCargo = AddInfoLine(frame, "Cargo Hold", "0 / 0");

            return frame;
        }

        private static Label AddInfoLine(VisualElement parent, string title, string value)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.marginBottom = 6;

            var label = new Label($"{title}:  {value}");
            label.name = $"Info_{title}";
            label.style.fontSize = 14;
            label.style.color = Color.white;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            row.Add(label);
            parent.Add(row);
            return label;
        }

        private VisualElement CreateFleetRow()
        {
            var row = new VisualElement { name = "FleetRow" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.alignItems = Align.FlexEnd;
            row.style.marginBottom = 16;

            for (int i = 0; i < FleetSize; i++)
                row.Add(CreateSlotColumn(i));

            return row;
        }

        private VisualElement CreateSlotColumn(int index)
        {
            var col = new VisualElement { name = $"FleetSlot_{index}" };
            col.style.alignItems = Align.Center;
            col.style.marginLeft = 10;
            col.style.marginRight = 10;

            var flagLabel = new Label(index == FlagShipIndex ? "Flag Ship" : " ");
            flagLabel.style.height = 36;
            flagLabel.style.minWidth = 92;
            flagLabel.style.fontSize = 13;
            flagLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            flagLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            flagLabel.style.color = index == FlagShipIndex ? Color.cyan : Color.clear;
            flagLabel.style.marginBottom = 6;
            col.Add(flagLabel);

            var frame = new VisualElement { name = $"SlotFrame_{index}" };
            frame.style.width = 88;
            frame.style.height = 88;
            frame.style.backgroundColor = new Color(0.12f, 0.16f, 0.30f, 0.95f);
            frame.style.borderTopWidth = 2;
            frame.style.borderRightWidth = 2;
            frame.style.borderBottomWidth = 2;
            frame.style.borderLeftWidth = 2;
            frame.style.borderTopColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.borderRightColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.borderBottomColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.borderLeftColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.position = Position.Relative;
            frame.style.overflow = Overflow.Hidden;
            
            ApplyHexSprite(frame);

            _slotFrames[index] = frame;
            col.Add(frame);

            var btn = new Button();
            btn.style.minWidth = 92;
            btn.style.height = 36;
            btn.style.marginTop = 8;
            btn.style.fontSize = 13;
            btn.style.color = Color.white;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;

            int captured = index;
            btn.clicked += () => OnSlotButtonClicked(captured);
            _slotButtons[index] = btn;
            col.Add(btn);

            return col;
        }

        private void ApplyHexSprite(VisualElement frame)
        {
            if (hexSlotSprite == null) return;

            frame.style.backgroundImage = new StyleBackground(hexSlotSprite);
            frame.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            frame.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            frame.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
        }

        private void OnSlotButtonClicked(int index)
        {
            if (_slotOccupied[index])
            {
                // _slotOccupied[index] = false;
                // RefreshSlotButton(index);
                Debug.Log($"DockHUD: slot {index} emptied");
                return;
            }

            _pendingDockSlot = index;
            OpenMyShipsPanel();
        }

        private void RefreshAllSlotButtons()
        {
            for (int i = 0; i < FleetSize; i++)
                RefreshSlotButton(i);
        }

        private void RefreshSlotButton(int index)
        {
            if (_slotButtons[index] == null) return;
            _slotButtons[index].text = _slotOccupied[index] ? "Remove Ship" : "Add Ship";
        }

        private VisualElement CreateCostsBlock()
        {
            var block = new VisualElement { name = "CostsBlock" };
            block.style.alignItems = Align.Center;
            block.style.marginBottom = 36;
            block.style.flexShrink = 0;

            var header = new Label("Costs");
            header.style.fontSize = 18;
            header.style.color = Color.white;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.marginBottom = 10;
            block.Add(header);

            var grid = new VisualElement { name = "MaterialsGrid" };
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.Center;
            grid.style.width = Length.Percent(90f);

            for (int i = 0; i < MaterialNames.Length; i++)
                grid.Add(CreateMaterialCell(MaterialNames[i]));

            block.Add(grid);
            return block;
        }

        private VisualElement CreateMaterialCell(string materialName)
        {
            var cell = new VisualElement { name = $"Material_{materialName}" };
            cell.style.width = Length.Percent(31f);
            cell.style.flexDirection = FlexDirection.Row;
            cell.style.alignItems = Align.Center;
            cell.style.marginBottom = 8;
            cell.style.marginRight = 6;
            cell.style.paddingTop = 4;
            cell.style.paddingBottom = 4;
            cell.style.paddingLeft = 6;
            cell.style.paddingRight = 6;

            var check = new Toggle { name = $"Check_{materialName}", value = false };
            check.style.marginRight = 6;
            cell.Add(check);

            var amount = new Label("0");
            amount.name = $"Amount_{materialName}";
            amount.style.flexGrow = 1;
            amount.style.fontSize = 14;
            amount.style.color = Color.white;
            amount.style.borderBottomWidth = 1;
            amount.style.borderBottomColor = new Color(0.75f, 0.85f, 1f, 0.85f);
            amount.style.paddingBottom = 2;
            amount.style.marginRight = 8;
            cell.Add(amount);
            
            var def = walletHolder != null && walletHolder.Catalog != null
                ? walletHolder.Catalog.Get(materialName)
                : null;

            var nameLab = new Label(def != null && !string.IsNullOrEmpty(def.displayName)
                ? def.displayName
                : materialName);
            nameLab.name = $"CostName_{materialName}";
            nameLab.style.fontSize = 11;
            nameLab.style.color = new Color(0.7f, 0.85f, 1f);
            nameLab.style.flexShrink = 0;
            nameLab.style.marginRight = 6;
            cell.Add(nameLab);
            

            var icon = new VisualElement { name = $"Icon_{materialName}" };
            icon.style.width = 22;
            icon.style.height = 22;
            icon.style.backgroundColor = new Color(0.25f, 0.32f, 0.48f, 0.9f);
            icon.style.borderTopLeftRadius = 4;
            icon.style.borderTopRightRadius = 4;
            icon.style.borderBottomLeftRadius = 4;
            icon.style.borderBottomRightRadius = 4;
            icon.style.flexShrink = 0;
            
            
            icon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            icon.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            icon.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            
            cell.Add(icon);

            
            amount.text = "0";

            // var def = walletHolder != null && walletHolder.Catalog != null
            //     ? walletHolder.Catalog.Get(materialName)
            //     : null;
            if (def?.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(def.icon);
                icon.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
                icon.style.backgroundColor = Color.clear;
            }

            return cell;
            
            
        }

        private VisualElement CreateRepairBlock()
        {
            var row = new VisualElement { name = "RepairBlock" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.alignItems = Align.FlexStart;
            row.style.marginTop = 52;
            row.style.flexShrink = 0;

            row.Add(CreateBeginRepairsColumn());
            row.Add(CreateInstantRepairColumn());
            return row;
        }

        private VisualElement CreateBeginRepairsColumn()
        {
            var col = new VisualElement();
            col.style.alignItems = Align.Center;
            col.style.marginRight = 40;

            var btn = new Button { text = "Begin Repairs" };
            StyleRepairButton(btn);
            col.Add(btn);

            var timerBox = new VisualElement { name = "RepairTimerBox" };
            StyleValueBox(timerBox);
            timerBox.style.marginTop = 8;

            var timerLabel = new Label("00:00");
            timerLabel.style.fontSize = 16;
            timerLabel.style.color = Color.white;
            timerLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            timerBox.Add(timerLabel);
            col.Add(timerBox);
            return col;
        }

        private VisualElement CreateInstantRepairColumn()
        {
            var col = new VisualElement();
            col.style.alignItems = Align.Center;

            var btn = new Button { text = "Repair Instantly" };
            StyleRepairButton(btn);
            col.Add(btn);

            var costRow = new VisualElement();
            costRow.style.flexDirection = FlexDirection.Row;
            costRow.style.alignItems = Align.Center;
            costRow.style.marginTop = 8;

            var coin = new VisualElement { name = "CoinIcon" };
            coin.style.width = 28;
            coin.style.height = 28;
            coin.style.marginRight = 8;
            coin.style.backgroundColor = new Color(0.55f, 0.45f, 0.12f, 0.95f);
            coin.style.borderTopLeftRadius = 14;
            coin.style.borderTopRightRadius = 14;
            coin.style.borderBottomLeftRadius = 14;
            coin.style.borderBottomRightRadius = 14;
            if (coinSprite != null)
            {
                coin.style.backgroundImage = new StyleBackground(coinSprite);
                coin.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            costRow.Add(coin);

            var costBox = new VisualElement { name = "InstantRepairCostBox" };
            StyleValueBox(costBox);

            var costLabel = new Label("0");
            costLabel.style.fontSize = 16;
            costLabel.style.color = Color.white;
            costLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            costBox.Add(costLabel);
            costRow.Add(costBox);

            col.Add(costRow);
            return col;
        }

        private static void StyleRepairButton(Button btn)
        {
            btn.style.minWidth = 170;
            btn.style.height = 42;
            btn.style.fontSize = 15;
            btn.style.color = Color.white;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }

        private static void StyleValueBox(VisualElement box)
        {
            box.style.minWidth = 110;
            box.style.height = 32;
            box.style.justifyContent = Justify.Center;
            box.style.alignItems = Align.Center;
            box.style.backgroundColor = new Color(0.08f, 0.12f, 0.24f, 0.95f);
            box.style.borderTopWidth = 1;
            box.style.borderRightWidth = 1;
            box.style.borderBottomWidth = 1;
            box.style.borderLeftWidth = 1;
            box.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.6f);
            box.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.6f);
            box.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.6f);
            box.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.6f);
            box.style.borderTopLeftRadius = 4;
            box.style.borderTopRightRadius = 4;
            box.style.borderBottomLeftRadius = 4;
            box.style.borderBottomRightRadius = 4;
        }
    }
}