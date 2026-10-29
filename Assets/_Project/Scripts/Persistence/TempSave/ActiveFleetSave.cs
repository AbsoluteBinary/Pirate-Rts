using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _Project.Scripts.Persistence.TempSave
{
    [Serializable]
    public class ActiveFleetSaveData
    {
        public int version = 1;
        public List<ActiveFleetRecord> fleets = new List<ActiveFleetRecord>();
    }

    [Serializable]
    public class ActiveFleetRecord
    {
        public string id;
        public string fleetName;
        public int slotIndex;
        public string sceneName;
        public Vector3 position;
        public List<ActiveFleetShipRecord> ships = new List<ActiveFleetShipRecord>();
        public string flagshipShipId;
        public bool hasPosition;
    }

    [Serializable]
    public class ActiveFleetShipRecord
    {
        public string shipId;
        public string shipName;
        public float currentHealth;
        public float maxHealth;
        public bool alive = true;
    }

    public class JsonActiveFleetSaveStore
    {
        public const int CurrentVersion = 1;
        public string FilePath { get; }

        public JsonActiveFleetSaveStore(string fileName = "ActiveFleets.json")
        {
            FilePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public void Write(ActiveFleetSaveData data)
        {
            data.version = CurrentVersion;
            if (data.fleets == null) data.fleets = new List<ActiveFleetRecord>();
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            Debug.Log($"<color=cyan>[ActiveFleet] Wrote {data.fleets.Count} fleets → {FilePath}</color>");
        }

        public bool TryRead(out ActiveFleetSaveData data)
        {
            data = null;
            if (!File.Exists(FilePath)) return false;

            data = JsonUtility.FromJson<ActiveFleetSaveData>(File.ReadAllText(FilePath));
            if (data == null) return false;
            if (data.fleets == null) data.fleets = new List<ActiveFleetRecord>();
            return true;
        }
    }
}