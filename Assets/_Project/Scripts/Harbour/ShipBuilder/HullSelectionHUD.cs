using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class HullSelectionHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument harbourUIDocument;

        public event Action<HullData> OnHullSelected;   // ← Key event

        private VisualElement _hullPanel;

        public void OpenHullSelection()
        {
            if (harbourUIDocument?.rootVisualElement == null) 
            {
                Debug.LogError("HullSelectionHUD: UIDocument is missing!");
                return;
            }

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
            _hullPanel.style.borderBottomLeftRadius = 12;
            _hullPanel.style.borderBottomRightRadius = 12;
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

            // Content Container
            var content = new VisualElement();
            content.style.flexDirection = FlexDirection.Column;
            content.style.paddingLeft = 15;
            content.style.paddingRight = 15;
            content.name = "HullContent";

            // Add Hull Cards
            // Add Hull Cards
            AddHullCard(content, "Gun Boat Hull", "1 Weapon • 1 Armour • 1 Engine", "Small agile hull",
                "Sprites/Hulls/Gunboat_Hull", 
                () => SelectHull("Sprites/Hulls/Gunboat_Hull", "Gun Boat Hull"));

            AddHullCard(content, "Skirmisher Hull", "3 Weapon • 1 Armour • 1 Engine", "Fast attack hull",
                "Sprites/Hulls/Skirmisher_Hull", 
                () => SelectHull("Sprites/Hulls/Skirmisher_Hull", "Skirmisher Hull"));

            AddHullCard(content, "HammerHead Hull", "3 Weapon • 2 Armour • 1 Engine", "Heavy combat hull",
                "Sprites/Hulls/HammerHead_Hull", 
                () => SelectHull("Sprites/Hulls/HammerHead_Hull", "HammerHead Hull"));

            _hullPanel.Add(content);
            root.Add(_hullPanel);

            Debug.Log("<color=green>Hull Selection Panel Opened with 3 cards</color>");
        }
        private void SelectHull(string spritePath, string hullName)
        {
            var shipBuilder = FindObjectOfType<ShipBuilderHUD>();
            if (shipBuilder != null)
            {
                shipBuilder.SetSelectedHull(spritePath, hullName);
            }
            else
            {
                Debug.LogError("ShipBuilderHUD not found in scene!");
            }

            CloseHullSelection();
        }

        private void AddHullCard(VisualElement parent, string hullName, string slots, string description, 
                        string spritePath, Action onSelect = null)
        {
            var card = new VisualElement();
            card.style.flexDirection = FlexDirection.Row;
            card.style.height = 120;
            card.style.backgroundColor = new Color(0.15f, 0.22f, 0.38f);
            card.style.borderTopLeftRadius = 10;
            card.style.borderTopRightRadius = 10;
            card.style.borderBottomLeftRadius = 10;
            card.style.borderBottomRightRadius = 10;
            card.style.paddingLeft = 12;
            card.style.paddingRight = 12;
            card.style.paddingTop = 12;
            card.style.paddingBottom = 12;
            card.style.marginBottom = 10;

            // === HEXAGONAL HULL PREVIEW ===
            var hullSlot = new VisualElement();
            hullSlot.style.width = 95;
            hullSlot.style.height = 95;
            hullSlot.style.backgroundColor = new Color(0.25f, 0.3f, 0.45f);
            hullSlot.style.borderTopLeftRadius = 48;
            hullSlot.style.borderTopRightRadius = 48;
            hullSlot.style.borderBottomLeftRadius = 48;
            hullSlot.style.borderBottomRightRadius = 48;
            hullSlot.style.borderTopWidth = 5;
            hullSlot.style.borderRightWidth = 5;
            hullSlot.style.borderBottomWidth = 5;
            hullSlot.style.borderLeftWidth = 5;
            hullSlot.style.borderTopColor = new Color(0.5f, 0.8f, 1f);
            hullSlot.style.marginRight = 20;
            hullSlot.style.alignSelf = Align.Center;

            // ←←← LOAD PREVIEW IMAGE ←←←
            Sprite hullSprite = Resources.Load<Sprite>(spritePath);
            if (hullSprite != null)
            {
                hullSlot.style.backgroundImage = new StyleBackground(hullSprite);
                hullSlot.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                hullSlot.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                hullSlot.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                Debug.Log($"<color=green>Loaded preview: {spritePath}</color>");
            }
            else
            {
                Debug.LogWarning($"⚠️ Could not load hull preview: {spritePath}");
            }

            card.Add(hullSlot);

            // Info section
            var info = new VisualElement();
            info.style.flexGrow = 1;
            info.style.flexDirection = FlexDirection.Column;
            info.style.justifyContent = Justify.Center;

            var nameLabel = new Label(hullName);
            nameLabel.style.fontSize = 19;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginBottom = 4;

            var slotsLabel = new Label(slots);
            slotsLabel.style.fontSize = 14;
            slotsLabel.style.color = new Color(0.6f, 0.9f, 1f);
            slotsLabel.style.marginBottom = 6;

            var descLabel = new Label(description);
            descLabel.style.fontSize = 13;
            descLabel.style.color = new Color(0.8f, 0.8f, 0.85f);

            info.Add(nameLabel);
            info.Add(slotsLabel);
            info.Add(descLabel);
            card.Add(info);

            // Select Button
            var selectBtn = new Button { text = "Select" };
            selectBtn.style.width = 130;
            selectBtn.style.height = 55;
            selectBtn.style.alignSelf = Align.Center;
            selectBtn.style.backgroundColor = new Color(0.25f, 0.6f, 0.95f);
            selectBtn.clicked += () => onSelect?.Invoke();
            card.Add(selectBtn);

            parent.Add(card);
        }
        

        private HullData CreateTempHullData(string name, string slots, string desc)
        {
            var temp = ScriptableObject.CreateInstance<HullData>();
            temp.hullName = name;
            temp.slotInfo = slots;
            temp.description = desc;
            return temp;
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