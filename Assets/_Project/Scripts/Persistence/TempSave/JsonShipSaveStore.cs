using System;
using System.IO;
using UnityEngine;

namespace _Project.Scripts.Persistence.TempSave
{
    public interface IShipSaveStore
    {
        void Write(ShipInventorySaveData data);
        bool TryRead(out ShipInventorySaveData data);
        string FilePath { get; }
    }

    public class JsonShipSaveStore : IShipSaveStore
    {
        public const int CurrentVersion = 1;
        public string FilePath { get; }

        public JsonShipSaveStore(string fileName = "ships_temp.json")
        {
            FilePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public void Write(ShipInventorySaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.version = CurrentVersion;
            if (data.ships == null) data.ships = new System.Collections.Generic.List<ShipBuildRecord>();

            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            Debug.Log($"<color=cyan>[ShipSave] Wrote {data.ships.Count} ships → {FilePath}</color>");
        }

        public bool TryRead(out ShipInventorySaveData data)
        {
            data = null;

            if (!File.Exists(FilePath))
            {
                Debug.LogWarning($"[ShipSave] No file at {FilePath}");
                return false;
            }

            data = JsonUtility.FromJson<ShipInventorySaveData>(File.ReadAllText(FilePath));
            if (data == null)
            {
                Debug.LogError("[ShipSave] Failed to parse JSON");
                return false;
            }

            if (data.version > CurrentVersion)
            {
                Debug.LogError($"[ShipSave] File v{data.version} newer than code {CurrentVersion}");
                data = null;
                return false;
            }

            if (data.ships == null) data.ships = new System.Collections.Generic.List<ShipBuildRecord>();
            Debug.Log($"<color=cyan>[ShipSave] Read v{data.version} | ships={data.ships.Count}</color>");
            return true;
        }
    }
}