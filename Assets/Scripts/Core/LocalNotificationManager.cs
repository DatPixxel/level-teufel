using System;
using UnityEngine;

namespace Sherlock.Core
{
    /// <summary>
    /// LocalNotificationManager — plant lokale Push-Benachrichtigungen.
    ///
    /// Benötigt: com.unity.mobile.notifications (Unity Package Manager)
    ///   → Window > Package Manager > "Mobile Notifications" suchen > Install
    ///
    /// Benachrichtigungen die geplant werden:
    ///   • Tägliche Erinnerung an Tagesbelohnung (24h)
    ///   • "Sherlocks Hinweise werden aufgefrischt!" (12h)
    ///   • "Ein neuer Fall wartet!" (3 Tage Inaktivität)
    ///
    /// Für Anfänger:
    ///   Dieses Script funktioniert erst wenn das Mobile Notifications Paket
    ///   installiert ist. Bis dahin zeigt es nur Log-Meldungen.
    /// </summary>
    public class LocalNotificationManager : MonoBehaviour
    {
        public static LocalNotificationManager Instance { get; private set; }

        private const string ChannelId   = "sherlock_main";
        private const string ChannelName = "Sherlock Benachrichtigungen";

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start() => Initialise();

        // ── Initialisierung ───────────────────────────────────────────────────

        void Initialise()
        {
#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
            var channel = new Unity.Notifications.Android.AndroidNotificationChannel
            {
                Id          = ChannelId,
                Name        = ChannelName,
                Importance  = Unity.Notifications.Android.Importance.Default,
                Description = "Spielbenachrichtigungen für Sherlock Merge",
            };
            Unity.Notifications.Android.AndroidNotificationCenter.RegisterNotificationChannel(channel);
            Debug.Log("[Notifications] Android channel registered.");
#elif UNITY_IOS && UNITY_MOBILE_NOTIFICATIONS
            Unity.Notifications.iOS.iOSNotificationCenter.RequestAuthorizationAsync(
                Unity.Notifications.iOS.AuthorizationOption.Alert |
                Unity.Notifications.iOS.AuthorizationOption.Badge |
                Unity.Notifications.iOS.AuthorizationOption.Sound);
            Debug.Log("[Notifications] iOS authorization requested.");
#else
            Debug.Log("[Notifications] Mobile Notifications Paket nicht installiert — Stub aktiv.");
#endif
        }

        // ── Öffentliche API ───────────────────────────────────────────────────

        /// <summary>
        /// Planlt alle Standard-Benachrichtigungen.
        /// Aufrufen wenn die App in den Hintergrund geht (OnApplicationPause).
        /// </summary>
        public void ScheduleAll()
        {
            CancelAll();

            // Tagesbelohnung — morgen um 10 Uhr
            var tomorrow10 = DateTime.Now.Date.AddDays(1).AddHours(10);
            Schedule(
                id:       100,
                title:    "Deine tägliche Belohnung wartet!",
                body:     "Sherlock braucht deine Hilfe. Hol dir deine Münzen ab!",
                fireAt:   tomorrow10
            );

            // Hinweis-Auffrischung — in 12 Stunden
            Schedule(
                id:       101,
                title:    "Sherlocks Hinweise sind aufgefrischt",
                body:     "Du hast wieder 3 Hinweise für deine Ermittlung.",
                fireAt:   DateTime.Now.AddHours(12)
            );

            // Inaktivitäts-Erinnerung — in 3 Tagen
            Schedule(
                id:       102,
                title:    "Ein neuer Fall wartet, Detektiv!",
                body:     "Das Rätsel löst sich nicht von selbst. Sherlock zählt auf dich!",
                fireAt:   DateTime.Now.AddDays(3)
            );
        }

        /// <summary>Alle geplanten Benachrichtigungen abbrechen (beim App-Start).</summary>
        public void CancelAll()
        {
#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
            Unity.Notifications.Android.AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS && UNITY_MOBILE_NOTIFICATIONS
            Unity.Notifications.iOS.iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        }

        // ── Interne Planung ───────────────────────────────────────────────────

        void Schedule(int id, string title, string body, DateTime fireAt)
        {
            if (fireAt <= DateTime.Now) return;

#if UNITY_ANDROID && UNITY_MOBILE_NOTIFICATIONS
            var notification = new Unity.Notifications.Android.AndroidNotification
            {
                Title        = title,
                Text         = body,
                FireTime     = fireAt,
                SmallIcon    = "icon_0",
                LargeIcon    = "icon_1",
            };
            Unity.Notifications.Android.AndroidNotificationCenter
                .SendNotificationWithExplicitID(notification, ChannelId, id);
#elif UNITY_IOS && UNITY_MOBILE_NOTIFICATIONS
            var trigger = new Unity.Notifications.iOS.iOSNotificationCalendarTrigger
            {
                Year   = fireAt.Year,  Month  = fireAt.Month,  Day    = fireAt.Day,
                Hour   = fireAt.Hour,  Minute = fireAt.Minute, Second = fireAt.Second,
                Repeats = false,
            };
            var notification = new Unity.Notifications.iOS.iOSNotification
            {
                Identifier             = id.ToString(),
                Title                  = title,
                Body                   = body,
                ShowInForeground       = false,
                Trigger                = trigger,
            };
            Unity.Notifications.iOS.iOSNotificationCenter.ScheduleNotification(notification);
#else
            Debug.Log($"[Notifications] Stub – würde planen: '{title}' um {fireAt:HH:mm dd.MM.yyyy}");
#endif
        }

        // ── App-Lebenszyklus ──────────────────────────────────────────────────

        void OnApplicationPause(bool pausing)
        {
            if (pausing)
                ScheduleAll();  // Benachrichtigungen planen wenn App in Hintergrund geht
            else
                CancelAll();    // Alle abbrechen wenn App wieder geöffnet wird
        }

        void OnApplicationQuit() => ScheduleAll();
    }
}
