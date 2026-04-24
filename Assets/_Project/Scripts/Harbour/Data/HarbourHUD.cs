using System;
using _Project.Scripts.Harbour.Data.SO;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.Data
{
    public class HarbourHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument harbourUIDocument;
        
        // Events for HarbourController to listen to
        public event Action OnBuildHarbourBaseClicked;
        public event Action OnExitBuildMode;
        public event Action<string, int> OnLandTileSlotClicked;

        private VisualElement _currentBuildPanel;
        
        // Reference to the inventory data
        [SerializeField] private LandTileInventorySO landTileInventory;

        // Event from inventory (so UI can refresh when count changes)
        private void OnEnable()
        {
            if (landTileInventory != null)
            {
                landTileInventory.OnCountChanged += RefreshSlotCount;
                // Optional: Listen for future changes if you want live updates
            }
        }

        private void OnDisable()
        {
            if  (landTileInventory != null)
            {
                landTileInventory.OnCountChanged -= RefreshSlotCount;
            }
        }

        /// <summary>
        /// Call this after placing a tile so the count label updates immediately (MVVM View refresh)
        /// </summary>
        public void RefreshSlotCount(int index)
        {
            if (_currentBuildPanel == null || landTileInventory == null) return;

            var contentArea = _currentBuildPanel.Q<VisualElement>("ContentArea");
            if (contentArea == null) return;

            // Find the slot at this index and update its count label
            var slots = contentArea.Query<VisualElement>().ToList();
            if (index < 0 || index >= slots.Count) return;

            var slot = slots[index];
            var countLabel = slot.Q<Label>();
            if (countLabel != null)
            {
                countLabel.text = landTileInventory.GetCount(index).ToString();
            }
        }

        public void RefreshUI(HarbourStateSO.HarbourMode mode)
        {
            if (harbourUIDocument?.rootVisualElement == null) return;

            harbourUIDocument.rootVisualElement.Clear();

            if (mode == HarbourStateSO.HarbourMode.Idle)
                BuildIdleHUD();
            else
                BuildHarbourBuildHUD();
        }

        // ===================================================================
        // IDLE HUD
        // ===================================================================
        public void BuildIdleHUD()
        {
            if (harbourUIDocument?.rootVisualElement == null) return;

            var root = harbourUIDocument.rootVisualElement;
            root.Clear();

            var topBar = new VisualElement { name = "TopBar" };
            topBar.style.position = Position.Absolute;
            topBar.style.top = 0;
            topBar.style.left = 0;
            topBar.style.right = 0;
            topBar.style.height = 60;
            topBar.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.justifyContent = Justify.FlexEnd;
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingRight = 20;

            var buildTrigger = new Button { text = "Build ▼" };
            buildTrigger.style.minWidth = 140;
            buildTrigger.style.height = 44;
            buildTrigger.style.marginLeft = 16;
            buildTrigger.style.fontSize = 17;
            buildTrigger.style.unityFontStyleAndWeight = FontStyle.Bold;
            buildTrigger.style.backgroundColor = new StyleColor(new Color(0.22f, 0.45f, 0.18f));
            buildTrigger.style.color = Color.white;
            buildTrigger.style.borderTopLeftRadius = 8;
            buildTrigger.style.borderTopRightRadius = 8;
            buildTrigger.style.borderBottomLeftRadius = 8;
            buildTrigger.style.borderBottomRightRadius = 8;

            var buildDropdown = new VisualElement { name = "BuildDropdown" };
            buildDropdown.style.position = Position.Absolute;
            buildDropdown.style.top = 60;
            buildDropdown.style.right = 20;
            buildDropdown.style.width = 240;
            buildDropdown.style.backgroundColor = new StyleColor(new Color(0.10f, 0.15f, 0.32f, 0.94f));
            buildDropdown.style.borderTopLeftRadius = 6;
            buildDropdown.style.borderTopRightRadius = 6;
            buildDropdown.style.borderBottomLeftRadius = 6;
            buildDropdown.style.borderBottomRightRadius = 6;
            buildDropdown.style.borderTopWidth = 1;
            buildDropdown.style.borderRightWidth = 1;
            buildDropdown.style.borderBottomWidth = 1;
            buildDropdown.style.borderLeftWidth = 1;
            buildDropdown.style.borderTopColor = new StyleColor(Color.cyan);
            buildDropdown.style.borderRightColor = new StyleColor(Color.cyan);
            buildDropdown.style.borderBottomColor = new StyleColor(Color.cyan);
            buildDropdown.style.borderLeftColor = new StyleColor(Color.cyan);
            buildDropdown.style.paddingTop = 8;
            buildDropdown.style.paddingBottom = 8;
            buildDropdown.style.paddingLeft = 8;
            buildDropdown.style.paddingRight = 8;
            buildDropdown.style.display = DisplayStyle.None;

            AddDropdownItem(buildDropdown, "Edit Harbour", () => OnBuildHarbourBaseClicked?.Invoke());
            AddDropdownItem(buildDropdown, "Save and Exit", () => Debug.Log("Save and Exit clicked"));
            AddDropdownItem(buildDropdown, "Exit Without Saving", () => Debug.Log("Exit Without Saving clicked"));

            bool dropdownVisible = false;
            buildTrigger.clicked += () =>
            {
                dropdownVisible = !dropdownVisible;
                buildDropdown.style.display = dropdownVisible ? DisplayStyle.Flex : DisplayStyle.None;

                buildTrigger.style.backgroundColor = dropdownVisible 
                    ? new StyleColor(new Color(0.32f, 0.55f, 0.28f)) 
                    : new StyleColor(new Color(0.22f, 0.45f, 0.18f));
            };

            topBar.Add(buildTrigger);
            topBar.Add(buildDropdown);
            root.Add(topBar);

            Debug.Log("<color=lime>HarbourHUD: Idle HUD with working Build dropdown restored</color>");
        }

        // ===================================================================
        // HARBOUR BUILD HUD
        // ===================================================================
        public void BuildHarbourBuildHUD()
        {
            if (harbourUIDocument?.rootVisualElement == null) return;

            var root = harbourUIDocument.rootVisualElement;
            root.Clear();

            var buildBar = new VisualElement { name = "BuildTopBar" };
            buildBar.style.position = Position.Absolute;
            buildBar.style.top = 0;
            buildBar.style.left = 0;
            buildBar.style.right = 0;
            buildBar.style.height = 60;
            buildBar.style.backgroundColor = new Color(0.85f, 0.35f, 0.1f, 0.95f);
            buildBar.style.flexDirection = FlexDirection.Row;
            buildBar.style.alignItems = Align.Center;
            buildBar.style.paddingLeft = 20;
            buildBar.style.paddingRight = 20;

            var toolsTrigger = new Button { text = "Build Tools ▼" };
            toolsTrigger.style.minWidth = 160;
            toolsTrigger.style.height = 44;
            toolsTrigger.style.fontSize = 17;
            toolsTrigger.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolsTrigger.style.backgroundColor = new StyleColor(new Color(0.22f, 0.45f, 0.18f));
            toolsTrigger.style.color = Color.white;
            toolsTrigger.style.borderTopLeftRadius = 8;
            toolsTrigger.style.borderTopRightRadius = 8;
            toolsTrigger.style.borderBottomLeftRadius = 8;
            toolsTrigger.style.borderBottomRightRadius = 8;

            var toolsDropdown = new VisualElement { name = "BuildToolsDropdown" };
            toolsDropdown.style.position = Position.Absolute;
            toolsDropdown.style.top = 60;
            toolsDropdown.style.left = 20;
            toolsDropdown.style.width = 240;
            toolsDropdown.style.backgroundColor = new StyleColor(new Color(0.10f, 0.15f, 0.32f, 0.94f));
            toolsDropdown.style.borderTopLeftRadius = 6;
            toolsDropdown.style.borderTopRightRadius = 6;
            toolsDropdown.style.borderBottomLeftRadius = 6;
            toolsDropdown.style.borderBottomRightRadius = 6;
            toolsDropdown.style.borderTopWidth = 1;
            toolsDropdown.style.borderRightWidth = 1;
            toolsDropdown.style.borderBottomWidth = 1;
            toolsDropdown.style.borderLeftWidth = 1;
            toolsDropdown.style.borderTopColor = new StyleColor(Color.cyan);
            toolsDropdown.style.borderRightColor = new StyleColor(Color.cyan);
            toolsDropdown.style.borderBottomColor = new StyleColor(Color.cyan);
            toolsDropdown.style.borderLeftColor = new StyleColor(Color.cyan);
            toolsDropdown.style.paddingTop = 8;
            toolsDropdown.style.paddingBottom = 8;
            toolsDropdown.style.paddingLeft = 8;
            toolsDropdown.style.paddingRight = 8;
            toolsDropdown.style.display = DisplayStyle.None;

            AddDropdownItem(toolsDropdown, "Build Harbour Base", () => OpenBuildPanel());
            AddDropdownItem(toolsDropdown, "Save Build",         () => Debug.Log("Save Build clicked"));
            AddDropdownItem(toolsDropdown, "Load Saved Build",   () => Debug.Log("Load Saved Build clicked"));

            bool toolsVisible = false;
            toolsTrigger.clicked += () =>
            {
                toolsVisible = !toolsVisible;
                toolsDropdown.style.display = toolsVisible ? DisplayStyle.Flex : DisplayStyle.None;

                toolsTrigger.style.backgroundColor = toolsVisible 
                    ? new StyleColor(new Color(0.32f, 0.55f, 0.28f)) 
                    : new StyleColor(new Color(0.22f, 0.45f, 0.18f));
            };

            buildBar.Add(toolsTrigger);
            buildBar.Add(toolsDropdown);

            var modeLabel = new Label("HARBOUR BUILD MODE");
            modeLabel.style.flexGrow = 1;
            modeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            modeLabel.style.fontSize = 24;
            modeLabel.style.color = Color.white;
            modeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            buildBar.Add(modeLabel);

            var exitBtn = new Button { text = "Exit Build Mode" };
            exitBtn.style.height = 44;
            exitBtn.style.fontSize = 16;
            exitBtn.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            exitBtn.style.color = Color.white;
            exitBtn.clicked += () => OnExitBuildMode?.Invoke();
            buildBar.Add(exitBtn);

            root.Add(buildBar);

            Debug.Log("<color=lime>HarbourHUD: Build HUD with Build Tools dropdown restored</color>");
        }

        // ===================================================================
        // FLOATING BUILD PANEL
        // ===================================================================
        private void OpenBuildPanel()
        {
            if (harbourUIDocument?.rootVisualElement == null) return;

            if (_currentBuildPanel != null)
                _currentBuildPanel.RemoveFromHierarchy();

            var root = harbourUIDocument.rootVisualElement;

            _currentBuildPanel = new VisualElement { name = "HarbourBuildPanel" };
            _currentBuildPanel.style.position = Position.Absolute;
            _currentBuildPanel.style.top = 70;
            _currentBuildPanel.style.right = 30;
            _currentBuildPanel.style.width = Length.Percent(25);
            _currentBuildPanel.style.height = Length.Percent(40);
            _currentBuildPanel.style.backgroundColor = new Color(0.08f, 0.12f, 0.25f, 0.97f);
            _currentBuildPanel.style.borderTopLeftRadius = 10;
            _currentBuildPanel.style.borderTopRightRadius = 10;
            _currentBuildPanel.style.borderBottomLeftRadius = 10;
            _currentBuildPanel.style.borderBottomRightRadius = 10;
            _currentBuildPanel.style.borderTopWidth = 2;
            _currentBuildPanel.style.borderRightWidth = 2;
            _currentBuildPanel.style.borderBottomWidth = 2;
            _currentBuildPanel.style.borderLeftWidth = 2;
            _currentBuildPanel.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
            _currentBuildPanel.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
            _currentBuildPanel.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            _currentBuildPanel.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
            _currentBuildPanel.style.paddingTop = 15;
            _currentBuildPanel.style.paddingRight = 15;
            _currentBuildPanel.style.paddingBottom = 15;
            _currentBuildPanel.style.paddingLeft = 15;
            _currentBuildPanel.pickingMode = PickingMode.Position;

            // Header
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 12;

            var title = new Label("Harbour Build");
            title.style.fontSize = 22;
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.fontSize = 20;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.style.width = 36;
            closeBtn.style.height = 36;
            closeBtn.style.borderTopLeftRadius = 18;
            closeBtn.style.borderTopRightRadius = 18;
            closeBtn.style.borderBottomLeftRadius = 18;
            closeBtn.style.borderBottomRightRadius = 18;
            closeBtn.clicked += CloseBuildPanel;
            header.Add(closeBtn);

            _currentBuildPanel.Add(header);

            // Tabs
            var tabContainer = new VisualElement();
            tabContainer.style.flexDirection = FlexDirection.Row;
            tabContainer.style.marginBottom = 15;

            var landTab = CreateTabButton("Land Tiles", () => ShowTabContent("Land Tiles"));
            tabContainer.Add(landTab);
            _currentBuildPanel.Add(tabContainer);

            // Content Area
            var contentArea = new VisualElement { name = "ContentArea" };
            contentArea.style.flexGrow = 1;
            contentArea.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.97f);
            contentArea.style.paddingTop = 12;
            contentArea.style.paddingBottom = 12;
            contentArea.style.paddingLeft = 12;
            contentArea.style.paddingRight = 12;
            contentArea.pickingMode = PickingMode.Position;

            _currentBuildPanel.Add(contentArea);
            root.Add(_currentBuildPanel);

            ShowTabContent("Land Tiles");

            Debug.Log("<color=green>Build Panel opened from 'Build Harbour Base'</color>");
        }

        private void CloseBuildPanel()
        {
            if (_currentBuildPanel != null)
            {
                _currentBuildPanel.RemoveFromHierarchy();
                _currentBuildPanel = null;
            }
        }

                private void ShowTabContent(string tabName)
        {
            var contentArea = _currentBuildPanel?.Q<VisualElement>("ContentArea");
            if (contentArea == null) return;

            contentArea.Clear();

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.FlexStart;
            grid.style.paddingTop = 8;
            grid.style.paddingBottom = 8;
            grid.style.paddingLeft = 8;
            grid.style.paddingRight = 8;

            for (int i = 0; i < 12; i++)
            {
                var slot = new VisualElement();
                slot.style.width = 72;
                slot.style.height = 72;
                slot.style.backgroundColor = new Color(0.25f, 0.3f, 0.45f);
                slot.style.borderTopLeftRadius = 6;
                slot.style.borderTopRightRadius = 6;
                slot.style.borderBottomLeftRadius = 6;
                slot.style.borderBottomRightRadius = 6;
                slot.style.marginRight = 12;
                slot.style.marginBottom = 12;

                int index = i;

                // Only fill slots that have data in the SO
                if (tabName == "Land Tiles" && landTileInventory != null && i < landTileInventory.tiles.Count)
                {
                    var entry = landTileInventory.tiles[i];

                    // Icon
                    if (entry.icon != null)
                    {
                        slot.style.backgroundImage = new StyleBackground(entry.icon);
                        slot.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                        slot.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                        slot.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                    }

                    // Count Label (MVVM View)
                    var countLabel = new Label();
                    countLabel.text = landTileInventory.GetCount(index).ToString();
                    countLabel.style.position = Position.Absolute;
                    countLabel.style.right = 4;
                    countLabel.style.bottom = 2;
                    countLabel.style.fontSize = 14;
                    countLabel.style.color = Color.white;
                    countLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    countLabel.style.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
                    countLabel.style.paddingLeft = 4;
                    countLabel.style.paddingRight = 4;
                    countLabel.style.borderTopLeftRadius = 3;
                    countLabel.style.borderTopRightRadius = 3;
                    countLabel.style.borderBottomLeftRadius = 3;
                    countLabel.style.borderBottomRightRadius = 3;

                    slot.Add(countLabel);

                    // Click handler only for valid slots
                    slot.RegisterCallback<ClickEvent>(evt => OnLandTileSlotClicked?.Invoke(tabName, index));
                }

                grid.Add(slot);
            }

            contentArea.Add(grid);
        }

        private Button CreateTabButton(string text, Action onClick)
        {
            var btn = new Button { text = text };
            btn.style.flexGrow = 1;
            btn.style.height = 42;
            btn.style.fontSize = 16;
            btn.style.marginRight = 6;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
            btn.clicked += onClick;
            return btn;
        }

        private void AddDropdownItem(VisualElement dropdownParent, string text, Action action)
        {
            var item = new Button { text = text };
            item.style.height = 42;
            item.style.marginLeft = 12;
            item.style.marginRight = 12;
            item.style.marginBottom = 4;
            item.style.fontSize = 16;
            item.style.backgroundColor = new StyleColor(new Color(0.14f, 0.18f, 0.34f));
            item.style.color = Color.white;
            item.style.borderTopLeftRadius = 4;
            item.style.borderTopRightRadius = 4;
            item.style.borderBottomLeftRadius = 4;
            item.style.borderBottomRightRadius = 4;

            item.clicked += () =>
            {
                action?.Invoke();
                dropdownParent.style.display = DisplayStyle.None;
            };

            dropdownParent.Add(item);
        }
    }
}