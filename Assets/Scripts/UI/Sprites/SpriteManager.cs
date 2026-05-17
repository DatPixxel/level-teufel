using System.Collections.Generic;
using UnityEngine;

namespace Sherlock.UI
{
    /// <summary>
    /// SpriteManager — lädt und cached alle Spielgrafiken zentral.
    ///
    /// Ordnerstruktur in Unity (alle unter Assets/Resources/):
    ///   Sprites/Items/          ← Item-Icons (letter_fragment.png, sealed_letter.png …)
    ///   Sprites/UI/             ← UI-Elemente (coin_icon.png, hint_icon.png …)
    ///   Sprites/Backgrounds/    ← HO-Szenen-Hintergründe (library_01.png …)
    ///   Sprites/Characters/     ← Sherlock, Watson Portraits
    ///
    /// Benennungskonvention:
    ///   Dateiname = itemId aus ItemData   z.B. "letter_fragment.png"
    ///   → SpriteManager.GetItemSprite("letter_fragment")
    ///
    /// Verwendung:
    ///   var sprite = SpriteManager.Instance.GetItemSprite("forensics_kit");
    ///   myImage.sprite = sprite;
    /// </summary>
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance { get; private set; }

        // Pfade relativ zu Resources/
        private const string ItemsPath       = "Sprites/Items/";
        private const string UIPath          = "Sprites/UI/";
        private const string BackgroundsPath = "Sprites/Backgrounds/";
        private const string CharactersPath  = "Sprites/Characters/";

        // Cache — vermeidet wiederholtes Laden von Disk
        private readonly Dictionary<string, Sprite> _cache = new();

        [Header("Fallback wenn kein Bild vorhanden")]
        [SerializeField] private Sprite fallbackItemSprite;
        [SerializeField] private Sprite fallbackBackgroundSprite;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PreloadItems();
        }

        // ── Vorabladen ────────────────────────────────────────────────────────

        void PreloadItems()
        {
            // Alle Sprites im Items-Ordner laden
            var sprites = Resources.LoadAll<Sprite>(ItemsPath.TrimEnd('/'));
            foreach (var s in sprites)
                _cache[s.name] = s;

            Debug.Log($"[SpriteManager] {sprites.Length} Item-Sprites geladen.");
        }

        // ── Öffentliche API ───────────────────────────────────────────────────

        /// <summary>Gibt den Sprite für ein Item zurück. Fallback wenn nicht vorhanden.</summary>
        public Sprite GetItemSprite(string itemId)
        {
            if (_cache.TryGetValue(itemId, out var cached)) return cached;

            // Lazy-Load: erst beim ersten Abruf laden
            var sprite = Resources.Load<Sprite>(ItemsPath + itemId);
            if (sprite != null)
            {
                _cache[itemId] = sprite;
                return sprite;
            }

            // Kein Bild? Placeholder generieren
            var placeholder = PlaceholderSpriteGenerator.GenerateItemSprite(itemId);
            _cache[itemId] = placeholder;
            return placeholder;
        }

        /// <summary>Hintergrundbild für eine HO-Szene.</summary>
        public Sprite GetBackground(string sceneId)
        {
            if (_cache.TryGetValue("bg_" + sceneId, out var cached)) return cached;

            var sprite = Resources.Load<Sprite>(BackgroundsPath + sceneId);
            if (sprite != null)
            {
                _cache["bg_" + sceneId] = sprite;
                return sprite;
            }
            return fallbackBackgroundSprite;
        }

        /// <summary>UI-Icon (Münze, Hinweis, etc.).</summary>
        public Sprite GetUI(string uiKey)
        {
            if (_cache.TryGetValue("ui_" + uiKey, out var cached)) return cached;
            var sprite = Resources.Load<Sprite>(UIPath + uiKey);
            if (sprite != null) _cache["ui_" + uiKey] = sprite;
            return sprite;
        }

        /// <summary>Charakter-Portrait (sherlock, watson).</summary>
        public Sprite GetCharacter(string characterId)
        {
            if (_cache.TryGetValue("char_" + characterId, out var cached)) return cached;
            var sprite = Resources.Load<Sprite>(CharactersPath + characterId);
            if (sprite != null) _cache["char_" + characterId] = sprite;
            return sprite;
        }

        // ── Sprite aus Texture erstellen (für dynamisch geladene PNGs) ────────

        /// <summary>
        /// Lädt eine PNG/JPG-Datei aus dem persistentDataPath (z.B. heruntergeladene Assets).
        /// </summary>
        public Sprite LoadFromFile(string absolutePath)
        {
            if (!System.IO.File.Exists(absolutePath)) return null;
            var data    = System.IO.File.ReadAllBytes(absolutePath);
            var tex     = new Texture2D(2, 2);
            if (!tex.LoadImage(data)) return null;
            return Texture2DToSprite(tex);
        }

        public static Sprite Texture2DToSprite(Texture2D tex)
        {
            return Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }
    }
}
