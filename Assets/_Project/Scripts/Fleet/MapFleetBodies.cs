using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public static class MapFleetBodies
    {
        private class Body
        {
            public string FleetName;
            public Transform Transform;
        }

        private static readonly List<Body> All = new List<Body>();

        public static void Clear()
        {
            All.Clear();
        }

        public static void Remember(string fleetName, Transform body)
        {
            if (string.IsNullOrEmpty(fleetName) || body == null)
                return;

            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].FleetName != fleetName)
                    continue;

                All[i].Transform = body;
                return;
            }

            All.Add(new Body { FleetName = fleetName, Transform = body });
        }

        public static void SavePositions()
        {
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Transform == null)
                    continue;

                ActiveFleets.SetPosition(All[i].FleetName, All[i].Transform.position);
            }
        }
    }
}
