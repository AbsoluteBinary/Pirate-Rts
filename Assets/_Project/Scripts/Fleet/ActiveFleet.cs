using System;
using System.Collections.Generic;
using _Project.Scripts.Harbour.ShipBuilder.Data;
using _Project.Scripts.Persistence.TempSave;
using UnityEngine;

namespace _Project.Scripts.Fleet
{
    public static class ActiveFleets
    {
        public const string FreeRoamScene = "FreeRoamScene";
        public const string CombatScene = "CombatScene";

        public static ActiveFleetSaveData Current { get; private set; } = new ActiveFleetSaveData();
        public static event Action Changed;

        public static void Load()
        {
            var store = new JsonActiveFleetSaveStore();
            Current = store.TryRead(out var data) && data != null
                ? data
                : new ActiveFleetSaveData();
            if (Current.fleets == null)
                Current.fleets = new List<ActiveFleetRecord>();
            Changed?.Invoke();
        }

        public static void Save()
        {
            if (Current == null)
                Current = new ActiveFleetSaveData();
            new JsonActiveFleetSaveStore().Write(Current);
            Changed?.Invoke();
        }

        public static void Recall(string fleetName)
        {
            if (Current?.fleets == null || string.IsNullOrWhiteSpace(fleetName))
                return;

            Current.fleets.RemoveAll(fleet => fleet != null && fleet.fleetName == fleetName);
            Save();
        }

        public static ActiveFleetRecord AtSlot(int slot)
        {
            if (Current?.fleets == null)
                return null;
            foreach (var fleet in Current.fleets)
            {
                if (fleet != null && fleet.slotIndex == slot)
                    return fleet;
            }
            return null;
        }

        public static float Health01(ActiveFleetRecord fleet)
        {
            if (fleet?.ships == null)
                return 0f;
            float current = 0f;
            float max = 0f;
            foreach (var ship in fleet.ships)
            {
                if (ship == null || !ship.alive)
                    continue;
                current += ship.currentHealth;
                max += ship.maxHealth;
            }
            return max <= 0f ? 0f : Mathf.Clamp01(current / max);
        }

        public static void Launch(string fleetName, ShipBlueprint[] ships, string sceneName)
        {
            if (string.IsNullOrWhiteSpace(fleetName))
                return;
            if (Current == null)
                Load();

            ActiveFleetRecord record = null;
            foreach (var fleet in Current.fleets)
            {
                if (fleet != null && fleet.fleetName == fleetName)
                {
                    record = fleet;
                    break;
                }
            }

            if (record == null)
            {
                int slot = NextSlot();
                if (slot < 0)
                {
                    Debug.LogWarning("[ActiveFleet] Four fleets are already out.");
                    return;
                }

                record = new ActiveFleetRecord
                {
                    id = Guid.NewGuid().ToString("N"),
                    fleetName = fleetName,
                    slotIndex = slot
                };
                Current.fleets.Add(record);
            }

            record.sceneName = sceneName;
            record.ships = new List<ActiveFleetShipRecord>();
            record.flagshipShipId = "";
            if (ships != null)
            {
                for (int i = 0; i < ships.Length; i++)
                {
                    if (ships[i] != null && i == PendingLaunch.FlagShipIndex)
                        record.flagshipShipId = ships[i].saveId;
                }
            }
            
            if (ships != null)
            {
                foreach (var ship in ships)
                {
                    if (ship == null)
                        continue;
                    record.ships.Add(new ActiveFleetShipRecord
                    {
                        shipId = ship.saveId,
                        shipName = ship.shipName,
                        currentHealth = ship.totalHealth,
                        maxHealth = ship.totalHealth,
                        alive = true
                    });
                }
            }

            Save();
        }

        public static void SetScene(string fleetName, string sceneName)
        {
            var record = Find(fleetName);
            if (record == null)
                return;
            record.sceneName = sceneName;
            Save();
        }

        public static void SetPosition(string fleetName, Vector3 position)
        {
            var record = Find(fleetName);
            if (record == null)
                return;
            record.position = position;
            record.hasPosition = true;
            Save();
        }

        public static void ReturnCombatToMap()
        {
            if (Current?.fleets == null)
                return;
            bool changed = false;
            foreach (var fleet in Current.fleets)
            {
                if (fleet != null && fleet.sceneName == CombatScene)
                {
                    fleet.sceneName = FreeRoamScene;
                    changed = true;
                }
            }
            if (changed)
                Save();
        }

        private static ActiveFleetRecord Find(string fleetName)
        {
            if (Current?.fleets == null || string.IsNullOrWhiteSpace(fleetName))
                return null;
            foreach (var fleet in Current.fleets)
            {
                if (fleet != null && fleet.fleetName == fleetName)
                    return fleet;
            }
            return null;
        }

        private static int NextSlot()
        {
            for (int slot = 0; slot < 4; slot++)
            {
                if (AtSlot(slot) == null)
                    return slot;
            }
            return -1;
        }
    }
}