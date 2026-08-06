using System.Collections.Generic;
using TGS;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Placement
{
    /// <summary>
    /// Lightweight, reusable multi-cell highlight service.
    /// Uses TGS CellSetColor and restores previous colours cleanly.
    /// </summary>
    public class CellHighlightService
    {
        private readonly Dictionary<TerrainGridSystem, Dictionary<int, Color>> _originalColors = new();
        private readonly List<(TerrainGridSystem grid, int cellIndex)> _activeHighlights = new();

        public void HighlightCells(TerrainGridSystem grid, IReadOnlyList<Cell> cells, Color color)
        {
            if (grid == null || cells == null) return;

            Clear(); // one active highlight set at a time for now

            if (!_originalColors.TryGetValue(grid, out var colorMap))
            {
                colorMap = new Dictionary<int, Color>();
                _originalColors[grid] = colorMap;
            }

            foreach (var cell in cells)
            {
                if (cell == null) continue;

                int index = cell.index;

                // Store original only once
                if (!colorMap.ContainsKey(index))
                    colorMap[index] = grid.CellGetColor(index);

                grid.CellSetColor(index, color);
                _activeHighlights.Add((grid, index));
            }
        }

        public void HighlightCells(TerrainGridSystem grid, IReadOnlyList<int> cellIndices, Color color)
        {
            if (grid == null || cellIndices == null) return;

            var cells = new List<Cell>(cellIndices.Count);
            foreach (int idx in cellIndices)
            {
                if (idx >= 0 && idx < grid.numCells)
                    cells.Add(grid.cells[idx]);
            }
            HighlightCells(grid, cells, color);
        }

        public void Clear()
        {
            foreach (var (grid, index) in _activeHighlights)
            {
                if (grid == null) continue;

                if (_originalColors.TryGetValue(grid, out var colorMap) &&
                    colorMap.TryGetValue(index, out Color original))
                {
                    grid.CellSetColor(index, original);
                }
            }

            _activeHighlights.Clear();
        }

        public void ForceClearAll()
        {
            Clear();
            _originalColors.Clear();
        }
    }
}