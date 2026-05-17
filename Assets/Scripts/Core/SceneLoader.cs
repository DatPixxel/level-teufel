using System.Collections;
using System.Collections.Generic;
using Sherlock.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sherlock.Core
{
    /// <summary>
    /// SceneLoader — manages additive scene loading and the transition between
    /// the persistent MergeBoard scene and individual Hidden Object scenes.
    ///
    /// Scene strategy:
    ///   - "MergeBoard" is always additively loaded and never unloaded.
    ///   - One HO scene at a time is additively loaded on top.
    ///   - A loading screen (optional) is shown between transitions.
    ///
    /// Usage:
    ///   SceneLoader.Instance.LoadHOScene("HO_Library_01");
    ///   SceneLoader.Instance.ShowMergeBoard();
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mergeBoardScene = "MergeBoard";

        [Header("Loading Screen (optional)")]
        [SerializeField] private GameObject loadingScreenRoot;
        [SerializeField] private Slider     loadingBar;
        [SerializeField] private float      minimumLoadTime = 0.5f;

        // Track the currently loaded HO scene so we can unload it
        private string    _currentHOScene;
        private bool      _mergeBoardLoaded;

        // ═════════════════════════════════════════════════════════════════════
        // Lifecycle
        // ═════════════════════════════════════════════════════════════════════

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // Ensure the merge board is always present
            if (!SceneManager.GetSceneByName(mergeBoardScene).isLoaded)
                StartCoroutine(LoadSceneAdditive(mergeBoardScene, onComplete: () => _mergeBoardLoaded = true));
            else
                _mergeBoardLoaded = true;
        }

        // ═════════════════════════════════════════════════════════════════════
        // Public API
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>Load a Hidden Object scene. Unloads the previous HO scene if one is active.</summary>
        public void LoadHOScene(string sceneName)
        {
            if (!GameState.Instance.UnlockedScenes.Contains(sceneName))
            {
                Debug.LogWarning($"[SceneLoader] Scene '{sceneName}' is not unlocked yet.");
                return;
            }
            StartCoroutine(TransitionToHOScene(sceneName));
        }

        /// <summary>Unload the current HO scene and show the merge board in full view.</summary>
        public void ShowMergeBoard()
        {
            if (string.IsNullOrEmpty(_currentHOScene)) return;
            StartCoroutine(UnloadHOScene(_currentHOScene));
        }

        /// <summary>For the first scene of a fresh save (no unlock check needed).</summary>
        public void LoadHOSceneForced(string sceneName) =>
            StartCoroutine(TransitionToHOScene(sceneName));

        // ═════════════════════════════════════════════════════════════════════
        // Coroutines
        // ═════════════════════════════════════════════════════════════════════

        IEnumerator TransitionToHOScene(string sceneName)
        {
            ShowLoadingScreen(true);

            // Unload previous HO scene if present
            if (!string.IsNullOrEmpty(_currentHOScene))
            {
                var unload = SceneManager.UnloadSceneAsync(_currentHOScene);
                while (unload != null && !unload.isDone)
                {
                    UpdateLoadBar(unload.progress * 0.3f);
                    yield return null;
                }
                _currentHOScene = null;
            }

            yield return LoadSceneAdditive(sceneName, progress =>
                UpdateLoadBar(0.3f + progress * 0.7f));

            _currentHOScene = sceneName;

            // Minimum load display so the screen doesn't flash
            yield return new WaitForSecondsRealtime(minimumLoadTime);

            ShowLoadingScreen(false);

            // Switch UI to Investigation view
            UI.GameUIManager.Instance?.SwitchToView(UI.GameUIManager.GameView.Investigation);
            AudioManager.Instance?.PlayMusic("investigation_theme");
        }

        IEnumerator UnloadHOScene(string sceneName)
        {
            ShowLoadingScreen(true);
            var op = SceneManager.UnloadSceneAsync(sceneName);
            while (op != null && !op.isDone)
            {
                UpdateLoadBar(op.progress);
                yield return null;
            }
            _currentHOScene = null;
            yield return new WaitForSecondsRealtime(minimumLoadTime * 0.5f);
            ShowLoadingScreen(false);

            UI.GameUIManager.Instance?.SwitchToView(UI.GameUIManager.GameView.Analysis);
            AudioManager.Instance?.PlayMusic("merge_theme");
        }

        IEnumerator LoadSceneAdditive(string sceneName, System.Action<float> onProgress = null,
                                      System.Action onComplete = null)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogError($"[SceneLoader] Scene not found in Build Settings: '{sceneName}'");
                yield break;
            }
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                onProgress?.Invoke(op.progress);
                yield return null;
            }
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;

            onProgress?.Invoke(1f);
            onComplete?.Invoke();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        void ShowLoadingScreen(bool show) => loadingScreenRoot?.SetActive(show);

        void UpdateLoadBar(float progress)
        {
            if (loadingBar != null) loadingBar.value = Mathf.Clamp01(progress);
        }
    }
}
