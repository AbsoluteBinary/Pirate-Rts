using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class EngineSelectionHUD : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private ModuleDatabase moduleDatabase;

        public event Action<EngineData, ModuleSlot> OnEngineEquipped;

        private VisualElement _enginePanel;
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

        public void OpenEngineSelection(ModuleSlot targetSlot)
        {
            if (root == null) return;
            if (root == null || moduleDatabase == null)
            {
                Debug.LogError("EngineSelectionHUD: Missing UIDocument or ModuleDatabase!");
                return;
            }

            _targetSlot = targetSlot;

            
            if (_enginePanel != null) _enginePanel.RemoveFromHierarchy();

            _enginePanel = new VisualElement { name = "EngineSelectionPanel" };
            _enginePanel.style.position = Position.Absolute;
            _enginePanel.style.top = 80;
            _enginePanel.style.left = 80;
            _enginePanel.style.right = 80;
            _enginePanel.style.bottom = 80;
            _enginePanel.style.backgroundColor = new Color(0.08f, 0.12f, 0.28f, 0.98f);
            _enginePanel.style.borderTopLeftRadius = 12;
            _enginePanel.style.borderTopRightRadius = 12;
            _enginePanel.style.borderBottomLeftRadius = 12;
            _enginePanel.style.borderBottomRightRadius = 12;
            _enginePanel.style.paddingLeft = 25;
            _enginePanel.style.paddingRight = 25;
            _enginePanel.style.paddingTop = 25;
            _enginePanel.style.paddingBottom = 25;

            var header = CreateHeader();
            _enginePanel.Add(header);

            var tabContainer = CreateTabContainer();
            _enginePanel.Add(tabContainer);

            _currentTabContent = new VisualElement { name = "EngineGridContent" };
            _currentTabContent.style.flexGrow = 1;
            _currentTabContent.style.marginTop = 15;
            _enginePanel.Add(_currentTabContent);

            root.Add(_enginePanel);

            ShowEnginesByCategory(EngineCategory.None);

            Debug.Log("<color=green>Engine Selection HUD Opened</color>");
        }

        private VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 15;

            var title = new Label("Select Engine");
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
            closeBtn.clicked += CloseEngineSelection;
            header.Add(closeBtn);

            return header;
        }

        private VisualElement CreateTabContainer()
        {
            var tabRow = new VisualElement();
            tabRow.style.flexDirection = FlexDirection.Row;
            tabRow.style.marginBottom = 10;
            tabRow.style.flexWrap = Wrap.Wrap;

            tabRow.Add(CreateTabButton(ALL_TAB, () => ShowEnginesByCategory(EngineCategory.None)));

            foreach (EngineCategory cat in Enum.GetValues(typeof(EngineCategory)))
            {
                if (cat == EngineCategory.None) continue;
                tabRow.Add(CreateTabButton(cat.ToString(), () => ShowEnginesByCategory(cat)));
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

        private void ShowEnginesByCategory(EngineCategory category)
        {
            if (_currentTabContent == null) return;

            _currentTabContent.Clear();

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.FlexStart;

            List<EngineData> engineList = moduleDatabase.GetFilteredEngines(category);

            foreach (var engine in engineList)
            {
                var card = CreateEngineCard(engine);
                grid.Add(card);
            }

            _currentTabContent.Add(grid);
        }

        private VisualElement CreateEngineCard(EngineData engine)
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
            if (engine.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(engine.icon);
                icon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            else
            {
                icon.style.backgroundColor = new Color(0.3f, 0.4f, 0.6f);
            }
            card.Add(icon);

            var nameLabel = new Label(engine.moduleName);
            nameLabel.style.fontSize = 16;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginTop = 8;
            card.Add(nameLabel);

            var statsLabel = new Label(engine.GetStatsSummary());
            statsLabel.style.fontSize = 12;
            statsLabel.style.color = new Color(0.7f, 0.85f, 1f);
            statsLabel.style.marginTop = 4;
            statsLabel.style.whiteSpace = WhiteSpace.Normal;
            card.Add(statsLabel);

            card.style.borderTopWidth = 4;
            card.style.borderTopColor = GetRarityColor(engine.rarity);

            card.RegisterCallback<ClickEvent>(evt => EquipEngineToSlot(engine));

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

        private void EquipEngineToSlot(EngineData engine)
        {
            if (_targetSlot == null || engine == null) return;

            string slotId = _targetSlot.slotId;
            _targetSlot.equippedModule = engine;
            OnEngineEquipped?.Invoke(engine, _targetSlot);
            CloseEngineSelection();

            Debug.Log($"<color=cyan>✅ Equipped {engine.moduleName} to slot {slotId}</color>");
        }

        public void CloseEngineSelection()
        {
            if (_enginePanel != null)
            {
                _enginePanel.RemoveFromHierarchy();
                _enginePanel = null;
            }
            _targetSlot = null;
        }
    }
}
