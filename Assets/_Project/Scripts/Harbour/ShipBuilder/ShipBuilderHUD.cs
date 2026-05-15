using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class ShipBuilderHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument harbourUIDocument;
        
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

        public void OpenShipBuilder()
        {
            if (harbourUIDocument?.rootVisualElement == null) return;

            var root = harbourUIDocument.rootVisualElement;
            if (_shipBuilderPanel != null) _shipBuilderPanel.RemoveFromHierarchy();

            _shipBuilderPanel = new VisualElement { name = "ShipBuilderPanel" };
            _shipBuilderPanel.style.position = Position.Absolute;
            _shipBuilderPanel.style.top = 80;
            _shipBuilderPanel.style.left = 50;
            _shipBuilderPanel.style.right = 50;
            _shipBuilderPanel.style.bottom = 50;
            _shipBuilderPanel.style.backgroundColor = new Color(0.05f, 0.08f, 0.22f, 0.98f);
            _shipBuilderPanel.style.borderTopLeftRadius = 12;
            _shipBuilderPanel.style.borderTopRightRadius = 12;
            _shipBuilderPanel.style.borderBottomLeftRadius = 12;
            _shipBuilderPanel.style.borderBottomRightRadius = 12;
            _shipBuilderPanel.style.paddingLeft = 20;
            _shipBuilderPanel.style.paddingRight = 20;
            _shipBuilderPanel.style.paddingTop = 20;
            _shipBuilderPanel.style.paddingBottom = 20;

            var header = CreateHeader();
            _shipBuilderPanel.Add(header);

            var selectHullBtn = new Button { text = "Select Hull" };
            selectHullBtn.style.fontSize = 18;
            selectHullBtn.style.height = 55;
            selectHullBtn.style.marginBottom = 15;
            selectHullBtn.clicked += () => hullSelectionHUD?.OpenHullSelection();
            _shipBuilderPanel.Add(selectHullBtn);

            var innerPanel = CreateHullPreviewArea();
            _shipBuilderPanel.Add(innerPanel);

            var statsPanel = CreateStatsPanel();
            _shipBuilderPanel.Add(statsPanel);

            root.Add(_shipBuilderPanel);
        }

        private VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 15;

            _hullNameLabel = new Label("No Hull Selected");
            _hullNameLabel.style.fontSize = 28;
            _hullNameLabel.style.color = Color.cyan;
            _hullNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(_hullNameLabel);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.fontSize = 24;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.clicked += CloseShipBuilder;
            header.Add(closeBtn);

            return header;
        }

        private VisualElement CreateHullPreviewArea()
        {
            var inner = new VisualElement();
            inner.style.flexGrow = 1;
            inner.style.alignItems = Align.Center;
            inner.style.justifyContent = Justify.Center;
            inner.style.paddingTop = 30;

            _hullCanvas = new VisualElement();
            _hullCanvas.style.position = Position.Relative;
            _hullCanvas.style.flexShrink = 0;
            _hullCanvas.style.flexGrow = 0;

            _hullImageElement = new VisualElement();

            _slotOverlay = new VisualElement();
            _slotOverlay.style.position = Position.Absolute;
            _slotOverlay.style.width = Length.Percent(100);
            _slotOverlay.style.height = Length.Percent(100);

            _hullCanvas.Add(_hullImageElement);
            _hullCanvas.Add(_slotOverlay);

            inner.Add(_hullCanvas);
            return inner;
        }

        public void SetSelectedHull(HullData hull)
        {
            _currentHull = hull;
            _currentLoadout = new ShipLoadout { hull = hull };   // Fresh loadout every time

            if (hull?.hullImage == null) return;

            _hullNameLabel.text = hull.hullName;

            var tex = hull.hullImage.texture;
            _hullCanvas.style.width = tex.width;
            _hullCanvas.style.height = tex.height;

            _hullImageElement.style.width = Length.Percent(100);
            _hullImageElement.style.height = Length.Percent(100);
            _hullImageElement.style.backgroundImage = new StyleBackground(hull.hullImage);

            DrawSlotVisuals();        // Will now correctly show default icons
            UpdateStatsDisplay();
        }

        private void DrawSlotVisuals()
        {
            _slotOverlay.Clear();
            if (!showSlotVisuals || _currentHull?.moduleSlots == null) return;

            foreach (var slot in _currentHull.moduleSlots)
            {
                var slotBtn = new Button();
                slotBtn.style.position = Position.Absolute;
                slotBtn.style.left = slot.pixelPosition.x - 36;
                slotBtn.style.top = slot.pixelPosition.y - 36;
                slotBtn.style.width = 72;
                slotBtn.style.height = 72;

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
                icon.style.width = 48;
                icon.style.height = 48;
                icon.style.alignSelf = Align.Center;
                icon.style.marginTop = 8;
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