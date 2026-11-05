using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _Project.Scripts.Fleet.Targets
{
    public static class TargetsFile
    {
        private const string FileName = "targets_temp.json";
        private const int CurrentVersion = 1;

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool TryRead(out TargetsSaveData data)
        {
            data = null;
            if (!File.Exists(FilePath))
            {
                Debug.LogWarning($"[Targets] No file at {FilePath}");
                return false;
            }

            string json = File.ReadAllText(FilePath);
            data = JsonUtility.FromJson<TargetsSaveData>(json);
            if (data == null)
            {
                Debug.LogError("[Targets] Failed to parse JSON");
                return false;
            }

            if (data.targets == null) data.targets = new List<TargetConditionRecord>();
            Debug.Log($"<color=cyan>[Targets] Read v{data.version} | count={data.targets.Count}</color>");
            return true;
        }

        public static void Write(TargetsSaveData data)
        {
            if (data == null) return;
            data.version = CurrentVersion;
            if (data.targets == null) data.targets = new List<TargetConditionRecord>();
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(FilePath, json);
            Debug.Log($"<color=cyan>[Targets] Wrote {FilePath}</color>");
        }

        public static bool TryGet(string targetId, out TargetConditionRecord record)
        {
            record = null;
            if (string.IsNullOrEmpty(targetId) || !TryRead(out var data) || data.targets == null)
                return false;

            foreach (var r in data.targets)
            {
                if (r.targetId == targetId)
                {
                    record = r;
                    return true;
                }
            }
            return false;
        }

        public static void WriteRecord(TargetConditionRecord record)
        {
            if (record == null || string.IsNullOrEmpty(record.targetId)) return;

            if (!TryRead(out var data))
                data = new TargetsSaveData();

            for (int i = 0; i < data.targets.Count; i++)
            {
                if (data.targets[i].targetId == record.targetId)
                {
                    data.targets[i] = record;
                    Write(data);
                    return;
                }
            }

            data.targets.Add(record);
            Write(data);
        }
    }
}