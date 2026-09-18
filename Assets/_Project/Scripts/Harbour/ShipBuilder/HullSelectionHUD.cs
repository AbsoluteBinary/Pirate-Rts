using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Harbour.Modules;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class HullSelectionHUD : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private ShipBuilderHUD shipBuilderHUD;
        [SerializeField] private HullData[] availableHulls;
        [SerializeField] private HullSlotMask mask;

        private VisualElement _hullPanel;
        private VisualElement _gridHost;
        private VisualElement _tabRow;
        private HullData.HullClass _activeClass = HullData.HullClass.Generic;
        private VisualElement root;

        private static readonly (string label, HullData.HullClass cls)[] Tabs =
        {
            ("Generic Hulls", HullData.HullClass.Generic),
            ("Advanced Hulls", HullData.HullClass.Advanced),
            ("Legendary Hulls", HullData.HullClass.Legendary),
            ("Garrison Hulls", HullData.HullClass.Garrison)
        };

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

        public void OpenHullSelection()
        {
            if (root == null) return;
            CloseHullSelection();

            _hullPanel = new VisualElement { name = "HullSelectionOverlay" };
            _hullPanel.style.position = Position.Absolute;
            _hullPanel.style.top = 0;
            _hullPanel.style.left = 0;
            _hullPanel.style.right = 0;
            _hullPanel.style.bottom = 0;
            _hullPanel.pickingMode = PickingMode.Position;

            var dimmer = new VisualElement();
            dimmer.style.position = Position.Absolute;
            dimmer.style.top = 0;
            dimmer.style.left = 0;
            dimmer.style.right = 0;
            dimmer.style.bottom = 0;
            dimmer.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
            dimmer.RegisterCallback<ClickEvent>(_ => CloseHullSelection());
            _hullPanel.Add(dimmer);

            var card = new VisualElement { name = "HullSelectionCard" };
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

            var title = new Label("Select Hull");
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
            closeBtn.clicked += CloseHullSelection;
            header.Add(closeBtn);
            card.Add(header);

            _tabRow = new VisualElement();
            _tabRow.style.flexDirection = FlexDirection.Row;
            _tabRow.style.flexWrap = Wrap.Wrap;
            _tabRow.style.marginBottom = 12;
            foreach (var tab in Tabs)
            {
                var cls = tab.cls;
                var btn = new Button { text = tab.label, name = $"Tab_{cls}" };
                StyleTab(btn, false);
                btn.clicked += () => ShowClass(cls);
                _tabRow.Add(btn);
            }
            card.Add(_tabRow);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            _gridHost = new VisualElement { name = "HullGrid" };
            _gridHost.style.flexDirection = FlexDirection.Row;
            _gridHost.style.flexWrap = Wrap.Wrap;
            _gridHost.style.justifyContent = Justify.FlexStart;
            scroll.Add(_gridHost);
            card.Add(scroll);

            _hullPanel.Add(card);
            root.Add(_hullPanel);
            ShowClass(HullData.HullClass.Generic);
        }

        private void ShowClass(HullData.HullClass cls)
        {
            _activeClass = cls;
            RefreshTabStyles();
            _gridHost.Clear();

            var list = new List<HullData>();
            if (availableHulls != null)
            {
                foreach (var h in availableHulls)
                {
                    if (h == null) continue;
                    if (h.hullClass == cls) list.Add(h);
                }
            }

            if (list.Count == 0)
            {
                var empty = new Label("No hulls in this class");
                empty.style.color = Color.white;
                empty.style.unityTextAlign = TextAnchor.MiddleCenter;
                empty.style.marginTop = 24;
                empty.style.width = Length.Percent(100);
                _gridHost.Add(empty);
                return;
            }

            foreach (var hull in list)
                _gridHost.Add(CreateHullTile(hull));
        }

        private void RefreshTabStyles()
        {
            if (_tabRow == null) return;
            foreach (var tab in Tabs)
            {
                var btn = _tabRow.Q<Button>($"Tab_{tab.cls}");
                if (btn != null) StyleTab(btn, tab.cls == _activeClass);
            }
        }

        private static void StyleTab(Button btn, bool on)
        {
            btn.style.height = 36;
            btn.style.flexGrow = 1;
            btn.style.flexShrink = 1;
            btn.style.flexBasis = Length.Percent(23);
            btn.style.minWidth = 120;
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

        private VisualElement CreateHullTile(HullData hull)
        {
            var cell = new VisualElement();
            cell.style.width = Length.Percent(23);
            cell.style.minWidth = 140;
            cell.style.marginRight = 8;
            cell.style.marginBottom = 12;
            cell.style.paddingTop = 8;
            cell.style.paddingBottom = 8;
            cell.style.paddingLeft = 8;
            cell.style.paddingRight = 8;
            cell.style.alignItems = Align.Center;
            cell.style.backgroundColor = new Color(0.14f, 0.18f, 0.34f);
            cell.style.borderTopWidth = 1;
            cell.style.borderRightWidth = 1;
            cell.style.borderBottomWidth = 1;
            cell.style.borderLeftWidth = 1;
            cell.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            cell.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            cell.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            cell.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            cell.style.borderTopLeftRadius = 8;
            cell.style.borderTopRightRadius = 8;
            cell.style.borderBottomLeftRadius = 8;
            cell.style.borderBottomRightRadius = 8;

            var imgBtn = new Button { text = "" };
            imgBtn.style.width = Length.Percent(100);
            imgBtn.style.height = 110;
            imgBtn.style.marginBottom = 6;
            imgBtn.style.backgroundColor = new Color(0.08f, 0.12f, 0.25f);
            imgBtn.style.borderTopLeftRadius = 6;
            imgBtn.style.borderTopRightRadius = 6;
            imgBtn.style.borderBottomLeftRadius = 6;
            imgBtn.style.borderBottomRightRadius = 6;
            if (hull.hullImage != null)
            {
                imgBtn.style.backgroundImage = new StyleBackground(hull.hullImage);
                imgBtn.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                imgBtn.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                imgBtn.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                //imgBtn.style.backgroundRepeat = BackgroundRepeat.NoRepeat;
            }
            imgBtn.clicked += () => SelectHull(hull);
            cell.Add(imgBtn);

            var nameLabel = new Label(hull.hullName);
            nameLabel.style.fontSize = 13;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            nameLabel.style.whiteSpace = WhiteSpace.Normal;
            cell.Add(nameLabel);

            return cell;
        }

        private void SelectHull(HullData selectedHull)
        {
            if (shipBuilderHUD != null)
            {
                shipBuilderHUD.SetSelectedHull(selectedHull);
                Debug.Log($"<color=cyan>Hull Selected: {selectedHull.hullName}</color>");
            }
            else
                Debug.LogError("ShipBuilderHUD reference is missing on HullSelectionHUD!");

            CloseHullSelection();
        }

        public void CloseHullSelection()
        {
            if (_hullPanel == null) return;
            _hullPanel.RemoveFromHierarchy();
            _hullPanel = null;
        }
    }
}