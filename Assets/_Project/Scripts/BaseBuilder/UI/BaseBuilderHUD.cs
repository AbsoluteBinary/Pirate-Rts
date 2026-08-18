using System;
using System.Collections.Generic;
using _Project.Scripts.BaseBuilder.Runtime.Core;
using _Project.Scripts.BaseBuilder.Runtime.Data;
using _Project.Scripts.Harbour.Data.SO;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.BaseBuilder.UI
{
    public class BaseBuilderHUD : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private BaseBuilderController builderController;
        [SerializeField] private LandTileInventorySO landTileInventory;
        [SerializeField] private WallInventorySO wallInventory;
        [SerializeField] private BuildingsInventorySO buildingsInventory;

        private readonly List<Label> buildingCountLabels = new List<Label>();
        private readonly List<Label> wallCountLabels = new List<Label>();
        #endregion

        #region Events

        public event Action OnSelectModeClicked;
        public event Action OnBuildOnWaterClicked;
        public event Action OnBuildOnLandClicked;
        public event Action OnExitClicked;
        public event Action OnPickUpClicked;
        public event Action OnDeleteClicked;
        public event Action OnLockClicked;
        public event Action OnUnlockClicked;

        #endregion

        #region Private State

        private VisualElement root;
        private VisualElement topBar;
        private VisualElement filterBar;
        private VisualElement inventoryPanel;
        private VisualElement wallsBuildingsPanel;
        private VisualElement currentlySelectedSlot;
        private readonly List<Label> slotCountLabels = new List<Label>();

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            panelRenderer.RegisterUIReloadCallback(OnUIReady);
            TrySubscribe();
        }

        private void Start()
        {
            // Backup in case builderController was assigned after OnEnable
            TrySubscribe();
        }

        private void TrySubscribe()
        {
            if (wallInventory != null)
            {
                wallInventory.OnCountChanged -= HandleWallCountChanged;
                wallInventory.OnCountChanged += HandleWallCountChanged;
            }
            
            if (builderController == null) return;

            builderController.OnLandTilePlaced -= HandleLandTilePlaced; // avoid double subscribe
            builderController.OnLandTilePlaced += HandleLandTilePlaced;
            landTileInventory.OnCountChanged += HandleLandCountChanged;
            
            
            buildingsInventory.OnCountChanged += HandleBuildingCountChanged;
        }
        

        private void OnDisable()
        {
            if (wallInventory != null)
                wallInventory.OnCountChanged -= HandleWallCountChanged;
            
            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReady);

            if (builderController != null)
                builderController.OnLandTilePlaced -= HandleLandTilePlaced;
            if (buildingsInventory != null)
            {
                buildingsInventory.OnCountChanged -= HandleBuildingCountChanged;
                buildingsInventory.OnCountChanged += HandleBuildingCountChanged;
            }
            builderController?.SetPointerOverUI(false);
        }
        
        private void OnUIReady(PanelRenderer renderer, VisualElement rootElement)
        {
            root = rootElement;
            BuildUI();
        }

        #endregion

        #region UI Build

        private void BuildUI()
        {
            root.Clear();
            currentlySelectedSlot = null;

            BuildTopBar();
            BuildFilterBar();
            BuildInventoryPanel();
            BuildWallsAndBuildingsPanel();
        }

        private void BuildTopBar()
        {
            topBar = new VisualElement();
            topBar.pickingMode = PickingMode.Position;
            topBar.style.position = Position.Absolute;
            topBar.style.top = 0;
            topBar.style.left = 0;
            topBar.style.right = 0;
            topBar.style.height = 56;
            topBar.style.backgroundColor = new Color(0.12f, 0.14f, 0.22f, 0.95f);
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingLeft = 16;
            topBar.style.paddingRight = 16;

            topBar.Add(CreateModeButton("Select", BuilderMode.Select));
            topBar.Add(CreateModeButton("Build on Water", BuilderMode.BuildOnWater));
            topBar.Add(CreateModeButton("Build on Land", BuilderMode.BuildOnLand));

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            topBar.Add(spacer);

            //buttonRow.Add(CreateInventoryButton("Save", OnSaveClicked));
            topBar.Add(CreateActionButton("Pick Up", () =>
            {
                builderController?.ClearSelectedPrefab();
                builderController?.SetMode(BuilderMode.PickUp);
                OnPickUpClicked?.Invoke(); // optional – keep if something else listens
                
                Debug.Log("<color=cyan>Mode → Pick Up</color>");
            }));
            
            topBar.Add(CreateActionButton("Delete", () =>
            {
                builderController?.ClearSelectedPrefab();
                builderController?.SetMode(BuilderMode.Delete);
                OnDeleteClicked?.Invoke();
                Debug.Log("<color=orange>Mode → Delete</color>");
            }));
            
            topBar.Add(CreateActionButton("Clear Selection", () =>
            {
                builderController?.ClearSelectedPrefab();
                //builderController?.SetMode(BuilderMode.Select);
                Debug.Log("<color=cyan>Selection cleared</color>");
            }));
            
            topBar.Add(CreateActionButton("Lock", () => OnLockClicked?.Invoke()));
            topBar.Add(CreateActionButton("Unlock", () => OnUnlockClicked?.Invoke()));

            var exitBtn = new Button { text = "Exit Build Mode" };
            StyleButton(exitBtn, new Color(0.55f, 0.15f, 0.15f));
            exitBtn.clicked += () => OnExitClicked?.Invoke();
            topBar.Add(exitBtn);

            root.Add(topBar);
            RegisterUIBlockers(topBar);
        }

        private void BuildFilterBar()
        {
            filterBar = new VisualElement();
            filterBar.pickingMode = PickingMode.Position;
            filterBar.style.position = Position.Absolute;
            filterBar.style.top = 56;
            filterBar.style.left = 0;
            filterBar.style.right = 0;
            filterBar.style.height = 42;
            filterBar.style.backgroundColor = new Color(0.10f, 0.12f, 0.18f, 0.92f);
            filterBar.style.flexDirection = FlexDirection.Row;
            filterBar.style.alignItems = Align.Center;
            filterBar.style.paddingLeft = 16;

            filterBar.Add(new Label("Filter:")
            {
                style =
                {
                    color = Color.white,
                    marginRight = 12,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            });

            filterBar.Add(CreateFilterButton("All", SelectFilter.All));
            filterBar.Add(CreateFilterButton("Walls", SelectFilter.Walls));
            filterBar.Add(CreateFilterButton("Turrets", SelectFilter.Turrets));
            filterBar.Add(CreateFilterButton("Land", SelectFilter.Land));

            root.Add(filterBar);
            RegisterUIBlockers(filterBar);
        }

        private void BuildInventoryPanel()
        {
            slotCountLabels.Clear();

            inventoryPanel = new VisualElement { name = "LandTileInventory" };
            inventoryPanel.pickingMode = PickingMode.Position;
            inventoryPanel.style.position = Position.Absolute;
            inventoryPanel.style.top = 110;
            inventoryPanel.style.right = 20;
            inventoryPanel.style.width = 320;
            inventoryPanel.style.maxHeight = 440;
            inventoryPanel.style.backgroundColor = new Color(0.08f, 0.11f, 0.20f, 0.96f);
            inventoryPanel.style.borderTopLeftRadius = 10;
            inventoryPanel.style.borderTopRightRadius = 10;
            inventoryPanel.style.borderBottomLeftRadius = 10;
            inventoryPanel.style.borderBottomRightRadius = 10;
            inventoryPanel.style.borderTopWidth = 2;
            inventoryPanel.style.borderRightWidth = 2;
            inventoryPanel.style.borderBottomWidth = 2;
            inventoryPanel.style.borderLeftWidth = 2;
            inventoryPanel.style.borderTopColor = new Color(0.3f, 0.6f, 1f);
            inventoryPanel.style.borderRightColor = new Color(0.3f, 0.6f, 1f);
            inventoryPanel.style.borderBottomColor = new Color(0.3f, 0.6f, 1f);
            inventoryPanel.style.borderLeftColor = new Color(0.3f, 0.6f, 1f);
            inventoryPanel.style.paddingTop = 12;
            inventoryPanel.style.paddingBottom = 12;
            inventoryPanel.style.paddingLeft = 12;
            inventoryPanel.style.paddingRight = 12;
            inventoryPanel.style.display = DisplayStyle.None;

            var header = new Label("Land Tiles");
            header.style.fontSize = 18;
            header.style.color = Color.white;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 10;
            inventoryPanel.Add(header);

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.FlexStart;

            if (landTileInventory != null && landTileInventory.tiles != null)
            {
                for (int i = 0; i < landTileInventory.tiles.Count; i++)
                {
                    int index = i;
                    grid.Add(CreateInventorySlot(landTileInventory.tiles[i], index));
                }
            }
            else
            {
                var emptyLabel = new Label("No Land Tiles assigned");
                emptyLabel.style.color = Color.gray;
                grid.Add(emptyLabel);
            }
            inventoryPanel.Add(grid);
            
            // ===== Bottom buttons =====
            var buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.justifyContent = Justify.SpaceBetween;
            buttonRow.style.marginTop = 12;

            buttonRow.Add(CreateInventoryButton("Clear", OnClearClicked));
            buttonRow.Add(CreateInventoryButton("Save", OnSaveClicked));
            buttonRow.Add(CreateInventoryButton("Load", OnLoadClicked));

            inventoryPanel.Add(buttonRow);
            
            root.Add(inventoryPanel);
            RegisterUIBlockers(inventoryPanel);
        }
        
        private void BuildWallsAndBuildingsPanel()
        {
            wallsBuildingsPanel = new VisualElement { name = "WallsAndBuildingsPanel" };
            wallsBuildingsPanel.pickingMode = PickingMode.Position;
            wallsBuildingsPanel.style.position = Position.Absolute;
            wallsBuildingsPanel.style.top = 110;
            wallsBuildingsPanel.style.right = 20;
            wallsBuildingsPanel.style.width = 320;
            wallsBuildingsPanel.style.maxHeight = 480;
            wallsBuildingsPanel.style.backgroundColor = new Color(0.08f, 0.11f, 0.20f, 0.96f);
            wallsBuildingsPanel.style.borderTopLeftRadius = 10;
            wallsBuildingsPanel.style.borderTopRightRadius = 10;
            wallsBuildingsPanel.style.borderBottomLeftRadius = 10;
            wallsBuildingsPanel.style.borderBottomRightRadius = 10;
            wallsBuildingsPanel.style.borderTopWidth = 2;
            wallsBuildingsPanel.style.borderRightWidth = 2;
            wallsBuildingsPanel.style.borderBottomWidth = 2;
            wallsBuildingsPanel.style.borderLeftWidth = 2;
            wallsBuildingsPanel.style.borderTopColor = new Color(0.3f, 0.75f, 0.45f);
            wallsBuildingsPanel.style.borderRightColor = new Color(0.3f, 0.75f, 0.45f);
            wallsBuildingsPanel.style.borderBottomColor = new Color(0.3f, 0.75f, 0.45f);
            wallsBuildingsPanel.style.borderLeftColor = new Color(0.3f, 0.75f, 0.45f);
            wallsBuildingsPanel.style.paddingTop = 12;
            wallsBuildingsPanel.style.paddingBottom = 12;
            wallsBuildingsPanel.style.paddingLeft = 12;
            wallsBuildingsPanel.style.paddingRight = 12;
            wallsBuildingsPanel.style.display = DisplayStyle.None;

            // Header
            var header = new Label("Walls and Buildings");
            header.style.fontSize = 18;
            header.style.color = Color.white;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 10;
            wallsBuildingsPanel.Add(header);

            // ===== Tabs =====
            var tabRow = new VisualElement();
            tabRow.style.flexDirection = FlexDirection.Row;
            tabRow.style.marginBottom = 10;

            var wallsTabBtn = CreateTabButton("Walls", true);
            var buildingsTabBtn = CreateTabButton("Buildings", false);

            tabRow.Add(wallsTabBtn);
            tabRow.Add(buildingsTabBtn);
            wallsBuildingsPanel.Add(tabRow);

            // Content containers
            var wallsContent = new VisualElement { name = "WallsContent" };
            var buildingsContent = new VisualElement { name = "BuildingsContent" };
            buildingsContent.style.display = DisplayStyle.None;

            // --- Walls Grid ---
            var wallsGrid = new VisualElement();
            wallsGrid.style.flexDirection = FlexDirection.Row;
            wallsGrid.style.flexWrap = Wrap.Wrap;
            wallsGrid.style.justifyContent = Justify.FlexStart;

            if (wallInventory != null && wallInventory.walls != null && wallInventory.walls.Count > 0)
            {
                for (int i = 0; i < wallInventory.walls.Count; i++)
                {
                    int index = i;
                    wallsGrid.Add(CreateWallSlot(wallInventory.walls[i], index));
                }
            }
            else
            {
                var emptyLabel = new Label("No Walls assigned");
                emptyLabel.style.color = Color.gray;
                wallsGrid.Add(emptyLabel);
            }
            wallsContent.Add(wallsGrid);

            // --- Buildings Grid  ---
            var buildingsGrid = new VisualElement();
            buildingsGrid.style.flexDirection = FlexDirection.Row;
            buildingsGrid.style.flexWrap = Wrap.Wrap;

            var buildingsPlaceholder = new Label("Buildings");
            buildingsPlaceholder.style.color = Color.gray;
            buildingsGrid.Add(buildingsPlaceholder);
            buildingsContent.Add(buildingsGrid);

            wallsBuildingsPanel.Add(wallsContent);
            wallsBuildingsPanel.Add(buildingsContent);
            
            // --- Buildings Grid ---
            buildingsGrid.style.flexDirection = FlexDirection.Row;
            buildingsGrid.style.flexWrap = Wrap.Wrap;
            buildingsGrid.style.justifyContent = Justify.FlexStart;

            if (buildingsInventory != null && buildingsInventory.buildings != null &&
                buildingsInventory.buildings.Count > 0)
            {
                for (int i = 0; i < buildingsInventory.buildings.Count; i++)
                {
                    int index = i;
                    buildingsGrid.Add(CreateBuildingSlot(buildingsInventory.buildings[i], index));
                }
            }
            else
            {
                var emptyLabel = new Label("No Buildings assigned");
                emptyLabel.style.color = Color.gray;
                buildingsGrid.Add(emptyLabel);
            }
            buildingsContent.Add(buildingsGrid);

            // Tab switching
            wallsTabBtn.clicked += () =>
            {
                wallsContent.style.display = DisplayStyle.Flex;
                buildingsContent.style.display = DisplayStyle.None;
                SetTabActive(wallsTabBtn, true);
                SetTabActive(buildingsTabBtn, false);
            };

            buildingsTabBtn.clicked += () =>
            {
                wallsContent.style.display = DisplayStyle.None;
                buildingsContent.style.display = DisplayStyle.Flex;
                SetTabActive(wallsTabBtn, false);
                SetTabActive(buildingsTabBtn, true);
            };

            // ===== Bottom buttons =====
            var buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.justifyContent = Justify.SpaceBetween;
            buttonRow.style.marginTop = 12;

            buttonRow.Add(CreateInventoryButton("Clear", OnWallsClearClicked));
            buttonRow.Add(CreateInventoryButton("Save", OnWallsSaveClicked));
            buttonRow.Add(CreateInventoryButton("Load", OnWallsLoadClicked));

            wallsBuildingsPanel.Add(buttonRow);

            root.Add(wallsBuildingsPanel);
            RegisterUIBlockers(wallsBuildingsPanel);
        }
        
        private VisualElement CreatePlaceholderSlot(string labelText)
        {
            var slot = new VisualElement();
            slot.style.width = 64;
            slot.style.height = 64;
            slot.style.marginRight = 8;
            slot.style.marginBottom = 8;
            slot.style.backgroundColor = new Color(0.15f, 0.18f, 0.25f, 1f);
            slot.style.borderTopLeftRadius = 6;
            slot.style.borderTopRightRadius = 6;
            slot.style.borderBottomLeftRadius = 6;
            slot.style.borderBottomRightRadius = 6;
            slot.style.justifyContent = Justify.Center;
            slot.style.alignItems = Align.Center;

            var label = new Label(labelText);
            label.style.fontSize = 11;
            label.style.color = Color.gray;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            slot.Add(label);

            return slot;
        }
        
        private VisualElement CreateBuildingSlot(BuildingsInventorySO.BuildingsEntry entry, int index)
        {
            var slot = new VisualElement();
            slot.pickingMode = PickingMode.Position;
            slot.style.width = 72;
            slot.style.height = 72;
            slot.style.backgroundColor = new Color(0.18f, 0.22f, 0.35f);
            slot.style.borderTopLeftRadius = 6;
            slot.style.borderTopRightRadius = 6;
            slot.style.borderBottomLeftRadius = 6;
            slot.style.borderBottomRightRadius = 6;
            slot.style.marginRight = 8;
            slot.style.marginBottom = 8;
            slot.style.justifyContent = Justify.Center;
            slot.style.alignItems = Align.Center;

            if (entry != null && entry.icon != null)
            {
                slot.style.backgroundImage = new StyleBackground(entry.icon);
                slot.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                slot.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                slot.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            else
            {
                var placeholder = new Label("B");
                placeholder.style.fontSize = 20;
                placeholder.style.color = new Color(0.7f, 0.75f, 0.9f);
                placeholder.style.unityFontStyleAndWeight = FontStyle.Bold;
                slot.Add(placeholder);
            }

            int count = buildingsInventory != null ? buildingsInventory.GetCount(index) : 0;
            var countLabel = new Label(count.ToString());
            while (buildingCountLabels.Count <= index)
                buildingCountLabels.Add(null);
            buildingCountLabels[index] = countLabel;

            countLabel.style.position = Position.Absolute;
            countLabel.style.right = 4;
            countLabel.style.bottom = 2;
            countLabel.style.fontSize = 13;
            countLabel.style.color = Color.white;
            countLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            countLabel.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            countLabel.style.paddingLeft = 4;
            countLabel.style.paddingRight = 4;
            countLabel.style.borderTopLeftRadius = 3;
            countLabel.style.borderTopRightRadius = 3;
            countLabel.style.borderBottomLeftRadius = 3;
            countLabel.style.borderBottomRightRadius = 3;
            slot.Add(countLabel);

            slot.RegisterCallback<ClickEvent>(evt =>
            {
                if (entry == null || entry.prefab == null) return;

                builderController?.SelectPrefab(
                    entry.prefab,
                    entry.size,   // add entry.size later if buildings are larger
                    isLand: false,    // object grid
                    inventoryIndex: index
                );

                HighlightSelectedSlot(slot);
            });

            return slot;
        }
        
        private Button CreateInventoryButton(string text, Action onClick)
        {
            var btn = new Button { text = text };
            btn.style.flexGrow = 1;
            btn.style.height = 36;
            btn.style.marginLeft = 4;
            btn.style.marginRight = 4;
            btn.style.fontSize = 13;
            btn.style.backgroundColor = new Color(0.16f, 0.20f, 0.32f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 5;
            btn.style.borderTopRightRadius = 5;
            btn.style.borderBottomLeftRadius = 5;
            btn.style.borderBottomRightRadius = 5;
            btn.clicked += onClick;
            return btn;
        }

        #region UIHelpers

        private Button CreateTabButton(string text, bool isActive)
        {
            var btn = new Button { text = text };
            btn.style.flexGrow = 1;
            btn.style.height = 32;
            btn.style.marginRight = 4;
            btn.style.fontSize = 13;
            btn.style.borderTopLeftRadius = 5;
            btn.style.borderTopRightRadius = 5;
            btn.style.borderBottomLeftRadius = 5;
            btn.style.borderBottomRightRadius = 5;

            SetTabActive(btn, isActive);
            return btn;
        }

        private void SetTabActive(Button btn, bool active)
        {
            if (active)
            {
                btn.style.backgroundColor = new Color(0.25f, 0.55f, 0.35f);
                btn.style.color = Color.white;
            }
            else
            {
                btn.style.backgroundColor = new Color(0.16f, 0.20f, 0.28f);
                btn.style.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }

        private VisualElement CreateWallSlot(WallInventorySO.WallEntry entry, int index)
        {
            var slot = new VisualElement();
            slot.pickingMode = PickingMode.Position;
            slot.style.width = 72;
            slot.style.height = 72;
            slot.style.backgroundColor = new Color(0.18f, 0.22f, 0.35f);
            slot.style.borderTopLeftRadius = 6;
            slot.style.borderTopRightRadius = 6;
            slot.style.borderBottomLeftRadius = 6;
            slot.style.borderBottomRightRadius = 6;
            slot.style.marginRight = 8;
            slot.style.marginBottom = 8;
            slot.style.justifyContent = Justify.Center;
            slot.style.alignItems = Align.Center;

            // Placeholder icon (or real icon later)
            if (entry != null && entry.icon != null)
            {
                slot.style.backgroundImage = new StyleBackground(entry.icon);
                slot.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                slot.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                slot.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            else
            {
                var placeholder = new Label("W");
                placeholder.style.fontSize = 20;
                placeholder.style.color = new Color(0.6f, 0.8f, 0.7f);
                placeholder.style.unityFontStyleAndWeight = FontStyle.Bold;
                slot.Add(placeholder);
            }

            // Count label
            int count = wallInventory != null ? wallInventory.GetCount(index) : 0;
            var countLabel = new Label(count.ToString());
            while (wallCountLabels.Count <= index)
                wallCountLabels.Add(null);
            wallCountLabels[index] = countLabel;
            
            countLabel.style.position = Position.Absolute;
            countLabel.style.right = 4;
            countLabel.style.bottom = 2;
            countLabel.style.fontSize = 13;
            countLabel.style.color = Color.white;
            countLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            countLabel.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            countLabel.style.paddingLeft = 4;
            countLabel.style.paddingRight = 4;
            countLabel.style.borderTopLeftRadius = 3;
            countLabel.style.borderTopRightRadius = 3;
            countLabel.style.borderBottomLeftRadius = 3;
            countLabel.style.borderBottomRightRadius = 3;
            slot.Add(countLabel);

            // Click to select
            slot.RegisterCallback<ClickEvent>(evt =>
            {
                if (entry == null || entry.prefab == null) return;

                builderController?.SelectPrefab(
                    entry.prefab,
                    Vector2Int.one,          // assuming 1×1 walls for now
                    isLand: false,           // walls go on Object Grid
                    inventoryIndex: index
                );

                HighlightSelectedSlot(slot);
            });

            return slot;
        }

        #endregion
        
        
        private void OnClearClicked()
        {
            builderController?.PlacementService.ClearLandTilesOnly(landTileInventory);

            // Refresh labels
            if (landTileInventory != null)
            {
                for (int i = 0; i < landTileInventory.tiles.Count; i++)
                {
                    if (i < slotCountLabels.Count && slotCountLabels[i] != null)
                        slotCountLabels[i].text = landTileInventory.GetCount(i).ToString();
                }
            }
        }

        private void OnSaveClicked()
        {
            Debug.Log("<color=yellow>Save clicked</color>");
            // TODO: basic save of layout + counts
        }

        private void OnLoadClicked()
        {
            Debug.Log("<color=yellow>Load clicked</color>");
            // TODO: basic load of layout + counts
        }
        
        private void OnWallsClearClicked()
        {
            builderController?.PlacementService.ClearObjectsOnly(wallInventory);

            // Refresh wall count labels if you have them (optional for now)
            Debug.Log("<color=yellow>Cleared all Walls + restored inventory counts</color>");
        }

        private void OnWallsSaveClicked()
        {
            Debug.Log("[WallsAndBuildings] Save clicked – wiring later");
        }

        private void OnWallsLoadClicked()
        {
            Debug.Log("[WallsAndBuildings] Load clicked – wiring later");
        }

        private VisualElement CreateInventorySlot(LandTileInventorySO.LandTileEntry entry, int index)
        {
            var slot = new VisualElement();
            slot.pickingMode = PickingMode.Position;
            slot.style.width = 72;
            slot.style.height = 72;
            slot.style.backgroundColor = new Color(0.18f, 0.22f, 0.35f);
            slot.style.borderTopLeftRadius = 6;
            slot.style.borderTopRightRadius = 6;
            slot.style.borderBottomLeftRadius = 6;
            slot.style.borderBottomRightRadius = 6;
            slot.style.marginRight = 8;
            slot.style.marginBottom = 8;
            slot.style.justifyContent = Justify.Center;
            slot.style.alignItems = Align.Center;

            if (entry != null && entry.icon != null)
            {
                slot.style.backgroundImage = new StyleBackground(entry.icon);
                slot.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                slot.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                slot.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }

            int count = landTileInventory != null ? landTileInventory.GetCount(index) : 0;
            var countLabel = new Label(count.ToString());
            countLabel.style.position = Position.Absolute;
            countLabel.style.right = 4;
            countLabel.style.bottom = 2;
            countLabel.style.fontSize = 13;
            countLabel.style.color = Color.white;
            countLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            countLabel.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            countLabel.style.paddingLeft = 4;
            countLabel.style.paddingRight = 4;
            countLabel.style.borderTopLeftRadius = 3;
            countLabel.style.borderTopRightRadius = 3;
            countLabel.style.borderBottomLeftRadius = 3;
            countLabel.style.borderBottomRightRadius = 3;
            slot.Add(countLabel);

            while (slotCountLabels.Count <= index)
                slotCountLabels.Add(null);
            slotCountLabels[index] = countLabel;

            slot.RegisterCallback<ClickEvent>(evt =>
            {
                if (entry == null) return;
                builderController?.SelectPrefab(entry.prefab, Vector2Int.one, isLand: true, inventoryIndex: index);
                HighlightSelectedSlot(slot);
            });

            return slot;
        }

        #endregion

        #region UI Click-Through Protection

        private void RegisterUIBlockers(VisualElement element)
        {
            if (element == null || builderController == null) return;

            element.RegisterCallback<PointerEnterEvent>(evt =>
            {
                builderController.SetPointerOverUI(true);
            });

            element.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                builderController.SetPointerOverUI(false);
            });
        }

        #endregion

        #region Inventory Helpers
        
        private void RefreshLandTileLabel(int index)
        {
            if (landTileInventory == null) return;
            if (index < 0 || index >= slotCountLabels.Count) return;
            if (slotCountLabels[index] == null) return;

            slotCountLabels[index].text = landTileInventory.GetCount(index).ToString();
        }

        private void HighlightSelectedSlot(VisualElement slot)
        {
            if (currentlySelectedSlot != null)
            {
                currentlySelectedSlot.style.borderTopWidth = 0;
                currentlySelectedSlot.style.borderRightWidth = 0;
                currentlySelectedSlot.style.borderBottomWidth = 0;
                currentlySelectedSlot.style.borderLeftWidth = 0;
            }

            currentlySelectedSlot = slot;
            slot.style.borderTopWidth = 2;
            slot.style.borderRightWidth = 2;
            slot.style.borderBottomWidth = 2;
            slot.style.borderLeftWidth = 2;
            slot.style.borderTopColor = new Color(0.3f, 0.9f, 1f);
            slot.style.borderRightColor = new Color(0.3f, 0.9f, 1f);
            slot.style.borderBottomColor = new Color(0.3f, 0.9f, 1f);
            slot.style.borderLeftColor = new Color(0.3f, 0.9f, 1f);
        }

        public void SetInventoryVisible(bool visible)
        {
            if (inventoryPanel != null)
                inventoryPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void HandleLandTilePlaced(int index)
        {
            if (landTileInventory == null) return;
            if (index < 0 || index >= landTileInventory.tiles.Count) return;

            // Consume from the same SO the HUD is displaying
            bool consumed = landTileInventory.ConsumeTile(index);
            if (!consumed)
            {
                Debug.LogWarning($"Could not consume tile at index {index} (count may be 0)");
                return;
            }
            
            // Update the label
            if (index < slotCountLabels.Count && slotCountLabels[index] != null)
                slotCountLabels[index].text = landTileInventory.GetCount(index).ToString();
        }
        
        private void HandleLandCountChanged(int index)
        {
            if (landTileInventory == null) return;
            if (index < 0 || index >= slotCountLabels.Count) return;

            var label = slotCountLabels[index];
            if (label == null) return;           // ← was missing / not enough
            if (label.panel == null) return;     // label not attached to UI anymore

            label.text = landTileInventory.GetCount(index).ToString();
        }
        
        private void HandleWallCountChanged(int index)
        {
            if (wallInventory == null) return;
            if (index < 0 || index >= wallCountLabels.Count) return;

            var label = wallCountLabels[index];
            if (label == null || label.panel == null) return;

            label.text = wallInventory.GetCount(index).ToString();
        }
        
        private void HandleBuildingCountChanged(int index)
        {
            Debug.Log($"[HUD] Building count changed index={index} count={buildingsInventory.GetCount(index)} labels={buildingCountLabels.Count}");
            if (buildingsInventory == null) return;
            if (index < 0 || index >= buildingCountLabels.Count) return;

            var label = buildingCountLabels[index];
            if (label == null || label.panel == null) return;

            label.text = buildingsInventory.GetCount(index).ToString();
        }

        #endregion

        #region Button Factories

        private Button CreateModeButton(string text, BuilderMode mode)
        {
            var btn = new Button { text = text };
            StyleModeButton(btn);

            btn.clicked += () =>
            {
                builderController?.SetMode(mode);

                // Show / hide panels based on mode
                if (inventoryPanel != null)
                    inventoryPanel.style.display = (mode == BuilderMode.BuildOnWater) ? DisplayStyle.Flex : DisplayStyle.None;

                if (wallsBuildingsPanel != null)
                    wallsBuildingsPanel.style.display = (mode == BuilderMode.BuildOnLand) ? DisplayStyle.Flex : DisplayStyle.None;

                switch (mode)
                {
                    case BuilderMode.Select: OnSelectModeClicked?.Invoke(); break;
                    case BuilderMode.BuildOnWater: OnBuildOnWaterClicked?.Invoke(); break;
                    case BuilderMode.BuildOnLand: OnBuildOnLandClicked?.Invoke(); break;
                }
            };

            return btn;
        }

        private Button CreateFilterButton(string text, SelectFilter filter)
        {
            var btn = new Button { text = text };
            StyleFilterButton(btn);
            btn.clicked += () => builderController?.SetSelectFilter(filter);
            return btn;
        }

        private Button CreateActionButton(string text, Action onClick)
        {
            var btn = new Button { text = text };
            StyleActionButton(btn);
            btn.clicked += onClick;
            return btn;
        }

        #endregion

        #region Styling

        private void StyleButton(Button btn, Color backgroundColor)
        {
            btn.style.height = 40;
            btn.style.paddingLeft = 16;
            btn.style.paddingRight = 16;
            btn.style.backgroundColor = backgroundColor;
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }

        private void StyleModeButton(Button btn)
        {
            btn.style.height = 40;
            btn.style.marginRight = 8;
            btn.style.paddingLeft = 14;
            btn.style.paddingRight = 14;
            btn.style.backgroundColor = new Color(0.20f, 0.25f, 0.40f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }

        private void StyleFilterButton(Button btn)
        {
            btn.style.height = 32;
            btn.style.marginRight = 6;
            btn.style.paddingLeft = 12;
            btn.style.paddingRight = 12;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.35f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 4;
            btn.style.borderTopRightRadius = 4;
            btn.style.borderBottomLeftRadius = 4;
            btn.style.borderBottomRightRadius = 4;
        }

        private void StyleActionButton(Button btn)
        {
            btn.style.height = 40;
            btn.style.marginRight = 8;
            btn.style.paddingLeft = 14;
            btn.style.paddingRight = 14;
            btn.style.backgroundColor = new Color(0.25f, 0.30f, 0.45f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
        }

        #endregion
    }
}