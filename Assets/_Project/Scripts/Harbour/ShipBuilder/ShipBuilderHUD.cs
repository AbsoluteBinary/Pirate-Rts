using System;
using System.Collections.Generic;
using _Project.Scripts.Harbour.Data.HUDData;
using _Project.Scripts.Harbour.Economy;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Harbour.Modules;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using _Project.Scripts.Persistence.TempSave;
using UnityEngine.UIElements.Experimental;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public class ShipBuilderHUD : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        private float _hullDrawScale = 1f;
        private VisualElement _saveNameDropdown;
        private TextField _saveNameField;
        private bool _saveDropdownOpen;
        private VisualElement _blueprintListOverlay;
        
        private TextField _shipNameField;
        private const string ShipNameHint = "Name this ship";
        
        [Header("References")]
        [SerializeField] private HullSelectionHUD hullSelectionHUD;
        [SerializeField] private BuiltShipInventory builtShipInventory;
        
        [Header("Wallet")]
        [SerializeField] private ResourceWalletHolder walletHolder;
        
        [Header("Instant Build")]
        [SerializeField] private Sprite instantBuildTokenIcon;
        [SerializeField] private int instantBuildTokenCost = 1;

        private Label _instantTokenCountLabel;
        private const string InstantTokenId = "PieceOfEight";
        
        [Header("Selection HUDs")]
        [SerializeField] private WeaponSelectionHUD weaponSelectionHUD;
        [SerializeField] private ArmourSelectionHUD armourSelectionHUD;
        [SerializeField] private EngineSelectionHUD engineSelectionHUD;
        [SerializeField] private ComponentSelectionHUD componentSelectionHUD;

        [Header("Build Timer")]
        private Label _buildTimerLabel;
        private bool _isBuilding;
        private float _buildRemaining;
        
        [Header("Visuals")]
        [SerializeField] private bool showSlotVisuals = true;
        private VisualElement _shipBuilderPanel;
        private VisualElement _hullCanvas;
        private VisualElement _hullImageElement;
        private VisualElement _slotOverlay;
        private Label _hullNameLabel;
        private Label _statsLabel;

        private HullData _currentHull;
        private ShipLoadout _currentLoadout;
        
        private bool _clearMode;
        private Button _clearBtn;
        
        private VisualElement root;

        #region Timer Ticker + Build Finish

        private void Update()
        {
            if (!_isBuilding) return;

            _buildRemaining -= Time.deltaTime;
            if (_buildRemaining <= 0f)
            {
                _buildRemaining = 0f;
                SetTimerLabel(0f);
                _isBuilding = false;
                CompleteBuild();
                return;
            }

            SetTimerLabel(_buildRemaining);
        }

        private void StartBuild()
        {
            if (_isBuilding)
            {
                Debug.LogWarning("[Build] Already in progress.");
                return;
            }

            if (_currentLoadout?.hull == null)
            {
                Debug.LogWarning("[Build] No hull selected.");
                return;
            }
            
            string shipName = _shipNameField != null ? _shipNameField.value.Trim() : "";
            if (string.IsNullOrEmpty(shipName) || shipName == ShipNameHint)
            {
                Debug.LogWarning("[Build] Name the ship first.");
                return;
            }
            
            if (!_currentLoadout.AllSlotsFilled())
            {
                Debug.LogWarning("[Build] Equip every slot first.");
                return;
            }

            _currentLoadout.RecalculateStats();

            if (walletHolder == null || walletHolder.Wallet == null)
            {
                Debug.LogWarning("[Build] No wallet — cannot start.");
                return;
            }

            if (!walletHolder.Wallet.CanAfford(_currentLoadout.totalResourceCosts))
            {
                Debug.LogWarning("[Build] Not enough resources.");
                return;
            }
            
            
            _buildRemaining = Mathf.Max(0f, _currentLoadout.totalBuildTime);
            SetTimerLabel(_buildRemaining);

            if (_buildRemaining <= 0.05f)
            {
                CompleteBuild();
                return;
            }

            _isBuilding = true;
            Debug.Log($"<color=cyan>[Build] Started {_buildRemaining:0}s</color>");
        }

        private void SetTimerLabel(float seconds)
        {
            if (_buildTimerLabel == null) return;
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            string prefix = _isBuilding || seconds > 0f ? "Building" : "Build Time";
            _buildTimerLabel.text = $"{prefix}: {m:00}:{s:00}";
        }

        private bool CompleteBuild()
        {
            if (_currentLoadout?.hull == null)
                return false;

            if (walletHolder?.Wallet == null ||
                !walletHolder.Wallet.TrySpendAll(_currentLoadout.totalResourceCosts))
            {
                Debug.LogWarning("[Build] Spend failed — ship not built.");
                if (_buildTimerLabel != null)
                    _buildTimerLabel.text = "Build failed";
                return false;
            }

            RefreshBuildCostNumbers();
            BuildCurrentShip();
            if (_buildTimerLabel != null)
                _buildTimerLabel.text = "Build complete";

            GetComponent<HarbourHUD>()?.RefreshIdleResourceAmounts();
            return true;
        }

        #endregion

        private void OnEnable()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            panelRenderer.RegisterUIReloadCallback(OnUIReady);
            
            

            if (builtShipInventory != null)
                builtShipInventory.EnsureHydrated();
        }

        private void OnDisable()
        {
            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReady);
            
            if (builtShipInventory != null)
                builtShipInventory.ClearRuntime();
        }
        
        private void RefreshBuildCostNumbers()
        {
            if (_shipBuilderPanel == null) return;

            string[] ids =
            {
                "Oil", "Iron", "Steel",
                "Energy", "Aluminium", "Lumber",
                "Alloy", "Cloth", "Uranium"
            };

            var map = new Dictionary<string, int>();
            if (_currentLoadout?.totalResourceCosts != null)
            {
                foreach (var c in _currentLoadout.totalResourceCosts)
                {
                    if (c == null || string.IsNullOrEmpty(c.id)) continue;
                    map[c.id] = c.amount;
                }
            }

            foreach (var id in ids)
            {
                var lab = _shipBuilderPanel.Q<Label>($"CostValue_{id}");
                if (lab == null) continue;
                lab.text = map.TryGetValue(id, out int n) ? n.ToString() : "0";
            }
        }

        private void OnUIReady(PanelRenderer renderer, VisualElement rootElement)
        {
            root = rootElement;
        }

        public void OpenShipBuilder()
        {
            if (root == null) return;

            if (_shipBuilderPanel != null)
                _shipBuilderPanel.RemoveFromHierarchy();

            _shipBuilderPanel = new VisualElement { name = "ShipBuilderHost" };
            _shipBuilderPanel.style.flexDirection = FlexDirection.Row;
            _shipBuilderPanel.style.alignItems = Align.Stretch;
            _shipBuilderPanel.style.position = Position.Absolute;
            _shipBuilderPanel.style.top = 0;
            _shipBuilderPanel.style.left = 0;
            _shipBuilderPanel.style.right = 240;
            _shipBuilderPanel.style.bottom = 12;
            _shipBuilderPanel.style.justifyContent = Justify.Center;
            _shipBuilderPanel.style.alignItems = Align.Center;

            var card = new VisualElement { name = "ShipBuilderPanel" };
            card.style.width = Length.Percent(94f);
            card.style.height = Length.Percent(92f);
            card.style.flexDirection = FlexDirection.Column;
            card.style.backgroundColor = new Color(0.06f, 0.10f, 0.22f, 0.98f);
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
            card.style.paddingTop = 16;
            card.style.paddingBottom = 16;
            card.style.paddingLeft = 20;
            card.style.paddingRight = 20;
            card.style.overflow = Overflow.Hidden;
            card.style.minWidth = 0;
            card.style.minHeight = 0;

            card.Add(CreateHeader());

            var body = new VisualElement { name = "BuilderBody" };
            body.style.flexDirection = FlexDirection.Row;
            body.style.flexGrow = 1;
            body.style.minHeight = 0;

            var main = new VisualElement { name = "BuilderMain" };
            main.style.flexGrow = 1;
            main.style.flexShrink = 1;
            main.style.minWidth = 0;
            main.style.flexDirection = FlexDirection.Column;

            var selectHullBtn = new Button { text = "Select Hull" };
            selectHullBtn.style.fontSize = 16;
            selectHullBtn.style.height = 44;
            selectHullBtn.style.marginBottom = 10;
            selectHullBtn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            selectHullBtn.style.color = Color.white;
            selectHullBtn.style.borderTopLeftRadius = 6;
            selectHullBtn.style.borderTopRightRadius = 6;
            selectHullBtn.style.borderBottomLeftRadius = 6;
            selectHullBtn.style.borderBottomRightRadius = 6;
            selectHullBtn.clicked += () => hullSelectionHUD?.OpenHullSelection();
            var hullRow = new VisualElement();
            hullRow.style.flexDirection = FlexDirection.Row;
            hullRow.style.alignItems = Align.Center;
            hullRow.style.marginBottom = 10;

            selectHullBtn.style.flexGrow = 1;
            selectHullBtn.style.marginBottom = 0;
            selectHullBtn.style.marginRight = 8;
            hullRow.Add(selectHullBtn);

            _clearBtn = new Button { text = "Clear" };
            _clearBtn.style.width = 88;
            _clearBtn.style.height = 44;
            _clearBtn.style.fontSize = 15;
            _clearBtn.style.color = Color.white;
            _clearBtn.style.borderTopLeftRadius = 6;
            _clearBtn.style.borderTopRightRadius = 6;
            _clearBtn.style.borderBottomLeftRadius = 6;
            _clearBtn.style.borderBottomRightRadius = 6;
            ApplyClearButtonLook(false);
            _clearBtn.clicked += ToggleClearMode;
            hullRow.Add(_clearBtn);

            main.Add(hullRow);
            
            

            main.Add(CreateHullPreviewArea());
            
            main.Add(CreateShipNameField());
            
            main.Add(CreateBuildCostRow());
            
            RefreshBuildCostNumbers();

            body.Add(main);
            
            card.Add(body);

            _shipBuilderPanel.Add(card);
            card.style.flexGrow = 1;
            card.style.width = StyleKeyword.Null;
            card.style.marginRight = 8;

            var side = CreateBuilderSidePanel();
            side.style.marginLeft = 0;
            side.style.height = Length.Percent(92f);
            _shipBuilderPanel.Add(side);
            root.Add(_shipBuilderPanel);
        }
        private VisualElement CreateShipNameField()
        {
            _shipNameField = new TextField { value = ShipNameHint };
            _shipNameField.style.height = 36;
            _shipNameField.style.fontSize = 16;
            _shipNameField.style.color = Color.white;
            _shipNameField.style.marginTop = 4;
            _shipNameField.style.marginBottom = 2;
            _shipNameField.style.paddingLeft = 8;
            _shipNameField.style.paddingRight = 8;
            _shipNameField.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            _shipNameField.style.borderTopWidth = 2;
            _shipNameField.style.borderRightWidth = 2;
            _shipNameField.style.borderBottomWidth = 2;
            _shipNameField.style.borderLeftWidth = 2;
            _shipNameField.style.borderTopLeftRadius = 6;
            _shipNameField.style.borderTopRightRadius = 6;
            _shipNameField.style.borderBottomLeftRadius = 6;
            _shipNameField.style.borderBottomRightRadius = 6;
            ApplyNameFieldLook(false);

            _shipNameField.RegisterCallback<FocusInEvent>(_ =>
            {
                if (_shipNameField.value == ShipNameHint)
                    _shipNameField.value = "";
            });

            return _shipNameField;
        }
        
        private void ToggleClearMode()
        {
            _clearMode = !_clearMode;
            ApplyClearButtonLook(_clearMode);
        }

        private void ApplyClearButtonLook(bool on)
        {
            if (_clearBtn == null) return;
            if (on)
            {
                _clearBtn.style.backgroundColor = new Color(0.85f, 0.45f, 0.12f);
                _clearBtn.style.borderTopWidth = 2;
                _clearBtn.style.borderRightWidth = 2;
                _clearBtn.style.borderBottomWidth = 2;
                _clearBtn.style.borderLeftWidth = 2;
                _clearBtn.style.borderTopColor = Color.cyan;
                _clearBtn.style.borderRightColor = Color.cyan;
                _clearBtn.style.borderBottomColor = Color.cyan;
                _clearBtn.style.borderLeftColor = Color.cyan;
            }
            else
            {
                _clearBtn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
                _clearBtn.style.borderTopWidth = 0;
                _clearBtn.style.borderRightWidth = 0;
                _clearBtn.style.borderBottomWidth = 0;
                _clearBtn.style.borderLeftWidth = 0;
            }
        }

            private VisualElement CreateHullPreviewArea()
            {
                var frame = new VisualElement { name = "HullPreviewFrame" };
                frame.style.flexGrow = 1;
                frame.style.flexShrink = 1;
                frame.style.minHeight = 0;
                frame.style.alignItems = Align.Center;
                frame.style.justifyContent = Justify.Center;
                frame.style.marginBottom = 12;
                frame.style.paddingTop = 8;
                frame.style.paddingBottom = 8;
                frame.style.paddingLeft = 8;
                frame.style.paddingRight = 8;
                frame.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
                frame.style.borderTopWidth = 2;
                frame.style.borderRightWidth = 2;
                frame.style.borderBottomWidth = 2;
                frame.style.borderLeftWidth = 2;
                frame.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
                frame.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
                frame.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
                frame.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
                frame.style.borderTopLeftRadius = 8;
                frame.style.borderTopRightRadius = 8;
                frame.style.borderBottomLeftRadius = 8;
                frame.style.borderBottomRightRadius = 8;
                

                _hullCanvas = new VisualElement { name = "HullCanvas" };
                _hullCanvas.style.position = Position.Relative;
                _hullCanvas.style.flexShrink = 0;

                _hullImageElement = new VisualElement { name = "HullImage" };
                _hullImageElement.style.position = Position.Absolute;
                _hullImageElement.style.top = 0;
                _hullImageElement.style.left = 0;
                _hullImageElement.style.right = 0;
                _hullImageElement.style.bottom = 0;
                _hullImageElement.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;

                _slotOverlay = new VisualElement { name = "SlotOverlay" };
                _slotOverlay.style.position = Position.Absolute;
                _slotOverlay.style.top = 0;
                _slotOverlay.style.left = 0;
                _slotOverlay.style.right = 0;
                _slotOverlay.style.bottom = 0;

                _hullCanvas.Add(_hullImageElement);
                _hullCanvas.Add(_slotOverlay);
                frame.Add(_hullCanvas);
                frame.RegisterCallback<GeometryChangedEvent>(_ => FitHullCanvas());
                return frame;
            }

            private VisualElement CreateHeader()
            {
                var header = new VisualElement();
                header.style.flexDirection = FlexDirection.Row;
                header.style.alignItems = Align.Center;
                header.style.marginBottom = 12;
                header.style.minHeight = 44;

                var leftPad = new VisualElement();
                leftPad.style.width = 44;
                leftPad.style.height = 44;
                header.Add(leftPad);

                var title = new Label("Build a Ship");
                title.style.flexGrow = 1;
                title.style.fontSize = 26;
                title.style.color = Color.cyan;
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                title.style.unityTextAlign = TextAnchor.MiddleCenter;
                header.Add(title);

                var closeBtn = new Button { text = "✕" };
                closeBtn.style.width = 44;
                closeBtn.style.height = 44;
                closeBtn.style.fontSize = 22;
                closeBtn.style.color = Color.white;
                closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
                closeBtn.style.borderTopLeftRadius = 8;
                closeBtn.style.borderTopRightRadius = 8;
                closeBtn.style.borderBottomLeftRadius = 8;
                closeBtn.style.borderBottomRightRadius = 8;
                closeBtn.clicked += CloseShipBuilder;
                header.Add(closeBtn);

                return header;
            }
            
            private VisualElement CreateBuilderSidePanel()
            {
                var side = new VisualElement { name = "BuilderSidePanel" };
                side.style.width = 280;
                side.style.flexShrink = 0;
                side.style.marginLeft = 14;
                side.style.paddingTop = 12;
                side.style.paddingBottom = 12;
                side.style.paddingLeft = 12;
                side.style.paddingRight = 12;
                side.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
                side.style.borderTopWidth = 2;
                side.style.borderRightWidth = 2;
                side.style.borderBottomWidth = 2;
                side.style.borderLeftWidth = 2;
                side.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
                side.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
                side.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
                side.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
                side.style.borderTopLeftRadius = 8;
                side.style.borderTopRightRadius = 8;
                side.style.borderBottomLeftRadius = 8;
                side.style.borderBottomRightRadius = 8;

                _hullNameLabel = new Label("No Hull Selected");
                _hullNameLabel.style.fontSize = 18;
                _hullNameLabel.style.color = Color.cyan;
                _hullNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                _hullNameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                _hullNameLabel.style.marginBottom = 12;
                _hullNameLabel.style.paddingBottom = 8;
                _hullNameLabel.style.borderBottomWidth = 1;
                _hullNameLabel.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.7f);
                side.Add(_hullNameLabel);

                var stats = CreateStatsPanel();
                stats.style.flexGrow = 1;
                stats.style.marginTop = 0;
                side.Add(stats);
                
                _buildTimerLabel = new Label("Build Time: --:--");
                _buildTimerLabel.style.fontSize = 16;
                _buildTimerLabel.style.color = Color.cyan;
                _buildTimerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                _buildTimerLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                _buildTimerLabel.style.marginTop = 8;
                _buildTimerLabel.style.marginBottom = 8;
                side.Add(_buildTimerLabel);

                side.Add(CreateSideActionButton("Start Build", StartBuild));

                //side.Add(CreateSideActionButton("Start Build", BuildCurrentShip));
                
                side.Add(CreateSideActionButton("Instant Build", InstantBuild));
                side.Add(CreateInstantTokenRow());
                
                side.Add(CreateSideActionButton("Save Blueprint", SaveBlueprintFromShipName));
                side.Add(CreateSideActionButton("Load Blueprint", OpenBlueprintListPanel));

                return side;
            }
            
            private VisualElement CreateInstantTokenRow()
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.justifyContent = Justify.Center;
                row.style.marginTop = 4;
                row.style.marginBottom = 8;

                var icon = new VisualElement();
                icon.style.width = 22;
                icon.style.height = 22;
                icon.style.flexShrink = 0;
                icon.style.marginRight = 8;

                var sprite = instantBuildTokenIcon;
                if (sprite == null && walletHolder?.Catalog != null)
                    sprite = walletHolder.Catalog.Get(InstantTokenId)?.icon;

                if (sprite != null)
                {
                    icon.style.backgroundImage = new StyleBackground(sprite);
                    icon.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
                }
                else
                    icon.style.backgroundColor = new Color(0.16f, 0.22f, 0.40f);

                row.Add(icon);

                int have = walletHolder != null && walletHolder.Wallet != null
                    ? walletHolder.Wallet.Get(InstantTokenId)
                    : 0;
                _instantTokenCountLabel = new Label(have.ToString());
                _instantTokenCountLabel.style.fontSize = 14;
                _instantTokenCountLabel.style.color = Color.white;
                _instantTokenCountLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(_instantTokenCountLabel);
                return row;
            }

            private void RefreshInstantTokenCount()
            {
                if (_instantTokenCountLabel == null) return;
                int have = walletHolder != null && walletHolder.Wallet != null
                    ? walletHolder.Wallet.Get(InstantTokenId)
                    : 0;
                _instantTokenCountLabel.text = have.ToString();
            }
            
            private void SaveBlueprintFromShipName()
            {
                string name = _shipNameField != null ? _shipNameField.value.Trim() : "";
                if (string.IsNullOrEmpty(name) || name == ShipNameHint)
                {
                    Debug.LogWarning("[ShipSave] Name the ship first.");
                    return;
                }

                if (_currentLoadout?.hull == null)
                {
                    Debug.LogWarning("[ShipSave] No hull selected.");
                    return;
                }

                _shipSave ??= new ShipSaveService(new JsonShipSaveStore());
                var record = _shipSave.Capture(_currentLoadout, name);
                if (record == null)
                {
                    Debug.LogError("[ShipSave] Capture failed.");
                    return;
                }

                _shipSave.AppendAndWrite(record);
                Debug.Log($"<color=lime>[ShipSave] Saved '{record.shipName}'</color>");
                OpenBlueprintListPanel();
            }
            
            private void InstantBuild()
            {
                if (_currentLoadout?.hull == null)
                {
                    Debug.LogWarning("[Build] No hull selected.");
                    return;
                }
                
                string shipName = _shipNameField != null ? _shipNameField.value.Trim() : "";
                if (string.IsNullOrEmpty(shipName) || shipName == ShipNameHint)
                {
                    Debug.LogWarning("[Build] Name the ship first.");
                    return;
                }

                _currentLoadout.RecalculateStats();

                if (walletHolder == null || walletHolder.Wallet == null)
                {
                    Debug.LogWarning("[Build] No wallet — cannot build.");
                    return;
                }
                
                if (!_currentLoadout.AllSlotsFilled())
                {
                    Debug.LogWarning("[Build] Equip every slot first.");
                    return;
                }

                if (!walletHolder.Wallet.CanAfford(_currentLoadout.totalResourceCosts))
                {
                    Debug.LogWarning("[Build] Not enough resources.");
                    return;
                }

                if (walletHolder.Wallet.Get(InstantTokenId) < instantBuildTokenCost)
                {
                    Debug.LogWarning("[Build] Need a Piece of Eight for Instant Build.");
                    return;
                }

                _isBuilding = false;
                _buildRemaining = 0f;

                if (!CompleteBuild())
                    return;

                walletHolder.Wallet.TrySpend(InstantTokenId, instantBuildTokenCost);
                walletHolder.Wallet.Save();
                RefreshInstantTokenCount();
                GetComponent<HarbourHUD>()?.RefreshIdleResourceAmounts();
            }
            
        private VisualElement CreateSaveBlueprintBlock()
        {
            var wrap = new VisualElement { name = "SaveBlueprintBlock" };
            wrap.style.marginTop = 6;

            var saveBtn = CreateSideActionButton("Save Blueprint", ToggleSaveNameDropdown);
            saveBtn.style.marginTop = 0;
            wrap.Add(saveBtn);

            _saveNameDropdown = new VisualElement { name = "SaveNameDropdown" };
            _saveNameDropdown.style.flexDirection = FlexDirection.Row;
            _saveNameDropdown.style.alignItems = Align.Center;
            _saveNameDropdown.style.overflow = Overflow.Hidden;
            _saveNameDropdown.style.height = 0;
            _saveNameDropdown.style.opacity = 0;
            _saveNameDropdown.style.marginTop = 0;
            _saveNameDropdown.style.backgroundColor = new Color(0.08f, 0.12f, 0.25f, 0.98f);
            _saveNameDropdown.style.borderBottomLeftRadius = 6;
            _saveNameDropdown.style.borderBottomRightRadius = 6;

            _saveNameField = new TextField { value = "" };
            _saveNameField.style.flexGrow = 1;
            _saveNameField.style.marginLeft = 4;
            _saveNameField.style.marginRight = 4;
            _saveNameField.RegisterCallback<KeyDownEvent>(OnSaveNameKeyDown, TrickleDown.TrickleDown);
            _saveNameDropdown.Add(_saveNameField);

            var ok = new Button { text = "Save" };
            ok.style.height = 32;
            ok.style.marginRight = 4;
            ok.style.fontSize = 13;
            ok.style.color = Color.white;
            ok.style.backgroundColor = new Color(0.12f, 0.42f, 0.38f);
            ok.style.borderTopLeftRadius = 4;
            ok.style.borderTopRightRadius = 4;
            ok.style.borderBottomLeftRadius = 4;
            ok.style.borderBottomRightRadius = 4;
            ok.clicked += ConfirmNamedSave;
            _saveNameDropdown.Add(ok);

            wrap.Add(_saveNameDropdown);
            _saveDropdownOpen = false;
            return wrap;
        }

        private void OnSaveNameKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
                return;
            evt.StopPropagation();
            ConfirmNamedSave();
        }

        private void ToggleSaveNameDropdown()
        {
            if (_currentLoadout?.hull == null)
            {
                Debug.LogWarning("[ShipSave] No hull selected.");
                return;
            }

            _saveDropdownOpen = !_saveDropdownOpen;
            float h = _saveDropdownOpen ? 40f : 0f;
            float o = _saveDropdownOpen ? 1f : 0f;
            _saveNameDropdown.experimental.animation.Start(
                new StyleValues { height = _saveDropdownOpen ? 0f : 40f, opacity = _saveDropdownOpen ? 0f : 1f },
                new StyleValues { height = h, opacity = o },
                180);

            if (_saveDropdownOpen)
            {
                if (string.IsNullOrWhiteSpace(_saveNameField.value) && _hullNameLabel != null)
                    _saveNameField.value = _hullNameLabel.text;
                _saveNameField.schedule.Execute(() => _saveNameField.Focus());
            }
        }

        private void ConfirmNamedSave()
        {
            string name = _saveNameField != null ? _saveNameField.value : null;
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("[ShipSave] Type a name first.");
                return;
            }

            if (_currentLoadout?.hull == null)
            {
                Debug.LogWarning("[ShipSave] No hull selected.");
                return;
            }

            _shipSave ??= new ShipSaveService(new JsonShipSaveStore());
            var record = _shipSave.Capture(_currentLoadout, name.Trim());
            if (record == null)
            {
                Debug.LogError("[ShipSave] Capture failed.");
                return;
            }

            _shipSave.AppendAndWrite(record);
            Debug.Log($"<color=lime>[ShipSave] Saved '{record.shipName}'</color>");

            if (_saveDropdownOpen)
                ToggleSaveNameDropdown();

            OpenBlueprintListPanel();
        }
        
        private void OpenBlueprintListPanel()
        {
            CloseBlueprintListPanel();

            _shipSave ??= new ShipSaveService(new JsonShipSaveStore());
            _shipSave.TryLoad(out var data);
            var ships = data?.ships;

            _blueprintListOverlay = new VisualElement { name = "BlueprintListOverlay" };
            _blueprintListOverlay.style.position = Position.Absolute;
            _blueprintListOverlay.style.top = 0;
            _blueprintListOverlay.style.left = 0;
            _blueprintListOverlay.style.right = 0;
            _blueprintListOverlay.style.bottom = 0;
            _blueprintListOverlay.pickingMode = PickingMode.Position;

            var dimmer = new VisualElement();
            dimmer.style.position = Position.Absolute;
            dimmer.style.top = 0;
            dimmer.style.left = 0;
            dimmer.style.right = 0;
            dimmer.style.bottom = 0;
            dimmer.style.backgroundColor = new Color(0f, 0f, 0f, 0.45f);
            dimmer.RegisterCallback<ClickEvent>(_ => CloseBlueprintListPanel());
            _blueprintListOverlay.Add(dimmer);

            var card = new VisualElement { name = "BlueprintListCard" };
            card.style.position = Position.Absolute;
            card.style.top = Length.Percent(18);
            card.style.bottom = Length.Percent(18);
            card.style.left = Length.Percent(32);
            card.style.right = Length.Percent(32);
            card.style.maxWidth = 420;
            card.style.alignSelf = Align.Center;
            card.style.backgroundColor = new Color(0.06f, 0.10f, 0.22f, 0.98f);
            card.style.paddingTop = 14;
            card.style.paddingBottom = 14;
            card.style.paddingLeft = 16;
            card.style.paddingRight = 16;
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

            var title = new Label("Ship Save/Load Blueprints");
            title.style.flexGrow = 1;
            title.style.fontSize = 18;
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
            close.clicked += CloseBlueprintListPanel;
            header.Add(close);
            card.Add(header);

            var list = new ScrollView();
            list.style.flexGrow = 1;
            if (ships == null || ships.Count == 0)
            {
                var empty = new Label("No blueprints saved");
                empty.style.color = Color.white;
                empty.style.unityTextAlign = TextAnchor.MiddleCenter;
                empty.style.marginTop = 20;
                list.Add(empty);
            }
            else
            {
                for (int i = ships.Count - 1; i >= 0; i--)
                {
                    var rec = ships[i];
                    if (rec == null) continue;
                    var row = new Button { text = rec.shipName };
                    row.style.height = 40;
                    row.style.marginBottom = 6;
                    row.style.unityTextAlign = TextAnchor.MiddleLeft;
                    row.style.color = Color.white;
                    row.style.backgroundColor = new Color(0.14f, 0.18f, 0.34f);
                    row.style.borderTopLeftRadius = 6;
                    row.style.borderTopRightRadius = 6;
                    row.style.borderBottomLeftRadius = 6;
                    row.style.borderBottomRightRadius = 6;
                    var captured = rec;
                    row.clicked += () =>
                    {
                        ApplyBlueprintRecord(captured);
                        CloseBlueprintListPanel();
                    };
                    list.Add(row);
                }
            }

            card.Add(list);
            _blueprintListOverlay.Add(card);
            _shipBuilderPanel.Add(_blueprintListOverlay);
        }

        private void CloseBlueprintListPanel()
        {
            if (_blueprintListOverlay == null) return;
            _blueprintListOverlay.RemoveFromHierarchy();
            _blueprintListOverlay = null;
        }
            
            private void BuildCurrentShip()
            {
                if (_currentLoadout?.hull == null)
                {
                    Debug.LogWarning("[Build] No hull selected.");
                    return;
                }

                if (builtShipInventory == null)
                {
                    Debug.LogError("[Build] Assign BuiltShipInventory on ShipBuilderHUD.");
                    return;
                }

                string shipName = _shipNameField != null ? _shipNameField.value.Trim() : "";
                if (string.IsNullOrEmpty(shipName) || shipName == ShipNameHint)
                {
                    Debug.LogWarning("[Build] Name the ship first.");
                    return;
                }

                _shipSave ??= new ShipSaveService(new JsonShipSaveStore());
                var record = _shipSave.Capture(_currentLoadout, shipName);
                if (record == null)
                {
                    Debug.LogError("[Build] Capture failed.");
                    return;
                }

                _shipSave.AppendAndWrite(record);

                var blueprint = ScriptableObject.CreateInstance<ShipBlueprint>();
                blueprint.PopulateFromLoadout(_currentLoadout, shipName);
                builtShipInventory.Register(blueprint);

                Debug.Log($"<color=lime>[Build] '{shipName}' saved + in yard | count={builtShipInventory.ships.Count}</color>");
            }

            private Button CreateSideActionButton(string text, Action onClick)
            {
                var btn = new Button { text = text };
                btn.style.height = 40;
                btn.style.marginTop = 6;
                btn.style.fontSize = 14;
                btn.style.color = Color.white;
                btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
                btn.style.borderTopLeftRadius = 6;
                btn.style.borderTopRightRadius = 6;
                btn.style.borderBottomLeftRadius = 6;
                btn.style.borderBottomRightRadius = 6;
                btn.clicked += onClick;
                return btn;
            }
        

        public void SetSelectedHull(HullData hull)
        {
            _currentHull = hull;
            _currentLoadout = new ShipLoadout { hull = hull };
            _currentLoadout.RecalculateStats();

            RefreshNameReadyLook();

            if (hull?.hullImage == null) return;

            _hullNameLabel.text = hull.hullName;

            var tex = hull.hullImage.texture;
            // _hullCanvas.style.width = tex.width;
            // _hullCanvas.style.height = tex.height;

            _hullImageElement.style.width = Length.Percent(100);
            _hullImageElement.style.height = Length.Percent(100);
            _hullImageElement.style.backgroundImage = new StyleBackground(hull.hullImage);
            _hullImageElement.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;

            DrawSlotVisuals();
            _currentLoadout.RecalculateStats();
            UpdateStatsDisplay();
            _hullCanvas.schedule.Execute(FitHullCanvas);
        }
        
        private VisualElement CreateBuildCostRow()
        {
            var block = new VisualElement { name = "BuildCostBlock" };
            block.style.flexShrink = 0;
            block.style.width = Length.Percent(100);
            
            var header = new Label("Costs");
            header.style.fontSize = 16;
            header.style.color = Color.cyan;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.marginTop = 0;
            header.style.marginBottom = 4;
            header.style.paddingBottom = 4;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            block.Add(header);

            string[] ids =
            {
                "Oil", "Iron", "Steel",
                "Energy", "Aluminium", "Lumber",
                "Alloy", "Cloth", "Uranium"
            };

            for (int r = 0; r < 3; r++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.Center;
                row.style.marginBottom = 6;
                row.style.flexWrap = Wrap.NoWrap;
                row.style.width = Length.Percent(100);
                row.style.flexShrink = 0;

                for (int c = 0; c < 3; c++)
                    row.Add(CreateCostCell(ids[r * 3 + c]));

                block.Add(row);
            }

            return block;
        }

        private VisualElement CreateCostCell(string id)
        {
            var cell = new VisualElement { name = $"CostCell_{id}" };
            cell.style.flexDirection = FlexDirection.Row;
            cell.style.alignItems = Align.Center;
            cell.style.flexGrow = 1;
            cell.style.flexShrink = 1;
            cell.style.flexBasis = Length.Percent(33f);
            cell.style.minWidth = 0;
            cell.style.marginLeft = 6;
            cell.style.marginRight = 6;

            var check = new Toggle();
            check.value = false;
            check.style.flexShrink = 0;
            check.style.marginRight = 6;
            cell.Add(check);
            
            
            var def = walletHolder != null && walletHolder.Catalog != null
                ? walletHolder.Catalog.Get(id)
                : null;

            var nameLab = new Label(def != null && !string.IsNullOrEmpty(def.displayName)
                ? def.displayName
                : id);
            nameLab.name = $"CostName_{id}";
            nameLab.style.fontSize = 11;
            nameLab.style.color = new Color(0.7f, 0.85f, 1f);
            nameLab.style.flexShrink = 0;
            nameLab.style.marginRight = 6;
            cell.Add(nameLab);
            

            var value = new Label("0");
            value.name = $"CostValue_{id}";
            value.style.flexGrow = 1;
            value.style.minWidth = 0;
            value.style.fontSize = 13;
            value.style.color = Color.white;
            value.style.borderBottomWidth = 1;
            value.style.borderBottomColor = Color.white;
            value.style.paddingBottom = 1;
            cell.Add(value);

            var iconBox = new VisualElement();
            iconBox.style.width = 22;
            iconBox.style.height = 22;
            iconBox.style.flexShrink = 0;
            iconBox.style.marginLeft = 8;
            iconBox.style.backgroundColor = new Color(0.16f, 0.22f, 0.40f);
            iconBox.style.borderTopWidth = 1;
            iconBox.style.borderRightWidth = 1;
            iconBox.style.borderBottomWidth = 1;
            iconBox.style.borderLeftWidth = 1;
            iconBox.style.borderTopColor = new Color(0.4f, 0.7f, 1f, 0.7f);
            iconBox.style.borderRightColor = new Color(0.4f, 0.7f, 1f, 0.7f);
            iconBox.style.borderBottomColor = new Color(0.4f, 0.7f, 1f, 0.7f);
            iconBox.style.borderLeftColor = new Color(0.4f, 0.7f, 1f, 0.7f);

            
            
            iconBox.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            iconBox.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            iconBox.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            
            cell.Add(iconBox);

            int have = walletHolder != null && walletHolder.Wallet != null
                ? walletHolder.Wallet.Get(id)
                : 0;
            value.text = "0";

            // var def = walletHolder != null && walletHolder.Catalog != null
            //     ? walletHolder.Catalog.Get(id)
            //     : null;
            if (def?.icon != null)
            {
                iconBox.style.backgroundImage = new StyleBackground(def.icon);
                iconBox.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
                iconBox.style.backgroundColor = Color.clear;
            }

            return cell;
        }
        
        private void RefreshNameReadyLook()
        {
            bool ready = _currentLoadout != null
                         && _currentHull != null
                         && _currentLoadout.AllSlotsFilled()
                         && _currentHull.moduleSlots != null
                         && _currentHull.moduleSlots.Length > 0;

            ApplyNameFieldLook(ready);

            if (!ready) return;
            if (_shipNameField == null) return;
            if (string.IsNullOrWhiteSpace(_shipNameField.value) || _shipNameField.value == ShipNameHint)
                _shipNameField.value = string.IsNullOrEmpty(_currentHull.hullName)
                    ? "Ready"
                    : _currentHull.hullName;
        }

        private void ApplyNameFieldLook(bool ready)
        {
            if (_shipNameField == null) return;
            var edge = ready ? Color.cyan : new Color(0.25f, 0.35f, 0.55f);
            _shipNameField.style.borderTopColor = edge;
            _shipNameField.style.borderRightColor = edge;
            _shipNameField.style.borderBottomColor = edge;
            _shipNameField.style.borderLeftColor = edge;
        }
        
        private VisualElement CreateBuildActionRow()
        {
            var row = new VisualElement { name = "BuildActionRow" };
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap = Wrap.NoWrap;
            row.style.justifyContent = Justify.Center;
            row.style.width = Length.Percent(100);
            row.style.flexShrink = 0;
            row.style.marginBottom = 10;

            row.Add(CreateActionButton("Save Build", SaveCurrentBuild));
            row.Add(CreateActionButton("Load Build", () =>
                Debug.Log("ShipBuilder: Load Build (stub)")));
            row.Add(CreateActionButton("Start Build", () =>
                Debug.Log("ShipBuilder: Build (stub)")));
            return row;
        }

        private Button CreateActionButton(string text, Action onClick)
        {
            var btn = new Button { text = text };
            btn.style.height = 40;
            btn.style.flexGrow = 1;
            btn.style.flexShrink = 1;
            btn.style.flexBasis = Length.Percent(33f);
            btn.style.minWidth = 0;
            btn.style.marginLeft = 6;
            btn.style.marginRight = 6;
            btn.style.fontSize = 14;
            btn.style.color = Color.white;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
            btn.clicked += onClick;
            return btn;
        }

        private void FitHullCanvas()
        {
            if (_hullCanvas == null || _hullCanvas.parent == null) return;
            if (_currentHull?.hullImage == null) return;

            var host = _hullCanvas.parent.contentRect;
            var tex = _currentHull.hullImage.texture;
            if (host.width < 8f || host.height < 8f) return;
            if (tex.width < 1 || tex.height < 1) return;

            _hullDrawScale = Mathf.Min(host.width / tex.width, host.height / tex.height);
            _hullDrawScale = Mathf.Clamp(_hullDrawScale, 0.35f, 1f);

            _hullCanvas.style.scale = StyleKeyword.Null;
            _hullCanvas.style.width = tex.width * _hullDrawScale;
            _hullCanvas.style.height = tex.height * _hullDrawScale;

            DrawSlotVisuals();
        }

        private void DrawSlotVisuals()
        {
            _slotOverlay.Clear();
            if (!showSlotVisuals || _currentHull?.moduleSlots == null) return;

            foreach (var slot in _currentHull.moduleSlots)
            {
                var slotBtn = new Button();
                slotBtn.style.position = Position.Absolute;
                float s = _hullDrawScale;
                slotBtn.style.left = slot.pixelPosition.x * s - 36f * s;
                slotBtn.style.top = slot.pixelPosition.y * s - 36f * s;
                slotBtn.style.width = 72f * s;
                slotBtn.style.height = 72f * s;
                
                slotBtn.style.position = Position.Absolute;
                slotBtn.style.left = slot.pixelPosition.x * s - 36f * s;
                slotBtn.style.top = slot.pixelPosition.y * s - 36f * s;
                slotBtn.style.width = 72f * s;
                slotBtn.style.height = 72f * s;
                slotBtn.style.paddingTop = 0;
                slotBtn.style.paddingBottom = 0;
                slotBtn.style.paddingLeft = 0;
                slotBtn.style.paddingRight = 0;
                slotBtn.style.alignItems = Align.Center;
                slotBtn.style.justifyContent = Justify.Center;
                slotBtn.style.overflow = Overflow.Hidden;
                

                // Frame styling
                slotBtn.style.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 0.18f);
                slotBtn.style.borderTopWidth = 3;
                slotBtn.style.borderRightWidth = 3;
                slotBtn.style.borderBottomWidth = 3;
                slotBtn.style.borderLeftWidth = 3;
                slotBtn.style.borderTopColor = new Color(0.4f, 0.8f, 1f, 0.9f);
                slotBtn.style.borderRightColor = new Color(0.4f, 0.8f, 1f, 0.9f);
                slotBtn.style.borderBottomColor = new Color(0.4f, 0.8f, 1f, 0.9f);
                slotBtn.style.borderLeftColor = new Color(0.4f, 0.8f, 1f, 0.9f);
                slotBtn.style.borderTopLeftRadius = 10;
                slotBtn.style.borderTopRightRadius = 10;
                slotBtn.style.borderBottomLeftRadius = 10;
                slotBtn.style.borderBottomRightRadius = 10;

                // Hover
                slotBtn.RegisterCallback<MouseEnterEvent>(_ => slotBtn.style.backgroundColor = new Color(0.3f, 0.6f, 1f, 0.35f));
                slotBtn.RegisterCallback<MouseLeaveEvent>(_ => slotBtn.style.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 0.18f));

                // === ICON LOGIC ===
                var icon = new VisualElement();
                icon.style.width = Length.Percent(100);
                icon.style.height = Length.Percent(100);
                icon.style.flexShrink = 0;
                icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

                Sprite displayIcon = null;
                Color tint = Color.white;

                if (slot.equippedModule != null && slot.equippedModule.icon != null)
                {
                    displayIcon = slot.equippedModule.icon;
                    tint = Color.white;
                    // Highlight equipped slot
                    slotBtn.style.backgroundColor = new Color(0.1f, 0.35f, 0.7f, 0.45f);
                }
                else if (slot.slotIcon != null)
                {
                    displayIcon = slot.slotIcon;
                    tint = new Color(0.65f, 0.65f, 0.65f, 0.75f);   // Greyed out
                }
                else
                {
                    // Fallback colored box
                    Color fallback = slot.acceptedType switch
                    {
                        ModuleType.Weapon => new Color(0.6f, 0.2f, 0.2f, 0.6f),
                        ModuleType.Armour => new Color(0.4f, 0.4f, 0.6f, 0.6f),
                        ModuleType.Engine => new Color(0.3f, 0.5f, 0.3f, 0.6f),
                        _ => new Color(0.5f, 0.5f, 0.5f, 0.6f)
                    };
                    icon.style.backgroundColor = fallback;
                }

                if (displayIcon != null)
                {
                    icon.style.backgroundImage = new StyleBackground(displayIcon);
                    icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    icon.style.unityBackgroundImageTintColor = tint;
                }

                slotBtn.Add(icon);

                // Click handler
                slotBtn.clicked += () => HandleSlotClick(slot);

                _slotOverlay.Add(slotBtn);
            }
        }

        private void HandleSlotClick(ModuleSlot slot)
        {
            Debug.Log($"<color=lime>Slot clicked: {slot.slotId} ({slot.acceptedType})</color>");

            if (_clearMode)
            {
                _currentLoadout?.UnequipSlot(slot);
                DrawSlotVisuals();
                UpdateStatsDisplay();
                RefreshBuildCostNumbers();
                RefreshNameReadyLook();
                return;
            }
            
            switch (slot.acceptedType)
            {
                case ModuleType.Weapon:
                    if (weaponSelectionHUD != null)
                    {
                        weaponSelectionHUD.OnWeaponEquipped += OnWeaponEquipped;
                        weaponSelectionHUD.OpenWeaponSelection(slot);
                    }
                    break;

                case ModuleType.Armour:
                    if (armourSelectionHUD != null)
                    {
                        armourSelectionHUD.OnArmourEquipped += OnArmourEquipped;
                        armourSelectionHUD.OpenArmourSelection(slot);
                    }
                    break;

                case ModuleType.Engine:
                    if (engineSelectionHUD != null)
                    {
                        engineSelectionHUD.OnEngineEquipped += OnEngineEquipped;
                        engineSelectionHUD.OpenEngineSelection(slot);
                    }
                    break;

                case ModuleType.Component:
                    if (componentSelectionHUD != null)
                    {
                        componentSelectionHUD.OnComponentEquipped += OnComponentEquipped;
                        componentSelectionHUD.OpenComponentSelection(slot);
                    }
                    break;
            }
        }

        // Event Handlers
        private void OnWeaponEquipped(WeaponData weapon, ModuleSlot slot)
        {
            weaponSelectionHUD.OnWeaponEquipped -= OnWeaponEquipped;
            if (_currentLoadout != null) _currentLoadout.EquipModule(slot, weapon);
            DrawSlotVisuals();   // Refresh visuals after equipping
            UpdateStatsDisplay();
            RefreshNameReadyLook();
        }

        private void OnArmourEquipped(ArmourData armour, ModuleSlot slot)
        {
            armourSelectionHUD.OnArmourEquipped -= OnArmourEquipped;
            if (_currentLoadout != null) _currentLoadout.EquipModule(slot, armour);
            DrawSlotVisuals();   // Refresh visuals after equipping
            UpdateStatsDisplay();
            RefreshNameReadyLook();
        }

        private void OnEngineEquipped(EngineData engine, ModuleSlot slot)
        {
            engineSelectionHUD.OnEngineEquipped -= OnEngineEquipped;
            if (_currentLoadout != null) _currentLoadout.EquipModule(slot, engine);
            DrawSlotVisuals();   // Refresh visuals after equipping
            UpdateStatsDisplay();
            RefreshNameReadyLook();
        }

        private void OnComponentEquipped(ComponentData component, ModuleSlot slot)
        {
            componentSelectionHUD.OnComponentEquipped -= OnComponentEquipped;
            if (_currentLoadout != null) _currentLoadout.EquipModule(slot, component);
            DrawSlotVisuals();   // Refresh visuals after equipping
            UpdateStatsDisplay();
            RefreshNameReadyLook();
        }

        public void UpdateStatsDisplay()
        {
            RefreshBuildCostNumbers();
            if (_statsLabel == null) return;

            float t = _currentLoadout != null ? _currentLoadout.totalBuildTime : 0f;
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);

            _statsLabel.text =
                $"Hull: {_currentHull?.hullName}\n" +
                $"Weight: {_currentLoadout?.totalWeight:F1}\n" +
                $"Build Time: {m:00}:{s:00}";
        }

        private VisualElement CreateStatsPanel()
        {
            var panel = new VisualElement { name = "UpdateTextBlock" };
            panel.style.width = Length.Percent(100);
            panel.style.flexShrink = 0;
            panel.style.flexGrow = 0;
            panel.style.marginTop = 8;
            panel.style.paddingTop = 8;
            panel.style.paddingBottom = 8;
            panel.style.paddingLeft = 15;
            panel.style.paddingRight = 15;
            panel.style.backgroundColor = new Color(0.08f, 0.12f, 0.25f, 0.95f);
            panel.style.borderTopLeftRadius = 8;
            panel.style.borderTopRightRadius = 8;
            panel.style.borderBottomLeftRadius = 8;
            panel.style.borderBottomRightRadius = 8;

            _statsLabel = new Label("Select a hull...");
            _statsLabel.style.fontSize = 15;
            _statsLabel.style.color = Color.white;
            _statsLabel.style.whiteSpace = WhiteSpace.Normal;
            _statsLabel.style.minWidth = 0;
            panel.Add(_statsLabel);

            return panel;
        }
        
        private ShipSaveService _shipSave;

        private void SaveCurrentBuild()
        {
            if (_currentLoadout?.hull == null)
            {
                Debug.LogWarning("[ShipSave] No hull selected.");
                return;
            }

            _shipSave ??= new ShipSaveService(new JsonShipSaveStore());

            string name = _hullNameLabel != null && !string.IsNullOrEmpty(_hullNameLabel.text)
                ? _hullNameLabel.text
                : _currentLoadout.hull.hullName;

            var record = _shipSave.Capture(_currentLoadout, name);
            if (record == null)
            {
                Debug.LogError("[ShipSave] Capture failed.");
                return;
            }

            _shipSave.AppendAndWrite(record);
            Debug.Log($"<color=lime>[ShipSave] Saved '{record.shipName}' hull={record.hullId} slots={record.slots.Count}</color>");
        }
        
        private void LoadLatestBlueprint()
        {
            if (_isBuilding)
            {
                Debug.LogWarning("[Load] Stop the build timer first.");
                return;
            }

            if (builtShipInventory == null)
            {
                Debug.LogError("[Load] Assign BuiltShipInventory.");
                return;
            }

            _shipSave ??= new ShipSaveService(new JsonShipSaveStore());
            if (!_shipSave.TryLoad(out var data) || data.ships == null || data.ships.Count == 0)
            {
                Debug.LogWarning("[Load] No blueprints in ships_temp.json.");
                return;
            }

            ApplyBlueprintRecord(data.ships[data.ships.Count - 1]);
        }

        private void ApplyBlueprintRecord(ShipBuildRecord record)
        {
            if (record == null) return;

            HullData hull = null;
            var catalog = builtShipInventory.hullCatalog;
            if (catalog != null)
            {
                foreach (var h in catalog)
                {
                    if (h == null) continue;
                    if (h.name == record.hullId || h.hullName == record.hullId)
                    {
                        hull = h;
                        break;
                    }
                }
            }

            if (hull == null)
            {
                Debug.LogError($"[Load] Hull not in catalog: {record.hullId}");
                return;
            }

            if (hull.moduleSlots != null)
            {
                foreach (var slot in hull.moduleSlots)
                    slot.equippedModule = null;
            }

            SetSelectedHull(hull);
            if (_hullNameLabel != null)
                _hullNameLabel.text = record.shipName;

            var db = builtShipInventory.moduleDatabase;
            db?.Initialize();

            if (record.slots != null && hull.moduleSlots != null && db != null)
            {
                foreach (var saved in record.slots)
                {
                    ModuleSlot live = null;
                    foreach (var s in hull.moduleSlots)
                    {
                        if (s != null && s.slotId == saved.slotId)
                        {
                            live = s;
                            break;
                        }
                    }

                    if (live == null)
                    {
                        Debug.LogWarning($"[Load] No slot {saved.slotId} on {hull.hullName}");
                        continue;
                    }

                    var module = db.GetModuleById(saved.moduleId);
                    if (module == null)
                    {
                        Debug.LogWarning($"[Load] Missing module {saved.moduleId}");
                        continue;
                    }

                    _currentLoadout.EquipModule(live, module);
                }
            }

            DrawSlotVisuals();
            UpdateStatsDisplay();
            Debug.Log($"<color=lime>[Load] '{record.shipName}' hull={record.hullId} slots={record.slots?.Count ?? 0}</color>");
        }

        public void CloseShipBuilder()
        {
            _isBuilding = false;
            _buildRemaining = 0f;
            
            ResetAllSlotsToDefault();           // ← New: Clear equipped modules

            if (_shipBuilderPanel != null)
            {
                _shipBuilderPanel.RemoveFromHierarchy();
                _shipBuilderPanel = null;
            }
            

            _currentHull = null;
            _currentLoadout = null;
            _clearMode = false;
        }
        private void ResetAllSlotsToDefault()
        {
            if (_currentHull?.moduleSlots == null) return;

            foreach (var slot in _currentHull.moduleSlots)
            {
                slot.equippedModule = null;     // Clear equipped module
            }

            Debug.Log("<color=yellow>ShipBuilder closed - All slots reset to default</color>");
        }
    }
}