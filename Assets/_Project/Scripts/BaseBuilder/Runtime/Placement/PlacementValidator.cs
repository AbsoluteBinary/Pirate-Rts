using _Project.Scripts.BaseBuilder.Runtime.Data;
using _Project.Scripts.BaseBuilder.Runtime.Placement;
using TGS;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Placement
{
    public class PlacementValidator
    {
        private readonly OccupationSystem occupation;
        private TerrainGridSystem landGrid;
        private TerrainGridSystem objectGrid;

        public PlacementValidator(OccupationSystem occupationSystem)
        {
            occupation = occupationSystem;
        }

        public void SetGrids(TerrainGridSystem land, TerrainGridSystem objects)
        {
            landGrid = land;
            objectGrid = objects;
        }

        public bool HasLandUnderneath(Vector3 worldPosition)
        {
            if (landGrid == null) return false;

            Vector3 landPos = worldPosition;
            landPos.y = landGrid.transform.position.y + 0.1f;

            Cell landCell = landGrid.CellGetAtWorldPosition(landPos, 0);
            if (landCell == null)
            {
                landPos.y += 0.5f;
                landCell = landGrid.CellGetAtWorldPosition(landPos, 0);
            }

            return landCell != null && occupation.IsLandOccupied(landCell.index);
        }

        public bool CanPlace(Vector3 worldPosition, Vector2Int size, bool isLandObject)
        {
            TerrainGridSystem grid = isLandObject ? landGrid : objectGrid;
            if (grid == null) return false;

            Cell originCell = grid.CellGetAtWorldPosition(worldPosition, 0);
            if (originCell == null) return false;

            // Footprint must be free
            if (!occupation.IsFootprintFree(originCell.index, size, grid.columnCount, grid.numCells, isLandObject))
                return false;

            // Objects require land underneath every cell of the footprint
            if (!isLandObject)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        int index = originCell.index + x + (y * grid.columnCount);
                        Vector3 cellWorld = grid.CellGetPosition(index);

                        if (!HasLandUnderneath(cellWorld))
                            return false;
                    }
                }
            }

            return true;
        }

        public Vector3 GetFootprintCenter(Cell originCell, Vector2Int size, TerrainGridSystem grid)
        {
            Vector3 originPos = grid.CellGetPosition(originCell.index);
            float cellSize = grid.cellSize.x;

            Vector3 center = originPos;
            center.x += (size.x - 1) * cellSize * 0.5f;
            center.z += (size.y - 1) * cellSize * 0.5f;
            return center;
        }
    }
}