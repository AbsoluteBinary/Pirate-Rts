using UnityEngine;
using UnityEngine.UIElements;

namespace Test_Scripts
{
    public class HarbourUIController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private void Awake()
        {
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;

            // Create main panel
            var mainPanel = new VisualElement
            {
                name = "MainHarbourPanel",
                style =
                {
                    backgroundColor = new StyleColor(new Color(0.1f, 0.15f, 0.25f, 0.85f)),
                    width = new Length(420, LengthUnit.Pixel),
                    height = new Length(520, LengthUnit.Pixel),
                    position = Position.Absolute,
                    left = new Length(50, LengthUnit.Pixel),
                    top = new Length(80, LengthUnit.Pixel),
                    //borderRadius = new StyleLength(12),
                    paddingTop = 20,
                    paddingBottom = 20
                }
            };

            // Title
            var title = new Label("Harbour Command")
            {
                style =
                {
                    fontSize = 28,
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginBottom = 20
                }
            };
            mainPanel.Add(title);

            // 3 buttons
            string[] buttonNames = { "Enter Build Mode", "View Resources", "Return to Open Sea" };

            foreach (var name in buttonNames)
            {
                var btn = new Button(() => Debug.Log($"{name} clicked"))
                {
                    text = name,
                    style =
                    {
                        height = 52,
                        fontSize = 18,
                        marginTop = 8,
                        marginBottom = 8,
                        marginLeft = 20,
                        marginRight = 20,
                        backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.8f, 0.9f))
                    }
                };
                mainPanel.Add(btn);
            }

            root.Add(mainPanel);
        }
    }
}
