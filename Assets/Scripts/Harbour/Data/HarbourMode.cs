using UnityEngine;

namespace Harbour.Data
{
    [CreateAssetMenu(fileName = "HarbourState", menuName = "Naval/Harbour/HarbourState", order = 1)]
    public class HarbourStateSO : ScriptableObject
    {
        public enum HarbourMode
        {
            Idle,
            HarbourBuild,
            ShipBuild
        }

        [Header("Current Mode")]
        public HarbourMode currentMode = HarbourMode.Idle;

        [Header("UI Visibility (persistent across reloads)")]
        public bool showIdleHud = true;
        public bool showHarbourBuildHud = false;
        public bool showShipBuildHud = false;

        [Header("World State")]
        public bool tgsGridEnabled = false;
        public bool idleCameraActive = true;
        public bool harbourBuildCameraActive = false;

        // Add more later: resources, progress percentages, unlocked builds, etc.
        // public float metalCount = 0f;
        // public float shipBuildProgress = 0f;
    }
}
