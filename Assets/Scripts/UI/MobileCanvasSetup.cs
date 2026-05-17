using UnityEngine;
using UnityEngine.UI;

namespace Sherlock.UI
{
    /// <summary>
    /// MobileCanvasSetup — konfiguriert den Canvas automatisch für mobile Bildschirme.
    ///
    /// Problem ohne dieses Script:
    ///   Ein iPhone 15 Pro hat 2556×1179 Pixel, ein altes Galaxy S8 hat 2960×1440.
    ///   Ohne CanvasScaler sehen Buttons auf jedem Gerät anders aus.
    ///
    /// Lösung:
    ///   "Scale With Screen Size" mit einer Referenzauflösung von 1080×1920 (Portrait).
    ///   Unity skaliert die UI automatisch auf jedes Gerät.
    ///
    /// Verwendung:
    ///   Hänge dieses Script an das Canvas-GameObject in jeder Szene.
    ///   Es konfiguriert sich selbst im Awake().
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class MobileCanvasSetup : MonoBehaviour
    {
        // Referenzauflösung — alle UI-Elemente werden für diese Größe designed
        // Portrait (Hochformat) für Merge-Spiele optimal
        private const float RefWidth  = 1080f;
        private const float RefHeight = 1920f;

        void Awake()
        {
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefWidth, RefHeight);

            // matchWidthOrHeight:
            //   0.0 = nur Breite beachten (gut für Landscape)
            //   1.0 = nur Höhe beachten (gut für Portrait)
            //   0.5 = Mix (gut für gemischte Orientierungen)
            scaler.matchWidthOrHeight  = 0.5f;
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            var canvas = GetComponent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Pixel Perfect ausschalten — besser für skalierte UI
                canvas.pixelPerfect = false;
            }

            // SafeArea-Panel suchen und aktivieren wenn vorhanden
            var safeArea = GetComponentInChildren<SafeAreaPanel>(includeInactive: true);
            if (safeArea != null) safeArea.gameObject.SetActive(true);
        }

        // ── Debug-Overlay im Editor ───────────────────────────────────────────
#if UNITY_EDITOR
        void OnGUI()
        {
            if (!Application.isPlaying) return;
            var safe = Screen.safeArea;
            GUI.Label(new Rect(10, 10, 300, 20),
                $"Screen: {Screen.width}×{Screen.height} | Safe: {safe.width}×{safe.height} | DPI: {Screen.dpi:F0}");
        }
#endif
    }
}
