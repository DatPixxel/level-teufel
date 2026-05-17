using System.Collections.Generic;
using UnityEngine;

namespace Sherlock.Data
{
    /// <summary>
    /// Central registry for all ItemData assets.
    /// Place all ItemData ScriptableObjects under Assets/Resources/Items/
    /// so Resources.LoadAll can auto-discover them at runtime.
    /// </summary>
    public class ItemDatabase : MonoBehaviour
    {
        public static ItemDatabase Instance { get; private set; }

        private readonly Dictionary<string, ItemData> _items = new();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAll();
        }

        void LoadAll()
        {
            var loaded = Resources.LoadAll<ItemData>("Items");
            foreach (var item in loaded)
            {
                if (_items.ContainsKey(item.itemId))
                {
                    Debug.LogWarning($"[ItemDatabase] Duplicate itemId '{item.itemId}' — skipping.");
                    continue;
                }
                _items[item.itemId] = item;
            }
            Debug.Log($"[ItemDatabase] Loaded {_items.Count} items.");
        }

        public ItemData Get(string itemId)
        {
            _items.TryGetValue(itemId, out var data);
            if (data == null) Debug.LogWarning($"[ItemDatabase] Unknown itemId '{itemId}'.");
            return data;
        }

        public bool TryGet(string itemId, out ItemData data) => _items.TryGetValue(itemId, out data);

        // Merge helper: returns the result item or null if no merge is defined.
        public ItemData GetMergeResult(string itemId)
        {
            if (!TryGet(itemId, out var data)) return null;
            return data.mergeResult;
        }
    }
}
