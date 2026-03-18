using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Managers.Registry
{
    public static class HarbourObjectRegistry
    {
        // All registered cameras (grouped by mode if needed)
        private static readonly Dictionary<string, Camera> cameras = new(); // key = mode or tag

        // TGS grid (only one expected)
        private static GameObject tgsGrid;

        // IO Boxes (scene-specific)
        private static readonly Dictionary<string, GameObject> ioBoxes = new(); // key = scene name or tag

        // Register methods (call from Awake/OnEnable)
        public static void RegisterCamera(string key, Camera cam)
        {
            if (cam != null && !cameras.ContainsKey(key))
            {
                cameras[key] = cam;
                //Debug.Log($"[HarbourRegistry] Registered camera '{key}' on {cam.gameObject.name}");
            }
        }

        public static void RegisterTGSGrid(GameObject grid)
        {
            if (grid != null && tgsGrid == null)
            {
                tgsGrid = grid;
                //Debug.Log($"[HarbourRegistry] Registered TGS Grid: {grid.name}");
            }
        }

        public static void RegisterIOBox(string key, GameObject box)
        {
            if (box != null && !ioBoxes.ContainsKey(key))
            {
                ioBoxes[key] = box;
                //Debug.Log($"[HarbourRegistry] Registered IO Box '{key}': {box.name}");
            }
        }

        // Unregister (call from OnDestroy/OnDisable)
        public static void UnregisterCamera(string key) => cameras.Remove(key);
        public static void UnregisterTGSGrid() => tgsGrid = null;
        public static void UnregisterIOBox(string key) => ioBoxes.Remove(key);

        // Disable all (call when loading starts)
        public static void DisableAllForLoading()
        {
            // Cameras
            foreach (var kvp in cameras)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    kvp.Value.gameObject.SetActive(false);
                    //Debug.Log($"[HarbourRegistry] Disabled camera '{kvp.Key}'");
                }
            }

            // TGS Grid
            if (tgsGrid != null)
            {
                tgsGrid.SetActive(false);
                //Debug.Log("[HarbourRegistry] Disabled TGS Grid");
            }

            // IO Boxes
            foreach (var kvp in ioBoxes)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.SetActive(false);
                    //Debug.Log($"[HarbourRegistry] Disabled IO Box '{kvp.Key}'");

                    // Bonus: disable listener & eventsystem inside
                    var listener = kvp.Value.GetComponentInChildren<AudioListener>(true);
                    if (listener != null) listener.enabled = false;

                    var eventSys = kvp.Value.GetComponentInChildren<EventSystem>(true);
                    if (eventSys != null) eventSys.enabled = false;
                }
            }
        }

        // Re-enable (call when returning to Idle or scene unload)
        public static void ReEnableAll()
        {
            // Reverse logic of DisableAllForLoading
            foreach (var kvp in cameras)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                    kvp.Value.gameObject.SetActive(true);
            }

            if (tgsGrid != null) tgsGrid.SetActive(true);

            foreach (var kvp in ioBoxes)
            {
                if (kvp.Value != null) kvp.Value.SetActive(true);
            }

            //Debug.Log("[HarbourRegistry] All objects re-enabled");
        }

        // Clear everything (e.g. on application quit or scene reset)
        public static void Clear()
        {
            cameras.Clear();
            tgsGrid = null;
            ioBoxes.Clear();
        }
    }
}