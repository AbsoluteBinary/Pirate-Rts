using UnityEngine;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    public static class HullLibrary
    {
        public const string Folder = "ShipBuilding/Hulls";

        public static HullData[] All => Resources.LoadAll<HullData>(Folder);
    }
}