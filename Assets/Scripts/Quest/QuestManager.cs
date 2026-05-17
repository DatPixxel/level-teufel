using System.Collections.Generic;
using Sherlock.Core;
using Sherlock.Data;
using Sherlock.UI;
using UnityEngine;

namespace Sherlock.Quest
{
    /// <summary>
    /// QuestManager tracks active and completed quests.
    /// It reacts to merge events and HO scene completions forwarded by other systems.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [SerializeField] private QuestData[] allQuests;
        [SerializeField] private string      firstQuestId = "q01_letter_fragment";

        private readonly Dictionary<string, QuestData>  _questMap  = new();
        private readonly Dictionary<string, int>        _craftCount = new(); // itemId → crafted count
        private QuestData                               _activeQuest;

        public QuestData ActiveQuest => _activeQuest;

        // Events
        public System.Action<QuestData> OnQuestCompleted;
        public System.Action<QuestData> OnQuestActivated;

        // ═════════════════════════════════════════════════════════════════════
        // Lifecycle
        // ═════════════════════════════════════════════════════════════════════

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            foreach (var q in allQuests) _questMap[q.questId] = q;
        }

        void Start()
        {
            // Resume from save or start fresh
            var gs = GameState.Instance;
            // Find the first quest that is not completed and not blocked by prerequisites
            foreach (var q in allQuests)
            {
                if (!gs.CompletedQuests.Contains(q.questId))
                {
                    ActivateQuest(q);
                    return;
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // Public API — other systems call these
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>Called by MergeManager every time a merge produces a new item.</summary>
        public void OnItemCrafted(string itemId)
        {
            _craftCount.TryGetValue(itemId, out var count);
            _craftCount[itemId] = count + 1;
            CheckActiveQuest();
        }

        /// <summary>Called by HiddenObjectController when a scene finishes.</summary>
        public void OnSceneCompleted(string sceneId) => CheckActiveQuest();

        // ═════════════════════════════════════════════════════════════════════
        // Internal
        // ═════════════════════════════════════════════════════════════════════

        void ActivateQuest(QuestData quest)
        {
            _activeQuest = quest;
            OnQuestActivated?.Invoke(quest);
            GameUIManager.Instance?.ShowQuestBanner(quest);
            Debug.Log($"[QuestManager] Activated: {quest.questId} — {quest.title}");
        }

        void CheckActiveQuest()
        {
            if (_activeQuest == null) return;
            if (!IsQuestComplete(_activeQuest)) return;

            CompleteQuest(_activeQuest);
        }

        bool IsQuestComplete(QuestData quest)
        {
            switch (quest.questType)
            {
                case QuestType.CraftItem:
                    // Pass if the craft count for the target item is >= 1
                    _craftCount.TryGetValue(quest.targetItemId, out var c);
                    return c >= Mathf.Max(1, quest.targetCount);

                case QuestType.FindItems:
                    // GameState tracks found objects; sum across all scenes
                    int found = 0;
                    foreach (var set in GameState.Instance.FoundObjects.Values)
                        found += set.Count;
                    return found >= quest.targetCount;

                case QuestType.MergeCount:
                    int total = 0;
                    foreach (var v in _craftCount.Values) total += v;
                    return total >= quest.targetCount;

                default: return false;
            }
        }

        void CompleteQuest(QuestData quest)
        {
            var gs = GameState.Instance;
            gs.CompletedQuests.Add(quest.questId);

            // Unlock rewards
            if (!string.IsNullOrEmpty(quest.unlocksSceneId))
                gs.UnlockedScenes.Add(quest.unlocksSceneId);

            if (quest.rewardCoins > 0)
                gs.AddCoins(quest.rewardCoins);

            if (quest.rewardItem != null)
                Merge.MergeManager.Instance?.SpawnItem(quest.rewardItem);

            SaveSystem.Save();

            OnQuestCompleted?.Invoke(quest);
            GameUIManager.Instance?.ShowQuestCompleteScreen(quest);

            Debug.Log($"[QuestManager] Completed: {quest.questId}");

            // Chain to next quest
            if (!string.IsNullOrEmpty(quest.unlocksQuestId)
                && _questMap.TryGetValue(quest.unlocksQuestId, out var next))
            {
                ActivateQuest(next);
            }
            else
            {
                _activeQuest = null;
                Debug.Log("[QuestManager] All quests completed — end of current content.");
            }
        }
    }
}
