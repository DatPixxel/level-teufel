using System.Collections;
using Sherlock.Ads;
using Sherlock.Analytics;
using Sherlock.Meta;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sherlock.Core
{
    /// <summary>
    /// Entry point of the application. Lives in "Bootstrap" scene (Build Index 0).
    /// Initialises every persistent singleton and then loads the correct starting scene.
    ///
    /// Build Settings scene order:
    ///   0 — Bootstrap         (this script lives here)
    ///   1 — MainMenu
    ///   2 — MergeBoard        (permanent Analysis view)
    ///   3 — HO_Library_01     (chapter 1 investigation)
    ///   4 — HO_Library_02     (chapter 2 investigation)
    ///   … additional HO scenes added as chapters grow
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Prefabs — drag persistent singleton prefabs here")]
        [SerializeField] private GameObject gameStatePrefab;
        [SerializeField] private GameObject itemDatabasePrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject iapManagerPrefab;
        [SerializeField] private GameObject leaderboardPrefab;
        [SerializeField] private GameObject backButtonHandlerPrefab;
        [SerializeField] private GameObject adsManagerPrefab;
        [SerializeField] private GameObject analyticsManagerPrefab;
        [SerializeField] private GameObject notificationManagerPrefab;
        [SerializeField] private GameObject dailyRewardPrefab;

        [Header("First scene to load after bootstrap")]
        [SerializeField] private string firstScene = "MainMenu";

        // Expose for other systems that need to track load progress
        public static float LoadProgress { get; private set; }
        public static bool  IsReady      { get; private set; }

        IEnumerator Start()
        {
            IsReady      = false;
            LoadProgress = 0f;

            // ── Instantiate persistent singletons ─────────────────────────────
            SpawnIfMissing<GameState>(gameStatePrefab,          "GameState");
            SpawnIfMissing<Data.ItemDatabase>(itemDatabasePrefab,    "ItemDatabase");
            SpawnIfMissing<AudioManager>(audioManagerPrefab,    "AudioManager");
            SpawnIfMissing<IAPManager>(iapManagerPrefab,        "IAPManager");
            SpawnIfMissing<LeaderboardService>(leaderboardPrefab,    "LeaderboardService");
            SpawnIfMissing<BackButtonHandler>(backButtonHandlerPrefab,     "BackButtonHandler");
            SpawnIfMissing<AdsManager>(adsManagerPrefab,                  "AdsManager");
            SpawnIfMissing<AnalyticsManager>(analyticsManagerPrefab,      "AnalyticsManager");
            SpawnIfMissing<LocalNotificationManager>(notificationManagerPrefab, "NotificationManager");
            SpawnIfMissing<DailyRewardSystem>(dailyRewardPrefab,          "DailyRewardSystem");

            LoadProgress = 0.3f;
            yield return null;   // let Awake() calls complete

            // ── Load save data ────────────────────────────────────────────────
            bool hasSave = SaveSystem.Load();
            Debug.Log(hasSave ? "[Bootstrapper] Save loaded." : "[Bootstrapper] No save — fresh start.");

            LoadProgress = 0.6f;
            yield return null;

            // ── Platform setup ────────────────────────────────────────────────
            MobileSetup.Apply();

            LoadProgress = 0.9f;
            yield return null;

            // ── Transition to first real scene ────────────────────────────────
            IsReady      = true;
            LoadProgress = 1f;

            var op = SceneManager.LoadSceneAsync(firstScene, LoadSceneMode.Single);
            while (op != null && !op.isDone) yield return null;
        }

        static void SpawnIfMissing<T>(GameObject prefab, string fallbackName) where T : MonoBehaviour
        {
            if (FindObjectOfType<T>() != null) return; // already exists (domain reload)

            GameObject go;
            if (prefab != null)
            {
                go = Instantiate(prefab);
            }
            else
            {
                go = new GameObject(fallbackName);
                go.AddComponent<T>();
            }
            DontDestroyOnLoad(go);
        }
    }
}
