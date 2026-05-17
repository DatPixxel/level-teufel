using UnityEngine;

namespace Sherlock.Core
{
    /// <summary>
    /// MobileSetup — plattformspezifische Einstellungen die beim Start gesetzt werden.
    ///
    /// Wird vom Bootstrapper automatisch aufgerufen.
    /// Keine manuelle Verwendung nötig.
    /// </summary>
    public static class MobileSetup
    {
        public static void Apply()
        {
            // ── Alle Plattformen ──────────────────────────────────────────────
            Application.targetFrameRate = 60;
            Screen.sleepTimeout         = SleepTimeout.NeverSleep;
            QualitySettings.vSyncCount  = 0;   // targetFrameRate übernimmt die Steuerung

            // ── Mobile (iOS + Android) ────────────────────────────────────────
#if UNITY_IOS || UNITY_ANDROID
            Input.multiTouchEnabled = true;

            // Portrait-Modus sperren (Merge-Spiele funktionieren hochkant am besten)
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToLandscapeLeft  = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait       = true;
            Screen.autorotateToPortraitUpsideDown = false;
#endif

            // ── Android-spezifisch ────────────────────────────────────────────
#if UNITY_ANDROID
            // Zeigt die Android Statusbar nicht über dem Spiel
            Screen.fullScreen = true;

            // Haptisches Feedback — Kurzes Vibrieren beim Merge
            // (direkt über AndroidJavaClass, kein Plugin nötig)
#endif

            // ── iOS-spezifisch ────────────────────────────────────────────────
#if UNITY_IOS
            // Verhindert dass der Screen während der Spielsitzung dunkel wird
            UnityEngine.iOS.Device.SetNoBackupFlag(
                Application.persistentDataPath, true); // iCloud-Backup ausschließen
#endif

            Debug.Log($"[MobileSetup] Applied. Platform: {Application.platform}, " +
                      $"Screen: {Screen.width}×{Screen.height}, DPI: {Screen.dpi}");
        }

        // ── Haptik ───────────────────────────────────────────────────────────

        /// <summary>Kurze Vibration beim Merge-Erfolg (Android only).</summary>
        public static void VibrateShort()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator    = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                vibrator.Call("vibrate", 40L); // 40ms
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[MobileSetup] Vibration failed: {e.Message}");
            }
#endif
            // iOS: Haptic Feedback via Unity's Handheld.Vibrate() oder iOS-Plugin
#if UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        /// <summary>DPI-angepasste Tap-Toleranz in Pixeln (wichtig für kleine Objekte in HO-Szenen).</summary>
        public static float TapTolerancePx()
        {
            float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
            return dpi * 0.12f; // ~12% eines Zolls ≈ angenehme Tap-Größe
        }
    }
}
