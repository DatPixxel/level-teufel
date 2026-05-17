using UnityEngine;

namespace Sherlock.Data
{
    /// <summary>
    /// Defines a single item type: its merge tier, the item it produces, and display info.
    /// Create assets via: Assets > Create > Sherlock > Item Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Sherlock/Item Data", order = 1)]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;          // unique key, e.g. "letter_fragment"
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Merge Chain")]
        public int tier;               // 1 = base, 2 = merged once, 3 = merged twice …
        public ItemData mergeResult;   // null on max-tier items
        public int sellValue;          // coins awarded when removed from board

        [Header("Discovery")]
        public bool foundInScene;      // true = first appears as a HO pick-up
        public string sourceSceneId;   // which HO scene spawns this item

        [Header("Story Gate")]
        public bool isQuestReward;     // true = completing merge chain triggers quest check
        public string questRewardId;   // id checked by QuestManager
    }
}
