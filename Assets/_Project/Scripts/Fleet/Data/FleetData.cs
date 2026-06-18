using System.Collections.Generic;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using UnityEngine;

namespace _Project.Scripts.Fleet.Data
{
    [CreateAssetMenu(menuName = "Fleet/Fleet Data", fileName = "New FleetData")]
    public class FleetData : ScriptableObject
    {
        public string fleetName = "Player Fleet";
        public List<ShipBlueprint> ships = new List<ShipBlueprint>();

        public ShipBlueprint GetFlagship() => ships.Count > 0 ? ships[0] : null;
    }
}