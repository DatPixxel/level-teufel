using System;
using System.Collections;
using System.Collections.Generic;
using Sherlock.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Sherlock.Tutorial
{
    /// <summary>
    /// TutorialManager — führt neue Spieler Schritt für Schritt durch das Spiel.
    ///
    /// Ablauf:
    ///   1. "Objekt finden" → zeigt Pfeil auf Find-Button
    ///   2. "Item auswählen" → zeigt Pfeil auf erstes Board-Item
    ///   3. "Item verschieben" → zeigt Pfeil auf Zielfeld
    ///   4. "Merge!" → zeigt Pfeil wenn zwei gleiche Items vorhanden
    ///   5. Tutorial abgeschlossen → nie wieder anzeigen
    ///
    /// Verwendung:
    ///   TutorialManager.Instance.StartIfNeeded();
    ///   (wird vom GameSessionManager beim ersten Start aufgerufen)
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Tutorial Overlay")]
        [SerializeField] private GameObject overlayRoot;       // dunkles Overlay
        [SerializeField] private Image      spotlightImage;   // heller Kreis über dem Ziel
        [SerializeField] private GameObject arrowIndicator;   // animierter Pfeil
        [SerializeField] private Text       tutorialText;     // Erklärungstext
        [SerializeField] private Button     skipButton;

        [Header("Timing")]
        [SerializeField] private float stepDelay     = 0.5f;
        [SerializeField] private float arrowBobSpeed = 2f;
        [SerializeField] private float arrowBobAmp   = 12f;

        private const string PrefKey = "tutorial_done";

        public bool IsDone    => PlayerPrefs.GetInt(PrefKey, 0) == 1;
        public bool IsRunning { get; private set; }

        // Delegate: TutorialStep liefert die Weltposition des Spotlight-Ziels
        private readonly Queue<TutorialStep> _steps = new();
        private Coroutine _bobCoroutine;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            skipButton?.onClick.AddListener(CompleteTutorial);
            overlayRoot?.SetActive(false);
        }

        // ── Öffentliche API ───────────────────────────────────────────────────

        public void StartIfNeeded()
        {
            if (IsDone) return;
            BuildSteps();
            StartCoroutine(RunTutorial());
        }

        public void CompleteTutorial()
        {
            StopAllCoroutines();
            PlayerPrefs.SetInt(PrefKey, 1);
            overlayRoot?.SetActive(false);
            IsRunning = false;
            Analytics.AnalyticsManager.Instance?.LogEvent("tutorial_completed");
        }

        // ── Schritte definieren ────────────────────────────────────────────────

        void BuildSteps()
        {
            _steps.Clear();

            _steps.Enqueue(new TutorialStep(
                "Willkommen, Detektiv!\nTippe auf 'Objekt finden'\num deinen ersten Hinweis zu sammeln.",
                () => FindButtonPosition("Btn_Objekt")
            ));

            _steps.Enqueue(new TutorialStep(
                "Gut! Das Objekt liegt jetzt\nauf dem Board.\nTippe darauf um es auszuwählen.",
                () => FindFirstBoardItem()
            ));

            _steps.Enqueue(new TutorialStep(
                "Nochmals 'Objekt finden' tippen\num ein zweites Item zu holen.",
                () => FindButtonPosition("Btn_Objekt")
            ));

            _steps.Enqueue(new TutorialStep(
                "Tippe jetzt auf das zweite Item\num beide zu kombinieren!\nGleiche Items → neues Item!",
                () => FindSecondBoardItem()
            ));

            _steps.Enqueue(new TutorialStep(
                "Fantastisch! Du hast deinen\nersten Merge gemacht.\nSo löst du Sherlocks Fälle!",
                () => Vector2.zero   // Mitte des Bildschirms
            ));
        }

        // ── Tutorial-Ablauf ───────────────────────────────────────────────────

        IEnumerator RunTutorial()
        {
            IsRunning = true;
            overlayRoot?.SetActive(true);

            while (_steps.Count > 0)
            {
                var step = _steps.Dequeue();
                yield return StartCoroutine(ShowStep(step));
            }

            CompleteTutorial();
        }

        IEnumerator ShowStep(TutorialStep step)
        {
            yield return new WaitForSeconds(stepDelay);

            // Text setzen
            if (tutorialText) tutorialText.text = step.Text;

            // Spotlight positionieren
            var pos = step.GetPosition();
            if (spotlightImage)
            {
                spotlightImage.rectTransform.anchoredPosition = pos;
                spotlightImage.gameObject.SetActive(pos != Vector2.zero);
            }

            // Pfeil positionieren
            if (arrowIndicator)
            {
                arrowIndicator.SetActive(pos != Vector2.zero);
                arrowIndicator.GetComponent<RectTransform>().anchoredPosition =
                    pos + new Vector2(0, 80);
            }

            // Pfeil-Bob-Animation
            if (_bobCoroutine != null) StopCoroutine(_bobCoroutine);
            if (pos != Vector2.zero && arrowIndicator != null)
                _bobCoroutine = StartCoroutine(BobArrow());

            // Warte auf Tap außerhalb des Overlays
            yield return new WaitForSeconds(0.3f);
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.touchCount > 0);
            yield return new WaitForSeconds(0.1f);
        }

        IEnumerator BobArrow()
        {
            var rt     = arrowIndicator.GetComponent<RectTransform>();
            var origin = rt.anchoredPosition;
            float t    = 0f;
            while (true)
            {
                t += Time.deltaTime * arrowBobSpeed;
                rt.anchoredPosition = origin + new Vector2(0, Mathf.Sin(t) * arrowBobAmp);
                yield return null;
            }
        }

        // ── Hilfsfunktionen zur Positionsermittlung ────────────────────────────

        Vector2 FindButtonPosition(string nameContains)
        {
            foreach (var btn in FindObjectsOfType<Button>())
                if (btn.name.Contains(nameContains))
                    return WorldToCanvasPos(btn.transform.position);
            return Vector2.zero;
        }

        Vector2 FindFirstBoardItem()
        {
            var item = FindObjectOfType<Merge.MergeItem>();
            if (item == null) return Vector2.zero;
            return WorldToCanvasPos(item.transform.position);
        }

        Vector2 FindSecondBoardItem()
        {
            var items = FindObjectsOfType<Merge.MergeItem>();
            if (items.Length < 2) return Vector2.zero;
            return WorldToCanvasPos(items[1].transform.position);
        }

        Vector2 WorldToCanvasPos(Vector3 worldPos)
        {
            var cam       = Camera.main;
            if (cam == null) return Vector2.zero;
            var screenPos = cam.WorldToScreenPoint(worldPos);
            // Konvertierung für ScreenSpaceOverlay Canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRoot?.GetComponent<RectTransform>(),
                screenPos, null, out var localPos);
            return localPos;
        }
    }

    // ── Datenklasse für einen Tutorial-Schritt ─────────────────────────────────

    public class TutorialStep
    {
        public string Text { get; }
        private readonly Func<Vector2> _positionResolver;

        public TutorialStep(string text, Func<Vector2> positionResolver)
        {
            Text              = text;
            _positionResolver = positionResolver;
        }

        public Vector2 GetPosition() => _positionResolver?.Invoke() ?? Vector2.zero;
    }
}
