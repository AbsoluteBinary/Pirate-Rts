using UnityEngine;
using UnityEngine.UIElements;

namespace Harbour.UIControllers
{
    public class ShipBuildHUDController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement dropdownPanel;
        private bool isDropdownOpen = false;

        private void Awake()
        {
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument not assigned on ShipBuildHUDController!", this);
                return;
            }

            CreateTopBar();
        }

        private void CreateTopBar()
        {
            var root = uiDocument.rootVisualElement;

            // Full-width top bar
            var topBar = new VisualElement { name = "ShipBuildTopBar" };
            topBar.style.position = Position.Absolute;
            topBar.style.top = 0;
            topBar.style.left = 0;
            topBar.style.width = new Length(100, LengthUnit.Percent);
            topBar.style.height = new Length(7, LengthUnit.Percent);
            topBar.style.backgroundColor = new StyleColor(new Color(0.12f, 0.08f, 0.25f, 0.95f));
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.justifyContent = Justify.FlexEnd;
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingRight = 20;

            var optionsTrigger = new Button(ToggleDropdown)
            {
                text = "Options ▼"
            };
            optionsTrigger.style.height = new Length(68, LengthUnit.Percent);
            optionsTrigger.style.minWidth = 180;

            dropdownPanel = new VisualElement { name = "DropdownPanel" };
            dropdownPanel.style.position = Position.Absolute;
            dropdownPanel.style.top = new Length(105, LengthUnit.Percent);
            dropdownPanel.style.right = 20;
            dropdownPanel.style.width = 280;
            dropdownPanel.style.backgroundColor = new StyleColor(new Color(0.15f, 0.10f, 0.30f, 0.98f));
            //dropdownPanel.style.borderWidth = 4;
            //dropdownPanel.style.borderColor = new StyleColor(Color.magenta);
            dropdownPanel.style.borderTopLeftRadius = 12;
            dropdownPanel.style.borderTopRightRadius = 12;
            dropdownPanel.style.borderBottomLeftRadius = 12;
            dropdownPanel.style.borderBottomRightRadius = 12;
            dropdownPanel.style.paddingTop = 12;
            dropdownPanel.style.paddingBottom = 12;
            dropdownPanel.style.display = DisplayStyle.None;

            AddDropdownButton("Save and Exit", () => Debug.Log("Save and Exit clicked"));
            AddDropdownButton("Exit without Saving", () => Debug.Log("Exit without Saving clicked"));

            topBar.Add(optionsTrigger);
            root.Add(topBar);
            root.Add(dropdownPanel);   // ← Same critical line

            Debug.Log("Ship Build HUD ready");
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
            Debug.Log($"Ship Build dropdown {(isDropdownOpen ? "OPENED" : "CLOSED")}");
        }

        private void CloseDropdown()
        {
            isDropdownOpen = false;
            dropdownPanel.style.display = DisplayStyle.None;
        }
    }
}
