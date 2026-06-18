using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Fleet.Data;           // FleetData + ShipBlueprint
using _Project.Scripts.Fleet;
using _Project.Scripts.Harbour.ShipBuilder.Data;

namespace _Project.Scripts.UI.Combat
{
    [RequireComponent(typeof(UIDocument))]
    public class ShipHUDController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement root;

        // === NEW: Fleet HUD ===
        private VisualElement fleetContainer;
        private readonly List<VisualElement> shipBars = new List<VisualElement>();

        private void OnEnable()
        {
            if (FleetManager.Instance != null)
                FleetManager.Instance.OnFleetChanged += RefreshFleetHUD;
        }

        private void OnDisable()
        {
            if (FleetManager.Instance != null)
                FleetManager.Instance.OnFleetChanged -= RefreshFleetHUD;
        }
        
        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            BuildHUD();
        }
        
        

        private void BuildHUD()
        {
            root.Clear();

            // ... your existing top HUD, health bar, resources, etc. ...

            CreateFleetHUD();

            // Optional: Refresh when fleet changes
            //if (FleetManager.Instance != null)
                //FleetManager.Instance.OnFleetChanged += RefreshFleetHUD;   // You can add this event later
        }

        private void CreateFleetHUD()
        {
            fleetContainer = new VisualElement();
            fleetContainer.style.position = Position.Absolute;
            fleetContainer.style.bottom = 25;
            fleetContainer.style.left = 25;
            fleetContainer.style.flexDirection = FlexDirection.Column;
            //fleetContainer.style.gap = 6;
            fleetContainer.style.backgroundColor = new Color(0.04f, 0.08f, 0.18f, 0.92f); // Dark Blue
            fleetContainer.style.paddingTop = 10;
            fleetContainer.style.paddingBottom = 10;
            fleetContainer.style.paddingLeft = 12;
            fleetContainer.style.paddingRight = 12;
            fleetContainer.style.borderTopLeftRadius = 8;
            fleetContainer.style.borderTopRightRadius = 8;
            fleetContainer.style.borderBottomLeftRadius = 8;
            fleetContainer.style.borderBottomRightRadius = 8;
            fleetContainer.style.width = 220;

            var title = new Label("FLEET");
            title.style.fontSize = 15;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new Color(1f, 0.55f, 0.1f); // Orange accent
            title.style.marginBottom = 8;
            fleetContainer.Add(title);

            root.Add(fleetContainer);

            RefreshFleetHUD();
        }

        public void RefreshFleetHUD()
        {
            // Clear previous bars
            foreach (var bar in shipBars)
                fleetContainer.Remove(bar);
            shipBars.Clear();

            if (FleetManager.Instance == null || FleetManager.Instance.currentFleetData == null)
                return;

            var fleetData = FleetManager.Instance.currentFleetData;

            for (int i = 0; i < fleetData.ships.Count; i++)
            {
                ShipBlueprint blueprint = fleetData.ships[i];
                if (blueprint == null) continue;
            
                var bar = CreateShipBar(blueprint, i == 0); // First ship = flagship
                fleetContainer.Add(bar);
                shipBars.Add(bar);
            }
        }

        private VisualElement CreateShipBar(ShipBlueprint blueprint, bool isFlagship)
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

            // Ship Name
            var nameLabel = new Label(blueprint.shipName);
            nameLabel.style.flexGrow = 1;
            nameLabel.style.color = Color.white;
            nameLabel.style.fontSize = 13;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            if (isFlagship)
                nameLabel.text = "★ " + blueprint.shipName;

            bar.Add(nameLabel);

            // Optional: Health or status indicator later
            return bar;
        }
    }
}