using _Project.Scripts.BaseBuilder.Runtime.Data;
using _Project.Scripts.BaseBuilder.Runtime.Modes;
using _Project.Scripts.BaseBuilder.Runtime.Placement;
using _Project.Scripts.BaseBuilder.Runtime.Selection;
using TGS;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Core
{
    public class BaseBuilderController : MonoBehaviour
    {
        [Header("Grid References")]
        [SerializeField] private TerrainGridSystem landGrid;
        [SerializeField] private TerrainGridSystem objectGrid;

        // Systems
        public BuilderModeSystem ModeSystem { get; private set; }
        public OccupationSystem OccupationSystem { get; private set; }
        public PlacementValidator Validator { get; private set; }
        public PlacementService PlacementService { get; private set; }
        public SelectionSystem SelectionSystem { get; private set; }

        // Convenience accessors
        public BuilderMode CurrentMode => ModeSystem.CurrentMode;
        public SelectFilter CurrentSelectFilter => ModeSystem.CurrentSelectFilter;

        private void Awake()
        {
            // Create systems
            ModeSystem = new BuilderModeSystem();
            OccupationSystem = new OccupationSystem();
            Validator = new PlacementValidator(OccupationSystem);
            PlacementService = new PlacementService(OccupationSystem, Validator);
            SelectionSystem = new SelectionSystem();

            // Wire mode changes to grid visibility
            ModeSystem.OnModeChanged += (mode) => UpdateGridVisibility();

            // Inject grids if already assigned
            if (landGrid != null || objectGrid != null)
            {
                PlacementService.SetGrids(landGrid, objectGrid);
            }
        }

        public void SetGrids(TerrainGridSystem land, TerrainGridSystem objects)
        {
            landGrid = land;
            objectGrid = objects;
            PlacementService.SetGrids(land, objects);
        }

        public void SetMode(BuilderMode mode) => ModeSystem.SetMode(mode);
        public void SetSelectFilter(SelectFilter filter) => ModeSystem.SetSelectFilter(filter);

        /// <summary>
        /// Called by HarbourController when entering / exiting Harbour Build mode.
        /// </summary>
        public void SetBuilderActive(bool active)
        {
            gameObject.SetActive(active);

            if (!active)
            {
                // Force both grids off when leaving build mode
                if (landGrid != null) landGrid.gameObject.SetActive(false);
                if (objectGrid != null) objectGrid.gameObject.SetActive(false);
                return;
            }

            // Entering build mode → start in Select (both grids off)
            ModeSystem.SetMode(BuilderMode.Select);
            UpdateGridVisibility();
        }

        public void UpdateGridVisibility()
        {
            bool showLand = ModeSystem.IsBuildOnWater;
            bool showObject = ModeSystem.IsBuildOnLand;

            if (landGrid != null)
                landGrid.gameObject.SetActive(showLand);

            if (objectGrid != null)
                objectGrid.gameObject.SetActive(showObject);

            Debug.Log($"<color=cyan>Grid Visibility → Land: {showLand} | Object: {showObject}</color>");
        }
    }
}