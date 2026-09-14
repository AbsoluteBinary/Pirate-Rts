using System;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class ShipBuilderHUD : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        private float _hullDrawScale = 1f;
        
        [Header("References")]
        [SerializeField] private HullSelectionHUD hullSelectionHUD;
        
        [Header("Selection HUDs")]
        [SerializeField] private WeaponSelectionHUD weaponSelectionHUD;
        [SerializeField] private ArmourSelectionHUD armourSelectionHUD;
        [SerializeField] private EngineSelectionHUD engineSelectionHUD;
        [SerializeField] private ComponentSelectionHUD componentSelectionHUD;

        [Header("Visuals")]
        [SerializeField] private bool showSlotVisuals = true;

        private VisualElement _shipBuilderPanel;
        private VisualElement _hullCanvas;
        private VisualElement _hullImageElement;
        private VisualElement _slotOverlay;
        private Label _hullNameLabel;
        private Label _statsLabel;

        private HullData _currentHull;
        private ShipLoadout _currentLoadout;
        
        private VisualElement root;

        private void OnEnable()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            panelRenderer.RegisterUIReloadCallback(OnUIReady);
        }

        private void OnDisable()
        {
            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReady);
        }

        private void OnUIReady(PanelRenderer renderer, VisualElement rootElement)
        {
            root = rootElement;
        }

        public void OpenShipBuilder()
        {
            if (root == null) return;

            if (_shipBuilderPanel != null)
                _shipBuilderPanel.RemoveFromHierarchy();

            _shipBuilderPanel = new VisualElement { name = "ShipBuilderHost" };
            _shipBuilderPanel.style.position = Position.Absolute;
            _shipBuilderPanel.style.top = 0;
            _shipBuilderPanel.style.left = 0;
            _shipBuilderPanel.style.right = 0;
            _shipBuilderPanel.style.bottom = 0;
            _shipBuilderPanel.style.justifyContent = Justify.Center;
            _shipBuilderPanel.style.alignItems = Align.Center;

            var card = new VisualElement { name = "ShipBuilderPanel" };
            card.style.width = Length.Percent(82f);
            card.style.height = Length.Percent(86f);
            card.style.maxWidth = 1200;
            card.style.flexDirection = FlexDirection.Column;
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
            card.style.paddingTop = 16;
            card.style.paddingBottom = 16;
            card.style.paddingLeft = 20;
            card.style.paddingRight = 20;

            card.Add(CreateHeader());

            var selectHullBtn = new Button { text = "Select Hull" };
            selectHullBtn.style.fontSize = 16;
            selectHullBtn.style.height = 44;
            selectHullBtn.style.marginBottom = 6;
            selectHullBtn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            selectHullBtn.style.color = Color.white;
            selectHullBtn.style.borderTopLeftRadius = 6;
            selectHullBtn.style.borderTopRightRadius = 6;
            selectHullBtn.style.borderBottomLeftRadius = 6;
            selectHullBtn.style.borderBottomRightRadius = 6;
            selectHullBtn.clicked += () => hullSelectionHUD?.OpenHullSelection();
            card.Add(selectHullBtn);

            _hullNameLabel = new Label("No Hull Selected");
            _hullNameLabel.style.fontSize = 14;
            _hullNameLabel.style.color = Color.white;
            _hullNameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _hullNameLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
            _hullNameLabel.style.borderBottomWidth = 1;
            _hullNameLabel.style.borderBottomColor = Color.white;
            _hullNameLabel.style.paddingBottom = 1;
            _hullNameLabel.style.marginBottom = 10;
            card.Add(_hullNameLabel);

            card.Add(CreateHullPreviewArea());
            card.Add(CreateBuildCostRow());
            card.Add(CreateBuildActionRow());
            card.Add(CreateStatsPanel());

            _shipBuilderPanel.Add(card);
            root.Add(_shipBuilderPanel);
        }

            private VisualElement CreateHullPreviewArea()
            {
                var frame = new VisualElement { name = "HullPreviewFrame" };
                frame.style.flexGrow = 1;
                frame.style.flexShrink = 1;
                frame.style.minHeight = 180;
                frame.style.alignItems = Align.Center;
                frame.style.justifyContent = Justify.Center;
                frame.style.marginBottom = 12;
                frame.style.paddingTop = 8;
                frame.style.paddingBottom = 8;
                frame.style.paddingLeft = 8;
                frame.style.paddingRight = 8;
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

                _hullCanvas = new VisualElement { name = "HullCanvas" };
                _hullCanvas.style.position = Position.Relative;
                _hullCanvas.style.flexShrink = 0;

                _hullImageElement = new VisualElement { name = "HullImage" };
                _hullImageElement.style.position = Position.Absolute;
                _hullImageElement.style.top = 0;
                _hullImageElement.style.left = 0;
                _hullImageElement.style.right = 0;
                _hullImageElement.style.bottom = 0;
                _hullImageElement.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;

                _slotOverlay = new VisualElement { name = "SlotOverlay" };
                _slotOverlay.style.position = Position.Absolute;
                _slotOverlay.style.top = 0;
                _slotOverlay.style.left = 0;
                _slotOverlay.style.right = 0;
                _slotOverlay.style.bottom = 0;

                _hullCanvas.Add(_hullImageElement);
                _hullCanvas.Add(_slotOverlay);
                frame.Add(_hullCanvas);
                frame.RegisterCallback<GeometryChangedEvent>(_ => FitHullCanvas());
                return frame;
            }

            private VisualElement CreateHeader()
            {
                var header = new VisualElement();
                header.style.flexDirection = FlexDirection.Row;
                header.style.alignItems = Align.Center;
                header.style.marginBottom = 12;
                header.style.minHeight = 44;

                var leftPad = new VisualElement();
                leftPad.style.width = 44;
                leftPad.style.height = 44;
                header.Add(leftPad);

                var title = new Label("Build a Ship");
                title.style.flexGrow = 1;
                title.style.fontSize = 26;
                title.style.color = Color.cyan;
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                title.style.unityTextAlign = TextAnchor.MiddleCenter;
                header.Add(title);

                var closeBtn = new Button { text = "✕" };
                closeBtn.style.width = 44;
                closeBtn.style.height = 44;
                closeBtn.style.fontSize = 22;
                closeBtn.style.color = Color.white;
                closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
                closeBtn.style.borderTopLeftRadius = 8;
                closeBtn.style.borderTopRightRadius = 8;
                closeBtn.style.borderBottomLeftRadius = 8;
                closeBtn.style.borderBottomRightRadius = 8;
                closeBtn.clicked += CloseShipBuilder;
                header.Add(closeBtn);

                return header;
            }
        

        public void SetSelectedHull(HullData hull)
        {
            _currentHull = hull;
            _currentLoadout = new ShipLoadout { hull = hull };

            if (hull?.hullImage == null) return;

            _hullNameLabel.text = hull.hullName;

            var tex = hull.hullImage.texture;
            // _hullCanvas.style.width = tex.width;
            // _hullCanvas.style.height = tex.height;

            _hullImageElement.style.width = Length.Percent(100);
            _hullImageElement.style.height = Length.Percent(100);
            _hullImageElement.style.backgroundImage = new StyleBackground(hull.hullImage);
            _hullImageElement.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;

            DrawSlotVisuals();
            UpdateStatsDisplay();
            _hullCanvas.schedule.Execute(FitHullCanvas);
        }
        
        private VisualElement CreateBuildCostRow()
        {
            var block = new VisualElement { name = "BuildCostBlock" };
            block.style.flexShrink = 0;
            block.style.marginBottom = 10;

            var header = new Label("Costs");
            header.style.fontSize = 16;
            header.style.color = Color.cyan;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.marginBottom = 8;
            block.Add(header);

            string[] ids =
            {
                "Oil", "Iron", "Steel",
                "Energy", "Aluminium", "Lumber",
                "Alloy", "Cloth", "Uranium"
            };

            for (int r = 0; r < 3; r++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.Center;
                row.style.marginBottom = 6;

                for (int c = 0; c < 3; c++)
                    row.Add(CreateCostCell(ids[r * 3 + c]));

                block.Add(row);
            }

            return block;
        }

        private VisualElement CreateCostCell(string id)
        {
            var cell = new VisualElement { name = $"CostCell_{id}" };
            cell.style.flexDirection = FlexDirection.Row;
            cell.style.alignItems = Align.Center;
            cell.style.marginLeft = 10;
            cell.style.marginRight = 10;
            cell.style.flexGrow = 1;
            cell.style.maxWidth = 220;

            var check = new Toggle();
            check.value = false;
            check.style.marginRight = 6;
            cell.Add(check);

            var value = new Label("0");
            value.name = $"CostValue_{id}";
            value.style.flexGrow = 1;
            value.style.fontSize = 13;
            value.style.color = Color.white;
            value.style.borderBottomWidth = 1;
            value.style.borderBottomColor = Color.white;
            value.style.paddingBottom = 1;
            
            cell.Add(value);

            var iconBox = new VisualElement();
            iconBox.style.width = 22;
            iconBox.style.height = 22;
            iconBox.style.marginLeft = 8;
            iconBox.style.backgroundColor = new Color(0.16f, 0.22f, 0.40f);
            iconBox.style.borderTopWidth = 1;
            iconBox.style.borderRightWidth = 1;
            iconBox.style.borderBottomWidth = 1;
            iconBox.style.borderLeftWidth = 1;
            iconBox.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.7f);
            iconBox.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.7f);
            iconBox.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.7f);
            iconBox.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.7f);
            cell.Add(iconBox);

            return cell;
        }
        
        private VisualElement CreateBuildActionRow()
        {
            var row = new VisualElement { name = "BuildActionRow" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.marginBottom = 10;
            row.style.flexShrink = 0;

            row.Add(CreateActionButton("Save Build", () =>  Debug.Log("ShipBuilder: Save Build (stub)")));
            row.Add(CreateActionButton("Load Build", () =>  Debug.Log("ShipBuilder: Load Build (stub)")));
            row.Add(CreateActionButton("Build", () =>  Debug.Log("ShipBuilder: Build (stub)")));
            return row;
        }

        private Button CreateActionButton(string text, Action onClick)
        {
            var btn = new Button { text = text };
            btn.style.height = 40;
            btn.style.minWidth = 130;
            btn.style.marginLeft = 6;
            btn.style.marginRight = 6;
            btn.style.fontSize = 14;
            btn.style.color = Color.white;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
            btn.clicked += onClick;
            return btn;
        }

        private void FitHullCanvas()
        {
            if (_hullCanvas == null || _hullCanvas.parent == null) return;
            if (_currentHull?.hullImage == null) return;

            var host = _hullCanvas.parent.contentRect;
            var tex = _currentHull.hullImage.texture;
            if (host.width < 8f || host.height < 8f) return;
            if (tex.width < 1 || tex.height < 1) return;

            _hullDrawScale = Mathf.Min(host.width / tex.width, host.height / tex.height);
            _hullDrawScale = Mathf.Clamp(_hullDrawScale, 0.35f, 1f);

            _hullCanvas.style.scale = StyleKeyword.Null;
            _hullCanvas.style.width = tex.width * _hullDrawScale;
            _hullCanvas.style.height = tex.height * _hullDrawScale;

            DrawSlotVisuals();
        }

        private void DrawSlotVisuals()
        {
            _slotOverlay.Clear();
            if (!showSlotVisuals || _currentHull?.moduleSlots == null) return;

            foreach (var slot in _currentHull.moduleSlots)
            {
                var slotBtn = new Button();
                slotBtn.style.position = Position.Absolute;
                float s = _hullDrawScale;
                slotBtn.style.left = slot.pixelPosition.x * s - 36f * s;
                slotBtn.style.top = slot.pixelPosition.y * s - 36f * s;
                slotBtn.style.width = 72f * s;
                slotBtn.style.height = 72f * s;

                // Frame styling
                slotBtn.style.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 0.18f);
                slotBtn.style.borderTopWidth = 3;
                slotBtn.style.borderRightWidth = 3;
                slotBtn.style.borderBottomWidth = 3;
                slotBtn.style.borderLeftWidth = 3;
                slotBtn.style.borderTopColor = new Color(0.4f, 0.8f, 1f, 0.9f);
                slotBtn.style.borderRightColor = new Color(0.4f, 0.8f, 1f, 0.9f);
                slotBtn.style.borderBottomColor = new Color(0.4f, 0.8f, 1f, 0.9f);
                slotBtn.style.borderLeftColor = new Color(0.4f, 0.8f, 1f, 0.9f);
                slotBtn.style.borderTopLeftRadius = 10;
                slotBtn.style.borderTopRightRadius = 10;
                slotBtn.style.borderBottomLeftRadius = 10;
                slotBtn.style.borderBottomRightRadius = 10;

                // Hover
                slotBtn.RegisterCallback<MouseEnterEvent>(_ => slotBtn.style.backgroundColor = new Color(0.3f, 0.6f, 1f, 0.35f));
                slotBtn.RegisterCallback<MouseLeaveEvent>(_ => slotBtn.style.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 0.18f));

                // === ICON LOGIC ===
                var icon = new VisualElement();
                icon.style.width = 48f * s;
                icon.style.height = 48f * s;
                icon.style.marginTop = 8f * s;
                icon.style.alignSelf = Align.Center;
                icon.style.borderTopLeftRadius = 6;

                Sprite displayIcon = null;
                Color tint = Color.white;

                if (slot.equippedModule != null && slot.equippedModule.icon != null)
                {
                    displayIcon = slot.equippedModule.icon;
                    tint = Color.white;
                    // Highlight equipped slot
                    slotBtn.style.backgroundColor = new Color(0.1f, 0.35f, 0.7f, 0.45f);
                }
                else if (slot.slotIcon != null)
                {
                    displayIcon = slot.slotIcon;
                    tint = new Color(0.65f, 0.65f, 0.65f, 0.75f);   // Greyed out
                }
                else
                {
                    // Fallback colored box
                    Color fallback = slot.acceptedType switch
                    {
                        ModuleType.Weapon => new Color(0.6f, 0.2f, 0.2f, 0.6f),
                        ModuleType.Armour => new Color(0.4f, 0.4f, 0.6f, 0.6f),
                        ModuleType.Engine => new Color(0.3f, 0.5f, 0.3f, 0.6f),
                        _ => new Color(0.5f, 0.5f, 0.5f, 0.6f)
                    };
                    icon.style.backgroundColor = fallback;
                }

                if (displayIcon != null)
                {
                    icon.style.backgroundImage = new StyleBackground(displayIcon);
                    icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    icon.style.unityBackgroundImageTintColor = tint;
                }

                slotBtn.Add(icon);

                // Click handler
                slotBtn.clicked += () => HandleSlotClick(slot);

                _slotOverlay.Add(slotBtn);
            }
        }

        private void HandleSlotClick(ModuleSlot slot)
        {
            Debug.Log($"<color=lime>Slot clicked: {slot.slotId} ({slot.acceptedType})</color>");

            switch (slot.acceptedType)
            {
                case ModuleType.Weapon:
                    if (weaponSelectionHUD != null)
                    {
                        weaponSelectionHUD.OnWeaponEquipped += OnWeaponEquipped;
                        weaponSelectionHUD.OpenWeaponSelection(slot);
                    }
                    break;

                case ModuleType.Armour:
                    if (armourSelectionHUD != null)
                    {
                        armourSelectionHUD.OnArmourEquipped += OnArmourEquipped;
                        armourSelectionHUD.OpenArmourSelection(slot);
                    }
                    break;

                case ModuleType.Engine:
                    if (engineSelectionHUD != null)
                    {
                        engineSelectionHUD.OnEngineEquipped += OnEngineEquipped;
                        engineSelectionHUD.OpenEngineSelection(slot);
                    }
                    break;

                case ModuleType.Component:
                    if (componentSelectionHUD != null)
                    {
                        componentSelectionHUD.OnComponentEquipped += OnComponentEquipped;
                        componentSelectionHUD.OpenComponentSelection(slot);
                    }
                    break;
            }
        }

        // Event Handlers
        private void OnWeaponEquipped(WeaponData weapon, ModuleSlot slot)
        {
            weaponSelectionHUD.OnWeaponEquipped -= OnWeaponEquipped;
            if (_currentLoadout != null) _currentLoadout.EquipModule(slot, weapon);
            DrawSlotVisuals();   // Refresh visuals after equipping
            UpdateStatsDisplay();
        }

        private void OnArmourEquipped(ArmourData armour, ModuleSlot slot)
        {
            armourSelectionHUD.OnArmourEquipped -= OnArmourEquipped;
            if (_currentLoadout != null) _currentLoadout.EquipModule(slot, armour);
            DrawSlotVisuals();   // Refresh visuals after equipping
            UpdateStatsDisplay();
        }

        private void OnEngineEquipped(EngineData engine, ModuleSlot slot)
        {
            engineSelectionHUD.OnEngineEquipped -= OnEngineEquipped;
            if (_currentLoadout != null) _currentLoadout.EquipModule(slot, engine);
            DrawSlotVisuals();   // Refresh visuals after equipping
            UpdateStatsDisplay();
        }

        private void OnComponentEquipped(ComponentData component, ModuleSlot slot)
        {
            componentSelectionHUD.OnComponentEquipped -= OnComponentEquipped;
            if (_currentLoadout != null) _currentLoadout.EquipModule(slot, component);
            DrawSlotVisuals();   // Refresh visuals after equipping
            UpdateStatsDisplay();
        }

        private void UpdateStatsDisplay()
        {
            if (_statsLabel == null) return;
            _statsLabel.text = $"Hull: {_currentHull?.hullName}\nWeight: {_currentLoadout?.totalWeight:F1}";
        }

        private VisualElement CreateStatsPanel()
        {
            var panel = new VisualElement();
            panel.style.backgroundColor = new Color(0.08f, 0.12f, 0.25f, 0.95f);
            panel.style.borderTopLeftRadius = 8;
            panel.style.borderTopRightRadius = 8;
            panel.style.borderBottomLeftRadius = 8;
            panel.style.borderBottomRightRadius = 8;
            panel.style.paddingLeft = 15;
            panel.style.paddingRight = 15;
            panel.style.marginTop = 15;
            panel.style.marginTop = 15;

            _statsLabel = new Label("Select a hull...");
            _statsLabel.style.fontSize = 15;
            _statsLabel.style.color = Color.white;
            panel.Add(_statsLabel);

            return panel;
        }

        public void CloseShipBuilder()
        {
            ResetAllSlotsToDefault();           // ← New: Clear equipped modules

            if (_shipBuilderPanel != null)
            {
                _shipBuilderPanel.RemoveFromHierarchy();
                _shipBuilderPanel = null;
            }

            _currentHull = null;
            _currentLoadout = null;
        }
        private void ResetAllSlotsToDefault()
        {
            if (_currentHull?.moduleSlots == null) return;

            foreach (var slot in _currentHull.moduleSlots)
            {
                slot.equippedModule = null;     // Clear equipped module
            }

            Debug.Log("<color=yellow>ShipBuilder closed - All slots reset to default</color>");
        }
    }
}