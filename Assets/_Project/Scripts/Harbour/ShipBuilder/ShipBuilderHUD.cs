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
        [SerializeField] private WeaponSelectionHUD weaponSelectionHUD;

        private VisualElement _shipBuilderPanel;
        private VisualElement _hullImageElement;
        private Label _hullNameLabel;
        private Label _statsLabel;

        private HullData _currentHull;
        private ShipLoadout _currentLoadout;
        private VisualElement _hullContainer;

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
            _shipBuilderPanel.style.paddingLeft = 20;
            _shipBuilderPanel.style.paddingRight = 20;

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

            _hullContainer = new VisualElement();
            _hullContainer.style.position = Position.Relative;

            _hullImageElement = new VisualElement();
            // Do NOT set backgroundSize → we control size manually for 1:1
            _hullImageElement.RegisterCallback<ClickEvent>(OnHullClicked);

            _hullContainer.Add(_hullImageElement);
            inner.Add(_hullContainer);
            return inner;
        }

        public void SetSelectedHull(HullData hull)
        {
            _currentHull = hull;
            _currentLoadout = new ShipLoadout { hull = hull };

            if (hull?.hullImage == null) return;

            _hullNameLabel.text = hull.hullName;

            var tex = hull.hullImage.texture;

            // === THIS IS THE KEY FOR TRUE 1:1 ===
            _hullContainer.style.width = tex.width;
            _hullContainer.style.height = tex.height;

            _hullImageElement.style.width = tex.width;
            _hullImageElement.style.height = tex.height;
            _hullImageElement.style.backgroundImage = new StyleBackground(hull.hullImage);

            UpdateStatsDisplay();
        }

        private void OnHullClicked(ClickEvent evt)
        {
            if (_currentHull?.moduleSlots == null) return;

            Vector2 localPos = evt.localPosition;
            Vector2 size = _hullImageElement.contentRect.size;

            float scaleX = _currentHull.hullImage.texture.width / size.x;
            float scaleY = _currentHull.hullImage.texture.height / size.y;

            int clickX = Mathf.FloorToInt(localPos.x * scaleX);
            int clickY = Mathf.FloorToInt((size.y - localPos.y) * scaleY);

            Debug.Log($"Clicked raw pixel: ({clickX}, {clickY})");

            ModuleSlot bestSlot = null;
            float bestDistance = float.MaxValue;

            foreach (var slot in _currentHull.moduleSlots)
            {
                float dx = clickX - slot.pixelPosition.x;
                float dy = clickY - slot.pixelPosition.y;
                float distSq = dx * dx + dy * dy;

                if (distSq < bestDistance)
                {
                    bestDistance = distSq;
                    bestSlot = slot;
                }
            }

            if (bestSlot != null)
            {
                float distance = Mathf.Sqrt(bestDistance);
                Debug.Log($"<color=yellow>Closest: {bestSlot.slotId} | Distance: {distance:F1} px</color>");

                if (distance < 20000) // Very loose for now
                {
                    Debug.Log($"<color=lime>✓ REGISTERED HIT → {bestSlot.slotId}</color>");

                    if (bestSlot.acceptedType == ModuleType.Weapon && weaponSelectionHUD != null)
                    {
                        weaponSelectionHUD.OnWeaponEquipped += OnWeaponEquipped;
                        weaponSelectionHUD.OpenWeaponSelection(bestSlot);
                    }
                }
            }
        }

        private void OnWeaponEquipped(WeaponData weapon, ModuleSlot slot)
        {
            weaponSelectionHUD.OnWeaponEquipped -= OnWeaponEquipped;
            if (_currentLoadout != null)
                _currentLoadout.EquipModule(slot, weapon);
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
            panel.style.paddingLeft = 15;
            panel.style.paddingRight = 15;
            panel.style.marginTop = 15;

            _statsLabel = new Label("Select a hull...");
            _statsLabel.style.fontSize = 15;
            _statsLabel.style.color = Color.white;
            panel.Add(_statsLabel);

            return panel;
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