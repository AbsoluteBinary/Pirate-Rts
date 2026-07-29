using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Data
{
    [System.Serializable]
    public class PlaceableObjectData
    {
        public GameObject Prefab;
        public Vector2Int SizeInCells = Vector2Int.one;
        public bool RequiresLandTile = true;
    }
}