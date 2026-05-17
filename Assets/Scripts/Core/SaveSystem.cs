using System;
using System.Collections.Generic;
using System.IO;
using Sherlock.Core;
using UnityEngine;

namespace Sherlock.Core
{
    [Serializable]
    public class SaveData
    {
        public int coins;
        public List<SerialKV> pendingInventory = new();
        public List<string> completedQuests    = new();
        public List<string> unlockedScenes     = new();
        // Board state is saved by MergeManager separately as mergeBoardSave.json
    }

    [Serializable]
    public struct SerialKV { public string Key; public int Value; }

    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "gamestate.json");

        public static void Save()
        {
            var gs  = GameState.Instance;
            var data = new SaveData { coins = gs.Coins };
            foreach (var kv in gs.PendingInventory)
                data.pendingInventory.Add(new SerialKV { Key = kv.Key, Value = kv.Value });
            foreach (var q in gs.CompletedQuests) data.completedQuests.Add(q);
            foreach (var s in gs.UnlockedScenes)  data.unlockedScenes.Add(s);

            File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
            Debug.Log($"[SaveSystem] Saved to {SavePath}");
        }

        public static bool Load()
        {
            if (!File.Exists(SavePath)) return false;
            try
            {
                var json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                GameState.Instance.RestoreFromSave(data);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed: {e.Message}");
                return false;
            }
        }

        public static void DeleteSave() => File.Delete(SavePath);
    }
}
