using UnityEngine;

namespace Sherlock.UI
{
    /// <summary>
    /// SafeAreaPanel — passt einen RectTransform an die Gerätegrenzen an.
    ///
    /// Warum nötig:
    ///   • iPhone: Notch oben, Home-Indicator unten
    ///   • Android: Punch-Hole-Kamera, Navigationbar, verschiedene Cutouts
    ///   • Ohne dieses Script werden Buttons hinter dem Notch versteckt
    ///
    /// Verwendung:
    ///   Hänge dieses Script an das ROOT-Panel deines Canvas.
    ///   Alle Kinder-Elemente werden dann automatisch korrekt platziert.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPanel : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect          _lastSafeArea;
        private Vector2       _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        void Awake()
        {
            _rt = GetComponent<RectTransform>();
            Apply(Screen.safeArea);
        }

        void Update()
        {
            // Nur neu berechnen wenn sich etwas geändert hat (Rotation, Fenstergröße)
            var safe = Screen.safeArea;
            if (safe        == _lastSafeArea
             && Screen.width  == _lastScreenSize.x
             && Screen.height == _lastScreenSize.y
             && Screen.orientation == _lastOrientation) return;

            Apply(safe);
        }

        void Apply(Rect safeArea)
        {
            _lastSafeArea    = safeArea;
            _lastScreenSize  = new Vector2(Screen.width, Screen.height);
            _lastOrientation = Screen.orientation;

            // Normalisierte Anchor-Koordinaten aus Pixel-Werten berechnen
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;

            // Offset auf Null — die Anker übernehmen die Positionierung
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
