using UnityEngine;
using UnityEngine.UIElements;

namespace Test_Scripts
{
    [RequireComponent(typeof(UIDocument))]
    public class GameHUDController : MonoBehaviour
    {
        private Button _buildTrigger;     // New
        private VisualElement _buildDropdown;  // New
        private bool _buildDropdownVisible = false;
        
        //private VisualElement _optionsTrigger;
        private VisualElement _dropdownPanel;
        private bool _dropdownVisible = false;
        private Button _optionsTrigger;
        
        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc == null || doc.rootVisualElement == null)
            {
                Debug.LogError("No UIDocument found!");
                return;
            }

            var root = doc.rootVisualElement;
            root.Clear();

            // ────────────────────────────────────────────────
            // TOP BAR — full-width-ish, thin height
            // ────────────────────────────────────────────────
            var topBar = new VisualElement { name = "TopBar" };
            topBar.style.position = Position.Absolute;
            topBar.style.top = 0;
            topBar.style.left = 0;
            topBar.style.right = 0;                  // stretches full width
            topBar.style.height = 60;                // thin fixed height — adjust as needed
            topBar.style.backgroundColor = new Color(0.05f, 0.08f, 0.18f, 0.92f); // dark semi-transparent
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.justifyContent = Justify.FlexEnd;  // ← pushes ALL children to right
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingRight = 20;

            // The 3 buttons — directly added to bar, auto-pushed right
            // AddSimpleButton(topBar, "Edit Harbour",       () => Debug.Log("Edit Harbour"));
            // AddSimpleButton(topBar, "Save and Exit",      () => Debug.Log("Save & Exit"));
            // AddSimpleButton(topBar, "Exit Without Saving",() => Debug.Log("Exit w/o Save"));
            
            
            // ─── NEW: Options dropdown trigger (placed right of the 3 buttons) ────────
            _optionsTrigger = new Button { text = "Options ▼" };
            _optionsTrigger.style.minWidth = 120;
            _optionsTrigger.style.height = 44;
            _optionsTrigger.style.marginLeft = 24;          // nice gap from previous buttons
            _optionsTrigger.style.fontSize = 17;
            _optionsTrigger.style.unityFontStyleAndWeight = FontStyle.Bold;
            _optionsTrigger.style.backgroundColor = new StyleColor(new Color(0.18f, 0.22f, 0.38f));
            _optionsTrigger.style.color = Color.white;
            // Block
            _optionsTrigger.style.borderTopLeftRadius     = 8;
            _optionsTrigger.style.borderTopRightRadius    = 8;
            _optionsTrigger.style.borderBottomLeftRadius  = 8;
            _optionsTrigger.style.borderBottomRightRadius = 8;
            // or shorthand for all corners:
            //_optionsTrigger.style.borderRadius = new StyleLength(8);
            
            _optionsTrigger.clicked += ToggleOptionsDropdown;

            topBar.Add(_optionsTrigger);
            
            

            // ─── Dropdown content (hidden by default) ─────────────────────────────────
            _dropdownPanel = new VisualElement { name = "OptionsDropdown" };
            _dropdownPanel.style.position = Position.Absolute;
            _dropdownPanel.style.top = 60;                  // below bar height — adjust if bar height changes
            _dropdownPanel.style.right = 20;                // align right edge nicely
            _dropdownPanel.style.width = 220;
            _dropdownPanel.style.backgroundColor = new StyleColor(new Color(0.10f, 0.15f, 0.32f, 0.94f));
            _dropdownPanel.style.borderTopWidth    = 1;
            _dropdownPanel.style.borderRightWidth  = 1;
            _dropdownPanel.style.borderBottomWidth = 1;
            _dropdownPanel.style.borderLeftWidth   = 1;
            _dropdownPanel.style.borderTopColor    = new StyleColor(Color.gray);
            _dropdownPanel.style.borderRightColor  = new StyleColor(Color.gray);
            _dropdownPanel.style.borderBottomColor = new StyleColor(Color.gray);
            _dropdownPanel.style.borderLeftColor   = new StyleColor(Color.gray);
            _dropdownPanel.style.borderTopLeftRadius     = 6;
            _dropdownPanel.style.borderTopRightRadius    = 6;
            _dropdownPanel.style.borderBottomLeftRadius  = 6;
            _dropdownPanel.style.borderBottomRightRadius = 6;
            _dropdownPanel.style.paddingTop = 8;
            _dropdownPanel.style.paddingBottom = 8;
            _dropdownPanel.style.display = DisplayStyle.None;

