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
        private VisualElement _tabRow;
        private VisualElement _gridHost;
        private ModuleSlot _targetSlot;
        private string _activeTab = "All";
        private VisualElement root;

        private const string AllTab = "All";

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
            if (root == null || moduleDatabase == null)
            {
                Debug.LogError("EngineSelectionHUD: Missing ModuleDatabase.");
                return;
            }

            _targetSlot = targetSlot;
            CloseEngineSelectionKeepSlot();

            _enginePanel = new VisualElement { name = "EngineSelectionOverlay" };
            _enginePanel.style.position = Position.Absolute;
            _enginePanel.style.top = 0;
            _enginePanel.style.left = 0;
            _enginePanel.style.right = 0;
            _enginePanel.style.bottom = 0;
            _enginePanel.pickingMode = PickingMode.Position;

            var dimmer = new VisualElement();
            dimmer.style.position = Position.Absolute;
            dimmer.style.top = 0;
            dimmer.style.left = 0;
            dimmer.style.right = 0;
            dimmer.style.bottom = 0;
            dimmer.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
            dimmer.RegisterCallback<ClickEvent>(_ => CloseEngineSelection());
            _enginePanel.Add(dimmer);

            var card = new VisualElement { name = "EngineSelectionCard" };
            card.style.position = Position.Absolute;
            card.style.top = Length.Percent(8);
            card.style.bottom = Length.Percent(8);
            card.style.left = Length.Percent(8);
            card.style.right = Length.Percent(8);
            card.style.backgroundColor = new Color(0.06f, 0.10f, 0.22f, 0.98f);
            card.style.paddingTop = 16;
            card.style.paddingBottom = 16;
            card.style.paddingLeft = 18;
            card.style.paddingRight = 18;
            card.style.borderTopWidth = 2;
            card.style.borderRightWidth = 2;
            card.style.borderBottomWidth = 2;
            card.style.borderLeftWidth = 2;
            card.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
            card.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
            card.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            card.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
            card.style.borderTopLeftRadius = 10;
            card.style.borderTopRightRadius = 10;
            card.style.borderBottomLeftRadius = 10;
            card.style.borderBottomRightRadius = 10;
            card.pickingMode = PickingMode.Position;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 12;

            var title = new Label("Select Engine");
            title.style.flexGrow = 1;
            title.style.fontSize = 22;
            title.style.color = Color.cyan;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.Add(title);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.width = 36;
            closeBtn.style.height = 36;
            closeBtn.style.fontSize = 18;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.style.borderTopLeftRadius = 6;
            closeBtn.style.borderTopRightRadius = 6;
            closeBtn.style.borderBottomLeftRadius = 6;
            closeBtn.style.borderBottomRightRadius = 6;
            closeBtn.clicked += CloseEngineSelection;
            header.Add(closeBtn);
            card.Add(header);

            _tabRow = new VisualElement { name = "EngineTabs" };
            _tabRow.style.flexDirection = FlexDirection.Row;
            _tabRow.style.flexWrap = Wrap.Wrap;
            _tabRow.style.marginBottom = 12;
            _tabRow.Add(MakeTab(AllTab, () => ShowEngines(EngineCategory.None, AllTab)));
            foreach (EngineCategory cat in Enum.GetValues(typeof(EngineCategory)))
            {
                if (cat == EngineCategory.None) continue;
                var captured = cat;
                _tabRow.Add(MakeTab(cat.ToString(), () => ShowEngines(captured, captured.ToString())));
            }
            card.Add(_tabRow);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            _gridHost = new VisualElement { name = "EngineGrid" };
            _gridHost.style.flexDirection = FlexDirection.Row;
            _gridHost.style.flexWrap = Wrap.Wrap;
            scroll.Add(_gridHost);
            card.Add(scroll);

            _enginePanel.Add(card);
            root.Add(_enginePanel);
            ShowEngines(EngineCategory.None, AllTab);
            Debug.Log("<color=green>Engine Selection HUD Opened</color>");
        }

        private Button MakeTab(string label, Action onClick)
        {
            var btn = new Button { text = label, name = $"Tab_{label}" };
            StyleTab(btn, false);
            btn.clicked += onClick;
            return btn;
        }

        private static void StyleTab(Button btn, bool on)
        {
            btn.style.height = 36;
            btn.style.flexGrow = 1;
            btn.style.flexShrink = 1;
            btn.style.flexBasis = Length.Percent(22);
            btn.style.minWidth = 90;
            btn.style.marginRight = 6;
            btn.style.marginBottom = 6;
            btn.style.fontSize = 13;
            btn.style.color = Color.white;
            btn.style.backgroundColor = on
                ? new Color(0.18f, 0.42f, 0.62f)
                : new Color(0.14f, 0.18f, 0.34f);
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }

        private void ShowEngines(EngineCategory category, string tabName)
        {
            _activeTab = tabName;
            if (_tabRow != null)
            {
                foreach (var child in _tabRow.Children())
                {
                    if (child is Button b)
                        StyleTab(b, b.name == $"Tab_{_activeTab}");
                }
            }

            _gridHost.Clear();
            List<EngineData> list = moduleDatabase.GetFilteredEngines(category);
            if (list == null || list.Count == 0)
            {
                var empty = new Label("No engines in this tab");
                empty.style.color = Color.white;
                empty.style.unityTextAlign = TextAnchor.MiddleCenter;
                empty.style.marginTop = 24;
                empty.style.width = Length.Percent(100);
                _gridHost.Add(empty);
                return;
            }

            foreach (var e in list)
            {
                if (e == null) continue;
                _gridHost.Add(CreateEngineTile(e));
            }
        }

        private VisualElement CreateEngineTile(EngineData engine)
        {
            var cell = new VisualElement();
            cell.style.width = Length.Percent(18);
            cell.style.minWidth = 120;
            cell.style.marginRight = 8;
            cell.style.marginBottom = 12;
            cell.style.paddingTop = 8;
            cell.style.paddingBottom = 8;
            cell.style.paddingLeft = 8;
            cell.style.paddingRight = 8;
            cell.style.alignItems = Align.Center;
            cell.style.backgroundColor = new Color(0.14f, 0.18f, 0.34f);
            cell.style.borderTopWidth = 3;
            cell.style.borderRightWidth = 1;
            cell.style.borderBottomWidth = 1;
            cell.style.borderLeftWidth = 1;
            cell.style.borderTopColor = GetRarityColor(engine.rarity);
            cell.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            cell.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            cell.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            cell.style.borderTopLeftRadius = 8;
            cell.style.borderTopRightRadius = 8;
            cell.style.borderBottomLeftRadius = 8;
            cell.style.borderBottomRightRadius = 8;

            var imgBtn = new Button { text = "" };
            imgBtn.style.width = Length.Percent(100);
            imgBtn.style.height = 90;
            imgBtn.style.marginBottom = 6;
            imgBtn.style.backgroundColor = new Color(0.08f, 0.12f, 0.25f);
            imgBtn.style.borderTopLeftRadius = 6;
            imgBtn.style.borderTopRightRadius = 6;
            imgBtn.style.borderBottomLeftRadius = 6;
            imgBtn.style.borderBottomRightRadius = 6;
            if (engine.icon != null)
            {
                imgBtn.style.backgroundImage = new StyleBackground(engine.icon);
                imgBtn.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                imgBtn.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                imgBtn.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                //imgBtn.style.backgroundRepeat = BackgroundRepeat.NoRepeat;
            }
            imgBtn.clicked += () => EquipEngineToSlot(engine);
            cell.Add(imgBtn);

            var nameLabel = new Label(engine.moduleName);
            nameLabel.style.fontSize = 13;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            nameLabel.style.whiteSpace = WhiteSpace.Normal;
            cell.Add(nameLabel);

            return cell;
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
            Debug.Log($"<color=cyan>Equipped {engine.moduleName} to slot {slotId}</color>");
        }

        private void CloseEngineSelectionKeepSlot()
        {
            if (_enginePanel == null) return;
            _enginePanel.RemoveFromHierarchy();
            _enginePanel = null;
        }

        public void CloseEngineSelection()
        {
            CloseEngineSelectionKeepSlot();
            _targetSlot = null;
        }
    }
}
