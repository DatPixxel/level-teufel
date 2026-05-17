using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sherlock.Core
{
    /// <summary>
    /// Runtime game-state singleton. Owned by a persistent GameObject in the bootstrapper scene.
    /// Serialised via SaveSystem when the player quits or reaches a checkpoint.
    /// </summary>
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        // ── Currency ──────────────────────────────────────────────────────────
        public int Coins { get; private set; }

        public event Action<int> OnCoinsChanged;

        public void AddCoins(int amount)
        {
            Coins += amount;
            OnCoinsChanged?.Invoke(Coins);
        }

        public bool SpendCoins(int amount)
        {
            if (Coins < amount) return false;
            Coins -= amount;
            OnCoinsChanged?.Invoke(Coins);
            return true;
        }

        // ── Inventory (items waiting to be placed on merge board) ─────────────
        // key = itemId, value = count in pending queue
        public Dictionary<string, int> PendingInventory { get; } = new();

        public event Action<string, int> OnInventoryChanged;

        public void AddToPendingInventory(string itemId, int count = 1)
        {
            PendingInventory.TryGetValue(itemId, out var current);
            PendingInventory[itemId] = current + count;
            OnInventoryChanged?.Invoke(itemId, PendingInventory[itemId]);
        }

        public bool ConsumeFromPendingInventory(string itemId, int count = 1)
        {
            if (!PendingInventory.TryGetValue(itemId, out var current) || current < count)
                return false;
            PendingInventory[itemId] = current - count;
            if (PendingInventory[itemId] == 0) PendingInventory.Remove(itemId);
            OnInventoryChanged?.Invoke(itemId, PendingInventory.GetValueOrDefault(itemId));
            return true;
        }

        // ── HO Scene progress ─────────────────────────────────────────────────
        // key = sceneId, value = set of objectIds already found
        public Dictionary<string, HashSet<string>> FoundObjects { get; } = new();

        public bool HasFound(string sceneId, string objectId)
        {
            return FoundObjects.TryGetValue(sceneId, out var set) && set.Contains(objectId);
        }

        public void MarkFound(string sceneId, string objectId)
        {
            if (!FoundObjects.ContainsKey(sceneId)) FoundObjects[sceneId] = new HashSet<string>();
            FoundObjects[sceneId].Add(objectId);
        }

        // ── Completed quests ──────────────────────────────────────────────────
        public HashSet<string> CompletedQuests { get; } = new();
        public HashSet<string> UnlockedScenes   { get; } = new();

        // ── Lifecycle ─────────────────────────────────────────────────────────
        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Called by SaveSystem on load
        public void RestoreFromSave(SaveData save)
        {
            Coins = save.coins;
            PendingInventory.Clear();
            foreach (var kv in save.pendingInventory) PendingInventory[kv.Key] = kv.Value;
            CompletedQuests.Clear();
            foreach (var q in save.completedQuests) CompletedQuests.Add(q);
            UnlockedScenes.Clear();
            foreach (var s in save.unlockedScenes) UnlockedScenes.Add(s);
        }
    }
}