            // Example items — change text/actions to whatever you need
            AddDropdownItem("Settings",         () => Debug.Log("Settings clicked"));
            AddDropdownItem("Audio / Graphics", () => Debug.Log("Audio/Graphics clicked"));
            AddDropdownItem("Help",             () => Debug.Log("Help clicked"));
            AddDropdownItem("Exit Game",        () => Application.Quit());

            topBar.Add(_dropdownPanel);   // attach to bar so it moves with it

            root.Add(topBar);
            
            // ─── Build Dropdown Trigger (placed right of Options) ──────────────────────
            _buildTrigger = new Button { text = "Build ▼" };
            _buildTrigger.style.minWidth = 120;
            _buildTrigger.style.height = 44;
            _buildTrigger.style.marginLeft = 16;           // small gap from Options
            _buildTrigger.style.fontSize = 17;
            _buildTrigger.style.unityFontStyleAndWeight = FontStyle.Bold;
            _buildTrigger.style.backgroundColor = new StyleColor(new Color(0.22f, 0.45f, 0.18f)); // slight green tint to distinguish, or match Options
            _buildTrigger.style.color = Color.white;
            _buildTrigger.style.borderTopLeftRadius     = 8;
            _buildTrigger.style.borderTopRightRadius    = 8;
            _buildTrigger.style.borderBottomLeftRadius  = 8;
            _buildTrigger.style.borderBottomRightRadius = 8;
            _buildTrigger.style.borderTopWidth    = 1;
            _buildTrigger.style.borderRightWidth  = 1;
            _buildTrigger.style.borderBottomWidth = 1;
            _buildTrigger.style.borderLeftWidth   = 1;
            _buildTrigger.style.borderTopColor    = new StyleColor(Color.gray);
            _buildTrigger.style.borderRightColor  = new StyleColor(Color.gray);
            _buildTrigger.style.borderBottomColor = new StyleColor(Color.gray);
            _buildTrigger.style.borderLeftColor   = new StyleColor(Color.gray);
            _buildTrigger.clicked += ToggleBuildDropdown;

            topBar.Add(_buildTrigger);

            // ─── Build Dropdown Panel (same style as Options) ──────────────────────────
            _buildDropdown = new VisualElement { name = "BuildDropdown" };
            _buildDropdown.style.position = Position.Absolute;
            _buildDropdown.style.top = 60;                 // adjust to match your bar height
            _buildDropdown.style.right = 20;               // align nicely to right edge
            _buildDropdown.style.width = 240;              // same as Options or adjust
            _buildDropdown.style.backgroundColor = new StyleColor(new Color(0.10f, 0.15f, 0.32f, 0.94f));
            _buildDropdown.style.borderTopLeftRadius     = 6;
            _buildDropdown.style.borderTopRightRadius    = 6;
            _buildDropdown.style.borderBottomLeftRadius  = 6;
            _buildDropdown.style.borderBottomRightRadius = 6;
            _buildDropdown.style.borderTopWidth    = 1;
            _buildDropdown.style.borderRightWidth  = 1;
            _buildDropdown.style.borderBottomWidth = 1;
            _buildDropdown.style.borderLeftWidth   = 1;
            _buildDropdown.style.borderTopColor    = new StyleColor(Color.cyan);
            _buildDropdown.style.borderRightColor  = new StyleColor(Color.cyan);
            _buildDropdown.style.borderBottomColor = new StyleColor(Color.cyan);
            _buildDropdown.style.borderLeftColor   = new StyleColor(Color.cyan);
            _buildDropdown.style.paddingTop = 8;
            _buildDropdown.style.paddingBottom = 8;
            _buildDropdown.style.display = DisplayStyle.None;

            // Move the 3 buttons here (exact same look & actions)
            AddDropdownItemToBuild("Edit Harbour",       () => Debug.Log("Edit Harbour clicked"));
            AddDropdownItemToBuild("Save and Exit",      () => Debug.Log("Save and Exit clicked"));
            AddDropdownItemToBuild("Exit Without Saving",() => Debug.Log("Exit Without Saving clicked"));

