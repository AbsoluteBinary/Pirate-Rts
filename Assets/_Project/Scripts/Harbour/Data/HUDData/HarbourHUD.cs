using System;
using _Project.Scripts.Harbour.Data.SO;
using _Project.Scripts.Harbour.Economy;
using _Project.Scripts.Harbour.ShipBuilder;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.Data.HUDData
{
    public class HarbourHUD : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private DockHUD dockHUD;
        [SerializeField] private ShipBuilderHUD shipBuilderHUD;
        [SerializeField] private ResourceHUD resourceHUD;
        [SerializeField] private ResourceWalletHolder walletHolder;
        public event Action OnResourcesClicked;
        public event Action OnBuildHarbourBaseClicked;
        public event Action OnExitBuildMode;
        public event Action OnBuildShipClicked;
        public event Action OnDockClicked;

        private VisualElement root;
        private HarbourStateSO.HarbourMode? pendingMode;

        private void OnEnable()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            if (panelRenderer == null)
            {
                Debug.LogError("HarbourHUD: PanelRenderer is missing.");
                return;
            }

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
            Debug.Log("<color=lime>HarbourHUD: root is now ready</color>");

            if (!pendingMode.HasValue) return;

            var mode = pendingMode.Value;
            pendingMode = null;
            RefreshUI(mode);
        }

        public void RefreshUI(HarbourStateSO.HarbourMode mode)
        {
            if (root == null)
            {
                pendingMode = mode;
                return;
            }

            if (mode != HarbourStateSO.HarbourMode.ShipBuilding)
                shipBuilderHUD?.CloseShipBuilder();

            if (mode != HarbourStateSO.HarbourMode.Dock)
                dockHUD?.CloseDock();
            
            

            switch (mode)
            {
                case HarbourStateSO.HarbourMode.Idle:
                    BuildIdleHUD();
                    break;
                case HarbourStateSO.HarbourMode.HarbourBuild:
                    BuildHarbourBuildHUD();
                    break;
                case HarbourStateSO.HarbourMode.ShipBuilding:
                    shipBuilderHUD?.OpenShipBuilder();
                    break;
                case HarbourStateSO.HarbourMode.Dock:
                    OpenDockPanel();
                    break;
            }
        }
        

        private void OpenDockPanel()
        {
            if (root == null)
            {
                Debug.LogError("HarbourHUD.OpenDockPanel: root is null.");
                return;
            }

            if (dockHUD == null)
            {
                Debug.LogError("HarbourHUD.OpenDockPanel: dockHUD is not assigned.");
                return;
            }

            root.Clear();
            dockHUD.OpenDock(root);
            root.Add(CreateIdleResourcePanel());
        }

        public void BuildIdleHUD()
        {
            if (root == null) return;

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

            AddDropdownItem(buildDropdown, "Edit Harbour", () => OnBuildHarbourBaseClicked?.Invoke());
            AddDropdownItem(buildDropdown, "Build a Ship", () => OnBuildShipClicked?.Invoke());
            AddDropdownItem(buildDropdown, "Dock", () => OnDockClicked?.Invoke());
            AddDropdownItem(buildDropdown, "Save and Exit", () => Debug.Log("Save and Exit clicked"));
            AddDropdownItem(buildDropdown, "Exit Without Saving", () => Debug.Log("Exit Without Saving clicked"));
            AddDropdownItem(buildDropdown, "Resources", () => OnResourcesClicked?.Invoke());

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

            
            root.Add(CreateIdleResourcePanel());
            root.schedule.Execute(RefreshIdleResourceAmounts);
            
            Debug.Log("<color=lime>HarbourHUD: Idle HUD (PanelRenderer)</color>");
        }
        
        private void RefreshIdleResourceAmounts()
        {
            if (root == null) return;
            var panel = root.Q("IdleResourcePanel");
            if (panel == null) return;

            string[] ids =
            {
                "Oil", "Iron", "Steel",
                "Energy", "Aluminium", "Lumber",
                "Alloy", "Cloth", "Uranium"
            };

            foreach (var id in ids)
            {
                var lab = panel.Q<Label>($"IdleRes_{id}");
                if (lab == null) continue;

                var def = walletHolder != null && walletHolder.Catalog != null
                    ? walletHolder.Catalog.Get(id)
                    : null;

                int have = walletHolder != null && walletHolder.Wallet != null
                    ? walletHolder.Wallet.Get(id)
                    : (def != null ? def.startingAmount : 0);

                lab.text = have.ToString();
            }
        }
        
        private VisualElement CreateIdleResourcePanel()
        {
            var panel = new VisualElement { name = "IdleResourcePanel" };
            panel.style.position = Position.Absolute;
            panel.style.right = 16;
            panel.style.bottom = 16;
            panel.style.width = 220;
            panel.style.backgroundColor = new Color(0.06f, 0.10f, 0.22f, 0.94f);
            panel.style.paddingTop = 10;
            panel.style.paddingBottom = 10;
            panel.style.paddingLeft = 12;
            panel.style.paddingRight = 12;
            panel.style.borderTopWidth = 2;
            panel.style.borderRightWidth = 2;
            panel.style.borderBottomWidth = 2;
            panel.style.borderLeftWidth = 2;
            panel.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
            panel.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
            panel.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            panel.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
            panel.style.borderTopLeftRadius = 8;
            panel.style.borderTopRightRadius = 8;
            panel.style.borderBottomLeftRadius = 8;
            panel.style.borderBottomRightRadius = 8;

            var title = new Label("Resources");
            title.style.fontSize = 14;
            title.style.color = Color.cyan;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 8;
            panel.Add(title);

            string[] ids =
            {
                "Oil", "Iron", "Steel",
                "Energy", "Aluminium", "Lumber",
                "Alloy", "Cloth", "Uranium"
            };

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.flexDirection = FlexDirection.Column;
            grid.style.flexWrap = Wrap.NoWrap;
            foreach (var id in ids)
                grid.Add(CreateIdleResourceCell(id));
            panel.Add(grid);
            
            
            return panel;
        }

        private VisualElement CreateIdleResourceCell(string id)
        {
            
            var def = walletHolder != null && walletHolder.Catalog != null
                ? walletHolder.Catalog.Get(id)
                : null;

            var cell = new VisualElement();
            cell.style.width = Length.Percent(100);
            cell.style.marginRight = 0;
            cell.style.flexDirection = FlexDirection.Row;
            cell.style.alignItems = Align.Center;
            cell.style.marginBottom = 4;

            var nameLab = new Label(def != null && !string.IsNullOrEmpty(def.displayName)
                ? def.displayName
                : id);
            nameLab.style.fontSize = 11;
            nameLab.style.color = new Color(0.7f, 0.85f, 1f);
            nameLab.style.width = 78;
            nameLab.style.flexShrink = 0;
            cell.Add(nameLab);

            int have = walletHolder != null && walletHolder.Wallet != null
                ? walletHolder.Wallet.Get(id)
                : (def != null ? def.startingAmount : 0);
            

            var amount = new Label(have.ToString()) { name = $"IdleRes_{id}" };
            amount.style.minWidth = 48;
            amount.style.flexGrow = 1;
            amount.style.flexShrink = 0;
            amount.style.fontSize = 13;
            amount.style.color = Color.white;
            amount.style.unityTextAlign = TextAnchor.MiddleRight;
            cell.Add(amount);

            var icon = new VisualElement();
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.flexShrink = 0;
            icon.style.marginLeft = 4;
            if (def?.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(def.icon);
                icon.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
            }
            else
                icon.style.backgroundColor = new Color(0.16f, 0.22f, 0.40f);
            cell.Add(icon);

            return cell;
        }

        public void BuildHarbourBuildHUD()
        {
            if (root == null) return;

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

            AddDropdownItem(toolsDropdown, "Save Build", () => Debug.Log("Save Build clicked"));
            AddDropdownItem(toolsDropdown, "Load Saved Build", () => Debug.Log("Load Saved Build clicked"));

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

            Debug.Log("<color=lime>HarbourHUD: Build HUD (PanelRenderer)</color>");
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