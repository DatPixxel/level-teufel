using System.Collections.Generic;
using UnityEngine;

namespace Sherlock.Analytics
{
    /// <summary>
    /// AnalyticsManager — trackt Spielerverhalten für Optimierung und Monetarisierung.
    ///
    /// Unterstützte Backends (wähle eines):
    ///   A) Unity Analytics (kostenlos, einfach)
    ///      → bereits in Unity enthalten, im Dashboard aktivieren
    ///   B) Firebase Analytics (kostenlos, mächtiger)
    ///      → Firebase Unity SDK von firebase.google.com
    ///   C) GameAnalytics (kostenlos, speziell für Games)
    ///      → gameanalytics.com
    ///
    /// Aktuell: STUB — Events werden nur in der Console geloggt.
    /// Zum Aktivieren: Unity Analytics im Unity Dashboard aktivieren und
    /// UNITY_ANALYTICS in Scripting Define Symbols eintragen.
    ///
    /// Wichtige Events die getrackt werden:
    ///   • session_start / session_end
    ///   • tutorial_completed
    ///   • merge_completed (welches Item, welche Stufe)
    ///   • quest_completed
    ///   • daily_reward_claimed
    ///   • ad_rewarded_completed
    ///   • iap_purchase_success
    ///   • board_full (Frustrations-Indikator)
    ///   • scene_completed (HO-Szene)
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        [SerializeField] private bool verboseLogging = true;

        private float   _sessionStart;
        private int     _mergesThisSession;
        private int     _itemsFoundThisSession;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            _sessionStart = Time.realtimeSinceStartup;
            LogEvent("session_start");
            SubscribeToGameEvents();
        }

        void OnApplicationQuit()
        {
            float duration = Time.realtimeSinceStartup - _sessionStart;
            LogEvent("session_end",
                ("duration_seconds", Mathf.RoundToInt(duration).ToString()),
                ("merges",           _mergesThisSession.ToString()),
                ("items_found",      _itemsFoundThisSession.ToString()));
        }

        void OnApplicationPause(bool pausing)
        {
            if (pausing) LogEvent("app_paused");
        }

        // ── Event-Subscriptions ───────────────────────────────────────────────

        void SubscribeToGameEvents()
        {
            // MergeManager Events
            if (Merge.MergeManager.Instance != null)
            {
                Merge.MergeManager.Instance.OnMergeCompleted += data =>
                {
                    _mergesThisSession++;
                    LogEvent("merge_completed",
                        ("item_id", data.itemId),
                        ("tier",    data.tier.ToString()));
                };

                Merge.MergeManager.Instance.OnBoardFull += () =>
                    LogEvent("board_full");

                Merge.MergeManager.Instance.OnItemSold += data =>
                    LogEvent("item_sold",
                        ("item_id",    data.itemId),
                        ("sell_value", data.sellValue.ToString()));
            }

            // QuestManager Events
            if (Quest.QuestManager.Instance != null)
            {
                Quest.QuestManager.Instance.OnQuestCompleted += quest =>
                    LogEvent("quest_completed", ("quest_id", quest.questId));
            }
        }

        // ── Öffentliche API ───────────────────────────────────────────────────

        /// <summary>Sendet ein Custom Event. Bis zu 10 Key-Value-Paare erlaubt.</summary>
        public void LogEvent(string eventName, params (string key, string value)[] parameters)
        {
#if UNITY_ANALYTICS
            var dict = new Dictionary<string, object>();
            foreach (var (key, value) in parameters) dict[key] = value;
            Unity.Services.Analytics.AnalyticsService.Instance.CustomData(eventName, dict);
#endif

#if FIREBASE_ANALYTICS
            var firebaseParams = new List<Firebase.Analytics.Parameter>();
            foreach (var (key, value) in parameters)
                firebaseParams.Add(new Firebase.Analytics.Parameter(key, value));
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
#endif
            if (verboseLogging)
            {
                var sb = new System.Text.StringBuilder($"[Analytics] {eventName}");
                foreach (var (key, value) in parameters) sb.Append($" | {key}={value}");
                Debug.Log(sb.ToString());
            }
        }

        // ── Convenience-Methoden ──────────────────────────────────────────────

        public void LogItemFound(string sceneId, string itemId)
        {
            _itemsFoundThisSession++;
            LogEvent("item_found", ("scene_id", sceneId), ("item_id", itemId));
        }

        public void LogSceneComplete(string sceneId, float durationSeconds)
        {
            LogEvent("ho_scene_completed",
                ("scene_id", sceneId),
                ("duration", Mathf.RoundToInt(durationSeconds).ToString()));
        }

        public void LogIAPSuccess(string productId, string localizedPrice)
        {
            LogEvent("iap_purchase_success",
                ("product_id",     productId),
                ("localized_price", localizedPrice));
        }

        public void LogChapterUnlocked(string chapterId)
        {
            LogEvent("chapter_unlocked", ("chapter_id", chapterId));
        }
    }
}
