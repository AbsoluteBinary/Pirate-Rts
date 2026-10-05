using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public class MapFleet
    {
        public string Name;
        public float Health01;
        public Transform Pawn;
    }

    public static class MapFleets
    {
        public const int Max = 4;
        public static readonly List<MapFleet> Active = new List<MapFleet>(Max);
        public static event Action Changed;
        public static int Selected = -1;

        public static void Prune()
        {
            Active.RemoveAll(fleet => fleet.Pawn == null);
            if (Selected >= Active.Count)
                Selected = Active.Count - 1;
        }

        public static void Register(string name, float health01, Transform pawn)
        {
            Prune();
            if (pawn == null)
                return;

            if (string.IsNullOrWhiteSpace(name))
                name = "Fleet";

            for (int i = 0; i < Active.Count; i++)
            {
                if (Active[i].Pawn == pawn)
                {
                    Active[i].Name = name;
                    Active[i].Health01 = Mathf.Clamp01(health01);
                    if (Selected < 0)
                        Selected = i;
                    Changed?.Invoke();
                    return;
                }
            }

            if (Active.Count >= Max)
                Active.RemoveAt(0);

            Active.Add(new MapFleet
            {
                Name = name,
                Health01 = Mathf.Clamp01(health01),
                Pawn = pawn
            });

            if (Selected < 0)
                Selected = Active.Count - 1;

            Changed?.Invoke();
        }
    }
}