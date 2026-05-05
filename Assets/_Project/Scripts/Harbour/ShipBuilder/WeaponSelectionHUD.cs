using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class WeaponSelectionHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument harbourUIDocument;

        private VisualElement _weaponPanel;
        private ShipBuilderHUD shipBuilderHUD;
        
        private void Awake()
        {
            shipBuilderHUD = FindObjectOfType<ShipBuilderHUD>();
        }

        public void OpenWeaponSelection()
        {
            if (harbourUIDocument?.rootVisualElement == null) return;

            var root = harbourUIDocument.rootVisualElement;
            if (_weaponPanel != null) _weaponPanel.RemoveFromHierarchy();

            _weaponPanel = new VisualElement { name = "WeaponSelectionPanel" };
            _weaponPanel.style.position = Position.Absolute;
            _weaponPanel.style.top = 100;
            _weaponPanel.style.left = 100;
            _weaponPanel.style.right = 100;
            _weaponPanel.style.bottom = 100;
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
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 20;

            var title = new Label("🔫 SELECT WEAPON");
            title.style.fontSize = 26;
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.fontSize = 22;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.clicked += CloseWeaponSelection;
            header.Add(closeBtn);

            _weaponPanel.Add(header);

            // Tabs
            var tabContainer = new VisualElement();
            tabContainer.style.flexDirection = FlexDirection.Row;
            tabContainer.style.marginBottom = 15;

            var allTab = CreateTabButton("All Weapons", () => ShowWeaponGrid("All"));
            var cannonTab = CreateTabButton("Cannons", () => ShowWeaponGrid("Cannons"));
            var missileTab = CreateTabButton("Missiles", () => ShowWeaponGrid("Missiles"));

            tabContainer.Add(allTab);
            tabContainer.Add(cannonTab);
            tabContainer.Add(missileTab);
            _weaponPanel.Add(tabContainer);

            // Grid Area
            var gridArea = new VisualElement { name = "WeaponGrid" };
            gridArea.style.flexGrow = 1;
            gridArea.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.97f);
            gridArea.style.paddingTop = 15;
            _weaponPanel.Add(gridArea);

            ShowWeaponGrid("All"); // Default tab

            root.Add(_weaponPanel);
            Debug.Log("<color=green>Weapon Selection Panel Opened</color>");
        }

        private Button CreateTabButton(string text, Action onClick)
        {
            var btn = new Button { text = text };
            btn.style.flexGrow = 1;
            btn.style.height = 45;
            btn.style.marginRight = 8;
            btn.style.fontSize = 16;
            btn.clicked += onClick;
            return btn;
        }

        private void ShowWeaponGrid(string category)
        {
            var grid = _weaponPanel.Q<VisualElement>("WeaponGrid");
            if (grid == null) return;
            grid.Clear();

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexWrap = Wrap.Wrap;
            container.style.justifyContent = Justify.FlexStart;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;

            // Create placeholder empty slots (for now)
            for (int i = 0; i < 12; i++)
            {
                var slot = new VisualElement();
                slot.style.width = 90;
                slot.style.height = 90;
                slot.style.backgroundColor = new Color(0.2f, 0.2f, 0.35f);
                slot.style.borderTopLeftRadius = 8;
                slot.style.borderTopRightRadius = 8;
                slot.style.borderBottomLeftRadius = 8;
                slot.style.borderBottomRightRadius = 8;
                slot.style.marginTop = 8;
                slot.style.marginBottom = 8;
                slot.style.marginLeft = 8;
                slot.style.marginRight = 8;
                slot.style.alignItems = Align.Center;
                slot.style.justifyContent = Justify.Center;

                // Placeholder weapon image (we'll use this for now)
                var weaponIcon = new VisualElement();
                weaponIcon.style.width = 70;
                weaponIcon.style.height = 70;
                weaponIcon.style.backgroundColor = new Color(0.9f, 0.6f, 0.2f); // Placeholder orange
                weaponIcon.style.borderTopLeftRadius = 6;
                weaponIcon.style.borderTopRightRadius = 6;
                weaponIcon.style.borderBottomLeftRadius = 6;
                weaponIcon.style.borderBottomRightRadius = 6;

                var label = new Label("Cannon");
                label.style.fontSize = 12;
                label.style.color = Color.white;
                label.style.unityTextAlign = TextAnchor.MiddleCenter;

                slot.Add(weaponIcon);
                slot.Add(label);

                // === EQUIP LOGIC ===
                slot.RegisterCallback<ClickEvent>(evt =>
                {
                    if (shipBuilderHUD != null && shipBuilderHUD.HasPendingWeaponSlot())
                    {
                        shipBuilderHUD.EquipWeaponToSlot("Basic Cannon", weaponIcon.style.backgroundColor.value);
                        CloseWeaponSelection();
                        Debug.Log("<color=green>✅ Weapon equipped to slot!</color>");
                    }
                });

                container.Add(slot);
            }

            grid.Add(container);
        }

        public void CloseWeaponSelection()
        {
            if (_weaponPanel != null)
            {
                _weaponPanel.RemoveFromHierarchy();
                _weaponPanel = null;
            }
        }
    }
}