using UnityEngine;

namespace _Project.Scripts.Persistence.TempSave
{
    public static class ShipHealthFile
    {
        public static bool TryGet(string saveId, out float current, out float max)
        {
            current = 0f;
            max = 0f;
            if (string.IsNullOrEmpty(saveId))
                return false;

            var store = new JsonShipSaveStore();
            if (!store.TryRead(out var data) || data?.ships == null)
                return false;

            for (int i = 0; i < data.ships.Count; i++)
            {
                var record = data.ships[i];
                if (record.id != saveId || record.maxHealth <= 0f)
                    continue;

                max = record.maxHealth;
                current = Mathf.Clamp(record.currentHealth, 0f, max);
                return true;
            }

            return false;
        }

        public static void Write(string saveId, float current, float max)
        {
            if (string.IsNullOrEmpty(saveId) || max <= 0f)
                return;

            var store = new JsonShipSaveStore();
            if (!store.TryRead(out var data) || data?.ships == null)
                return;

            for (int i = 0; i < data.ships.Count; i++)
            {
                var record = data.ships[i];
                if (record.id != saveId)
                    continue;

                record.maxHealth = max;
                record.currentHealth = Mathf.Clamp(current, 0f, max);
                store.Write(data);
                return;
            }
        }
    }
}