using System;
using _Project.Scripts.Harbour.Economy;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.Data.HUDData
{
    public class ResourceHUD : MonoBehaviour
    {
        [SerializeField] private ResourceWalletHolder walletHolder;

        public event Action OnCloseRequested;

        private VisualElement _root;
        private VisualElement _panelRoot;

        private static readonly string[] Materials =
        {
            "Oil", "Iron", "Steel",
            "Energy", "Aluminium", "Lumber",
            "Alloy", "Cloth", "Uranium"
        };

        private static readonly string[] Currencies = { "Gold", "Silver" };
        private static readonly string[] Tokens = { "PieceOfEight", "Shilling" };

        public void OpenResources(VisualElement root)
        {
            if (root == null)
            {
                Debug.LogError("ResourceHUD.OpenResources: root is null.");
                return;
            }

            CloseResources();
            _root = root;

            _panelRoot = new VisualElement { name = "ResourceOverlay" };
            _panelRoot.style.position = Position.Absolute;
            _panelRoot.style.top = 0;
            _panelRoot.style.left = 0;
            _panelRoot.style.right = 0;
            _panelRoot.style.bottom = 0;
            _panelRoot.pickingMode = PickingMode.Position;

            var card = new VisualElement { name = "ResourceCard" };
            card.style.position = Position.Absolute;
            card.style.top = Length.Percent(10);
            card.style.bottom = Length.Percent(10);
            card.style.left = Length.Percent(18);
            card.style.right = Length.Percent(18);
            card.style.backgroundColor = new Color(0.06f, 0.10f, 0.22f, 0.98f);
            card.style.paddingTop = 16;
            card.style.paddingBottom = 16;
            card.style.paddingLeft = 20;
            card.style.paddingRight = 20;
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

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 14;

            var title = new Label("Resources");
            title.style.flexGrow = 1;
            title.style.fontSize = 22;
            title.style.color = Color.cyan;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.Add(title);

            var close = new Button { text = "✕" };
            close.style.width = 36;
            close.style.height = 36;
            close.style.fontSize = 18;
            close.style.color = Color.white;
            close.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            close.style.borderTopLeftRadius = 6;
            close.style.borderTopRightRadius = 6;
            close.style.borderBottomLeftRadius = 6;
            close.style.borderBottomRightRadius = 6;
            close.clicked += () => OnCloseRequested?.Invoke();
            header.Add(close);
            card.Add(header);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.Add(MakeSection("Materials", Materials));
            scroll.Add(MakeSection("Currencies", Currencies));
            scroll.Add(MakeSection("Tokens", Tokens));
            card.Add(scroll);

            _panelRoot.Add(card);
            _root.Add(_panelRoot);
        }

        public void CloseResources()
        {
            if (_panelRoot == null) return;
            _panelRoot.RemoveFromHierarchy();
            _panelRoot = null;
        }

        private VisualElement MakeSection(string heading, string[] ids)
        {
            var block = new VisualElement();
            block.style.marginBottom = 16;

            var h = new Label(heading);
            h.style.fontSize = 16;
            h.style.color = Color.white;
            h.style.unityFontStyleAndWeight = FontStyle.Bold;
            h.style.unityTextAlign = TextAnchor.MiddleCenter;
            h.style.marginBottom = 8;
            block.Add(h);

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.Center;
            foreach (var id in ids)
                grid.Add(MakeCell(id));
            block.Add(grid);
            return block;
        }

        private VisualElement MakeCell(string id)
        {
            var def = walletHolder != null && walletHolder.Catalog != null
                ? walletHolder.Catalog.Get(id)
                : null;

            var cell = new VisualElement { name = $"ResCell_{id}" };
            cell.style.width = Length.Percent(31);
            cell.style.minWidth = 140;
            cell.style.flexDirection = FlexDirection.Row;
            cell.style.alignItems = Align.Center;
            cell.style.marginRight = 8;
            cell.style.marginBottom = 8;
            cell.style.paddingTop = 6;
            cell.style.paddingBottom = 6;
            cell.style.paddingLeft = 8;
            cell.style.paddingRight = 8;
            cell.style.backgroundColor = new Color(0.14f, 0.18f, 0.34f);
            cell.style.borderTopLeftRadius = 6;
            cell.style.borderTopRightRadius = 6;
            cell.style.borderBottomLeftRadius = 6;
            cell.style.borderBottomRightRadius = 6;

            var nameLab = new Label(def != null && !string.IsNullOrEmpty(def.displayName)
                ? def.displayName
                : id);
            nameLab.style.fontSize = 12;
            nameLab.style.color = new Color(0.7f, 0.85f, 1f);
            nameLab.style.flexShrink = 0;
            nameLab.style.marginRight = 8;
            cell.Add(nameLab);

            int have = walletHolder != null && walletHolder.Wallet != null
                ? walletHolder.Wallet.Get(id)
                : 0;
            var amount = new Label(have.ToString());
            amount.name = $"ResAmount_{id}";
            amount.style.flexGrow = 1;
            amount.style.fontSize = 14;
            amount.style.color = Color.white;
            amount.style.borderBottomWidth = 1;
            amount.style.borderBottomColor = Color.white;
            amount.style.paddingBottom = 1;
            cell.Add(amount);

            var icon = new VisualElement();
            icon.style.width = 22;
            icon.style.height = 22;
            icon.style.flexShrink = 0;
            icon.style.marginLeft = 8;
            icon.style.backgroundColor = new Color(0.16f, 0.22f, 0.40f);
            if (def?.icon != null)
            {
                icon.style.backgroundImage = new StyleBackground(def.icon);
                icon.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
                icon.style.backgroundColor = Color.clear;
            }
            cell.Add(icon);

            return cell;
        }
    }
}