using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Placement
{
    public class OccupationSystem
    {
        private readonly HashSet<int> occupiedLandCells = new HashSet<int>();
        private readonly HashSet<int> occupiedObjectCells = new HashSet<int>();

        public IReadOnlyCollection<int> OccupiedLandCells => occupiedLandCells;
        public IReadOnlyCollection<int> OccupiedObjectCells => occupiedObjectCells;

        public void Clear()
        {
            occupiedLandCells.Clear();
            occupiedObjectCells.Clear();
        }

        public bool IsLandOccupied(int cellIndex)
        {
            return occupiedLandCells.Contains(cellIndex);
        }

        public bool IsObjectOccupied(int cellIndex)
        {
            return occupiedObjectCells.Contains(cellIndex);
        }

        public void OccupyLand(int cellIndex)
        {
            occupiedLandCells.Add(cellIndex);
        }

        public void OccupyObject(int cellIndex)
        {
            occupiedObjectCells.Add(cellIndex);
        }

        public void FreeLand(int cellIndex)
        {
            occupiedLandCells.Remove(cellIndex);
        }

        public void FreeObject(int cellIndex)
        {
            occupiedObjectCells.Remove(cellIndex);
        }

        public void OccupyFootprint(int originIndex, Vector2Int size, int columnCount, bool isLand)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int index = originIndex + x + (y * columnCount);
                    if (isLand)
                        occupiedLandCells.Add(index);
                    else
                        occupiedObjectCells.Add(index);
                }
            }
        }

        public void FreeFootprint(int originIndex, Vector2Int size, int columnCount, bool isLand)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int index = originIndex + x + (y * columnCount);
                    if (isLand)
                        occupiedLandCells.Remove(index);
                    else
                        occupiedObjectCells.Remove(index);
                }
            }
        }

        public bool IsFootprintFree(int originIndex, Vector2Int size, int columnCount, int totalCells, bool isLand)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int index = originIndex + x + (y * columnCount);
                    if (index < 0 || index >= totalCells)
                        return false;

                    if (isLand && occupiedLandCells.Contains(index))
                        return false;

                    if (!isLand && occupiedObjectCells.Contains(index))
                        return false;
                }
            }
            return true;
        }
    }
}