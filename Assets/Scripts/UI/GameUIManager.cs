using Sherlock.Data;
using Sherlock.Quest;
using UnityEngine;
using UnityEngine.UI;

namespace Sherlock.UI
{
    /// <summary>
    /// Central UI manager. Switches between Investigation (HO) and Analysis (Merge)
    /// views, and surfaces quest / item-found feedback panels.
    ///
    /// All panel references are assigned in the inspector on the persistent HUD canvas.
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        // ── Panels ────────────────────────────────────────────────────────────
        [Header("Root Panels")]
        [SerializeField] private GameObject investigationPanel;
        [SerializeField] private GameObject analysisPanel;

        [Header("HUD — both views")]
        [SerializeField] private Text   coinLabel;
        [SerializeField] private Button switchToInvestigationBtn;
        [SerializeField] private Button switchToAnalysisBtn;

        [Header("Investigation HUD")]
        [SerializeField] private Text   itemCountLabel;    // "7 / 12 found"
        [SerializeField] private Button hintButton;
        [SerializeField] private Text   hintCountLabel;

        [Header("Item-Found Popup")]
        [SerializeField] private GameObject itemFoundPopup;
        [SerializeField] private Image      itemFoundIcon;
        [SerializeField] private Text       itemFoundName;

        [Header("Scene-Complete Screen")]
        [SerializeField] private GameObject sceneCompleteScreen;
        [SerializeField] private Button     goToMergeBtn;

        [Header("Quest Panels")]
        [SerializeField] private GameObject questBannerPanel;
        [SerializeField] private Text       questBannerTitle;
        [SerializeField] private Text       questBannerDesc;

        [SerializeField] private GameObject questCompletePanel;
        [SerializeField] private Text       questCompleteTitle;
        [SerializeField] private Button     questCompleteContinueBtn;

        // ── State ─────────────────────────────────────────────────────────────
        public enum GameView { Investigation, Analysis }
        public GameView CurrentView { get; private set; }

        // ═════════════════════════════════════════════════════════════════════
        // Lifecycle
        // ═════════════════════════════════════════════════════════════════════

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            SwitchToView(GameView.Investigation);

            switchToInvestigationBtn?.onClick.AddListener(() => SwitchToView(GameView.Investigation));
            switchToAnalysisBtn?.onClick.AddListener(()    => SwitchToView(GameView.Analysis));
            goToMergeBtn?.onClick.AddListener(()           => SwitchToView(GameView.Analysis));
            questCompleteContinueBtn?.onClick.AddListener(() => questCompletePanel?.SetActive(false));

            // Coin HUD
            Core.GameState.Instance.OnCoinsChanged += UpdateCoinLabel;
            UpdateCoinLabel(Core.GameState.Instance.Coins);
        }

        void OnDestroy()
        {
            if (Core.GameState.Instance != null)
                Core.GameState.Instance.OnCoinsChanged -= UpdateCoinLabel;
        }

        // ═════════════════════════════════════════════════════════════════════
        // View switching
        // ═════════════════════════════════════════════════════════════════════

        public void SwitchToView(GameView view)
        {
            CurrentView = view;
            investigationPanel?.SetActive(view == GameView.Investigation);
            analysisPanel?.SetActive(view == GameView.Analysis);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Called by HiddenObjectController
        // ═════════════════════════════════════════════════════════════════════

        public void UpdateItemCount(int found, int total)
        {
            if (itemCountLabel) itemCountLabel.text = $"{found} / {total}";
        }

        public void ShowItemFoundPopup(ItemData data)
        {
            if (itemFoundPopup == null || data == null) return;
            itemFoundIcon.sprite = data.icon;
            itemFoundName.text   = data.displayName;
            itemFoundPopup.SetActive(true);
            CancelInvoke(nameof(HideItemFoundPopup));
            Invoke(nameof(HideItemFoundPopup), 2f);
        }

        void HideItemFoundPopup() => itemFoundPopup?.SetActive(false);

        public void ShowSceneCompleteScreen(string sceneId)
        {
            sceneCompleteScreen?.SetActive(true);
        }

        public void UpdateHintUI(int currentHints, float rechargeProgress)
        {
            if (hintCountLabel) hintCountLabel.text = currentHints.ToString();
            // Extend: drive a radial fill image with rechargeProgress
        }

        // ═════════════════════════════════════════════════════════════════════
        // Called by QuestManager
        // ═════════════════════════════════════════════════════════════════════

        public void ShowQuestBanner(QuestData quest)
        {
            if (questBannerPanel == null) return;
            questBannerTitle.text = quest.title;
            questBannerDesc.text  = quest.narrative;
            questBannerPanel.SetActive(true);
            CancelInvoke(nameof(HideQuestBanner));
            Invoke(nameof(HideQuestBanner), 4f);
        }

        void HideQuestBanner() => questBannerPanel?.SetActive(false);

        public void ShowQuestCompleteScreen(QuestData quest)
        {
            if (questCompletePanel == null) return;
            questCompleteTitle.text = $"Case Solved: {quest.title}";
            questCompletePanel.SetActive(true);
        }

        // ═════════════════════════════════════════════════════════════════════
        // HUD updates
        // ═════════════════════════════════════════════════════════════════════

        void UpdateCoinLabel(int coins)
        {
            if (coinLabel) coinLabel.text = coins.ToString("N0");
        }
    }
}
