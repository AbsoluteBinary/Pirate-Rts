using _Project.Scripts.UI.Manager;
using TGS;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using ClickEvent = UnityEngine.UIElements.ClickEvent;

namespace _Project.Scripts.Harbour.Data
{
    public class HarbourController : MonoBehaviour
    {
        [SerializeField] private GameObject bootComponentIOBox;
        [SerializeField] private GameObject harbourComponentIOBox;
        public static HarbourController Instance { get; private set; }

        [SerializeField] private HarbourStateSO state;

        [SerializeField] private UIDocument harbourUIDocument;

        [SerializeField] private Camera idleCamera;
        [SerializeField] private Camera harbourBuildCamera;

        [SerializeField] private GameObject tgsGridObject;
        
        [Header("Reset / Default State")]
        [SerializeField] private bool resetOnEveryHarbourEntry = true;

        [SerializeField] private string[] panelsToHideOnReset = { "BuildPanel", "ExtraHarbourUI" };
        [SerializeField] private string[] panelsToShowOnReset = { "IdleHUD", "MainHarbourInfo" };

        [SerializeField] private GameObject[] objectsToEnableOnReset;
        [SerializeField] private GameObject[] objectsToDisableOnReset;
        
        [SerializeField] private GameObject playerBuildGO;

        [SerializeField] private GameObject testTile;

        [SerializeField] private EventSystem eventSystem;
        
        [SerializeField] private TerrainGridSystem tgs;   // Drag your TGS object here in Inspector

        private VisualElement _currentBuildPanel;
        private GameObject _currentPreviewTile;
        private bool _isInPreviewMode = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void OnHarbourEntered()
        {
            if (resetOnEveryHarbourEntry)
            {
                ResetToDefaultState();
            }
            else if (state.currentMode != HarbourStateSO.HarbourMode.Idle)
            {
                ResetToDefaultState();
            }

            ApplyHarbourState();
        }

        private void Start()
        {
            DisableAllPanelInputConfigurations();
            ApplyHarbourState();
        }

        public void SetMode(HarbourStateSO.HarbourMode newMode)
        {
            state.currentMode = newMode;
            ApplyHarbourState();
        }

        private void ApplyHarbourState()
        {
            DisableAllPanelInputConfigurations();

            if (harbourUIDocument != null && harbourUIDocument.rootVisualElement != null)
            {
                harbourUIDocument.rootVisualElement.Clear();
            }

            if (playerBuildGO != null) 
                playerBuildGO.SetActive(false);

            switch (state.currentMode)
            {
                case HarbourStateSO.HarbourMode.Idle:
                    BuildIdleHUD();
                    SetCamera(idleCamera, true);
                    SetCamera(harbourBuildCamera, false);
                    SetTGSGrid(false);
                    break;

                case HarbourStateSO.HarbourMode.HarbourBuild:
                    BuildHarbourBuildHUD();
                    SetCamera(idleCamera, false);
                    SetCamera(harbourBuildCamera, true);
                    SetTGSGrid(true);
                    if (playerBuildGO != null) playerBuildGO.SetActive(true);
                    break;
            }

            Debug.Log($"<color=lime>✅ Applied HarbourState: {state.currentMode}</color>");
        }

        // ===================================================================
        // IDLE HUD
        // ===================================================================
        private void BuildIdleHUD()
        {
            if (harbourUIDocument == null || harbourUIDocument.rootVisualElement == null) return;

            var root = harbourUIDocument.rootVisualElement;
            root.Clear();

            var topBar = new VisualElement { name = "TopBar" };
            topBar.style.position = Position.Absolute;
            topBar.style.top = 0;
            topBar.style.left = 0;
            topBar.style.right = 0;
            topBar.style.height = 60;
            topBar.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f);
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.justifyContent = Justify.FlexEnd;
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingRight = 20;

