using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class ComponentSelectionHUD : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private ModuleDatabase moduleDatabase;

        public event Action<ComponentData, ModuleSlot> OnComponentEquipped;

        private VisualElement _componentPanel;
        private ModuleSlot _targetSlot;
        private VisualElement _currentTabContent;

        private const string ALL_TAB = "All";

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
        public void OpenComponentSelection(ModuleSlot targetSlot)
        {
            if (root == null || moduleDatabase == null)
            {
                Debug.LogError("ComponentSelectionHUD: Missing UIDocument or ModuleDatabase!");
                return;
            }

            _targetSlot = targetSlot;

            if (root == null) return;
            if (_componentPanel != null) _componentPanel.RemoveFromHierarchy();

            _componentPanel = new VisualElement { name = "ComponentSelectionPanel" };
            _componentPanel.style.position = Position.Absolute;
            _componentPanel.style.top = 80;
            _componentPanel.style.left = 80;
            _componentPanel.style.right = 80;
            _componentPanel.style.bottom = 80;
            _componentPanel.style.backgroundColor = new Color(0.08f, 0.12f, 0.28f, 0.98f);
            _componentPanel.style.borderTopLeftRadius = 12;
            _componentPanel.style.borderTopRightRadius = 12;
            _componentPanel.style.borderBottomLeftRadius = 12;
            _componentPanel.style.borderBottomRightRadius = 12;
            _componentPanel.style.paddingLeft = 25;
            _componentPanel.style.paddingRight = 25;
            _componentPanel.style.paddingTop = 25;
            _componentPanel.style.paddingBottom = 25;

            var header = CreateHeader();
            _componentPanel.Add(header);

            var tabContainer = CreateTabContainer();
            _componentPanel.Add(tabContainer);

            _currentTabContent = new VisualElement { name = "ComponentGridContent" };
            _currentTabContent.style.flexGrow = 1;
            _currentTabContent.style.marginTop = 15;
            _componentPanel.Add(_currentTabContent);

            root.Add(_componentPanel);

            ShowComponentsByCategory(ComponentCategory.None);

            Debug.Log("<color=green>Component Selection HUD Opened</color>");
        }

        private VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 15;

            var title = new Label("Select Component");
            title.style.fontSize = 26;
            title.style.color = Color.cyan;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.fontSize = 24;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.style.width = 50;
            closeBtn.style.height = 50;
            closeBtn.style.borderTopLeftRadius = 25;
            closeBtn.style.borderTopRightRadius = 25;
            closeBtn.style.borderBottomLeftRadius = 25;
            closeBtn.style.borderBottomRightRadius = 25;
            closeBtn.clicked += CloseComponentSelection;
            header.Add(closeBtn);

            return header;
        }

        private VisualElement CreateTabContainer()
        {
            var tabRow = new VisualElement();
            tabRow.style.flexDirection = FlexDirection.Row;
            tabRow.style.marginBottom = 10;
            tabRow.style.flexWrap = Wrap.Wrap;

            tabRow.Add(CreateTabButton(ALL_TAB, () => ShowComponentsByCategory(ComponentCategory.None)));

            foreach (ComponentCategory cat in Enum.GetValues(typeof(ComponentCategory)))
            {
                if (cat == ComponentCategory.None) continue;
                tabRow.Add(CreateTabButton(cat.ToString(), () => ShowComponentsByCategory(cat)));
            }

            return tabRow;
        }

        private Button CreateTabButton(string text, Action onClick)
        {
            var btn = new Button { text = text };
            btn.style.height = 48;
            btn.style.minWidth = 110;
            btn.style.marginRight = 8;
            btn.style.marginBottom = 8;
            btn.style.fontSize = 16;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
            btn.clicked += onClick;
            return btn;
        }

        private void ShowComponentsByCategory(ComponentCategory category)
        {
            if (_currentTabContent == null) return;

            _currentTabContent.Clear();

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.FlexStart;

            List<ComponentData> componentList = moduleDatabase.GetFilteredComponents(category);

            foreach (var component in componentList)
            {
                var card = CreateComponentCard(component);
                grid.Add(card);
            }

            _currentTabContent.Add(grid);
        }

        private VisualElement CreateComponentCard(ComponentData component)
        {
            var card = new VisualElement();
            card.style.width = 160;
            card.style.height = 220;
            card.style.backgroundColor = new Color(0.15f, 0.22f, 0.38f);
            card.style.borderTopLeftRadius = 10;
            card.style.borderTopRightRadius = 10;
            card.style.borderBottomLeftRadius = 10;
            card.style.borderBottomRightRadius = 10;
            card.style.paddingTop = 10;
            card.style.paddingBottom = 10;
            card.style.alignItems = Align.Center;

            var icon = new VisualElement();
            icon.style.width = 100;
            icon.style.height = 100;
            if (component.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(component.icon);
                icon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            else
            {
                icon.style.backgroundColor = new Color(0.3f, 0.4f, 0.6f);
            }
            card.Add(icon);

            var nameLabel = new Label(component.moduleName);
            nameLabel.style.fontSize = 16;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginTop = 8;
            card.Add(nameLabel);

            var statsLabel = new Label(component.GetStatsSummary());
            statsLabel.style.fontSize = 12;
            statsLabel.style.color = new Color(0.7f, 0.85f, 1f);
            statsLabel.style.marginTop = 4;
            statsLabel.style.whiteSpace = WhiteSpace.Normal;
            card.Add(statsLabel);

            card.style.borderTopWidth = 4;
            card.style.borderTopColor = GetRarityColor(component.rarity);

            card.RegisterCallback<ClickEvent>(evt => EquipComponentToSlot(component));

            return card;
        }

        private Color GetRarityColor(ModuleRarity rarity)
        {
            return rarity switch
            {
                ModuleRarity.Common => new Color(0.6f, 0.6f, 0.6f),
                ModuleRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                ModuleRarity.Rare => new Color(0.2f, 0.6f, 1f),
                ModuleRarity.Epic => new Color(0.8f, 0.2f, 1f),
                ModuleRarity.Legendary => new Color(1f, 0.7f, 0.1f),
                _ => Color.gray
            };
        }

        private void EquipComponentToSlot(ComponentData component)
        {
            if (_targetSlot == null || component == null) return;

            _targetSlot.equippedModule = component;
            OnComponentEquipped?.Invoke(component, _targetSlot);
            CloseComponentSelection();

            Debug.Log($"<color=cyan>✅ Equipped {component.moduleName} to slot {_targetSlot.slotId}</color>");
        }

        public void CloseComponentSelection()
        {
            if (_componentPanel != null)
            {
                _componentPanel.RemoveFromHierarchy();
                _componentPanel = null;
            }
            _targetSlot = null;
        }
    }
}