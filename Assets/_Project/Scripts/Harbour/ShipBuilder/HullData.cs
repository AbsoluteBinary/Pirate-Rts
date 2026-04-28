using UnityEngine;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    [CreateAssetMenu(menuName = "Harbour/Hull Data")]
    public class HullData : ScriptableObject
    {
        public string hullName;
        public Sprite hullImage;
        public string slotInfo;      // e.g. "1 Weapon • 1 Armour • 1 Engine"
        public string description;
    }
}