using System;
using _Project.Scripts.Harbour.Data.SO;
using _Project.Scripts.Harbour.ShipBuilder;
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
        public event Action OnBuildShipClicked;
        public event Action<string, int> OnLandTileSlotClicked;

        private VisualElement _currentBuildPanel;
        
        // Reference to the inventory data
        [SerializeField] private ShipBuilderHUD shipBuilderHUD;
        

        public void RefreshUI(HarbourStateSO.HarbourMode mode)
        {
            if (harbourUIDocument?.rootVisualElement == null) return;

            // Close any open ship panel when changing modes
            if (mode != HarbourStateSO.HarbourMode.ShipBuilding && shipBuilderHUD != null)
                shipBuilderHUD.CloseShipBuilder();

            if (mode == HarbourStateSO.HarbourMode.Idle)
                BuildIdleHUD();
            else if (mode == HarbourStateSO.HarbourMode.HarbourBuild)
                BuildHarbourBuildHUD();
            else if (mode == HarbourStateSO.HarbourMode.ShipBuilding)
                shipBuilderHUD?.OpenShipBuilder();
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
            buildDropdown.style.width = 260;
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

            // Updated dropdown items
            AddDropdownItem(buildDropdown, "Edit Harbour",     () => OnBuildHarbourBaseClicked?.Invoke());
            AddDropdownItem(buildDropdown, "Build a Ship",     () => OnBuildShipClicked?.Invoke());
            AddDropdownItem(buildDropdown, "Save and Exit",    () => Debug.Log("Save and Exit clicked"));
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

            Debug.Log("<color=lime>HarbourHUD: Idle HUD updated with 'Build a Ship' option</color>");
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