using System;
using Sherlock.Core;
using UnityEngine;

namespace Sherlock.Ads
{
    /// <summary>
    /// AdsManager — verwaltet Werbung (Rewarded Ads + Interstitials).
    ///
    /// Unterstützte Netzwerke (wähle eines):
    ///   A) Unity Ads (einfachste Integration, kostenlos)
    ///      → Package Manager: com.unity.ads
    ///   B) Google AdMob (höhere eCPM, komplexer)
    ///      → Google Mobile Ads Unity Plugin von admob.google.com
    ///
    /// Aktuell: STUB — alle Methoden funktionieren, zeigen aber keine echte Werbung.
    /// Um Unity Ads zu aktivieren: UNITY_ADS Symbol in Player Settings > Scripting
    /// Define Symbols eintragen und das Paket installieren.
    ///
    /// Rewarded Ad Belohnungen:
    ///   • Doppelte Tagesbelohnung
    ///   • Extra Hinweis in HO-Szene (anstatt Münzen ausgeben)
    ///   • Board-Erweiterung für 30 Minuten
    /// </summary>
    public class AdsManager : MonoBehaviour
    {
        public static AdsManager Instance { get; private set; }

        [Header("Unity Ads Konfiguration")]
        [SerializeField] private string androidGameId = "YOUR_ANDROID_GAME_ID";
        [SerializeField] private string iosGameId     = "YOUR_IOS_GAME_ID";
        [SerializeField] private bool   testMode      = true;   // IMMER true während Entwicklung!

        [Header("Ad Unit IDs")]
        [SerializeField] private string rewardedAdId     = "Rewarded_Android";
        [SerializeField] private string interstitialAdId = "Interstitial_Android";
        [SerializeField] private string bannerAdId       = "Banner_Android";

        // Cooldown: kein Interstitial öfter als alle X Minuten
        [SerializeField] private float interstitialCooldownMinutes = 5f;

        private float _lastInterstitialTime = -999f;
        private bool  _isInitialised;

        public bool IsAdFree => Meta.IAPManager.Instance?.IsAdFree ?? false;

        // Events
        public event Action          OnRewardedAdCompleted;
        public event Action<string>  OnRewardedAdFailed;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start() => InitialiseAds();

        // ── Initialisierung ───────────────────────────────────────────────────

        void InitialiseAds()
        {
#if UNITY_ADS
            string gameId = Application.platform == RuntimePlatform.IPhonePlayer
                            ? iosGameId : androidGameId;

            if (!Advertisement.isInitialized && Advertisement.isSupported)
            {
                Advertisement.Initialize(gameId, testMode, this);
            }
#else
            _isInitialised = true;
            Debug.Log("[AdsManager] Stub aktiv. Unity Ads Paket nicht installiert.");
#endif
        }

        // ── Rewarded Ad ───────────────────────────────────────────────────────

        /// <summary>
        /// Zeigt einen Rewarded Ad.
        /// onReward wird aufgerufen wenn der Spieler das Video vollständig angeschaut hat.
        /// </summary>
        public void ShowRewardedAd(Action onReward, string placement = "extra_hint")
        {
            if (IsAdFree) { onReward?.Invoke(); return; }

#if UNITY_ADS
            if (!_isInitialised || !Advertisement.IsReady(rewardedAdId))
            {
                Debug.LogWarning("[AdsManager] Rewarded Ad nicht bereit.");
                OnRewardedAdFailed?.Invoke("not_ready");
                return;
            }
            var options = new ShowOptions { resultCallback = result => HandleRewardedResult(result, onReward) };
            Advertisement.Show(rewardedAdId, options);
#else
            // Stub: sofort belohnen (Editor/Entwicklung)
            Debug.Log($"[AdsManager] Stub – Rewarded Ad für '{placement}' simuliert.");
            Analytics.AnalyticsManager.Instance?.LogEvent("ad_rewarded_shown", ("placement", placement));
            onReward?.Invoke();
            OnRewardedAdCompleted?.Invoke();
#endif
        }

        // ── Interstitial Ad ───────────────────────────────────────────────────

        /// <summary>
        /// Zeigt einen Interstitial Ad zwischen Szenen (mit Cooldown).
        /// Nie aufrufen mitten im Gameplay — nur bei natürlichen Pausen.
        /// </summary>
        public void ShowInterstitialIfReady()
        {
            if (IsAdFree) return;

            float minutesSinceLast = (Time.realtimeSinceStartup - _lastInterstitialTime) / 60f;
            if (minutesSinceLast < interstitialCooldownMinutes) return;

#if UNITY_ADS
            if (!Advertisement.IsReady(interstitialAdId)) return;
            Advertisement.Show(interstitialAdId);
#else
            Debug.Log("[AdsManager] Stub – Interstitial Ad simuliert.");
            Analytics.AnalyticsManager.Instance?.LogEvent("ad_interstitial_shown");
#endif
            _lastInterstitialTime = Time.realtimeSinceStartup;
        }

        // ── Spezifische Belohnungen ────────────────────────────────────────────

        /// <summary>Spieler schaut Werbung → bekommt Extra-Hinweis gratis.</summary>
        public void WatchAdForHint(HiddenObject.HintSystem hintSystem)
        {
            ShowRewardedAd(() =>
            {
                hintSystem.UseHint();
                Analytics.AnalyticsManager.Instance?.LogEvent("ad_reward_hint");
            }, "hint");
        }

        /// <summary>Spieler schaut Werbung → bekommt 50 Münzen.</summary>
        public void WatchAdForCoins(int coinAmount = 50)
        {
            ShowRewardedAd(() =>
            {
                GameState.Instance.AddCoins(coinAmount);
                SaveSystem.Save();
                Analytics.AnalyticsManager.Instance?.LogEvent("ad_reward_coins",
                    ("amount", coinAmount.ToString()));
            }, "coins");
        }

        /// <summary>Spieler schaut Werbung → Tagesbelohnung verdoppeln.</summary>
        public void WatchAdToDoubleReward(int baseCoins)
        {
            ShowRewardedAd(() =>
            {
                GameState.Instance.AddCoins(baseCoins); // nochmals den gleichen Betrag
                Analytics.AnalyticsManager.Instance?.LogEvent("ad_reward_double_daily");
            }, "double_daily");
        }

        // ── Unity Ads Callbacks (nur mit UNITY_ADS aktiv) ─────────────────────

#if UNITY_ADS
        void HandleRewardedResult(ShowResult result, Action onReward)
        {
            switch (result)
            {
                case ShowResult.Finished:
                    onReward?.Invoke();
                    OnRewardedAdCompleted?.Invoke();
                    Analytics.AnalyticsManager.Instance?.LogEvent("ad_rewarded_completed");
                    break;
                case ShowResult.Skipped:
                    Debug.Log("[AdsManager] Rewarded Ad übersprungen.");
                    OnRewardedAdFailed?.Invoke("skipped");
                    break;
                case ShowResult.Failed:
                    Debug.LogWarning("[AdsManager] Rewarded Ad fehlgeschlagen.");
                    OnRewardedAdFailed?.Invoke("failed");
                    break;
            }
        }
#endif
    }
}
