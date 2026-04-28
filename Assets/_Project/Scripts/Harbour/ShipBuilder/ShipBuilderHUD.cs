using UnityEngine;
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

        // Reference to child panels
        [SerializeField] private HullSelectionHUD hullSelectionHUD;

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
            _shipBuilderPanel.style.borderBottomLeftRadius = 12;
            _shipBuilderPanel.style.borderBottomRightRadius = 12;
            _shipBuilderPanel.style.paddingLeft = 20;
            _shipBuilderPanel.style.paddingRight = 20;
            _shipBuilderPanel.style.paddingBottom = 20;
            _shipBuilderPanel.style.paddingTop = 20;
            

            // Header
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 20;

            var title = new Label("🚢 SHIP BUILDER");
            title.style.fontSize = 28;
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
            closeBtn.clicked += CloseShipBuilder;
            header.Add(closeBtn);

            _shipBuilderPanel.Add(header);

            // Select Hull Button
            var selectHullBtn = new Button { text = "Select Hull" };
            selectHullBtn.style.fontSize = 18;
            selectHullBtn.style.height = 55;
            selectHullBtn.clicked += () => hullSelectionHUD?.OpenHullSelection();
            _shipBuilderPanel.Add(selectHullBtn);

            // === NEW INNER DISPLAY PANEL (for selected hull) ===
            var innerPanel = new VisualElement { name = "InnerHullDisplay" };
            innerPanel.style.flexGrow = 1;
            innerPanel.style.backgroundColor = new Color(0.1f, 0.15f, 0.28f, 0.85f);
            innerPanel.style.borderTopLeftRadius = 10;
            innerPanel.style.borderTopRightRadius = 10;
            innerPanel.style.borderBottomLeftRadius = 10;
            innerPanel.style.borderBottomRightRadius = 10;
            innerPanel.style.marginTop = 15;
            innerPanel.style.paddingLeft = 20;
            innerPanel.style.paddingRight = 20;
            innerPanel.style.paddingBottom = 20;
            innerPanel.style.paddingTop = 20;

            // This is where the selected hull image + module slots will go
            _mainHullSlot = new VisualElement();
            _mainHullSlot.style.width = 220;
            _mainHullSlot.style.height = 220;
            _mainHullSlot.style.backgroundColor = new Color(0.2f, 0.25f, 0.4f);
            _mainHullSlot.style.borderTopLeftRadius = 110;
            _mainHullSlot.style.borderTopRightRadius = 110;
            _mainHullSlot.style.borderBottomLeftRadius = 110;
            _mainHullSlot.style.borderBottomRightRadius = 110;
            _mainHullSlot.style.alignSelf = Align.Center;
            _mainHullSlot.style.marginBottom = 15;

            innerPanel.Add(_mainHullSlot);
            _shipBuilderPanel.Add(innerPanel);

            root.Add(_shipBuilderPanel);

            Debug.Log("<color=green>Ship Builder Panel Opened with Inner Display Area</color>");
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
        public void SetSelectedHull(string spriteName, string hullName)
        {
            if (_selectedHullNameLabel != null)
                _selectedHullNameLabel.text = hullName;

            if (_mainHullSlot != null)
            {
                // Hard-coded test path
                Sprite hullSprite = Resources.Load<Sprite>("Sprites/Hulls/Gunboat_Hull");

                if (hullSprite != null)
                {
                    _mainHullSlot.style.backgroundImage = new StyleBackground(hullSprite);
                    _mainHullSlot.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                    Debug.Log("<color=green>✅ SUCCESS - Image loaded!</color>");
                }
                else
                {
                    Debug.LogError("❌ STILL FAILED to load Sprites/Hulls/Gunboat_Hull");
                    _mainHullSlot.style.backgroundColor = new Color(0.3f, 0.45f, 0.7f);
                }
            }
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