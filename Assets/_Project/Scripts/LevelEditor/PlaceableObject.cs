using UnityEngine;

namespace _Project.Scripts.LevelEditor
{
    /// <summary>
    /// Attach this to any prefab that can be placed in the Combat Scene Builder.
    /// Defines how many Object Grid cells the object occupies.
    /// </summary>
    public class PlaceableObject : MonoBehaviour
    {
        [Header("Grid Footprint")]
        [Tooltip("How many Object Grid cells this object occupies (X = width, Y = depth)")]
        public Vector2Int sizeInCells = new Vector2Int(1, 1);

        [Header("Optional")]
        [Tooltip("If true, this object must be placed on Land Tiles (usual for buildings, turrets, walls)")]
        public bool requiresLandTile = true;
    }
}
