using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class ShipBuilderHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument harbourUIDocument;

        private VisualElement _shipBuilderPanel;
        private VisualElement _mainHullSlot;
        private Label _selectedHullNameLabel;
        private HullData _currentSelectedHull;
        
        // Hot-reload with New Input System
        private InputAction _refreshAction;
        private string lastSpritePath = "";
        private string lastHullName = "";

        // Reference to child panels
        [SerializeField] private HullSelectionHUD hullSelectionHUD;
        [SerializeField] private WeaponSelectionHUD weaponSelectionHUD;
        
        private HullData _lastSelectedHull;

        private VisualElement _selectedWeaponSlot;   // ← Remembers which slot was clicked
        
        private void OnEnable()
        {
            // Setup refresh hotkey (R key)
            _refreshAction = new InputAction(binding: "<Keyboard>/r");
            _refreshAction.performed += ctx => RefreshShipBuilderUI();
            _refreshAction.Enable();
        }

        private void OnDisable()
        {
            _refreshAction?.Dispose();
        }

        private void RefreshShipBuilderUI()
        {
            if (_shipBuilderPanel == null) return;

            // Close and reopen the panel
            CloseShipBuilder();
            OpenShipBuilder();
            
            RefreshCurrentHull();

            // Re-apply the last selected hull (if any)
            if (_lastSelectedHull != null)
            {
                SetSelectedHull(_lastSelectedHull);
                Debug.Log($"<color=cyan>🔄 Refreshed Ship Builder with: {_lastSelectedHull.hullName}</color>");
            }
            else
            {
                Debug.Log("<color=cyan>🔄 Ship Builder UI Refreshed (no previous hull)</color>");
            }
        }
        
        public void OpenShipBuilder()
        {
            if (harbourUIDocument?.rootVisualElement == null) return;

            var root = harbourUIDocument.rootVisualElement;
            if (_shipBuilderPanel != null) _shipBuilderPanel.RemoveFromHierarchy();

            _shipBuilderPanel = new VisualElement { name = "ShipBuilderPanel" };
            _shipBuilderPanel.style.position = Position.Absolute;
            _shipBuilderPanel.style.top = 50;
            _shipBuilderPanel.style.left = 30;
            _shipBuilderPanel.style.right = 30;
            _shipBuilderPanel.style.bottom = 50;
            _shipBuilderPanel.style.backgroundColor = new Color(0.025f, 0.04f, 0.12f, 0.98f);
            _shipBuilderPanel.style.borderTopLeftRadius = 12;
            _shipBuilderPanel.style.borderTopRightRadius = 12;
            _shipBuilderPanel.style.borderBottomLeftRadius = 12;
            _shipBuilderPanel.style.borderBottomRightRadius = 12;
            _shipBuilderPanel.style.flexDirection = FlexDirection.Column;
            _shipBuilderPanel.style.overflow = Overflow.Hidden;

            // Top Tabs
            _shipBuilderPanel.Add(CreateTopTabs());

            // Main Content
            var mainContent = new VisualElement();
            mainContent.style.flexDirection = FlexDirection.Row;
            mainContent.style.flexGrow = 1;
            mainContent.style.paddingTop = 10;
            mainContent.style.paddingLeft = 12;
            mainContent.style.paddingRight = 12;
            mainContent.style.paddingBottom = 12;

            mainContent.Add(CreateLeftPanel());
            mainContent.Add(CreateCenterHullArea());   // Now contains module slots
            mainContent.Add(CreateRightPanel());

            _shipBuilderPanel.Add(mainContent);

            // Bottom Bar (with Select Hull button)
            _shipBuilderPanel.Add(CreateBottomBar());

            root.Add(_shipBuilderPanel);

            Debug.Log("<color=green>✅ Ship Builder with Module Slots + Bottom Select Hull</color>");
        }
        
        private VisualElement CreateTopTabs() 
        {
            var tabs = new VisualElement();
            tabs.style.flexDirection = FlexDirection.Row;
            tabs.style.height = 55;
            tabs.style.backgroundColor = new Color(0.08f, 0.12f, 0.25f);
            tabs.style.borderBottomWidth = 3;
            tabs.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);

            string[] tabNames = { "Ship", "Crew", "Wayrl", "Duyle", "Defense" };
            foreach (var name in tabNames)
            {
                var btn = new Button { text = name };
                btn.style.flexGrow = 1;
                btn.style.height = 55;
                btn.style.fontSize = 18;
                btn.style.color = name == "Ship" ? Color.cyan : Color.white;
                btn.style.backgroundColor = name == "Ship" ? new Color(0.15f, 0.28f, 0.5f) : new Color(0.1f, 0.15f, 0.3f);
                tabs.Add(btn);
            }
            return tabs;
        }

        private VisualElement CreateLeftPanel()
        {
            var left = new VisualElement();
            left.style.width = 280;
            left.style.marginRight = 15;
            left.style.backgroundColor = new Color(0.06f, 0.1f, 0.22f);
            left.style.borderTopLeftRadius = 10;
            left.style.borderBottomLeftRadius = 10;
            left.style.paddingLeft = 15;
            left.style.paddingRight = 15;
            left.style.paddingTop = 15;
            left.style.paddingBottom = 15;

            // Title
            var title = new Label("HMS Dauntless");
            title.style.fontSize = 22;
            title.style.color = Color.cyan;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            left.Add(title);

            // Hull Image (small)
            var smallHull = new VisualElement();
            smallHull.style.width = 240;
            smallHull.style.height = 80;
            smallHull.style.backgroundColor = new Color(0.2f, 0.25f, 0.35f);
            smallHull.style.marginTop = 10;
            smallHull.style.marginBottom = 15;
            left.Add(smallHull);

            // Captain & Status (add more labels as needed)
            left.Add(new Label("Captain: Admiral Vance") { style = { color = Color.white, fontSize = 15 } });
            left.Add(new Label("Fleet Level: 45") { style = { color = Color.white, fontSize = 15 } });

            return left;
        }
        

        private VisualElement CreateCenterHullArea()
        {
            var center = new VisualElement();
            center.style.flexGrow = 1;
            center.style.alignItems = Align.Center;
            center.style.justifyContent = Justify.Center;
            center.style.position = Position.Relative;

            // Main Hull Preview
            _mainHullSlot = new VisualElement();
            _mainHullSlot.style.width = 520;
            _mainHullSlot.style.height = 260;
            _mainHullSlot.style.backgroundColor = new Color(0.12f, 0.18f, 0.32f);
            _mainHullSlot.style.borderTopLeftRadius = 12;
            _mainHullSlot.style.borderTopRightRadius = 12;
            _mainHullSlot.style.borderBottomLeftRadius = 12;
            _mainHullSlot.style.borderBottomRightRadius = 12;
            _mainHullSlot.style.alignSelf = Align.Center;

            _mainHullSlot.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            _mainHullSlot.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            _mainHullSlot.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);

            center.Add(_mainHullSlot);

            // === IMPROVED MODULE SLOTS (better centered) ===
            // CreateModuleSlot(center, "Weapon",   -165, -75);   // Top Left
            // CreateModuleSlot(center, "Weapon",    165, -75);   // Top Right
            // CreateModuleSlot(center, "Armour",   -165,  75);   // Bottom Left
            // CreateModuleSlot(center, "Engine",    165,  75);   // Bottom Right

            // Hull Name
            _selectedHullNameLabel = new Label("No Hull Selected");
            _selectedHullNameLabel.style.fontSize = 26;
            _selectedHullNameLabel.style.color = Color.cyan;
            _selectedHullNameLabel.style.marginTop = 15;
            _selectedHullNameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            center.Add(_selectedHullNameLabel);

            return center;
        }

        

        private VisualElement CreateRightPanel()
        {
            var right = new VisualElement();
            right.style.width = 280;
            right.style.marginLeft = 15;
            right.style.backgroundColor = new Color(0.06f, 0.1f, 0.22f);
            right.style.borderTopRightRadius = 10;
            right.style.borderBottomRightRadius = 10;
            right.style.paddingLeft = 15;
            right.style.paddingRight = 15;
            right.style.paddingBottom = 15;
            right.style.paddingTop = 15;

            var hullTitle = new Label("Gunboat Hull");
            hullTitle.style.fontSize = 20;
            hullTitle.style.color = Color.white;
            right.Add(hullTitle);

            // Add more stats here later
            right.Add(new Label("Level 1000") { style = { color = new Color(0.6f, 0.9f, 1f) } });

            return right;
        }

        private VisualElement CreateBottomBar()
        {
            var bottom = new VisualElement();
            bottom.style.flexDirection = FlexDirection.Row;
            bottom.style.height = 80;
            bottom.style.backgroundColor = new Color(0.08f, 0.12f, 0.25f);
            bottom.style.justifyContent = Justify.SpaceBetween;
            bottom.style.alignItems = Align.Center;
            bottom.style.paddingLeft = 30;
            bottom.style.paddingRight = 30;

            // Select Hull Button
            var selectBtn = new Button { text = "Select Hull" };
            selectBtn.style.height = 55;
            selectBtn.style.fontSize = 18;
            selectBtn.style.minWidth = 200;
            selectBtn.style.backgroundColor = new Color(0.2f, 0.5f, 0.9f);
            selectBtn.clicked += () => hullSelectionHUD?.OpenHullSelection();
            bottom.Add(selectBtn);

            // Spacer
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            bottom.Add(spacer);

            // Build Button
            var buildBtn = new Button { text = "BUILD" };
            buildBtn.style.height = 55;
            buildBtn.style.fontSize = 20;
            buildBtn.style.color = Color.white;
            buildBtn.style.backgroundColor = new Color(0.1f, 0.6f, 0.2f);
            buildBtn.style.minWidth = 160;
            bottom.Add(buildBtn);

            // Cancel / Exit Button
            var cancelBtn = new Button { text = "CANCEL" };
            cancelBtn.style.height = 55;
            cancelBtn.style.fontSize = 20;
            cancelBtn.style.color = Color.white;
            cancelBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            cancelBtn.style.minWidth = 160;
            cancelBtn.clicked += CloseShipBuilder;        // ← Now closes the panel
            bottom.Add(cancelBtn);

            return bottom;
        }
        
        
        private void OnHullSelected(HullData selectedHull)
        {
            _currentSelectedHull = selectedHull;

            if (selectedHull == null) return;

            _selectedHullNameLabel.text = selectedHull.hullName;

            if (selectedHull.hullImage != null)
            {
                _mainHullSlot.style.backgroundImage = new StyleBackground(selectedHull.hullImage);
            }
            else
            {
                _mainHullSlot.style.backgroundColor = new Color(0.35f, 0.45f, 0.65f); // fallback
            }

            Debug.Log($"<color=cyan>Hull Selected: {selectedHull.hullName}</color>");
        }
        
        public void SetSelectedHull(HullData selectedHull)
        {
            _lastSelectedHull = selectedHull;        // ← Important: Remember it for refresh

            if (selectedHull == null) return;

            // Update name and description
            if (_selectedHullNameLabel != null)
                _selectedHullNameLabel.text = selectedHull.hullName;

            var descLabel = _shipBuilderPanel?.Q<Label>("HullDescriptionLabel");
            if (descLabel != null)
                descLabel.text = selectedHull.description;

            // Load baked hull sprite + create slots
            if (_mainHullSlot != null && selectedHull.hullImage != null)
            {
                _mainHullSlot.style.backgroundImage = new StyleBackground(selectedHull.hullImage);
                _mainHullSlot.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                _mainHullSlot.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                _mainHullSlot.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);

                CreateModuleSlots(selectedHull);
        
                Debug.Log($"<color=green>✅ Applied Hull: {selectedHull.hullName}</color>");
            }
        }

        private void CreateModuleSlots(HullData hull)
        {
            if (_mainHullSlot == null || hull?.slots == null) return;

            _mainHullSlot.Clear();

            foreach (var slotData in hull.slots)
            {
                var hitbox = new VisualElement();
                hitbox.name = $"{slotData.type}Slot";
                hitbox.style.position = Position.Absolute;
                hitbox.style.width = new Length(82, LengthUnit.Pixel);
                hitbox.style.height = new Length(82, LengthUnit.Pixel);

                hitbox.style.left = Length.Percent(slotData.xPercent * 100 - 4.1f);
                hitbox.style.top  = Length.Percent(slotData.yPercent * 100 - 4.1f);

                hitbox.style.backgroundColor = new Color(0, 0, 0, 0f);

                // === MAIN INTERACTIVITY ===
                hitbox.RegisterCallback<ClickEvent>(evt =>
                {
                    if (slotData.type == "Weapon")
                    {
                        _selectedWeaponSlot = hitbox;                    // Remember which slot was clicked
                        weaponSelectionHUD?.OpenWeaponSelection();
                        Debug.Log("<color=cyan>🔫 Weapon menu opened for this slot</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=yellow>{slotData.type} slot clicked (coming soon)</color>");
                    }
                });

                // Hover feedback
                hitbox.RegisterCallback<MouseEnterEvent>(evt => hitbox.style.backgroundColor = new Color(1f, 1f, 1f, 0.12f));
                hitbox.RegisterCallback<MouseLeaveEvent>(evt => hitbox.style.backgroundColor = new Color(0, 0, 0, 0f));

                _mainHullSlot.Add(hitbox);
            }
        }
        
        public bool HasPendingWeaponSlot() => _selectedWeaponSlot != null;

        public void EquipWeaponToSlot(string weaponName, Color weaponColor)
        {
            if (_selectedWeaponSlot == null) return;

            // Clear previous content
            _selectedWeaponSlot.Clear();

            // Create weapon visual
            var weaponVisual = new VisualElement();
            weaponVisual.style.width = new Length(68, LengthUnit.Pixel);
            weaponVisual.style.height = new Length(68, LengthUnit.Pixel);
            weaponVisual.style.backgroundColor = weaponColor;
            weaponVisual.style.borderTopLeftRadius = 8;
            weaponVisual.style.borderTopRightRadius = 8;
            weaponVisual.style.borderBottomLeftRadius = 8;
            weaponVisual.style.borderBottomRightRadius = 8;

            var label = new Label(weaponName.Substring(0, 1)); // First letter
            label.style.fontSize = 28;
            label.style.color = Color.white;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            weaponVisual.Add(label);

            _selectedWeaponSlot.Add(weaponVisual);

            // Clear pending slot
            _selectedWeaponSlot = null;

            Debug.Log($"<color=green>Weapon '{weaponName}' equipped!</color>");
        }
        
        public void RefreshCurrentHull()
        {
            if (_currentSelectedHull != null)
            {
                SetSelectedHull(_currentSelectedHull);
            }
        }

        // Simple helper - you can make this more advanced later
        private string GetHullDescription(string hullName)
        {
            return hullName switch
            {
                "Gun Boat Hull" => "Small agile hull perfect for fast strikes and scouting.",
                "Skirmisher Hull" => "Fast attack hull with strong offensive capabilities.",
                "HammerHead Hull" => "Heavy combat hull built for durability and firepower.",
                _ => "Select a hull to begin building your ship..."
            };
        }

        public void CloseShipBuilder()
        {
            if (_shipBuilderPanel != null)
            {
                _shipBuilderPanel.RemoveFromHierarchy();
                _shipBuilderPanel = null;
            }
        }
    }
}