using System;
using System.Collections.Generic;
using Sherlock.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Sherlock.Core
{
    /// <summary>
    /// DailyRewardSystem — belohnt Spieler täglich für das Einloggen.
    ///
    /// Mechanik:
    ///   • 7-Tage-Kalender: Tag 1–6 geben Münzen, Tag 7 gibt ein Item + Münzen
    ///   • Aufeinanderfolgende Tage: Counter steigt
    ///   • Einen Tag verpasst: Counter resettet auf Tag 1
    ///   • Wird beim App-Start vom GameSessionManager geprüft
    ///
    /// Verwendung:
    ///   DailyRewardSystem.Instance.CheckAndShow();
    /// </summary>
    public class DailyRewardSystem : MonoBehaviour
    {
        public static DailyRewardSystem Instance { get; private set; }

        // ── Belohnungstabelle ─────────────────────────────────────────────────
        [Serializable]
        public class DayReward
        {
            public string label;        // Anzeigename, z.B. "Tag 1"
            public int    coins;
            public string itemId;       // leer = kein Item-Reward
            public Sprite icon;
        }

        [Header("Belohnungen (7 Tage)")]
        [SerializeField] private DayReward[] rewards = new DayReward[]
        {
            new() { label = "Tag 1", coins = 50  },
            new() { label = "Tag 2", coins = 100 },
            new() { label = "Tag 3", coins = 150 },
            new() { label = "Tag 4", coins = 200 },
            new() { label = "Tag 5", coins = 250, itemId = "tobacco_ash"     },
            new() { label = "Tag 6", coins = 300, itemId = "muddy_footprint" },
            new() { label = "Tag 7", coins = 500, itemId = "letter_fragment" },
        };

        [Header("UI")]
        [SerializeField] private GameObject  panelRoot;
        [SerializeField] private Transform   dayContainer;    // 7 Kinder-Objekte
        [SerializeField] private Text        titleText;
        [SerializeField] private Text        rewardCoinsText;
        [SerializeField] private Text        rewardItemText;
        [SerializeField] private Button      claimButton;
        [SerializeField] private GameObject  alreadyClaimedNote;

        // PlayerPrefs Schlüssel
        private const string PrefLastClaim  = "daily_last_claim";
        private const string PrefStreak     = "daily_streak";

        private int _currentDay;    // 0-basiert (0–6)
        private bool _canClaim;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            claimButton?.onClick.AddListener(ClaimReward);
            panelRoot?.SetActive(false);
        }

        // ── Öffentliche API ───────────────────────────────────────────────────

        /// <summary>Prüft ob heute schon geclaimed wurde und zeigt das Panel wenn nicht.</summary>
        public void CheckAndShow()
        {
            _currentDay = CalculateCurrentDay();
            _canClaim   = CanClaimToday();

            if (!_canClaim && !ShouldShowAnyway()) return;

            ShowPanel();
        }

        // ── Berechnung ────────────────────────────────────────────────────────

        int CalculateCurrentDay()
        {
            var lastClaimStr = PlayerPrefs.GetString(PrefLastClaim, "");
            int streak       = PlayerPrefs.GetInt(PrefStreak, 0);

            if (string.IsNullOrEmpty(lastClaimStr))
                return 0; // erster Besuch

            if (!DateTime.TryParse(lastClaimStr, out var lastClaim))
                return 0;

            var daysSinceLast = (DateTime.UtcNow.Date - lastClaim.Date).Days;

            if (daysSinceLast == 0)
                return streak % rewards.Length;   // heute schon geclaimed
            if (daysSinceLast == 1)
                return (streak + 1) % rewards.Length;   // aufeinanderfolgend
            // Mehr als 1 Tag verpasst → Reset
            return 0;
        }

        bool CanClaimToday()
        {
            var lastClaimStr = PlayerPrefs.GetString(PrefLastClaim, "");
            if (string.IsNullOrEmpty(lastClaimStr)) return true;
            if (!DateTime.TryParse(lastClaimStr, out var lastClaim)) return true;
            return lastClaim.Date < DateTime.UtcNow.Date;
        }

        // Zeige Panel auch wenn schon geclaimed (read-only Ansicht)
        bool ShouldShowAnyway() => false;   // auf true setzen für read-only Anzeige

        // ── UI ────────────────────────────────────────────────────────────────

        void ShowPanel()
        {
            panelRoot?.SetActive(true);

            var reward = rewards[_currentDay];

            if (titleText)       titleText.text       = _canClaim ? "Tägliche Belohnung!" : "Bereits eingesammelt";
            if (rewardCoinsText) rewardCoinsText.text  = $"+{reward.coins} Münzen";
            if (rewardItemText)  rewardItemText.text   = string.IsNullOrEmpty(reward.itemId)
                                                             ? ""
                                                             : $"+1 {reward.itemId}";

            claimButton?.gameObject.SetActive(_canClaim);
            alreadyClaimedNote?.SetActive(!_canClaim);

            // Kinder-Tages-Felder aktualisieren
            RefreshDayButtons();

            Analytics.AnalyticsManager.Instance?.LogEvent("daily_reward_shown",
                ("day", _currentDay.ToString()));
        }

        void RefreshDayButtons()
        {
            if (dayContainer == null) return;
            int streak = PlayerPrefs.GetInt(PrefStreak, 0);

            for (int i = 0; i < dayContainer.childCount && i < rewards.Length; i++)
            {
                var child = dayContainer.GetChild(i);
                var img   = child.GetComponent<Image>();
                var lbl   = child.GetComponentInChildren<Text>();

                if (lbl) lbl.text = rewards[i].label + $"\n{rewards[i].coins}🪙";

                // Farbkodierung
                if (img)
                {
                    img.color = i < streak       ? new Color(0.4f, 0.7f, 0.4f) // abgeholt
                              : i == _currentDay ? new Color(1.0f, 0.9f, 0.3f) // heute
                                                 : new Color(0.3f, 0.3f, 0.3f); // zukünftig
                }
            }
        }

        // ── Claim ─────────────────────────────────────────────────────────────

        void ClaimReward()
        {
            if (!_canClaim) return;

            var reward = rewards[_currentDay];
            var gs     = GameState.Instance;

            gs.AddCoins(reward.coins);

            if (!string.IsNullOrEmpty(reward.itemId))
                gs.AddToPendingInventory(reward.itemId);

            // Streak & Datum speichern
            int newStreak = _currentDay + 1;
            PlayerPrefs.SetInt(PrefStreak, newStreak);
            PlayerPrefs.SetString(PrefLastClaim, DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();

            SaveSystem.Save();

            Analytics.AnalyticsManager.Instance?.LogEvent("daily_reward_claimed",
                ("day", _currentDay.ToString()),
                ("coins", reward.coins.ToString()));

            // Panel schließen nach kurzem Delay
            _canClaim = false;
            claimButton?.gameObject.SetActive(false);
            alreadyClaimedNote?.SetActive(true);
            Invoke(nameof(HidePanel), 1.5f);
        }

        void HidePanel() => panelRoot?.SetActive(false);
    }
}
