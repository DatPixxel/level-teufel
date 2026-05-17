using UnityEngine;

namespace Sherlock.Data
{
    public enum QuestType { CraftItem, FindItems, MergeCount }

    [CreateAssetMenu(fileName = "NewQuest", menuName = "Sherlock/Quest Data", order = 2)]
    public class QuestData : ScriptableObject
    {
        [Header("Identity")]
        public string questId;
        public string title;
        [TextArea] public string narrative;
        public Sprite chapterArt;

        [Header("Objective")]
        public QuestType questType;
        public string targetItemId;    // for CraftItem: the itemId that must exist on board
        public int targetCount;        // for FindItems / MergeCount

        [Header("Rewards")]
        public string unlocksSceneId;  // HO scene that becomes available on completion
        public string unlocksQuestId;  // next quest in the chain
        public int rewardCoins;
        public ItemData rewardItem;    // optional starter item placed on board
    }
}