            var optionsTrigger = new Button { text = "Options ▼" };
            optionsTrigger.style.minWidth = 120;
            optionsTrigger.style.height = 44;
            optionsTrigger.style.marginLeft = 24;
            optionsTrigger.style.fontSize = 17;
            optionsTrigger.style.unityFontStyleAndWeight = FontStyle.Bold;
            optionsTrigger.style.backgroundColor = new StyleColor(new Color(0.18f, 0.22f, 0.38f));
            optionsTrigger.style.color = Color.white;
            optionsTrigger.style.borderTopLeftRadius = 8;
            optionsTrigger.style.borderTopRightRadius = 8;
            optionsTrigger.style.borderBottomLeftRadius = 8;
            optionsTrigger.style.borderBottomRightRadius = 8;
            topBar.Add(optionsTrigger);

            var buildTrigger = new Button { text = "Build ▼" };
            buildTrigger.style.minWidth = 120;
            buildTrigger.style.height = 44;
            buildTrigger.style.marginLeft = 16;
            buildTrigger.style.fontSize = 17;
            buildTrigger.style.unityFontStyleAndWeight = FontStyle.Bold;
            buildTrigger.style.backgroundColor = new StyleColor(new Color(0.22f, 0.45f, 0.18f));
            buildTrigger.style.color = Color.white;
            buildTrigger.style.borderTopLeftRadius = 8;
            buildTrigger.style.borderTopRightRadius = 8;
            buildTrigger.style.borderBottomLeftRadius = 8;
            buildTrigger.style.borderBottomRightRadius = 8;

            var buildDropdown = new VisualElement { name = "BuildDropdown" };
            buildDropdown.style.position = Position.Absolute;
            buildDropdown.style.top = 60;
            buildDropdown.style.right = 20;
            buildDropdown.style.width = 240;
            buildDropdown.style.backgroundColor = new StyleColor(new Color(0.10f, 0.15f, 0.32f, 0.94f));
            buildDropdown.style.borderTopLeftRadius = 6;
            buildDropdown.style.borderTopRightRadius = 6;
            buildDropdown.style.borderBottomLeftRadius = 6;
            buildDropdown.style.borderBottomRightRadius = 6;
            buildDropdown.style.borderTopWidth = 1;
            buildDropdown.style.borderRightWidth = 1;
            buildDropdown.style.borderBottomWidth = 1;
            buildDropdown.style.borderLeftWidth = 1;
            buildDropdown.style.borderTopColor = new StyleColor(Color.cyan);
            buildDropdown.style.borderRightColor = new StyleColor(Color.cyan);
            buildDropdown.style.borderBottomColor = new StyleColor(Color.cyan);
            buildDropdown.style.borderLeftColor = new StyleColor(Color.cyan);
            buildDropdown.style.paddingTop = 8;
            buildDropdown.style.paddingBottom = 8;
            buildDropdown.style.paddingLeft = 8;
            buildDropdown.style.paddingRight = 8;
            buildDropdown.style.display = DisplayStyle.None;

            AddDropdownItem(buildDropdown, "Edit Harbour",       () => SetMode(HarbourStateSO.HarbourMode.HarbourBuild));
            AddDropdownItem(buildDropdown, "Save and Exit",      () => Debug.Log("Save and Exit clicked"));
            AddDropdownItem(buildDropdown, "Exit Without Saving",() => Debug.Log("Exit Without Saving clicked"));

            bool dropdownVisible = false;
            buildTrigger.clicked += () =>
            {
                dropdownVisible = !dropdownVisible;
                buildDropdown.style.display = dropdownVisible ? DisplayStyle.Flex : DisplayStyle.None;

                buildTrigger.style.backgroundColor = dropdownVisible 
                    ? new StyleColor(new Color(0.32f, 0.55f, 0.28f)) 
                    : new StyleColor(new Color(0.22f, 0.45f, 0.18f));
            };

            topBar.Add(buildTrigger);
            topBar.Add(buildDropdown);
            root.Add(topBar);

