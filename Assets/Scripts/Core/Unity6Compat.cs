using UnityEngine;

namespace Sherlock.Core
{
    /// <summary>
    /// Unity6Compat — kapselt alle API-Aufrufe die sich in Unity 6 geändert haben.
    ///
    /// Unity 6 (6000.x) Änderungen die dieses File abdeckt:
    ///   • FindObjectOfType   → FindFirstObjectByType
    ///   • FindObjectsOfType  → FindObjectsByType
    ///   • Built-in Font Pfad → robuster Fallback-Mechanismus
    ///
    /// Verwendung:
    ///   Unity6Compat.FindFirst<MyComponent>()
    ///   Unity6Compat.FindAll<MyComponent>()
    ///   Unity6Compat.GetFont()
    /// </summary>
    public static class Unity6Compat
    {
        // ── Font ──────────────────────────────────────────────────────────────

        private static Font _cachedFont;

        /// <summary>
        /// Gibt den Standard-Unity-Font zurück — funktioniert in Unity 2022 und Unity 6.
        /// </summary>
        public static Font GetFont()
        {
            if (_cachedFont != null) return _cachedFont;

            // 1. Versuch: Unity 2022 / Unity 6 Built-in
            _cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_cachedFont != null) return _cachedFont;

            // 2. Versuch: Unity 2019–2021 Built-in
            _cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (_cachedFont != null) return _cachedFont;

            // 3. Versuch: Betriebssystem-Font (immer verfügbar)
            _cachedFont = Font.CreateDynamicFontFromOSFont(
                new[] { "Arial", "Helvetica Neue", "Helvetica", "sans-serif" }, 14);
            if (_cachedFont != null) return _cachedFont;

            // 4. Letzter Ausweg: leerer Font (verhindert NullReferenceException)
            Debug.LogWarning("[Unity6Compat] Kein System-Font gefunden — Text wird möglicherweise nicht angezeigt.");
            _cachedFont = new Font("Fallback");
            return _cachedFont;
        }

        // ── FindObject ────────────────────────────────────────────────────────

        /// <summary>Ersatz für das veraltete FindObjectOfType in Unity 6.</summary>
        public static T FindFirst<T>() where T : Object
        {
#if UNITY_6000_0_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        /// <summary>Ersatz für das veraltete FindObjectsOfType in Unity 6.</summary>
        public static T[] FindAll<T>() where T : Object
        {
#if UNITY_6000_0_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsInactive.Exclude);
#else
            return Object.FindObjectsOfType<T>();
#endif
        }
    }
}
