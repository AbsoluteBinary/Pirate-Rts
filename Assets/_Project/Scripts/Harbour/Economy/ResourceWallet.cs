using System.Collections.Generic;
using _Project.Scripts.Persistence.TempSave;
using UnityEngine;

namespace _Project.Scripts.Harbour.Economy
{
    public class ResourceWallet
    {
        private readonly Dictionary<string, int> _amounts = new();
        private readonly JsonResourceSaveStore _store;

        public ResourceWallet(JsonResourceSaveStore store = null)
        {
            _store = store ?? new JsonResourceSaveStore();
        }

        public void EnsureFromCatalog(ResourceCatalog catalog)
        {
            if (catalog?.resources == null) return;
            foreach (var def in catalog.resources)
            {
                if (def == null || string.IsNullOrEmpty(def.id)) continue;
                if (!_amounts.ContainsKey(def.id))
                    _amounts[def.id] = def.startingAmount;
            }
            
        }

        public void LoadOrCreate(ResourceCatalog catalog)
        {
            _amounts.Clear();
            EnsureFromCatalog(catalog);

            bool hadFile = _store.TryRead(out var data);
            if (hadFile && data.amounts != null)
            {
                foreach (var rec in data.amounts)
                {
                    if (rec == null || string.IsNullOrEmpty(rec.id)) continue;
                    _amounts[rec.id] = rec.amount;
                }
            }
            else if (catalog?.resources != null)
            {
                foreach (var def in catalog.resources)
                {
                    if (def == null || string.IsNullOrEmpty(def.id)) continue;
                    _amounts[def.id] = def.startingAmount;
                }
            }

            EnsureFromCatalog(catalog);
            Save();
        }

        public int Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            return _amounts.TryGetValue(id, out int n) ? n : 0;
        }

        public void Add(string id, int amount)
        {
            if (string.IsNullOrEmpty(id) || amount == 0) return;
            _amounts[id] = Get(id) + amount;
        }

        public bool TrySpend(string id, int amount)
        {
            if (amount <= 0) return true;
            if (Get(id) < amount) return false;
            _amounts[id] = Get(id) - amount;
            return true;
        }
        
        public bool CanAfford(IReadOnlyList<ResourceCost> costs)
        {
            if (costs == null) return true;
            foreach (var c in costs)
            {
                if (c == null || string.IsNullOrEmpty(c.id) || c.amount <= 0) continue;
                if (Get(c.id) < c.amount) return false;
            }
            return true;
        }

        public bool TrySpendAll(IReadOnlyList<ResourceCost> costs)
        {
            if (!CanAfford(costs)) return false;
            if (costs != null)
            {
                foreach (var c in costs)
                {
                    if (c == null || c.amount <= 0) continue;
                    TrySpend(c.id, c.amount);
                }
            }
            Save();
            return true;
        }

        public void Save()
        {
            var data = new ResourceWalletSaveData { version = JsonResourceSaveStore.CurrentVersion };
            foreach (var kv in _amounts)
                data.amounts.Add(new ResourceAmountRecord { id = kv.Key, amount = kv.Value });
            _store.Write(data);
        }
    }
}