            var profile = new VisualElement { name = "ProfilePanel" };
            profile.style.position = Position.Absolute;
            profile.style.top = 10;
            profile.style.left = 10;
            profile.style.width = 180;
            profile.style.height = 140;
            profile.style.backgroundColor = new Color(0.12f, 0.18f, 0.35f, 0.8f);

            var label = new Label("Profile\n(placeholder)");
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = Color.white;
            label.style.fontSize = 18;
            profile.Add(label);
            root.Add(profile);

            Debug.Log("<color=lime>Idle HUD built</color>");
        }

        // ===================================================================
        // HARBOUR BUILD HUD
        // ===================================================================
        private void BuildHarbourBuildHUD()
        {
            if (harbourUIDocument == null || harbourUIDocument.rootVisualElement == null) return;

            var root = harbourUIDocument.rootVisualElement;
            root.Clear();

            var buildBar = new VisualElement { name = "BuildTopBar" };
            buildBar.style.position = Position.Absolute;
            buildBar.style.top = 0;
            buildBar.style.left = 0;
            buildBar.style.right = 0;
            buildBar.style.height = 60;
            buildBar.style.backgroundColor = new Color(0.85f, 0.35f, 0.1f, 0.95f);
            buildBar.style.flexDirection = FlexDirection.Row;
            buildBar.style.alignItems = Align.Center;
            buildBar.style.paddingLeft = 20;
            buildBar.style.paddingRight = 20;

            var toolsTrigger = new Button { text = "Build Tools ▼" };
            toolsTrigger.style.minWidth = 160;
            toolsTrigger.style.height = 44;
            toolsTrigger.style.fontSize = 17;
            toolsTrigger.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolsTrigger.style.backgroundColor = new StyleColor(new Color(0.22f, 0.45f, 0.18f));
            toolsTrigger.style.color = Color.white;
            toolsTrigger.style.borderTopLeftRadius = 8;
            toolsTrigger.style.borderTopRightRadius = 8;
            toolsTrigger.style.borderBottomLeftRadius = 8;
            toolsTrigger.style.borderBottomRightRadius = 8;

            var toolsDropdown = new VisualElement { name = "BuildToolsDropdown" };
            toolsDropdown.style.position = Position.Absolute;
            toolsDropdown.style.top = 60;
            toolsDropdown.style.left = 20;
            toolsDropdown.style.width = 240;
            toolsDropdown.style.backgroundColor = new StyleColor(new Color(0.10f, 0.15f, 0.32f, 0.94f));
            toolsDropdown.style.borderTopLeftRadius = 6;
            toolsDropdown.style.borderTopRightRadius = 6;
            toolsDropdown.style.borderBottomLeftRadius = 6;
            toolsDropdown.style.borderBottomRightRadius = 6;
            toolsDropdown.style.borderTopWidth = 1;
            toolsDropdown.style.borderRightWidth = 1;
            toolsDropdown.style.borderBottomWidth = 1;
            toolsDropdown.style.borderLeftWidth = 1;
            toolsDropdown.style.borderTopColor = new StyleColor(Color.cyan);
            toolsDropdown.style.borderRightColor = new StyleColor(Color.cyan);
            toolsDropdown.style.borderBottomColor = new StyleColor(Color.cyan);
            toolsDropdown.style.borderLeftColor = new StyleColor(Color.cyan);
            toolsDropdown.style.paddingTop = 8;
            toolsDropdown.style.paddingBottom = 8;
            toolsDropdown.style.paddingLeft = 8;
            toolsDropdown.style.paddingRight = 8;
            toolsDropdown.style.display = DisplayStyle.None;

            AddDropdownItem(toolsDropdown, "Build Harbour Base", OpenBuildPanel);
            AddDropdownItem(toolsDropdown, "Save Build",         () => Debug.Log("Save Build clicked"));
            AddDropdownItem(toolsDropdown, "Load Saved Build",   () => Debug.Log("Load Saved Build clicked"));

            bool toolsVisible = false;
            toolsTrigger.clicked += () =>
            {
                toolsVisible = !toolsVisible;
                toolsDropdown.style.display = toolsVisible ? DisplayStyle.Flex : DisplayStyle.None;

                toolsTrigger.style.backgroundColor = toolsVisible 
                    ? new StyleColor(new Color(0.32f, 0.55f, 0.28f)) 
                    : new StyleColor(new Color(0.22f, 0.45f, 0.18f));
            };

            buildBar.Add(toolsTrigger);
            buildBar.Add(toolsDropdown);

            var modeLabel = new Label("HARBOUR BUILD MODE");
            modeLabel.style.flexGrow = 1;
            modeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            modeLabel.style.fontSize = 24;
            modeLabel.style.color = Color.white;
            modeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            buildBar.Add(modeLabel);

            var exitBtn = new Button { text = "Exit Build Mode" };
            exitBtn.style.height = 44;
            exitBtn.style.fontSize = 16;
            exitBtn.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            exitBtn.style.color = Color.white;
            exitBtn.clicked += () => SetMode(HarbourStateSO.HarbourMode.Idle);
            buildBar.Add(exitBtn);

            root.Add(buildBar);

            Debug.Log("<color=lime>Build HUD built</color>");
        }

