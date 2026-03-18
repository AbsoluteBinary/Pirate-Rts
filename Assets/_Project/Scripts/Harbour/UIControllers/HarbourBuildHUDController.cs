using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.Harbour.UIControllers
{
    public class HarbourBuildHUDController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement dropdownPanel;
        private bool isDropdownOpen = false;

        private void Awake()
        {
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument not assigned on HarbourBuildHUDController!", this);
                return;
            }

            CreateTopBar();
        }

        private void CreateTopBar()
        {
            var root = uiDocument.rootVisualElement;

            // Full-width top bar
            var topBar = new VisualElement { name = "HarbourBuildTopBar" };
            topBar.style.position = Position.Absolute;
            topBar.style.top = 0;
            topBar.style.left = 0;
            topBar.style.width = new Length(100, LengthUnit.Percent);
            topBar.style.height = new Length(7, LengthUnit.Percent);
            topBar.style.backgroundColor = new StyleColor(new Color(0.08f, 0.18f, 0.35f, 0.95f));
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.justifyContent = Justify.FlexEnd;
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingRight = 20;

            // Options trigger button
            var optionsTrigger = new Button(ToggleDropdown)
            {
                text = "Options ▼"
            };
            optionsTrigger.style.height = new Length(68, LengthUnit.Percent);
            optionsTrigger.style.minWidth = 180;

            // Dropdown Panel (added to ROOT - this is the key that worked in TopHUDController)
            dropdownPanel = new VisualElement { name = "DropdownPanel" };
            dropdownPanel.style.position = Position.Absolute;
            dropdownPanel.style.top = new Length(105, LengthUnit.Percent);
            dropdownPanel.style.right = 20;
            dropdownPanel.style.width = 280;
            dropdownPanel.style.backgroundColor = new StyleColor(new Color(0.09f, 0.20f, 0.38f, 0.98f));
            //dropdownPanel.style.borderWidth = 4;
            //dropdownPanel.style.borderColor = new StyleColor(Color.cyan);
            dropdownPanel.style.borderTopLeftRadius = 12;
            dropdownPanel.style.borderTopRightRadius = 12;
            dropdownPanel.style.borderBottomLeftRadius = 12;
            dropdownPanel.style.borderBottomRightRadius = 12;
            dropdownPanel.style.paddingTop = 12;
            dropdownPanel.style.paddingBottom = 12;
            dropdownPanel.style.display = DisplayStyle.None;

            // 2 buttons
            AddDropdownButton("Save and Exit", () => Debug.Log("Save and Exit clicked"));
            AddDropdownButton("Exit without Saving", () => Debug.Log("Exit without Saving clicked"));

            topBar.Add(optionsTrigger);
            root.Add(topBar);
            root.Add(dropdownPanel);   // ← Critical line (same as working TopHUDController)

            Debug.Log("Harbour Build HUD ready");
        }

        private void AddDropdownButton(string text, System.Action onClick)
        {
            var btn = new Button(() =>
            {
                onClick?.Invoke();
                CloseDropdown();
            })
            {
                text = text
            };
            btn.style.height = 52;
            btn.style.marginTop = 6;
            btn.style.marginBottom = 6;
            btn.style.marginLeft = 12;
            btn.style.marginRight = 12;
            btn.style.fontSize = 18;
            dropdownPanel.Add(btn);
        }

        private void ToggleDropdown()
        {
            isDropdownOpen = !isDropdownOpen;
            dropdownPanel.style.display = isDropdownOpen ? DisplayStyle.Flex : DisplayStyle.None;
            Debug.Log($"Harbour Build dropdown {(isDropdownOpen ? "OPENED" : "CLOSED")}");
        }

        private void CloseDropdown()
        {
            isDropdownOpen = false;
            dropdownPanel.style.display = DisplayStyle.None;
        }
    }
}
