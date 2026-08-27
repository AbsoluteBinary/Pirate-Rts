using UnityEngine;
using _Project.Scripts.BaseBuilder.Runtime.Inventory;

namespace _Project.Scripts.BaseBuilder.Runtime.Data
{
    public class StoredBuilding
    {
        public PlaceableKind Kind;
        public int Level;
        public Vector2Int Size;

        public StoredBuilding(PlaceableKind kind, int level, Vector2Int size)
        {
            Kind = kind;
            Level = level;
            Size = size;
        }
    }
}