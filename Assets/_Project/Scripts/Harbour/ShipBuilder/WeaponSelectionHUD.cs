using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class WeaponSelectionHUD : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private ModuleDatabase moduleDatabase;

        // Event fired when a weapon is selected
        public event Action<WeaponData, ModuleSlot> OnWeaponEquipped;

        private VisualElement _weaponPanel;
        private ModuleSlot _targetSlot;           // The slot that was clicked
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
        public void OpenWeaponSelection(ModuleSlot targetSlot)
        {
            if (root == null) return;
            
            if (moduleDatabase == null)
            {
                Debug.LogError("WeaponSelectionHUD: Missing UIDocument or ModuleDatabase!");
                return;
            }

            _targetSlot = targetSlot;

            
            if (_weaponPanel != null) _weaponPanel.RemoveFromHierarchy();

            _weaponPanel = new VisualElement { name = "WeaponSelectionPanel" };
            _weaponPanel.style.position = Position.Absolute;
            _weaponPanel.style.top = 80;
            _weaponPanel.style.left = 80;
            _weaponPanel.style.right = 80;
            _weaponPanel.style.bottom = 80;
            _weaponPanel.style.backgroundColor = new Color(0.08f, 0.12f, 0.28f, 0.98f);
            _weaponPanel.style.borderTopLeftRadius = 12;
            _weaponPanel.style.borderTopRightRadius = 12;
            _weaponPanel.style.borderBottomLeftRadius = 12;
            _weaponPanel.style.borderBottomRightRadius = 12;
            _weaponPanel.style.paddingLeft = 25;
            _weaponPanel.style.paddingRight = 25;
            _weaponPanel.style.paddingTop = 25;
            _weaponPanel.style.paddingBottom = 25;

            // Header
            var header = CreateHeader();
            _weaponPanel.Add(header);

            // Tabs
            var tabContainer = CreateTabContainer();
            _weaponPanel.Add(tabContainer);

            // Content Area
            _currentTabContent = new VisualElement { name = "WeaponGridContent" };
            _currentTabContent.style.flexGrow = 1;
            _currentTabContent.style.marginTop = 15;
            _weaponPanel.Add(_currentTabContent);

            root.Add(_weaponPanel);

            // Show "All" by default
            ShowWeaponsByCategory(WeaponCategory.None);

            Debug.Log("<color=green>Weapon Selection HUD Opened</color>");
        }

        private VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 15;

            var title = new Label("Select Weapon");
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
            closeBtn.clicked += CloseWeaponSelection;
            header.Add(closeBtn);

            return header;
        }

        private VisualElement CreateTabContainer()
        {
            var tabRow = new VisualElement();
            tabRow.style.flexDirection = FlexDirection.Row;
            tabRow.style.marginBottom = 10;
            tabRow.style.flexWrap = Wrap.Wrap;

            // All Tab
            tabRow.Add(CreateTabButton(ALL_TAB, () => ShowWeaponsByCategory(WeaponCategory.None)));

            // Category Tabs
            foreach (WeaponCategory cat in Enum.GetValues(typeof(WeaponCategory)))
            {
                if (cat == WeaponCategory.None) continue;
                tabRow.Add(CreateTabButton(cat.ToString(), () => ShowWeaponsByCategory(cat)));
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

        private void ShowWeaponsByCategory(WeaponCategory category)
        {
            if (_currentTabContent == null) return;

            _currentTabContent.Clear();

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.FlexStart;
            //grid.style.gap = 12;

            List<WeaponData> weapons = moduleDatabase.GetFilteredWeapons(category);

            foreach (var weapon in weapons)
            {
                var card = CreateWeaponCard(weapon);
                grid.Add(card);
            }

            _currentTabContent.Add(grid);
        }

        private VisualElement CreateWeaponCard(WeaponData weapon)
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

            // Icon
            var icon = new VisualElement();
            icon.style.width = 100;
            icon.style.height = 100;
            if (weapon.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(weapon.icon);
                icon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            else
            {
                icon.style.backgroundColor = new Color(0.3f, 0.4f, 0.6f);
            }
            card.Add(icon);

            // Name
            var nameLabel = new Label(weapon.moduleName);
            nameLabel.style.fontSize = 16;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginTop = 8;
            card.Add(nameLabel);

            // Stats
            var statsLabel = new Label(weapon.GetStatsSummary());
            statsLabel.style.fontSize = 12;
            statsLabel.style.color = new Color(0.7f, 0.85f, 1f);
            statsLabel.style.marginTop = 4;
            statsLabel.style.whiteSpace = WhiteSpace.Normal;
            card.Add(statsLabel);

            // Rarity color accent
            card.style.borderTopWidth = 4;
            card.style.borderTopColor = GetRarityColor(weapon.rarity);

            // Click to equip
            card.RegisterCallback<ClickEvent>(evt =>
            {
                EquipWeaponToSlot(weapon);
            });

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

        private void EquipWeaponToSlot(WeaponData weapon)
        {
            if (_targetSlot == null || weapon == null) return;

            string slotId = _targetSlot.slotId;
            _targetSlot.equippedModule = weapon;
            OnWeaponEquipped?.Invoke(weapon, _targetSlot);
            CloseWeaponSelection();

            Debug.Log($"<color=cyan>Equipped {weapon.moduleName} to slot {slotId}</color>");
        }

        public void CloseWeaponSelection()
        {
            if (_weaponPanel != null)
            {
                _weaponPanel.RemoveFromHierarchy();
                _weaponPanel = null;
            }
            _targetSlot = null;
        }
    }
}