        // ===================================================================
        // FLOATING BUILD PANEL
        // ===================================================================
        private void OpenBuildPanel()
        {
            if (harbourUIDocument == null || harbourUIDocument.rootVisualElement == null) return;

            if (_currentBuildPanel != null)
                _currentBuildPanel.RemoveFromHierarchy();

            var root = harbourUIDocument.rootVisualElement;

            _currentBuildPanel = new VisualElement { name = "HarbourBuildPanel" };
            _currentBuildPanel.style.position = Position.Absolute;
            _currentBuildPanel.style.top = 70;
            _currentBuildPanel.style.right = 30;
            _currentBuildPanel.style.width = Length.Percent(25);
            _currentBuildPanel.style.height = Length.Percent(40);
            _currentBuildPanel.style.backgroundColor = new Color(0.08f, 0.12f, 0.25f, 0.97f);
            _currentBuildPanel.style.borderTopLeftRadius = 10;
            _currentBuildPanel.style.borderTopRightRadius = 10;
            _currentBuildPanel.style.borderBottomLeftRadius = 10;
            _currentBuildPanel.style.borderBottomRightRadius = 10;
            _currentBuildPanel.style.borderTopWidth = 2;
            _currentBuildPanel.style.borderRightWidth = 2;
            _currentBuildPanel.style.borderBottomWidth = 2;
            _currentBuildPanel.style.borderLeftWidth = 2;
            _currentBuildPanel.style.borderTopColor = new Color(0.4f, 0.7f, 1f);
            _currentBuildPanel.style.borderRightColor = new Color(0.4f, 0.7f, 1f);
            _currentBuildPanel.style.borderBottomColor = new Color(0.4f, 0.7f, 1f);
            _currentBuildPanel.style.borderLeftColor = new Color(0.4f, 0.7f, 1f);
            _currentBuildPanel.style.paddingTop = 15;
            _currentBuildPanel.style.paddingRight = 15;
            _currentBuildPanel.style.paddingBottom = 15;
            _currentBuildPanel.style.paddingLeft = 15;
            _currentBuildPanel.pickingMode = PickingMode.Position;

            // Header
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 12;
            header.pickingMode = PickingMode.Position;

            var title = new Label("Harbour Build");
            title.style.fontSize = 22;
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);

            var closeBtn = new Button { text = "✕" };
            closeBtn.style.fontSize = 20;
            closeBtn.style.color = Color.white;
            closeBtn.style.backgroundColor = new Color(0.7f, 0.15f, 0.15f);
            closeBtn.style.width = 36;
            closeBtn.style.height = 36;
            closeBtn.style.borderTopLeftRadius = 18;
            closeBtn.style.borderTopRightRadius = 18;
            closeBtn.style.borderBottomLeftRadius = 18;
            closeBtn.style.borderBottomRightRadius = 18;
            closeBtn.clicked += CloseBuildPanel;
            header.Add(closeBtn);

