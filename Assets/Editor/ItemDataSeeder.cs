#if UNITY_EDITOR
using Sherlock.Data;
using UnityEditor;
using UnityEngine;

namespace Sherlock.Editor
{
    /// <summary>
    /// Editor utility that creates all ItemData and QuestData ScriptableObjects
    /// for Chapter 1 — Baker Street.
    ///
    /// Menu: Sherlock > Seed Chapter 1 Data
    ///
    /// Run once on a fresh project. Assets land in:
    ///   Assets/Resources/Items/          ← ItemData (auto-discovered by ItemDatabase)
    ///   Assets/Resources/Quests/         ← QuestData (loaded by QuestManager)
    /// </summary>
    public static class ItemDataSeeder
    {
        private const string ItemsPath  = "Assets/Resources/Items";
        private const string QuestsPath = "Assets/Resources/Quests";

        [MenuItem("Sherlock/Seed Chapter 1 Data")]
        public static void SeedChapter1()
        {
            EnsureFolder(ItemsPath);
            EnsureFolder(QuestsPath);

            // ── Merge Chain 1: Letter → Forensics Kit ─────────────────────────
            var fragment   = CreateItem("letter_fragment",   "Briefschnipsel",          1, null);
            var letter     = CreateItem("sealed_letter",     "Versiegelter Brief",       2, fragment);
            var encDoc     = CreateItem("encrypted_doc",     "Verschlüsseltes Dokument", 3, letter);
            var decoded    = CreateItem("decoded_message",   "Entschlüsselte Nachricht", 4, encDoc);
            var forensics  = CreateItem("forensics_kit",     "Forensik-Kit",             5, decoded,
                                        isQuestReward: true, questRewardId: "q01_forensics_kit");

            // Wire merge results
            SetMergeResult(fragment,  letter);
            SetMergeResult(letter,    encDoc);
            SetMergeResult(encDoc,    decoded);
            SetMergeResult(decoded,   forensics);
            // forensics is max tier — mergeResult stays null

            // ── Merge Chain 2: Tobacco → Pipe ─────────────────────────────────
            var tobacco    = CreateItem("tobacco_ash",       "Tabakaschen",              1, null,
                                        foundInScene: true, sourceSceneId: "library_01");
            var pouch      = CreateItem("tobacco_pouch",     "Tabakbeutel",              2, tobacco);
            var pipe       = CreateItem("sherlock_pipe",     "Sherlocks Pfeife",         3, pouch,
                                        isQuestReward: true, questRewardId: "q02_pipe");

            SetMergeResult(tobacco, pouch);
            SetMergeResult(pouch,   pipe);

            // ── Merge Chain 3: Footprint → Watson's Report ────────────────────
            var footprint  = CreateItem("muddy_footprint",  "Schlammiger Abdruck",      1, null,
                                        foundInScene: true, sourceSceneId: "crime_scene_01");
            var cast       = CreateItem("plaster_cast",     "Gipsabguss",               2, footprint);
            var report     = CreateItem("watson_report",    "Watson's Bericht",         3, cast,
                                        isQuestReward: true, questRewardId: "q03_report");

            SetMergeResult(footprint, cast);
            SetMergeResult(cast,      report);

            // ── Starter items (placed on board at game start) ─────────────────
            CreateItem("magnifying_glass", "Lupe",     1, null, sellValue: 5);
            CreateItem("notebook",         "Notizbuch", 1, null, sellValue: 5);

            // ── Quests ─────────────────────────────────────────────────────────
            CreateQuest(
                id:              "q01_letter_fragment",
                title:           "Kapitel 1: Der Verschlüsselte Brief",
                narrative:       "Watson, dieser Briefschnipsel ist kein Zufall. " +
                                 "Kombiniere die Fragmente bis du die Wahrheit enthüllst.",
                type:            QuestType.CraftItem,
                targetItemId:    "forensics_kit",
                targetCount:     1,
                unlocksScene:    "HO_Library_02",
                unlocksQuest:    "q02_tobacco_pipe",
                rewardCoins:     300
            );

            CreateQuest(
                id:              "q02_tobacco_pipe",
                title:           "Kapitel 2: Die Rauchende Kanone",
                narrative:       "Der Tabakgeruch am Tatort ist unverkennbar. " +
                                 "Rekonstruiere Sherlocks Pfeife, um den Schuldigen zu überführen.",
                type:            QuestType.CraftItem,
                targetItemId:    "sherlock_pipe",
                targetCount:     1,
                unlocksScene:    "HO_CrimeScene_01",
                unlocksQuest:    "q03_watson_report",
                rewardCoins:     400
            );

            CreateQuest(
                id:              "q03_watson_report",
                title:           "Kapitel 3: Watsons Zeugenaussage",
                narrative:       "Die Fußabdrücke am Tatort erzählen eine Geschichte. " +
                                 "Erstelle den vollständigen Bericht, um das Muster zu erkennen.",
                type:            QuestType.CraftItem,
                targetItemId:    "watson_report",
                targetCount:     1,
                unlocksScene:    "HO_Library_03",
                unlocksQuest:    "",          // end of current content
                rewardCoins:     500
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ItemDataSeeder] Chapter 1 data seeded successfully.");
            EditorUtility.DisplayDialog("Seeder", "Chapter 1 data created!\n\nCheck:\n" +
                "Assets/Resources/Items/\nAssets/Resources/Quests/", "OK");
        }

        // ═════════════════════════════════════════════════════════════════════
        // Helpers
        // ═════════════════════════════════════════════════════════════════════

        static ItemData CreateItem(
            string id, string displayName, int tier, ItemData placeholder,
            int sellValue = 10,
            bool foundInScene = false, string sourceSceneId = "",
            bool isQuestReward = false, string questRewardId = "")
        {
            var path = $"{ItemsPath}/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (existing != null) return existing;   // do not overwrite

            var asset           = ScriptableObject.CreateInstance<ItemData>();
            asset.itemId        = id;
            asset.displayName   = displayName;
            asset.tier          = tier;
            asset.sellValue     = sellValue;
            asset.foundInScene  = foundInScene;
            asset.sourceSceneId = sourceSceneId;
            asset.isQuestReward = isQuestReward;
            asset.questRewardId = questRewardId;
            // icon left null — assign manually in inspector

            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static void SetMergeResult(ItemData item, ItemData result)
        {
            if (item == null) return;
            item.mergeResult = result;
            EditorUtility.SetDirty(item);
        }

        static void CreateQuest(
            string id, string title, string narrative,
            QuestType type, string targetItemId, int targetCount,
            string unlocksScene, string unlocksQuest, int rewardCoins)
        {
            var path = $"{QuestsPath}/{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<QuestData>(path) != null) return;

            var asset              = ScriptableObject.CreateInstance<QuestData>();
            asset.questId          = id;
            asset.title            = title;
            asset.narrative        = narrative;
            asset.questType        = type;
            asset.targetItemId     = targetItemId;
            asset.targetCount      = targetCount;
            asset.unlocksSceneId   = unlocksScene;
            asset.unlocksQuestId   = unlocksQuest;
            asset.rewardCoins      = rewardCoins;

            AssetDatabase.CreateAsset(asset, path);
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts  = path.Split('/');
                var parent = string.Join("/", parts[..^1]);
                AssetDatabase.CreateFolder(parent, parts[^1]);
            }
        }
    }
}
#endif
