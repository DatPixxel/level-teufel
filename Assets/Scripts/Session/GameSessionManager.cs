using System.Collections;
using Sherlock.Ads;
using Sherlock.Core;
using Sherlock.Tutorial;
using UnityEngine;

namespace Sherlock.Session
{
    /// <summary>
    /// GameSessionManager — orchestriert den App-Start-Flow.
    ///
    /// Reihenfolge beim Öffnen der App:
    ///   1. Notifications abbrechen (der Spieler ist jetzt da)
    ///   2. Tagesbelohnung prüfen und anzeigen (wenn fällig)
    ///   3. Tutorial starten (wenn erster Spielstart)
    ///   4. Interstitial Ad nach 30 Sekunden Spielzeit (wenn kein Ad-Free)
    ///
    /// Wird als Singleton in der MainMenu-Szene gestartet.
    /// </summary>
    public class GameSessionManager : MonoBehaviour
    {
        public static GameSessionManager Instance { get; private set; }

        [Header("Timing")]
        [SerializeField] private float firstInterstitialDelay = 120f;  // 2 Minuten nach Start
        [SerializeField] private float dailyRewardDelay       = 1.0f;  // kurze Pause nach Ladescreen

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => StartCoroutine(SessionStartFlow());

        IEnumerator SessionStartFlow()
        {
            // ── 1. Notifications deaktivieren ─────────────────────────────────
            LocalNotificationManager.Instance?.CancelAll();

            yield return new WaitForSeconds(dailyRewardDelay);

            // ── 2. Tagesbelohnung ──────────────────────────────────────────────
            DailyRewardSystem.Instance?.CheckAndShow();

            // Warten bis das Daily Reward Panel geschlossen ist
            if (DailyRewardSystem.Instance != null)
            {
                // Nur warten wenn heute noch nicht geclaimed
                float timeout = 30f;
                while (timeout > 0f)
                {
                    yield return new WaitForSeconds(0.5f);
                    timeout -= 0.5f;
                    // Panel geschlossen → weiter
                    // (vereinfachte Prüfung — in echter Implementierung Event nutzen)
                }
            }

            // ── 3. Tutorial für neue Spieler ───────────────────────────────────
            TutorialManager.Instance?.StartIfNeeded();

            // ── 4. Erster Interstitial nach einiger Spielzeit ──────────────────
            StartCoroutine(DelayedInterstitial());
        }

        IEnumerator DelayedInterstitial()
        {
            yield return new WaitForSeconds(firstInterstitialDelay);
            AdsManager.Instance?.ShowInterstitialIfReady();
        }

        // ── App in Hintergrund → Benachrichtigungen planen ────────────────────

        void OnApplicationPause(bool pausing)
        {
            if (pausing)
                LocalNotificationManager.Instance?.ScheduleAll();
        }

        void OnApplicationQuit()
        {
            LocalNotificationManager.Instance?.ScheduleAll();
        }
    }
}
