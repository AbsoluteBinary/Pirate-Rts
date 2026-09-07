using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.Data.HUDData
{
    public class DockHUD : MonoBehaviour
    {
        private const int FleetSize = 5;
        private const int FlagShipIndex = 2;

        [Header("Slot Visuals (assign later)")]
        [SerializeField] private Sprite hexSlotSprite;

        [Header("Repair (assign later)")]
        [SerializeField] private Sprite coinSprite;

        public event Action OnCloseRequested;

        private VisualElement _root;
        private VisualElement _dockRoot;
        private readonly bool[] _slotOccupied = new bool[FleetSize];
        private readonly VisualElement[] _slotFrames = new VisualElement[FleetSize];
        private readonly Button[] _slotButtons = new Button[FleetSize];

        private static readonly string[] MaterialNames =
        {
            "Oil", "Iron", "Steel",
            "Energy", "Aluminium", "Lumber",
            "Alloy", "Cloth", "Uranium"
        };

        public bool IsOpen => _dockRoot != null && _dockRoot.parent != null;

        public void OpenDock(VisualElement root)
        {
            if (root == null)
            {
                Debug.LogError("DockHUD.OpenDock: root is null.");
                return;
            }

            _root = root;
            CloseDock();

            _dockRoot = new VisualElement { name = "DockRoot" };
            _dockRoot.style.position = Position.Absolute;
            _dockRoot.style.top = 0;
            _dockRoot.style.left = 0;
            _dockRoot.style.right = 0;
            _dockRoot.style.bottom = 0;
            _dockRoot.style.flexDirection = FlexDirection.Column;
            _dockRoot.style.paddingTop = 12;
            _dockRoot.style.paddingBottom = 12;
            _dockRoot.style.paddingLeft = 16;
            _dockRoot.style.paddingRight = 16;

            _dockRoot.Add(CreateOuterFrame("DockTopFrame", Length.Percent(30f)));
            _dockRoot.Add(CreateMainFrame());
            _dockRoot.Add(CreateOuterFrame("DockBottomFrame", Length.Percent(30f)));

            _root.Add(_dockRoot);
            RefreshAllSlotButtons();
        }

        public void CloseDock()
        {
            if (_dockRoot == null) return;
            _dockRoot.RemoveFromHierarchy();
            _dockRoot = null;
        }

        private void RequestClose()
        {
            CloseDock();
            OnCloseRequested?.Invoke();
        }

        private VisualElement CreateOuterFrame(string name, Length height)
        {
            var frame = new VisualElement { name = name };
            frame.style.height = height;
            frame.style.flexShrink = 0;
            frame.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            frame.style.borderTopWidth = 1;
            frame.style.borderRightWidth = 1;
            frame.style.borderBottomWidth = 1;
            frame.style.borderLeftWidth = 1;
            frame.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.45f);
            frame.style.borderTopLeftRadius = 8;
            frame.style.borderTopRightRadius = 8;
            frame.style.borderBottomLeftRadius = 8;
            frame.style.borderBottomRightRadius = 8;
            frame.style.marginBottom = 8;
            return frame;
        }

        private VisualElement CreateMainFrame()
        {
            var main = new VisualElement { name = "DockMainFrame" };
            main.style.flexGrow = 1;
            main.style.flexShrink = 0;
            main.style.backgroundColor = new Color(0.06f, 0.10f, 0.22f, 0.96f);
            main.style.borderTopWidth = 2;
            main.style.borderRightWidth = 2;
            main.style.borderBottomWidth = 2;
            main.style.borderLeftWidth = 2;
            main.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
            main.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
            main.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            main.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
            main.style.borderTopLeftRadius = 10;
            main.style.borderTopRightRadius = 10;
            main.style.borderBottomLeftRadius = 10;
            main.style.borderBottomRightRadius = 10;
            main.style.paddingTop = 12;
            main.style.paddingBottom = 16;
            main.style.paddingLeft = 16;
            main.style.paddingRight = 16;
            main.style.flexDirection = FlexDirection.Column;
            main.style.marginBottom = 8;

            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 8;

            var title = new Label("Dock");
            title.style.fontSize = 22;
            title.style.color = Color.cyan;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerRow.Add(title);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.width = 36;
            closeBtn.style.height = 36;
            closeBtn.style.fontSize = 18;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.style.borderTopLeftRadius = 18;
            closeBtn.style.borderTopRightRadius = 18;
            closeBtn.style.borderBottomLeftRadius = 18;
            closeBtn.style.borderBottomRightRadius = 18;
            closeBtn.clicked += RequestClose;
            headerRow.Add(closeBtn);
            main.Add(headerRow);

            var filler = new VisualElement();
            filler.style.flexGrow = 1;
            main.Add(filler);

            main.Add(CreateFleetRow());
            main.Add(CreateCostsBlock());
            main.Add(CreateRepairBlock());
            return main;
        }

        private VisualElement CreateFleetRow()
        {
            var row = new VisualElement { name = "FleetRow" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.alignItems = Align.FlexEnd;
            row.style.marginBottom = 16;

            for (int i = 0; i < FleetSize; i++)
                row.Add(CreateSlotColumn(i));

            return row;
        }

        private VisualElement CreateSlotColumn(int index)
        {
            var col = new VisualElement { name = $"FleetSlot_{index}" };
            col.style.alignItems = Align.Center;
            col.style.marginLeft = 10;
            col.style.marginRight = 10;

            var flagLabel = new Label(index == FlagShipIndex ? "Flag Ship" : " ");
            flagLabel.style.height = 36;
            flagLabel.style.minWidth = 92;
            flagLabel.style.fontSize = 13;
            flagLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            flagLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            flagLabel.style.color = index == FlagShipIndex ? Color.cyan : Color.clear;
            flagLabel.style.marginBottom = 6;
            col.Add(flagLabel);

            var frame = new VisualElement { name = $"SlotFrame_{index}" };
            frame.style.width = 88;
            frame.style.height = 88;
            frame.style.backgroundColor = new Color(0.12f, 0.16f, 0.30f, 0.95f);
            frame.style.borderTopWidth = 2;
            frame.style.borderRightWidth = 2;
            frame.style.borderBottomWidth = 2;
            frame.style.borderLeftWidth = 2;
            frame.style.borderTopColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.borderRightColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.borderBottomColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            frame.style.borderLeftColor = new Color(0.4f, 0.8f, 1f, 0.9f);
            ApplyHexSprite(frame);

            _slotFrames[index] = frame;
            col.Add(frame);

            var btn = new Button();
            btn.style.minWidth = 92;
            btn.style.height = 36;
            btn.style.marginTop = 8;
            btn.style.fontSize = 13;
            btn.style.color = Color.white;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;

            int captured = index;
            btn.clicked += () => OnSlotButtonClicked(captured);
            _slotButtons[index] = btn;
            col.Add(btn);

            return col;
        }

        private void ApplyHexSprite(VisualElement frame)
        {
            if (hexSlotSprite == null) return;

            frame.style.backgroundImage = new StyleBackground(hexSlotSprite);
            frame.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            frame.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            frame.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
        }

        private void OnSlotButtonClicked(int index)
        {
            _slotOccupied[index] = !_slotOccupied[index];
            RefreshSlotButton(index);
            Debug.Log($"DockHUD: slot {index} -> {(_slotOccupied[index] ? "occupied (Remove Ship)" : "empty (Add Ship)")}");
        }

        private void RefreshAllSlotButtons()
        {
            for (int i = 0; i < FleetSize; i++)
                RefreshSlotButton(i);
        }

        private void RefreshSlotButton(int index)
        {
            if (_slotButtons[index] == null) return;
            _slotButtons[index].text = _slotOccupied[index] ? "Remove Ship" : "Add Ship";
        }

        private VisualElement CreateCostsBlock()
        {
            var block = new VisualElement { name = "CostsBlock" };
            block.style.alignItems = Align.Center;
            block.style.marginBottom = 14;

            var header = new Label("Costs");
            header.style.fontSize = 18;
            header.style.color = Color.white;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.marginBottom = 10;
            block.Add(header);

            var grid = new VisualElement { name = "MaterialsGrid" };
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.Center;
            grid.style.width = Length.Percent(90f);

            for (int i = 0; i < MaterialNames.Length; i++)
                grid.Add(CreateMaterialCell(MaterialNames[i]));

            block.Add(grid);
            return block;
        }

        private VisualElement CreateMaterialCell(string materialName)
        {
            var cell = new VisualElement { name = $"Material_{materialName}" };
            cell.style.width = Length.Percent(31f);
            cell.style.flexDirection = FlexDirection.Row;
            cell.style.alignItems = Align.Center;
            cell.style.marginBottom = 8;
            cell.style.marginRight = 6;
            cell.style.paddingTop = 4;
            cell.style.paddingBottom = 4;
            cell.style.paddingLeft = 6;
            cell.style.paddingRight = 6;

            var check = new Toggle { name = $"Check_{materialName}", value = false };
            check.style.marginRight = 6;
            cell.Add(check);

            var amount = new Label("0");
            amount.name = $"Amount_{materialName}";
            amount.style.flexGrow = 1;
            amount.style.fontSize = 14;
            amount.style.color = Color.white;
            amount.style.borderBottomWidth = 1;
            amount.style.borderBottomColor = new Color(0.75f, 0.85f, 1f, 0.85f);
            amount.style.paddingBottom = 2;
            amount.style.marginRight = 8;
            cell.Add(amount);

            var icon = new VisualElement { name = $"Icon_{materialName}" };
            icon.style.width = 22;
            icon.style.height = 22;
            icon.style.backgroundColor = new Color(0.25f, 0.32f, 0.48f, 0.9f);
            icon.style.borderTopLeftRadius = 4;
            icon.style.borderTopRightRadius = 4;
            icon.style.borderBottomLeftRadius = 4;
            icon.style.borderBottomRightRadius = 4;
            icon.style.flexShrink = 0;
            cell.Add(icon);

            return cell;
        }

        private VisualElement CreateRepairBlock()
        {
            var row = new VisualElement { name = "RepairBlock" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.Center;
            row.style.alignItems = Align.FlexStart;

            row.Add(CreateBeginRepairsColumn());
            row.Add(CreateInstantRepairColumn());
            return row;
        }

        private VisualElement CreateBeginRepairsColumn()
        {
            var col = new VisualElement();
            col.style.alignItems = Align.Center;
            col.style.marginRight = 40;

            var btn = new Button { text = "Begin Repairs" };
            StyleRepairButton(btn);
            col.Add(btn);

            var timerBox = new VisualElement { name = "RepairTimerBox" };
            StyleValueBox(timerBox);
            timerBox.style.marginTop = 8;

            var timerLabel = new Label("00:00");
            timerLabel.style.fontSize = 16;
            timerLabel.style.color = Color.white;
            timerLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            timerBox.Add(timerLabel);
            col.Add(timerBox);
            return col;
        }

        private VisualElement CreateInstantRepairColumn()
        {
            var col = new VisualElement();
            col.style.alignItems = Align.Center;

            var btn = new Button { text = "Repair Instantly" };
            StyleRepairButton(btn);
            col.Add(btn);

            var costRow = new VisualElement();
            costRow.style.flexDirection = FlexDirection.Row;
            costRow.style.alignItems = Align.Center;
            costRow.style.marginTop = 8;

            var coin = new VisualElement { name = "CoinIcon" };
            coin.style.width = 28;
            coin.style.height = 28;
            coin.style.marginRight = 8;
            coin.style.backgroundColor = new Color(0.55f, 0.45f, 0.12f, 0.95f);
            coin.style.borderTopLeftRadius = 14;
            coin.style.borderTopRightRadius = 14;
            coin.style.borderBottomLeftRadius = 14;
            coin.style.borderBottomRightRadius = 14;
            if (coinSprite != null)
            {
                coin.style.backgroundImage = new StyleBackground(coinSprite);
                coin.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            costRow.Add(coin);

            var costBox = new VisualElement { name = "InstantRepairCostBox" };
            StyleValueBox(costBox);

            var costLabel = new Label("0");
            costLabel.style.fontSize = 16;
            costLabel.style.color = Color.white;
            costLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            costBox.Add(costLabel);
            costRow.Add(costBox);

            col.Add(costRow);
            return col;
        }

        private static void StyleRepairButton(Button btn)
        {
            btn.style.minWidth = 170;
            btn.style.height = 42;
            btn.style.fontSize = 15;
            btn.style.color = Color.white;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }

        private static void StyleValueBox(VisualElement box)
        {
            box.style.minWidth = 110;
            box.style.height = 32;
            box.style.justifyContent = Justify.Center;
            box.style.alignItems = Align.Center;
            box.style.backgroundColor = new Color(0.08f, 0.12f, 0.24f, 0.95f);
            box.style.borderTopWidth = 1;
            box.style.borderRightWidth = 1;
            box.style.borderBottomWidth = 1;
            box.style.borderLeftWidth = 1;
            box.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.6f);
            box.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.6f);
            box.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.6f);
            box.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.6f);
            box.style.borderTopLeftRadius = 4;
            box.style.borderTopRightRadius = 4;
            box.style.borderBottomLeftRadius = 4;
            box.style.borderBottomRightRadius = 4;
        }
    }
}