using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public static class PendingBattle
    {
        public static bool Active;
        public static string TargetId;
        public static Vector3 MapPosition;
        public static bool HasMapPosition;

        public static void Engage(string targetId, Vector3 mapPosition)
        {
            Active = true;
            TargetId = targetId;
            MapPosition = mapPosition;
            HasMapPosition = true;
        }

        public static void Clear()
        {
            Active = false;
            TargetId = null;
            HasMapPosition = false;
        }
    }
}