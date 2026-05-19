using Sherlock.Core;
using Sherlock.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Sherlock.UI
{
    /// <summary>
    /// MainMenu scene controller. Handles New Game / Continue / Settings / Credits.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button continueBtn;
        [SerializeField] private Button newGameBtn;
        [SerializeField] private Button settingsBtn;
        [SerializeField] private Button leaderboardBtn;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider     musicSlider;
        [SerializeField] private Slider     sfxSlider;
        [SerializeField] private Toggle     musicToggle;
        [SerializeField] private Button     restorePurchasesBtn;

        [Header("Leaderboard Panel")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private Transform  leaderboardEntryContainer;
        [SerializeField] private GameObject leaderboardEntryPrefab;  // Text-based row prefab

        void Start()
        {
            bool hasSave = System.IO.File.Exists(
                System.IO.Path.Combine(Application.persistentDataPath, "gamestate.json"));

            if (continueBtn) continueBtn.interactable = hasSave;

            continueBtn?.onClick.AddListener(Continue);
            newGameBtn?.onClick.AddListener(NewGame);
            settingsBtn?.onClick.AddListener(() => settingsPanel?.SetActive(true));
            leaderboardBtn?.onClick.AddListener(OpenLeaderboard);
            restorePurchasesBtn?.onClick.AddListener(() => IAPManager.Instance?.RestorePurchases());

            // Settings defaults
            if (musicSlider) musicSlider.value = PlayerPrefs.GetFloat("pref_music_vol", 0.6f);
            if (sfxSlider)   sfxSlider.value   = PlayerPrefs.GetFloat("pref_sfx_vol", 1f);
            if (musicToggle) musicToggle.isOn   = PlayerPrefs.GetInt("pref_music_on", 1) == 1;

            musicSlider?.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
            sfxSlider?.onValueChanged.AddListener(v   => AudioManager.Instance?.SetSfxVolume(v));
            musicToggle?.onValueChanged.AddListener(on => AudioManager.Instance?.ToggleMusic(on));

            AudioManager.Instance?.PlayMusic("main_menu_theme");
        }

        void Continue()
        {
            // SceneLoader will honour UnlockedScenes from the loaded save
            SceneLoader.Instance?.LoadHOSceneForced("HO_Library_01");
        }

        void NewGame()
        {
            SaveSystem.DeleteSave();
            GameState.Instance.UnlockedScenes.Add("HO_Library_01");
            SceneLoader.Instance?.LoadHOSceneForced("HO_Library_01");
        }

        void OpenLeaderboard()
        {
            leaderboardPanel?.SetActive(true);
            LeaderboardService.Instance.OnFetchComplete += PopulateLeaderboard;
            LeaderboardService.Instance.FetchTopScores(20);
        }

        void PopulateLeaderboard(System.Collections.Generic.List<LeaderboardService.LeaderboardEntry> entries)
        {
            LeaderboardService.Instance.OnFetchComplete -= PopulateLeaderboard;

            if (leaderboardEntryContainer == null) return;
            foreach (Transform child in leaderboardEntryContainer) Destroy(child.gameObject);

            foreach (var e in entries)
            {
                var row = Instantiate(leaderboardEntryPrefab, leaderboardEntryContainer);
                var label = row.GetComponentInChildren<Text>();
                if (label) label.text = $"{e.rank,3}.  {e.displayName,-20}  {e.score:N0}";
            }
        }
    }
}
