using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Harbour.ShipBuilder
{
    [CreateAssetMenu(menuName = "Harbour/Hull Slot Mask")]
    public class HullSlotMask : ScriptableObject
    {
        public HullData hullData;
        public Texture2D maskTexture;

        [System.Serializable]
        public class SlotMapping
        {
            public Color colorKey;        // Exact color from GIMP
            public ModuleSlot slotData;
        }

        public List<SlotMapping> mappings = new();

        private Dictionary<Color, ModuleSlot> _colorToSlot;

        public void Initialize()
        {
            _colorToSlot = new Dictionary<Color, ModuleSlot>();
            foreach (var map in mappings)
            {
                // Use 24-bit color (ignore alpha)
                Color key = new Color(
                    Mathf.Round(map.colorKey.r * 255f) / 255f,
                    Mathf.Round(map.colorKey.g * 255f) / 255f,
                    Mathf.Round(map.colorKey.b * 255f) / 255f,
                    1f);
                
                _colorToSlot[key] = map.slotData;
            }
        }

        public ModuleSlot GetSlotAtPixel(Color pixelColor)
        {
            if (_colorToSlot == null) Initialize();

            // Round to nearest 8-bit value
            Color rounded = new Color(
                Mathf.Round(pixelColor.r * 255f) / 255f,
                Mathf.Round(pixelColor.g * 255f) / 255f,
                Mathf.Round(pixelColor.b * 255f) / 255f,
                1f);

            if (_colorToSlot.TryGetValue(rounded, out ModuleSlot slot))
                return slot;

            // Extra tolerance for any compression/rounding issues
            foreach (var kvp in _colorToSlot)
            {
                float diff = Mathf.Abs(kvp.Key.r - pixelColor.r) +
                             Mathf.Abs(kvp.Key.g - pixelColor.g) +
                             Mathf.Abs(kvp.Key.b - pixelColor.b);

                if (diff < 0.05f)
                    return kvp.Value;
            }

            return null;
        }

        private float ColorDistance(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
        }
    }
}
