using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _Project.Scripts.Persistence.TempSave
{
    [Serializable]
    public class FleetInventorySaveData
    {
        public int version = 1;
        public List<FleetRecord> fleets = new List<FleetRecord>();
    }

    [Serializable]
    public class FleetRecord
    {
        public string id;
        public string fleetName;
        public int flagIndex;
        public List<string> shipIds = new List<string>();
    }

    public class JsonFleetSaveStore
    {
        public const int CurrentVersion = 1;
        public string FilePath { get; }

        public JsonFleetSaveStore(string fileName = "fleets_temp.json")
        {
            FilePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public void Write(FleetInventorySaveData data)
        {
            data.version = CurrentVersion;
            if (data.fleets == null) data.fleets = new List<FleetRecord>();
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            Debug.Log($"<color=cyan>[FleetSave] Wrote {data.fleets.Count} fleets → {FilePath}</color>");
        }

        public bool TryRead(out FleetInventorySaveData data)
        {
            data = null;
            if (!File.Exists(FilePath)) return false;

            data = JsonUtility.FromJson<FleetInventorySaveData>(File.ReadAllText(FilePath));
            if (data == null) return false;
            if (data.fleets == null) data.fleets = new List<FleetRecord>();
            return true;
        }
    }
}