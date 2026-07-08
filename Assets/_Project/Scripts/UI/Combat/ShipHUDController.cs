using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Fleet;
using _Project.Scripts.PlayerShip_Movement.Combat;

namespace _Project.Scripts.UI.Combat
{
    [RequireComponent(typeof(UIDocument))]
    public class ShipHUDController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement fleetContainer;
        private readonly List<VisualElement> shipBars = new List<VisualElement>();
        private VisualElement selectedBar;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            BuildHUD();
        }

        private void BuildHUD()
        {
            CreateFleetHUD();
        }

        private void CreateFleetHUD()
        {
            fleetContainer = new VisualElement();
            fleetContainer.style.position = Position.Absolute;
            fleetContainer.style.bottom = 25;
            fleetContainer.style.left = 25;
            fleetContainer.style.flexDirection = FlexDirection.Column;
            fleetContainer.style.backgroundColor = new Color(0.04f, 0.08f, 0.18f, 0.92f);
            fleetContainer.style.paddingTop = 10;
            fleetContainer.style.paddingBottom = 10;
            fleetContainer.style.paddingLeft = 12;
            fleetContainer.style.paddingRight = 12;
            fleetContainer.style.borderTopLeftRadius = 8;
            fleetContainer.style.borderTopRightRadius = 8;
            fleetContainer.style.borderBottomLeftRadius = 8;
            fleetContainer.style.borderBottomRightRadius = 8;
            fleetContainer.style.width = 220;

            // === Clickable "FLEET" Title (Select All) ===
            var title = new Label("FLEET");
            title.style.fontSize = 15;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new Color(1f, 0.55f, 0.1f);
            title.style.marginBottom = 8;
            title.style.unityTextAlign = TextAnchor.MiddleLeft;

            title.RegisterCallback<ClickEvent>(evt =>
            {
                SelectAllShips();
            });

            fleetContainer.Add(title);
            root.Add(fleetContainer);

            RefreshFleetHUD();
        }

        public void RefreshFleetHUD()
        {
            foreach (var bar in shipBars)
                fleetContainer.Remove(bar);
            shipBars.Clear();
            selectedBar = null;

            if (FleetManager.Instance?.activeShips == null)
                return;

            foreach (var ship in FleetManager.Instance.activeShips)
            {
                if (ship == null) continue;

                bool isFlagship = FleetManager.Instance.flagship == ship;
                var bar = CreateShipBar(ship, isFlagship);
                fleetContainer.Add(bar);
                shipBars.Add(bar);
            }
        }

        private VisualElement CreateShipBar(PlayerCombatMovementController shipController, bool isFlagship)
        {
            var bar = new VisualElement();
            bar.style.flexDirection = FlexDirection.Row;
            bar.style.alignItems = Align.Center;
            bar.style.height = 32;
            bar.style.backgroundColor = new Color(0.08f, 0.15f, 0.3f, 0.95f);
            bar.style.paddingLeft = 12;
            bar.style.paddingRight = 12;
            bar.style.borderTopLeftRadius = 5;
            bar.style.borderTopRightRadius = 5;
            bar.style.borderBottomLeftRadius = 5;
            bar.style.borderBottomRightRadius = 5;
            bar.style.marginBottom = 6; // Spacing between bars

            bar.userData = shipController;

            var nameLabel = new Label(shipController.gameObject.name);
            nameLabel.style.flexGrow = 1;
            nameLabel.style.color = Color.white;
            nameLabel.style.fontSize = 13;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            if (isFlagship)
                nameLabel.text = "★ " + shipController.gameObject.name;

            bar.Add(nameLabel);

            // === Click to select ship ===
            bar.RegisterCallback<ClickEvent>(evt =>
            {
                SelectShip(bar, shipController);
            });

            // Hover effect
            bar.RegisterCallback<MouseEnterEvent>(evt =>
            {
                if (bar != selectedBar)
                    bar.style.backgroundColor = new Color(0.12f, 0.22f, 0.45f, 0.95f);
            });

            bar.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (bar != selectedBar)
                    bar.style.backgroundColor = new Color(0.08f, 0.15f, 0.3f, 0.95f);
            });

            return bar;
        }

        private void SelectShip(VisualElement bar, PlayerCombatMovementController shipController)
        {
            // Deselect previous bar
            if (selectedBar != null && selectedBar != bar)
            {
                selectedBar.style.backgroundColor = new Color(0.08f, 0.15f, 0.3f, 0.95f);
            }

            // Select new bar
            selectedBar = bar;
            bar.style.backgroundColor = new Color(0.2f, 0.35f, 0.65f, 0.95f);

            FleetManager.Instance?.SelectSingle(shipController);
        }

        private void SelectAllShips()
        {
            if (FleetManager.Instance == null) return;

            FleetManager.Instance.SelectAll();

            // Highlight all bars
            foreach (var bar in shipBars)
            {
                bar.style.backgroundColor = new Color(0.2f, 0.35f, 0.65f, 0.95f);
            }

            selectedBar = null;
        }

        // Call this if selection changes from outside the UI
        public void UpdateSelectionVisuals()
        {
            if (FleetManager.Instance == null) return;

            foreach (var bar in shipBars)
            {
                if (bar.userData is PlayerCombatMovementController controller)
                {
                    bool isSelected = FleetManager.Instance.selectedShips.Contains(controller);

                    bar.style.backgroundColor = isSelected
                        ? new Color(0.2f, 0.35f, 0.65f, 0.95f)
                        : new Color(0.08f, 0.15f, 0.3f, 0.95f);
                }
            }
        }
    }
}