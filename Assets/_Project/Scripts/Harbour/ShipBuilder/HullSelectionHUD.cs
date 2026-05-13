using System;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class HullSelectionHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument harbourUIDocument;
        [SerializeField] private ShipBuilderHUD shipBuilderHUD;   // ← Direct reference (best)

        // Optional: Reference to database if you want to load hulls dynamically later
        [SerializeField] private HullData[] availableHulls;       // Drag your HullData assets here in Inspector
        [SerializeField] private HullSlotMask mask;
        private VisualElement _hullPanel;

        public void OpenHullSelection()
        {
            if (harbourUIDocument?.rootVisualElement == null) return;

            var root = harbourUIDocument.rootVisualElement;
            if (_hullPanel != null) _hullPanel.RemoveFromHierarchy();

            _hullPanel = new VisualElement { name = "HullSelectionPanel" };
            _hullPanel.style.position = Position.Absolute;
            _hullPanel.style.top = 100;
            _hullPanel.style.left = 100;
            _hullPanel.style.right = 100;
            _hullPanel.style.bottom = 100;
            _hullPanel.style.backgroundColor = new Color(0.08f, 0.12f, 0.28f, 0.98f);
            _hullPanel.style.borderTopLeftRadius = 12;
            _hullPanel.style.borderTopRightRadius = 12;
            _hullPanel.style.paddingLeft = 25;
            _hullPanel.style.paddingRight = 25;
            _hullPanel.style.paddingTop = 25;
            _hullPanel.style.paddingBottom = 25;

            // Header
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 20;

            var title = new Label("Select Hull");
            title.style.fontSize = 26;
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.fontSize = 22;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.clicked += CloseHullSelection;
            header.Add(closeBtn);

            _hullPanel.Add(header);

            // Hull Cards
            var content = new VisualElement();
            content.style.flexDirection = FlexDirection.Column;
            //content.style.gap = 12;

            foreach (var hull in availableHulls)
            {
                if (hull == null) continue;
                var card = CreateHullCard(hull);
                content.Add(card);
            }

            _hullPanel.Add(content);
            root.Add(_hullPanel);

            Debug.Log("<color=green>Hull Selection Opened with real HullData assets</color>");
        }

        private VisualElement CreateHullCard(HullData hull)
        {
            var card = new VisualElement();
            card.style.flexDirection = FlexDirection.Row;
            card.style.height = 130;
            card.style.backgroundColor = new Color(0.15f, 0.22f, 0.38f);
            card.style.borderTopLeftRadius = 10;
            card.style.borderTopRightRadius = 10;
            card.style.paddingLeft = 12;
            card.style.paddingRight = 12;
            card.style.paddingTop = 12;
            card.style.paddingBottom = 12;
            card.style.marginBottom = 8;

            // Hull Preview
            var preview = new VisualElement();
            preview.style.width = 110;
            preview.style.height = 110;
            preview.style.marginRight = 20;
            preview.style.alignSelf = Align.Center;
            if (hull.hullImage != null)
            {
                preview.style.backgroundImage = new StyleBackground(hull.hullImage);
                preview.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            card.Add(preview);

            // Info
            var info = new VisualElement();
            info.style.flexGrow = 1;
            info.style.flexDirection = FlexDirection.Column;
            info.style.justifyContent = Justify.Center;

            var nameLabel = new Label(hull.hullName);
            nameLabel.style.fontSize = 20;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            var slotsLabel = new Label(hull.slotInfo);
            slotsLabel.style.fontSize = 14;
            slotsLabel.style.color = new Color(0.6f, 0.9f, 1f);

            var descLabel = new Label(hull.description);
            descLabel.style.fontSize = 13;
            descLabel.style.color = new Color(0.8f, 0.8f, 0.85f);

            info.Add(nameLabel);
            info.Add(slotsLabel);
            info.Add(descLabel);
            card.Add(info);

            // Select Button
            var selectBtn = new Button { text = "Select" };
            selectBtn.style.width = 140;
            selectBtn.style.height = 60;
            selectBtn.style.alignSelf = Align.Center;
            selectBtn.style.backgroundColor = new Color(0.25f, 0.6f, 0.95f);
            selectBtn.clicked += () => SelectHull(hull);
            card.Add(selectBtn);

            return card;
        }

        private void SelectHull(HullData selectedHull)
        {
            if (shipBuilderHUD != null)
            {
                shipBuilderHUD.SetSelectedHull(selectedHull);
                Debug.Log($"<color=cyan>Hull Selected: {selectedHull.hullName}</color>");
            }
            else
            {
                Debug.LogError("ShipBuilderHUD reference is missing on HullSelectionHUD!");
            }

            CloseHullSelection();
        }

        public void CloseHullSelection()
        {
            if (_hullPanel != null)
            {
                _hullPanel.RemoveFromHierarchy();
                _hullPanel = null;
            }
        }
    }
}