using _Project.Scripts.Harbour.Data;
using _Project.Scripts.Harbour.Data.SO;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.UIControllers
{
    [RequireComponent(typeof(UIDocument))]
    public class HarbourBuildHUDController : MonoBehaviour
    {
        private Button _buildToolsTrigger;
        private VisualElement _buildToolsDropdown;
        private bool _buildToolsDropdownVisible = false;

        // ─── Camera References (for Build ↔ Idle toggle) ─────────────────────
        //[SerializeField] private GameObject playerCameraRoot;     // "Player View - CameraRoot"
        //[SerializeField] private GameObject buildCameraRoot;
        
        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc == null || doc.rootVisualElement == null)
            {
                Debug.LogError("<color=yellow>No UIDocument found on Build HUD!</color>");
                return;
            }

            var root = doc.rootVisualElement;
            root.Clear();

            // ────────────────────────────────────────────────
            // MAIN BUILD TOP BAR — full width, thin
            // ────────────────────────────────────────────────
            var buildBar = new VisualElement { name = "BuildTopBar" };
            buildBar.style.position = Position.Absolute;
            buildBar.style.top = 0;
            buildBar.style.left = 0;
            buildBar.style.right = 0;
            buildBar.style.height = 60;
            buildBar.style.backgroundColor = new Color(0.85f, 0.35f, 0.1f, 0.95f); // distinct build mode color
            buildBar.style.flexDirection = FlexDirection.Row;
            buildBar.style.alignItems = Align.Center;
            buildBar.style.paddingLeft = 20;
            buildBar.style.paddingRight = 20;

            // ─── LEFT SIDE: Build Tools Dropdown Trigger ─────────────────────
            _buildToolsTrigger = new Button { text = "Build Tools ▼" };
            _buildToolsTrigger.style.minWidth = 160;
            _buildToolsTrigger.style.height = 44;
            _buildToolsTrigger.style.fontSize = 17;
            _buildToolsTrigger.style.unityFontStyleAndWeight = FontStyle.Bold;
            _buildToolsTrigger.style.backgroundColor = new StyleColor(new Color(0.22f, 0.45f, 0.18f));
            _buildToolsTrigger.style.color = Color.white;
            _buildToolsTrigger.style.borderTopLeftRadius = 8;
            _buildToolsTrigger.style.borderTopRightRadius = 8;
            _buildToolsTrigger.style.borderBottomLeftRadius = 8;
            _buildToolsTrigger.style.borderBottomRightRadius = 8;
            _buildToolsTrigger.clicked += ToggleBuildToolsDropdown;

            buildBar.Add(_buildToolsTrigger);

            // ─── CENTER: Mode Title ─────────────────────────────────────────
            var modeLabel = new Label("HARBOUR BUILD MODE");
            modeLabel.style.flexGrow = 1;                    // pushes it to center
            modeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            modeLabel.style.fontSize = 24;
            modeLabel.style.color = Color.white;
            modeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            buildBar.Add(modeLabel);

            // ─── RIGHT SIDE: Exit Button ────────────────────────────────────
            var exitBtn = new Button { text = "Exit Build Mode" };
            exitBtn.style.height = 44;
            exitBtn.style.fontSize = 16;
            exitBtn.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            exitBtn.style.color = Color.white;
            exitBtn.clicked += OnExitBuildClicked;
            buildBar.Add(exitBtn);

            root.Add(buildBar);

            // ─── Build Tools Dropdown Panel (hidden by default) ─────────────
            _buildToolsDropdown = new VisualElement { name = "BuildToolsDropdown" };
            _buildToolsDropdown.style.position = Position.Absolute;
            _buildToolsDropdown.style.top = 60;               // below the bar
            _buildToolsDropdown.style.left = 20;              // aligned under the trigger button
            _buildToolsDropdown.style.width = 220;
            _buildToolsDropdown.style.backgroundColor = new StyleColor(new Color(0.10f, 0.15f, 0.32f, 0.94f));
            _buildToolsDropdown.style.borderTopLeftRadius = 6;
            _buildToolsDropdown.style.borderTopRightRadius = 6;
            _buildToolsDropdown.style.borderBottomLeftRadius = 6;
            _buildToolsDropdown.style.borderBottomRightRadius = 6;
            _buildToolsDropdown.style.borderTopWidth = 1;
            _buildToolsDropdown.style.borderRightWidth = 1;
            _buildToolsDropdown.style.borderBottomWidth = 1;
            _buildToolsDropdown.style.borderLeftWidth = 1;
            _buildToolsDropdown.style.borderTopColor = new StyleColor(Color.cyan);
            _buildToolsDropdown.style.borderRightColor = new StyleColor(Color.cyan);
            _buildToolsDropdown.style.borderBottomColor = new StyleColor(Color.cyan);
            _buildToolsDropdown.style.borderLeftColor = new StyleColor(Color.cyan);
            _buildToolsDropdown.style.paddingTop = 8;
            _buildToolsDropdown.style.paddingBottom = 8;
            _buildToolsDropdown.style.display = DisplayStyle.None;

            // Add the 3 requested buttons
            AddDropdownItem("Build Base",       () => Debug.Log("Build Base clicked"));
            AddDropdownItem("Save Build",       () => Debug.Log("Save Build clicked"));
            AddDropdownItem("Load Saved Build", () => Debug.Log("Load Saved Build clicked"));

            buildBar.Add(_buildToolsDropdown);   // attach to bar so positioning is relative

            Debug.Log("<color=lime>Harbour Build HUD with Build Tools dropdown loaded</color>");
        }

        private void AddDropdownItem(string text, System.Action action)
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
                ToggleBuildToolsDropdown();   // auto-close after selection
            };

            _buildToolsDropdown.Add(item);
        }

        private void ToggleBuildToolsDropdown()
        {
            _buildToolsDropdownVisible = !_buildToolsDropdownVisible;
            _buildToolsDropdown.style.display = _buildToolsDropdownVisible ? DisplayStyle.Flex : DisplayStyle.None;

            // Visual feedback on trigger button
            _buildToolsTrigger.style.backgroundColor = _buildToolsDropdownVisible
                ? new StyleColor(new Color(0.32f, 0.55f, 0.28f))
                : new StyleColor(new Color(0.22f, 0.45f, 0.18f));
        }

        private void OnExitBuildClicked()
        {
            // Use your actual normal/idle mode here (from your enum)
            HarbourController.Instance.SetMode(HarbourStateSO.HarbourMode.Idle);
        }
        
    }
}