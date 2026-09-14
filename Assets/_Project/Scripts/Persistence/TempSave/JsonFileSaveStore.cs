using System;
using System.IO;
using UnityEngine;

namespace _Project.Scripts.Persistence.TempSave
{
    public interface IBuilderSaveStore
    {
        void Write(BuilderSaveData data);
        bool TryRead(out BuilderSaveData data);
        string FilePath { get; }
    }

    public class JsonFileSaveStore : IBuilderSaveStore
    {
        public const int CurrentVersion = 1;

        public string FilePath { get; }

        public JsonFileSaveStore(string fileName = "harbour_layout_temp.json")
        {
            FilePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public void Write(BuilderSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            data.version = CurrentVersion;
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(FilePath, json);
            Debug.Log($"<color=cyan>[Save] Wrote {FilePath}</color>");
        }

        public bool TryRead(out BuilderSaveData data)
        {
            data = null;

            if (!File.Exists(FilePath))
            {
                Debug.LogWarning($"[Save] No file at {FilePath}");
                return false;
            }

            string json = File.ReadAllText(FilePath);
            data = JsonUtility.FromJson<BuilderSaveData>(json);

            if (data == null)
            {
                Debug.LogError("[Save] Failed to parse JSON");
                return false;
            }

            if (data.version > CurrentVersion)
            {
                Debug.LogError($"[Save] File version {data.version} newer than code {CurrentVersion}");
                data = null;
                return false;
            }

            if (data.counts == null) data.counts = new System.Collections.Generic.List<SlotCountRecord>();
            if (data.placed == null) data.placed = new System.Collections.Generic.List<PlacedRecord>();

            Debug.Log($"<color=cyan>[Save] Read v{data.version} | counts={data.counts.Count} placed={data.placed.Count}</color>");
            return true;
        }
    }
}