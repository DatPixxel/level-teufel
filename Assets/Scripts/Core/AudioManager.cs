using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sherlock.Core
{
    /// <summary>
    /// AudioManager — centralised sound and music system.
    ///
    /// Usage:
    ///   AudioManager.Instance.PlaySfx("merge_success");
    ///   AudioManager.Instance.PlayMusic("investigation_theme", fadeIn: true);
    ///   AudioManager.Instance.SetMusicVolume(0.5f);
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambienceSource;

        [Header("SFX Clips — key must match the string passed to PlaySfx()")]
        [SerializeField] private List<NamedClip> sfxClips  = new();

        [Header("Music Tracks — key must match the string passed to PlayMusic()")]
        [SerializeField] private List<NamedClip> musicTracks = new();

        [Header("Defaults")]
        [SerializeField] [Range(0f,1f)] private float defaultMusicVolume  = 0.6f;
        [SerializeField] [Range(0f,1f)] private float defaultSfxVolume    = 1.0f;
        [SerializeField]                private float musicFadeDuration   = 1.2f;

        [System.Serializable]
        public struct NamedClip { public string key; public AudioClip clip; }

        // ── Private ───────────────────────────────────────────────────────────
        private readonly Dictionary<string, AudioClip> _sfxMap   = new();
        private readonly Dictionary<string, AudioClip> _musicMap = new();
        private Coroutine _fadeCoroutine;

        // PlayerPrefs keys
        private const string PrefMusicVol  = "pref_music_vol";
        private const string PrefSfxVol    = "pref_sfx_vol";
        private const string PrefMusicOn   = "pref_music_on";

        // ═════════════════════════════════════════════════════════════════════
        // Lifecycle
        // ═════════════════════════════════════════════════════════════════════

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildMaps();
            RestorePrefs();
        }

        void BuildMaps()
        {
            foreach (var nc in sfxClips)   if (nc.clip) _sfxMap[nc.key]   = nc.clip;
            foreach (var nc in musicTracks) if (nc.clip) _musicMap[nc.key] = nc.clip;
        }

        void RestorePrefs()
        {
            float musicVol = PlayerPrefs.GetFloat(PrefMusicVol, defaultMusicVolume);
            float sfxVol   = PlayerPrefs.GetFloat(PrefSfxVol,   defaultSfxVolume);

            if (musicSource)    musicSource.volume    = musicVol;
            if (sfxSource)      sfxSource.volume      = sfxVol;
            if (ambienceSource) ambienceSource.volume = musicVol * 0.5f;

            bool musicOn = PlayerPrefs.GetInt(PrefMusicOn, 1) == 1;
            if (musicSource) musicSource.mute = !musicOn;
        }

        // ═════════════════════════════════════════════════════════════════════
        // Public API
        // ═════════════════════════════════════════════════════════════════════

        // ── SFX ──────────────────────────────────────────────────────────────

        public void PlaySfx(string key)
        {
            if (!_sfxMap.TryGetValue(key, out var clip))
            {
                Debug.LogWarning($"[AudioManager] SFX key not found: '{key}'");
                return;
            }
            sfxSource.PlayOneShot(clip);
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        // ── Music ─────────────────────────────────────────────────────────────

        public void PlayMusic(string key, bool fadeIn = true, bool loop = true)
        {
            if (!_musicMap.TryGetValue(key, out var clip))
            {
                Debug.LogWarning($"[AudioManager] Music key not found: '{key}'");
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying) return;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            if (fadeIn)
                _fadeCoroutine = StartCoroutine(CrossFade(clip, loop));
            else
            {
                musicSource.clip   = clip;
                musicSource.loop   = loop;
                musicSource.volume = PlayerPrefs.GetFloat(PrefMusicVol, defaultMusicVolume);
                musicSource.Play();
            }
        }

        public void StopMusic(bool fadeOut = true)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            if (fadeOut)
                _fadeCoroutine = StartCoroutine(FadeOut());
            else
                musicSource.Stop();
        }

        // ── Volume / Toggle ───────────────────────────────────────────────────

        public void SetMusicVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (musicSource) musicSource.volume = volume;
            PlayerPrefs.SetFloat(PrefMusicVol, volume);
        }

        public void SetSfxVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (sfxSource) sfxSource.volume = volume;
            PlayerPrefs.SetFloat(PrefSfxVol, volume);
        }

        public void ToggleMusic(bool on)
        {
            if (musicSource) musicSource.mute = !on;
            PlayerPrefs.SetInt(PrefMusicOn, on ? 1 : 0);
        }

        // ── Ambience ──────────────────────────────────────────────────────────

        public void PlayAmbience(string key)
        {
            if (!_sfxMap.TryGetValue(key, out var clip)) return;
            if (ambienceSource.clip == clip && ambienceSource.isPlaying) return;
            ambienceSource.clip   = clip;
            ambienceSource.loop   = true;
            ambienceSource.Play();
        }

        public void StopAmbience() => ambienceSource?.Stop();

        // ═════════════════════════════════════════════════════════════════════
        // Coroutines
        // ═════════════════════════════════════════════════════════════════════

        IEnumerator CrossFade(AudioClip newClip, bool loop)
        {
            float targetVol = PlayerPrefs.GetFloat(PrefMusicVol, defaultMusicVolume);

            // Fade out current track
            float t = 0f;
            float startVol = musicSource.volume;
            while (t < musicFadeDuration * 0.5f)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / (musicFadeDuration * 0.5f));
                yield return null;
            }

            musicSource.Stop();
            musicSource.clip   = newClip;
            musicSource.loop   = loop;
            musicSource.volume = 0f;
            musicSource.Play();

            // Fade in new track
            t = 0f;
            while (t < musicFadeDuration * 0.5f)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVol, t / (musicFadeDuration * 0.5f));
                yield return null;
            }
            musicSource.volume = targetVol;
        }

        IEnumerator FadeOut()
        {
            float startVol = musicSource.volume;
            float t = 0f;
            while (t < musicFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / musicFadeDuration);
                yield return null;
            }
            musicSource.Stop();
        }
    }
}
