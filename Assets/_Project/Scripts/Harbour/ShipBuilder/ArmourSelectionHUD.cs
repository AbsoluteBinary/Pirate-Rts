using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class ArmourSelectionHUD : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private ModuleDatabase moduleDatabase;

        public event Action<ArmourData, ModuleSlot> OnArmourEquipped;

        private VisualElement _armourPanel;
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

        public void OpenArmourSelection(ModuleSlot targetSlot)
        {
            
            if (root == null || moduleDatabase == null)
            {
                Debug.LogError("ArmourSelectionHUD: Missing UIDocument or ModuleDatabase!");
                return;
            }

            _targetSlot = targetSlot;

            if (root == null) return;
            if (_armourPanel != null) _armourPanel.RemoveFromHierarchy();

            _armourPanel = new VisualElement { name = "ArmourSelectionPanel" };
            _armourPanel.style.position = Position.Absolute;
            _armourPanel.style.top = 80;
            _armourPanel.style.left = 80;
            _armourPanel.style.right = 80;
            _armourPanel.style.bottom = 80;
            _armourPanel.style.backgroundColor = new Color(0.08f, 0.12f, 0.28f, 0.98f);
            _armourPanel.style.borderTopLeftRadius = 12;
            _armourPanel.style.borderTopRightRadius = 12;
            _armourPanel.style.borderBottomLeftRadius = 12;
            _armourPanel.style.borderBottomRightRadius = 12;
            _armourPanel.style.paddingLeft = 25;
            _armourPanel.style.paddingRight = 25;
            _armourPanel.style.paddingTop = 25;
            _armourPanel.style.paddingBottom = 25;

            var header = CreateHeader();
            _armourPanel.Add(header);

            var tabContainer = CreateTabContainer();
            _armourPanel.Add(tabContainer);

            _currentTabContent = new VisualElement { name = "ArmourGridContent" };
            _currentTabContent.style.flexGrow = 1;
            _currentTabContent.style.marginTop = 15;
            _armourPanel.Add(_currentTabContent);

            root.Add(_armourPanel);

            ShowArmourByCategory(ArmourCategory.Light);

            Debug.Log("<color=green>Armour Selection HUD Opened</color>");
        }

        private VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 15;

            var title = new Label("Select Armour");
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
            closeBtn.clicked += CloseArmourSelection;
            header.Add(closeBtn);

            return header;
        }

        private VisualElement CreateTabContainer()
        {
            var tabRow = new VisualElement();
            tabRow.style.flexDirection = FlexDirection.Row;
            tabRow.style.marginBottom = 10;
            tabRow.style.flexWrap = Wrap.Wrap;

            tabRow.Add(CreateTabButton(ALL_TAB, () => ShowArmourByCategory(ArmourCategory.None)));

            foreach (ArmourCategory cat in Enum.GetValues(typeof(ArmourCategory)))
            {
                if (cat == ArmourCategory.None) continue;
                tabRow.Add(CreateTabButton(cat.ToString(), () => ShowArmourByCategory(cat)));
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

        private void ShowArmourByCategory(ArmourCategory category)
        {
            if (_currentTabContent == null) return;

            _currentTabContent.Clear();

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.FlexStart;

            List<ArmourData> armourList = moduleDatabase.GetFilteredArmour(category);

            foreach (var armour in armourList)
            {
                var card = CreateArmourCard(armour);
                grid.Add(card);
            }

            _currentTabContent.Add(grid);
        }

        private VisualElement CreateArmourCard(ArmourData armour)
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
            if (armour.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(armour.icon);
                icon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            else
            {
                icon.style.backgroundColor = new Color(0.3f, 0.4f, 0.6f);
            }
            card.Add(icon);

            var nameLabel = new Label(armour.moduleName);
            nameLabel.style.fontSize = 16;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginTop = 8;
            card.Add(nameLabel);

            var statsLabel = new Label(armour.GetStatsSummary());
            statsLabel.style.fontSize = 12;
            statsLabel.style.color = new Color(0.7f, 0.85f, 1f);
            statsLabel.style.marginTop = 4;
            statsLabel.style.whiteSpace = WhiteSpace.Normal;
            card.Add(statsLabel);

            card.style.borderTopWidth = 4;
            card.style.borderTopColor = GetRarityColor(armour.rarity);

            card.RegisterCallback<ClickEvent>(evt => EquipArmourToSlot(armour));

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

        private void EquipArmourToSlot(ArmourData armour)
        {
            if (_targetSlot == null || armour == null) return;

            string slotId = _targetSlot.slotId;
            _targetSlot.equippedModule = armour;
            OnArmourEquipped?.Invoke(armour, _targetSlot);
            CloseArmourSelection();

            Debug.Log($"<color=cyan>✅ Equipped {armour.moduleName} to slot {slotId}</color>");
        }

        public void CloseArmourSelection()
        {
            if (_armourPanel != null)
            {
                _armourPanel.RemoveFromHierarchy();
                _armourPanel = null;
            }
            _targetSlot = null;
        }
    }
}