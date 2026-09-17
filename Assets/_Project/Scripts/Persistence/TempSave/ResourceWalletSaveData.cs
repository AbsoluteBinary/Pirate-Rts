using System;
using System.Collections.Generic;

namespace _Project.Scripts.Persistence.TempSave
{
    [Serializable]
    public class ResourceWalletSaveData
    {
        public int version = 1;
        public List<ResourceAmountRecord> amounts = new List<ResourceAmountRecord>();
    }

    [Serializable]
    public class ResourceAmountRecord
    {
        public string id;
        public int amount;
    }
}