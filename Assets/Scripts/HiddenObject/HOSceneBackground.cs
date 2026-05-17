using Sherlock.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Sherlock.HiddenObject
{
    /// <summary>
    /// HOSceneBackground — zeigt das Hintergrundbild einer Hidden-Object-Szene.
    ///
    /// Verwendung in Unity:
    ///   1. Leeres GameObject in der HO-Szene erstellen
    ///   2. Dieses Script anhängen
    ///   3. sceneId auf den Dateinamen des Bildes setzen (ohne .png)
    ///      z.B. sceneId = "library_01"
    ///      → Unity sucht nach: Assets/Resources/Sprites/Backgrounds/library_01.png
    ///
    /// Das Bild wird automatisch auf den Bildschirm skaliert und zentriert.
    /// Die Szenen-Kamera kann dann für Zoom/Pan verwendet werden.
    ///
    /// Empfohlene Bildgröße: 2048×2048 px oder 3072×2048 px (Landscape)
    /// Dateiformat: PNG (mit Transparenz) oder JPG (ohne)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HOSceneBackground : MonoBehaviour
    {
        [Header("Szenen-Identifikation")]
        [Tooltip("Dateiname ohne .png  z.B. 'library_01'")]
        [SerializeField] private string sceneId;

        [Header("Skalierung")]
        [SerializeField] private bool fitToCamera = true;
        [SerializeField] private Camera hoCamera;

        private SpriteRenderer _sr;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            LoadBackground();
        }

        void LoadBackground()
        {
            Sprite sprite = null;

            // 1. Versuche echtes Bild zu laden
            if (SpriteManager.Instance != null)
                sprite = SpriteManager.Instance.GetBackground(sceneId);

            // 2. Fallback: direkt aus Resources
            if (sprite == null)
                sprite = Resources.Load<Sprite>($"Sprites/Backgrounds/{sceneId}");

            // 3. Letzter Fallback: Placeholder generieren
            if (sprite == null)
            {
                sprite = PlaceholderSpriteGenerator.GenerateSceneBackground(sceneId);
                Debug.Log($"[HOSceneBackground] Kein Bild für '{sceneId}' gefunden — Placeholder wird verwendet.");
            }

            _sr.sprite = sprite;

            if (fitToCamera) FitToCamera();
        }

        void FitToCamera()
        {
            if (hoCamera == null) hoCamera = Camera.main;
            if (hoCamera == null || _sr.sprite == null) return;

            // Sprite auf Kamera-Sichtbereich skalieren
            float camHeight = hoCamera.orthographicSize * 2f;
            float camWidth  = camHeight * hoCamera.aspect;

            float spriteWidth  = _sr.sprite.bounds.size.x;
            float spriteHeight = _sr.sprite.bounds.size.y;

            float scaleX = camWidth  / spriteWidth;
            float scaleY = camHeight / spriteHeight;

            // Cover-Modus: größerer Faktor → Bild füllt den Bildschirm komplett
            float scale = Mathf.Max(scaleX, scaleY);
            transform.localScale = Vector3.one * scale;
        }

        /// <summary>Bild zur Laufzeit wechseln (z.B. beim Kapitelwechsel).</summary>
        public void SetScene(string newSceneId)
        {
            sceneId = newSceneId;
            LoadBackground();
        }
    }
}
