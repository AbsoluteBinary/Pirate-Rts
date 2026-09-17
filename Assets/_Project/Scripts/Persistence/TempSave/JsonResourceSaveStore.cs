using System;
using System.IO;
using UnityEngine;

namespace _Project.Scripts.Persistence.TempSave
{
    public class JsonResourceSaveStore
    {
        public const int CurrentVersion = 1;
        public string FilePath { get; }

        public JsonResourceSaveStore(string fileName = "wallet_temp.json")
        {
            FilePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        public void Write(ResourceWalletSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            data.version = CurrentVersion;
            if (data.amounts == null) data.amounts = new System.Collections.Generic.List<ResourceAmountRecord>();
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            Debug.Log($"<color=cyan>[Wallet] Wrote {data.amounts.Count} ids → {FilePath}</color>");
        }

        public bool TryRead(out ResourceWalletSaveData data)
        {
            data = null;
            if (!File.Exists(FilePath))
            {
                Debug.LogWarning($"[Wallet] No file at {FilePath}");
                return false;
            }

            data = JsonUtility.FromJson<ResourceWalletSaveData>(File.ReadAllText(FilePath));
            if (data == null)
            {
                Debug.LogError("[Wallet] Failed to parse JSON");
                return false;
            }

            if (data.amounts == null) data.amounts = new System.Collections.Generic.List<ResourceAmountRecord>();
            Debug.Log($"<color=cyan>[Wallet] Read v{data.version} | ids={data.amounts.Count}</color>");
            return true;
        }
    }
}