using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Sherlock.Meta
{
    /// <summary>
    /// LeaderboardService — placeholder for a global leaderboard.
    ///
    /// Implementation paths (choose one):
    ///   • Apple Game Center (GKLeaderboard) via Unity GameKit plugin
    ///   • Google Play Games leaderboards
    ///   • Custom back-end REST endpoint (stub implemented below)
    ///   • Unity Gaming Services Leaderboards (com.unity.services.leaderboards)
    /// </summary>
    public class LeaderboardService : MonoBehaviour
    {
        public static LeaderboardService Instance { get; private set; }

        [Serializable]
        public class LeaderboardEntry
        {
            public string playerId;
            public string displayName;
            public int    score;
            public int    rank;
        }

        [Serializable]
        private class LeaderboardResponse { public List<LeaderboardEntry> entries; }

        // TODO: Replace with your actual endpoint
        private const string BaseUrl  = "https://api.example.com/sherlock/leaderboard";
        private const string BoardId  = "global_merges";

        public event Action<List<LeaderboardEntry>> OnFetchComplete;
        public event Action<string>                 OnFetchError;
        public event Action                         OnSubmitSuccess;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Fetch top entries ─────────────────────────────────────────────────

        public void FetchTopScores(int count = 20) =>
            StartCoroutine(FetchCoroutine(count));

        IEnumerator FetchCoroutine(int count)
        {
            var url = $"{BaseUrl}/{BoardId}?limit={count}";
            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("Authorization", $"Bearer {GetAuthToken()}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[Leaderboard] Fetch failed: {req.error}");
                OnFetchError?.Invoke(req.error);

                // Return stub data in development
#if UNITY_EDITOR
                OnFetchComplete?.Invoke(GenerateStubEntries(count));
#endif
                yield break;
            }

            var response = JsonUtility.FromJson<LeaderboardResponse>(req.downloadHandler.text);
            OnFetchComplete?.Invoke(response?.entries ?? new List<LeaderboardEntry>());
        }

        // ── Submit score ──────────────────────────────────────────────────────

        public void SubmitScore(int score) => StartCoroutine(SubmitCoroutine(score));

        IEnumerator SubmitCoroutine(int score)
        {
            var payload = JsonUtility.ToJson(new { boardId = BoardId, score });
            using var req = new UnityWebRequest($"{BaseUrl}/submit", "POST");
            req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(payload));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type",  "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {GetAuthToken()}");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                OnSubmitSuccess?.Invoke();
            else
                Debug.LogWarning($"[Leaderboard] Submit failed: {req.error}");
        }

        // ── Auth token stub ───────────────────────────────────────────────────

        string GetAuthToken()
        {
            // TODO: integrate with your auth system (Game Center token, Firebase JWT, etc.)
            return PlayerPrefs.GetString("auth_token", "dev_token");
        }

        // ── Editor stub data ──────────────────────────────────────────────────

        static List<LeaderboardEntry> GenerateStubEntries(int count)
        {
            var list = new List<LeaderboardEntry>();
            for (int i = 0; i < count; i++)
                list.Add(new LeaderboardEntry
                {
                    rank        = i + 1,
                    playerId    = $"player_{i:000}",
                    displayName = $"Detective #{i + 1}",
                    score       = Mathf.Max(0, 10000 - i * 450 + UnityEngine.Random.Range(-50, 50)),
                });
            return list;
        }
    }
}