            _currentBuildPanel.Add(header);

            // Tabs
            var tabContainer = new VisualElement();
            tabContainer.style.flexDirection = FlexDirection.Row;
            tabContainer.style.marginBottom = 15;
            tabContainer.pickingMode = PickingMode.Position;

            var landTab     = CreateTabButton("Land Tiles",   () => ShowTabContent("Land Tiles"));
            var buildingTab = CreateTabButton("Buildings",    () => ShowTabContent("Buildings"));
            var defenceTab  = CreateTabButton("Defences",     () => ShowTabContent("Defences"));

            tabContainer.Add(landTab);
            tabContainer.Add(buildingTab);
            tabContainer.Add(defenceTab);
            _currentBuildPanel.Add(tabContainer);

            // Content Area
            var contentArea = new VisualElement { name = "ContentArea" };
            contentArea.style.flexGrow = 1;
            contentArea.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.97f);
            contentArea.style.borderTopLeftRadius = 6;
            contentArea.style.borderTopRightRadius = 6;
            contentArea.style.borderBottomLeftRadius = 6;
            contentArea.style.borderBottomRightRadius = 6;
            contentArea.style.paddingTop = 12;
            contentArea.style.paddingBottom = 12;
            contentArea.style.paddingLeft = 12;
            contentArea.style.paddingRight = 12;
            contentArea.pickingMode = PickingMode.Position;

            _currentBuildPanel.Add(contentArea);

            root.Add(_currentBuildPanel);

            Disable3DRaycasting();
            DisableEventSystem();

            ShowTabContent("Land Tiles");

            Debug.Log("<color=lime>Harbour Build Panel opened with strong blocking</color>");
        }

        private void CloseBuildPanel()
        {
            if (_currentBuildPanel != null)
            {
                _currentBuildPanel.RemoveFromHierarchy();
                _currentBuildPanel = null;
            }

            Enable3DRaycasting();
            EnableEventSystem();
        }

        private void Disable3DRaycasting()
        {
            var raycasters = FindObjectsByType<PhysicsRaycaster>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var r in raycasters) r.enabled = false;
        }

        private void Enable3DRaycasting()
        {
            var raycasters = FindObjectsByType<PhysicsRaycaster>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var r in raycasters) r.enabled = true;
        }

        private void DisableEventSystem()
        {
            if (eventSystem != null)
                eventSystem.enabled = false;
        }

        private void EnableEventSystem()
        {
            if (eventSystem != null)
                eventSystem.enabled = true;
        }

        private void ShowTabContent(string tabName)
        {
            var contentArea = _currentBuildPanel?.Q<VisualElement>("ContentArea");
            if (contentArea == null) return;

            contentArea.Clear();

            var grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.justifyContent = Justify.FlexStart;
            grid.style.alignContent = Align.FlexStart;
            grid.style.paddingTop = 8;
            grid.style.paddingBottom = 8;
            grid.style.paddingLeft = 8;
            grid.style.paddingRight = 8;

            for (int i = 0; i < 12; i++)
            {
                var slot = new VisualElement();
                slot.style.width = 72;
                slot.style.height = 72;
                slot.style.backgroundColor = new Color(0.25f, 0.3f, 0.45f);
                slot.style.borderTopLeftRadius = 6;
                slot.style.borderTopRightRadius = 6;
                slot.style.borderBottomLeftRadius = 6;
                slot.style.borderBottomRightRadius = 6;
                slot.style.borderTopWidth = 2;
                slot.style.borderRightWidth = 2;
                slot.style.borderBottomWidth = 2;
                slot.style.borderLeftWidth = 2;
                slot.style.borderTopColor = new Color(0.6f, 0.7f, 0.9f);
                slot.style.borderRightColor = new Color(0.8f, 0.8f, 0.9f);
                slot.style.borderBottomColor = new Color(0.8f, 0.8f, 0.9f);
                slot.style.borderLeftColor = new Color(0.8f, 0.8f, 0.9f);
                slot.style.marginRight = 12;
                slot.style.marginBottom = 12;

                if (tabName == "Land Tiles")
                {
                    string iconName = i switch
                    {
                        0 => "Sprites/GrassTile",
                        1 => "Sprites/SandTile",
                        2 => "Sprites/RockTile",
                        _ => "Sprites/DefaultLand"
                    };

                    Sprite sprite = Resources.Load<Sprite>(iconName);

                    if (sprite != null)
                    {
                        slot.style.backgroundImage = new StyleBackground(sprite);
                        slot.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                        slot.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                        slot.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                    }
                    else
                    {
                        var fallback = new Label(iconName.Replace("Sprites/", ""));
                        fallback.style.fontSize = 10;
                        fallback.style.color = Color.white;
                        fallback.style.unityTextAlign = TextAnchor.MiddleCenter;
                        slot.Add(fallback);
                    }
                }

                int index = i;
                slot.RegisterCallback<ClickEvent>(evt => OnSlotClicked(tabName, index));

                grid.Add(slot);
            }

            contentArea.Add(grid);
        }

        private Button CreateTabButton(string text, System.Action onClick)
        {
            var btn = new Button { text = text };
            btn.style.flexGrow = 1;
            btn.style.height = 42;
            btn.style.fontSize = 16;
            btn.style.marginRight = 6;
            btn.style.backgroundColor = new Color(0.18f, 0.22f, 0.38f);
            btn.style.color = Color.white;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
            btn.clicked += onClick;
            return btn;
        }

        private void AddDropdownItem(VisualElement dropdownParent, string text, System.Action action)
        {
            var item = new Button { text = text };
            item.style.height = 42;
            item.style.marginLeft = 12;
            item.style.marginRight = 12;
            item.style.marginBottom = 4;
            item.style.fontSize = 16;
            item.style.backgroundColor = new StyleColor(new Color(0.14f, 0.18f, 0.34f));
            item.style.color = Color.white;
            item.style.borderTopLeftRadius = 4;
            item.style.borderTopRightRadius = 4;
            item.style.borderBottomLeftRadius = 4;
            item.style.borderBottomRightRadius = 4;

            item.clicked += () =>
            {
                action?.Invoke();
                dropdownParent.style.display = DisplayStyle.None;
            };

            dropdownParent.Add(item);
        }

        private void OnSlotClicked(string tabName, int slotIndex)
        {
            if (tabName != "Land Tiles") return;

            Debug.Log($"Slot {slotIndex} clicked in Land Tiles tab");

            if (testTile == null)
            {
                Debug.LogWarning("testTile prefab is not assigned in HarbourController!");
                return;
            }

            StartPreviewMode(testTile);
        }

        private void StartPreviewMode(GameObject prefabToPreview)
        {
            if (prefabToPreview == null) 
            {
                Debug.LogWarning("StartPreviewMode received null prefab!");
                return;
            }

            if (_currentPreviewTile != null)
                Destroy(_currentPreviewTile);

            _currentPreviewTile = Instantiate(prefabToPreview);
            _currentPreviewTile.name = "Preview_Tile";

            // Force visible scale and height
            _currentPreviewTile.transform.localScale = new Vector3(1.5f, 0.4f, 1.5f);   // bigger and thicker
            _currentPreviewTile.transform.position = new Vector3(0, 5, 0);             // start high so we can see it

            // Make sure it's visible (semi-transparent)
            var rend = _currentPreviewTile.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 0.75f;
                rend.material.color = c;

                // Force standard shader if needed
                if (rend.material.shader.name.Contains("Legacy"))
                    rend.material.shader = Shader.Find("Standard");
            }

            _isInPreviewMode = true;
            Debug.Log("<color=green>Preview instantiated with forced visibility</color>");
        }
        

        private void Update()
        {
            if (_isInPreviewMode && _currentPreviewTile != null)
            {
                UpdatePreviewPosition();
            }

            if (_isInPreviewMode && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                ConfirmPlacement();
            }

            if (_isInPreviewMode && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                CancelPreview();
            }
        }

        private void UpdatePreviewPosition()
        {
            if (Mouse.current == null) return;

            Camera cam = harbourBuildCamera;
            if (cam == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
            {
                Vector3 pos = hit.point;
                pos.y = 0.3f;                    // higher lift so it's clearly visible above water

                pos.x = Mathf.Round(pos.x);
                pos.z = Mathf.Round(pos.z);

                _currentPreviewTile.transform.position = pos;
            }
        }

        private void ConfirmPlacement()
        {
            if (_currentPreviewTile == null) return;

            Vector3 placePos = _currentPreviewTile.transform.position;

            Destroy(_currentPreviewTile);
            _currentPreviewTile = null;
            _isInPreviewMode = false;

            if (testTile != null)
            {
                GameObject realTile = Instantiate(testTile, placePos, Quaternion.identity);
                realTile.name = "Placed_LandTile";

                var rend = realTile.GetComponent<Renderer>();
                if (rend != null)
                {
                    Color c = rend.material.color;
                    c.a = 1f;
                    rend.material.color = c;
                }

                Debug.Log($"<color=green>Land Tile placed at cell center {placePos}</color>");
            }
        }

        private void CancelPreview()
        {
            if (_currentPreviewTile != null)
            {
                Destroy(_currentPreviewTile);
                _currentPreviewTile = null;
            }
            _isInPreviewMode = false;
            Debug.Log("<color=yellow>Preview cancelled</color>");
        }

        private void DisableAllPanelInputConfigurations()
        {
            var configs = FindObjectsByType<UnityEngine.UIElements.PanelInputConfiguration>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var config in configs)
            {
                if (config != null)
                {
                    config.enabled = false;
                    if (harbourUIDocument == null || config.gameObject != harbourUIDocument.gameObject)
                    {
                        Destroy(config);
                    }
                }
            }
        }

        private void SetCamera(Camera cam, bool active)
        {
            if (cam != null) cam.gameObject.SetActive(active);
        }

        private void SetTGSGrid(bool enabled)
        {
            if (tgsGridObject != null) tgsGridObject.SetActive(enabled);
        }

        public void ResetToDefaultState()
        {
            SetMode(HarbourStateSO.HarbourMode.Idle);

            if (UIManager.Instance != null)
            {
                foreach (var panelName in panelsToHideOnReset)
                    UIManager.Instance.SetPanelVisible(panelName, false);

                foreach (var panelName in panelsToShowOnReset)
                    UIManager.Instance.SetPanelVisible(panelName, true);
            }

            foreach (var go in objectsToEnableOnReset)
                if (go != null) go.SetActive(true);

            foreach (var go in objectsToDisableOnReset)
                if (go != null) go.SetActive(false);
        }

        public void DisableBootComponents() => DisableComponentsOnContainer(bootComponentIOBox);
        public void DisableHarbourComponents() => DisableComponentsOnContainer(harbourComponentIOBox);

        private void DisableComponentsOnContainer(GameObject container)
        {
            if (container == null) return;
            container.SetActive(false);

            var listener = container.GetComponentInChildren<AudioListener>(true);
            if (listener != null) listener.enabled = false;

            var eventSystem = container.GetComponentInChildren<EventSystem>(true);
            if (eventSystem != null) eventSystem.enabled = false;
        }

        public void EnterHarbourBuildMode() => SetMode(HarbourStateSO.HarbourMode.HarbourBuild);
        public void ReturnToIdle() => SetMode(HarbourStateSO.HarbourMode.Idle);
    }
}