            topBar.Add(_buildDropdown);

            // ────────────────────────────────────────────────
            // PROFILE PANEL — top-left placeholder
            // ────────────────────────────────────────────────
            var profile = new VisualElement { name = "ProfilePanel" };
            profile.style.position = Position.Absolute;
            profile.style.top = 10;
            profile.style.left = 10;
            profile.style.width = 180;
            profile.style.height = 140;
            profile.style.backgroundColor = new Color(0.12f, 0.18f, 0.35f, 0.8f);
            //profile.style.borderWidth = 2;
            //profile.style.borderColor = new Color(0, 0.8f, 1f);
            //profile.style.borderRadius = 8;

            var label = new Label("Profile\n(placeholder)");
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = Color.white;
            label.style.fontSize = 18;
            profile.Add(label);

            root.Add(profile);

            Debug.Log("<color=lime>Minimal HUD: Top bar with 3 right-aligned buttons + top-left profile panel</color>");
        }

        private void AddSimpleButton(VisualElement parent, string text, System.Action onClick)
        {
            var btn = new Button { text = text };
            btn.style.minWidth = 140;
            btn.style.height = 44;
            btn.style.marginLeft = 12;
            btn.style.fontSize = 16;
            btn.style.backgroundColor = new Color(0.15f, 0.45f, 0.95f);
            btn.style.color = Color.white;
            //btn.style.borderRadius = 6;
            btn.clicked += () => onClick?.Invoke();
            parent.Add(btn);
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
            item.style.borderTopLeftRadius     = 4;
            item.style.borderTopRightRadius    = 4;
            item.style.borderBottomLeftRadius  = 4;
            item.style.borderBottomRightRadius = 4;

            item.clicked += () =>
            {
                action?.Invoke();
                ToggleOptionsDropdown();           // auto-close after selection
            };

            _dropdownPanel.Add(item);
        }

        private void ToggleOptionsDropdown()   // ← for Options
        {
            bool willBeOpen = !_dropdownVisible;

            // If we're about to open Options → close Build first
            if (willBeOpen && _buildDropdownVisible)
            {
                _buildDropdownVisible = false;
                _buildDropdown.style.display = DisplayStyle.None;
            }

            _dropdownVisible = willBeOpen;
            _dropdownPanel.style.display = _dropdownVisible ? DisplayStyle.Flex : DisplayStyle.None;
            
            _optionsTrigger.style.backgroundColor = _dropdownVisible 
                ? new StyleColor(new Color(0.28f, 0.32f, 0.48f))   // brighter when open
                : new StyleColor(new Color(0.18f, 0.22f, 0.38f));  // normal
        }

        private void ToggleBuildDropdown()
        {
            bool willBeOpen = !_buildDropdownVisible;

            // If we're about to open Build → close Options first
            if (willBeOpen && _dropdownVisible)
            {
                _dropdownVisible = false;
                _dropdownPanel.style.display = DisplayStyle.None;
            }

            _buildDropdownVisible = willBeOpen;
            _buildDropdown.style.display = _buildDropdownVisible ? DisplayStyle.Flex : DisplayStyle.None;
            
            _buildTrigger.style.backgroundColor = _buildDropdownVisible 
                ? new StyleColor(new Color(0.32f, 0.55f, 0.28f))   // brighter green when open
                : new StyleColor(new Color(0.22f, 0.45f, 0.18f));
        }
        
        private void AddDropdownItemToBuild(string text, System.Action action)
        {
            var item = new Button { text = text };
            item.style.height = 42;
            item.style.marginLeft = 12;
            item.style.marginRight = 12;
            item.style.marginBottom = 4;
            item.style.fontSize = 16;
            item.style.backgroundColor = new StyleColor(new Color(0.14f, 0.18f, 0.34f));
            item.style.color = Color.white;
            item.style.borderTopLeftRadius     = 4;
            item.style.borderTopRightRadius    = 4;
            item.style.borderBottomLeftRadius  = 4;
            item.style.borderBottomRightRadius = 4;

            item.clicked += () =>
            {
                action?.Invoke();
                ToggleBuildDropdown();  // close after click
            };

            _buildDropdown.Add(item);
        }
    }
}
