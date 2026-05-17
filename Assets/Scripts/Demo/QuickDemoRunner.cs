using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sherlock.Demo
{
    /// <summary>
    /// QuickDemoRunner — ANFÄNGER-EINSTIEG (Android &amp; iOS ready)
    ///
    /// Dieses Script erstellt ALLES selbst:
    ///   • Ein 5×6 Merge-Board aus farbigen Feldern
    ///   • Klick / Tap zum Auswählen und Verschieben
    ///   • Merge-Logik (zwei gleiche → nächste Stufe)
    ///   • "Objekt finden" Button (simuliert Hidden Object Szene)
    ///   • Münz-Anzeige, Verkaufen-Funktion
    ///   • Haptisches Feedback beim Merge (Android + iOS)
    ///
    /// Wie benutzen:
    ///   1. Neues Unity 2D Projekt erstellen (Android Build Support installiert)
    ///   2. Alle Scripts in Assets/Scripts/ kopieren
    ///   3. Leere Szene öffnen
    ///   4. Leeres GameObject erstellen → dieses Script anhängen
    ///   5. Play drücken → fertig! (auch auf Handy direkt testbar)
    /// </summary>
    public class QuickDemoRunner : MonoBehaviour
    {
        // ── Konfiguration ─────────────────────────────────────────────────────
        private const int   COLS      = 5;
        private const int   ROWS      = 6;
        private const float CELL_SIZE = 100f;   // Pixel
        private const float GAP       = 8f;

        // ── Item-Definitionen (kein ScriptableObject nötig für Demo) ──────────
        private static readonly DemoItem[] Items = new DemoItem[]
        {
            new("letter_fragment",  "Schnipsel",    1, new Color(0.95f,0.85f,0.60f)),
            new("sealed_letter",    "Brief",        2, new Color(0.90f,0.70f,0.30f)),
            new("encrypted_doc",    "Dokument",     3, new Color(0.70f,0.50f,0.20f)),
            new("decoded_message",  "Nachricht",    4, new Color(0.50f,0.35f,0.10f)),
            new("forensics_kit",    "Forensik-Kit", 5, new Color(0.30f,0.20f,0.05f)),
        };

        // ── Interner Zustand ──────────────────────────────────────────────────
        private DemoCell[,]  _grid;
        private Canvas       _canvas;
        private Text         _coinText;
        private Text         _statusText;
        private int          _coins = 100;

        // Drag-State
        private DemoCell     _dragSourceCell;
        private GameObject   _dragGhost;

        // ═════════════════════════════════════════════════════════════════════
        // Einstieg
        // ═════════════════════════════════════════════════════════════════════

        void Start()
        {
            BuildUI();
            BuildGrid();
            PlaceStarterItems();
        }

        // ═════════════════════════════════════════════════════════════════════
        // UI aufbauen
        // ═════════════════════════════════════════════════════════════════════

        void BuildUI()
        {
            // Canvas
            var canvasGO       = new GameObject("Canvas");
            _canvas            = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem (für Button-Klicks)
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Hintergrund
            var bg           = CreatePanel(canvasGO.transform, "Background",
                                           new Color(0.13f, 0.10f, 0.08f));
            bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bg.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            bg.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            // Titel
            var title = CreateLabel(bg.transform, "Sherlock: Mystery Merge — DEMO",
                                    22, Color.white);
            var titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 1f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.pivot     = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0, -10);
            titleRT.sizeDelta        = new Vector2(0, 40);

            // Münz-Anzeige
            _coinText = CreateLabel(bg.transform, $"Münzen: {_coins}", 16, Color.yellow)
                            .GetComponent<Text>();
            var coinRT = _coinText.GetComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(0f, 1f);
            coinRT.anchorMax = new Vector2(0f, 1f);
            coinRT.pivot     = new Vector2(0f, 1f);
            coinRT.anchoredPosition = new Vector2(10, -60);
            coinRT.sizeDelta        = new Vector2(200, 30);

            // Status-Text
            _statusText = CreateLabel(bg.transform, "Kombiniere gleiche Gegenstände!", 14,
                                      new Color(0.8f,0.9f,1f)).GetComponent<Text>();
            var stRT = _statusText.GetComponent<RectTransform>();
            stRT.anchorMin = new Vector2(0f, 0f);
            stRT.anchorMax = new Vector2(1f, 0f);
            stRT.pivot     = new Vector2(0.5f, 0f);
            stRT.anchoredPosition = new Vector2(0, 10);
            stRT.sizeDelta        = new Vector2(0, 30);

            // "Objekt finden" Button (simuliert HO-Fund)
            CreateButton(bg.transform, "Objekt finden  +1 Schnipsel",
                         new Vector2(10, -100), new Vector2(260, 44),
                         new Color(0.2f, 0.5f, 0.8f),
                         OnFindObjectClicked);

            // "Verkaufen" Button
            CreateButton(bg.transform, "Ausgewähltes verkaufen  (+10 Münzen)",
                         new Vector2(10, -154), new Vector2(260, 44),
                         new Color(0.6f, 0.3f, 0.1f),
                         OnSellClicked);

            // Legende
            BuildLegend(bg.transform);
        }

        void BuildLegend(Transform parent)
        {
            float startY = -210f;
            CreateLabel(parent, "Merge-Kette:", 13, Color.white,
                        new Vector2(10, startY), new Vector2(200, 24));

            for (int i = 0; i < Items.Length; i++)
            {
                var item = Items[i];
                // Farbblock
                var block    = CreatePanel(parent, $"legend_{i}",
                                           item.Color,
                                           new Vector2(10, startY - 28 - i * 28),
                                           new Vector2(20, 20));

                // Label rechts davon
                var lbl = CreateLabel(parent,
                    $"Stufe {item.Tier}: {item.Name}" + (i < Items.Length - 1 ? "  →" : "  ★ MAX"),
                    12, Color.white,
                    new Vector2(36, startY - 28 - i * 28),
                    new Vector2(220, 22));
                lbl.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // Grid aufbauen
        // ═════════════════════════════════════════════════════════════════════

        void BuildGrid()
        {
            _grid = new DemoCell[COLS, ROWS];

            // Grid-Container: rechts ausgerichtet
            var gridGO  = new GameObject("Grid");
            gridGO.transform.SetParent(_canvas.transform, false);
            var gridRT  = gridGO.AddComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(1f, 0.5f);
            gridRT.anchorMax = new Vector2(1f, 0.5f);
            gridRT.pivot     = new Vector2(1f, 0.5f);

            float totalW = COLS * (CELL_SIZE + GAP) - GAP;
            float totalH = ROWS * (CELL_SIZE + GAP) - GAP;
            gridRT.sizeDelta        = new Vector2(totalW, totalH);
            gridRT.anchoredPosition = new Vector2(-20, 0);

            for (int c = 0; c < COLS; c++)
            {
                for (int r = 0; r < ROWS; r++)
                {
                    float x = c * (CELL_SIZE + GAP);
                    float y = r * (CELL_SIZE + GAP);

                    var cellGO = CreatePanel(gridRT, $"Cell_{c}_{r}",
                                             new Color(0.22f, 0.18f, 0.14f),
                                             new Vector2(x, y),
                                             new Vector2(CELL_SIZE, CELL_SIZE));

                    // Border
                    var outline = cellGO.AddComponent<Outline>();
                    outline.effectColor    = new Color(0.5f, 0.4f, 0.2f, 0.8f);
                    outline.effectDistance = new Vector2(2, -2);

                    var cell = new DemoCell(c, r, cellGO);
                    _grid[c, r] = cell;

                    // Click-Handler
                    int cc = c, rr = r;
                    var btn = cellGO.AddComponent<Button>();
                    btn.onClick.AddListener(() => OnCellClicked(cc, rr));
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // Starter-Items
        // ═════════════════════════════════════════════════════════════════════

        void PlaceStarterItems()
        {
            // 3× Schnipsel als Startgabe
            PlaceItem(0, 0, Items[0]);
            PlaceItem(1, 0, Items[0]);
            PlaceItem(2, 0, Items[0]);
            SetStatus("Tipp: Klick auf ein Feld wählt es aus. Klick auf ein anderes Feld verschiebt es oder merged!");
        }

        // ═════════════════════════════════════════════════════════════════════
        // Klick-Logik (Auswahl + Drag-Simulation)
        // ═════════════════════════════════════════════════════════════════════

        private DemoCell _selectedCell;

        void OnCellClicked(int c, int r)
        {
            var target = _grid[c, r];

            // Nichts ausgewählt → auswählen
            if (_selectedCell == null)
            {
                if (target.IsEmpty) return;
                Select(target);
                return;
            }

            // Gleiche Zelle → abwählen
            if (_selectedCell == target)
            {
                Deselect();
                return;
            }

            // Ziel leer → verschieben
            if (target.IsEmpty)
            {
                MoveItem(_selectedCell, target);
                Deselect();
                return;
            }

            // Ziel gleicher Typ → merge
            if (target.Item?.Id == _selectedCell.Item?.Id)
            {
                TryMerge(_selectedCell, target);
                Deselect();
                return;
            }

            // Ziel anderer Typ → neu auswählen
            Deselect();
            Select(target);
        }

        void Select(DemoCell cell)
        {
            _selectedCell = cell;
            cell.SetHighlight(true);
            SetStatus($"Ausgewählt: {cell.Item?.Name} (Stufe {cell.Item?.Tier}) — klick auf ein anderes Feld");
        }

        void Deselect()
        {
            _selectedCell?.SetHighlight(false);
            _selectedCell = null;
        }

        void MoveItem(DemoCell from, DemoCell to)
        {
            to.Place(from.Item);
            from.Clear();
            SetStatus($"{to.Item?.Name} verschoben.");
        }

        void TryMerge(DemoCell from, DemoCell to)
        {
            int tier = to.Item.Tier;
            if (tier >= Items.Length)
            {
                SetStatus($"Max-Stufe erreicht — {to.Item.Name} kann nicht weiter kombiniert werden!");
                return;
            }

            var result = Items[tier]; // tier ist 1-basiert, Array ist 0-basiert → Index = tier
            from.Clear();
            to.Place(result);

            StartCoroutine(FlashCell(to));
            Core.MobileSetup.VibrateShort();
            SetStatus($"✓ Kombiniert zu: {result.Name} (Stufe {result.Tier})!");

            if (result.Tier == Items.Length)
                SetStatus($"★ FORENSIK-KIT ERSTELLT! Du hast Kapitel 1 abgeschlossen! ★");
        }

        IEnumerator FlashCell(DemoCell cell)
        {
            var img = cell.GameObject.GetComponent<Image>();
            var orig = img.color;
            img.color = Color.white;
            yield return new WaitForSeconds(0.12f);
            img.color = orig;
        }

        // ═════════════════════════════════════════════════════════════════════
        // Button-Callbacks
        // ═════════════════════════════════════════════════════════════════════

        void OnFindObjectClicked()
        {
            // Simuliert einen Fund im Hidden-Object-Modus
            var freeCell = FindFreeCell();
            if (freeCell == null)
            {
                SetStatus("Board ist voll! Verkaufe Gegenstände, um Platz zu machen.");
                return;
            }
            PlaceItem(freeCell.Col, freeCell.Row, Items[0]);
            SetStatus("Objekt gefunden: Briefschnipsel! Jetzt auf dem Board platziert.");
        }

        void OnSellClicked()
        {
            if (_selectedCell == null || _selectedCell.IsEmpty)
            {
                SetStatus("Wähle zuerst ein Feld aus, dann 'Verkaufen'.");
                return;
            }
            var sold = _selectedCell.Item;
            _selectedCell.Clear();
            Deselect();
            _coins += 10;
            UpdateCoins();
            SetStatus($"{sold.Name} verkauft. +10 Münzen!");
        }

        // ═════════════════════════════════════════════════════════════════════
        // Hilfsfunktionen
        // ═════════════════════════════════════════════════════════════════════

        void PlaceItem(int c, int r, DemoItem item) => _grid[c, r].Place(item);

        DemoCell FindFreeCell()
        {
            for (int r = 0; r < ROWS; r++)
                for (int c = 0; c < COLS; c++)
                    if (_grid[c, r].IsEmpty) return _grid[c, r];
            return null;
        }

        void SetStatus(string msg)
        {
            if (_statusText) _statusText.text = msg;
        }

        void UpdateCoins()
        {
            if (_coinText) _coinText.text = $"Münzen: {_coins}";
        }

        // ═════════════════════════════════════════════════════════════════════
        // UI-Hilfsmethoden (erstellen Objekte ohne Prefabs)
        // ═════════════════════════════════════════════════════════════════════

        static GameObject CreatePanel(Transform parent, string name, Color color,
                                       Vector2 anchoredPos = default,
                                       Vector2 size        = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot     = new Vector2(0f, 0f);
            if (size        != default) rt.sizeDelta        = size;
            if (anchoredPos != default) rt.anchoredPosition = anchoredPos;
            return go;
        }

        static GameObject CreateLabel(Transform parent, string text, int fontSize, Color color,
                                       Vector2 anchoredPos = default,
                                       Vector2 size        = default)
        {
            var go = new GameObject("Label_" + text[..Mathf.Min(8, text.Length)]);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = fontSize;
            t.color     = color;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.alignment = TextAnchor.MiddleCenter;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot     = new Vector2(0f, 1f);
            if (size        != default) rt.sizeDelta        = size;
            if (anchoredPos != default) rt.anchoredPosition = anchoredPos;
            return go;
        }

        static void CreateButton(Transform parent, string label,
                                  Vector2 anchoredPos, Vector2 size,
                                  Color color, UnityEngine.Events.UnityAction onClick)
        {
            var go  = CreatePanel(parent, "Btn_" + label, color, anchoredPos, size);
            var btn = go.AddComponent<Button>();

            // Hover-Tint
            var colors          = btn.colors;
            colors.highlightedColor = Color.white;
            colors.pressedColor     = new Color(0.7f, 0.7f, 0.7f);
            btn.colors          = colors;
            btn.targetGraphic   = go.GetComponent<Image>();
            btn.onClick.AddListener(onClick);

            // Outline
            var outline         = go.AddComponent<Outline>();
            outline.effectColor = Color.white;

            // Text
            var textGO          = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var t               = textGO.AddComponent<Text>();
            t.text              = label;
            t.fontSize          = 13;
            t.color             = Color.white;
            t.font              = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.alignment         = TextAnchor.MiddleCenter;
            var rt              = textGO.GetComponent<RectTransform>();
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.offsetMin        = Vector2.zero;
            rt.offsetMax        = Vector2.zero;
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Datenklassen (nur für Demo — kein ScriptableObject nötig)
    // ═════════════════════════════════════════════════════════════════════════

    public class DemoItem
    {
        public string Id    { get; }
        public string Name  { get; }
        public int    Tier  { get; }
        public Color  Color { get; }
        public DemoItem(string id, string name, int tier, Color color)
        { Id = id; Name = name; Tier = tier; Color = color; }
    }

    public class DemoCell
    {
        public int        Col        { get; }
        public int        Row        { get; }
        public GameObject GameObject { get; }
        public DemoItem   Item       { get; private set; }
        public bool       IsEmpty    => Item == null;

        private Image  _bg;
        private Text   _label;
        private static readonly Color EmptyColor = new(0.22f, 0.18f, 0.14f);

        public DemoCell(int col, int row, GameObject go)
        {
            Col = col; Row = row; GameObject = go;
            _bg = go.GetComponent<Image>();

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            _label = labelGO.AddComponent<Text>();
            _label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _label.fontSize  = 11;
            _label.color     = Color.white;
            _label.alignment = TextAnchor.MiddleCenter;
            var rt = labelGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2, 2);
            rt.offsetMax = new Vector2(-2, -2);
        }

        public void Place(DemoItem item)
        {
            Item       = item;
            _bg.color  = item.Color;
            _label.text = $"Stufe {item.Tier}\n{item.Name}";
        }

        public void Clear()
        {
            Item       = null;
            _bg.color  = EmptyColor;
            _label.text = "";
            SetHighlight(false);
        }

        public void SetHighlight(bool on)
        {
            var outline = GameObject.GetComponent<Outline>();
            if (outline == null) return;
            outline.effectColor    = on ? Color.yellow : new Color(0.5f, 0.4f, 0.2f, 0.8f);
            outline.effectDistance = on ? new Vector2(4, -4) : new Vector2(2, -2);
        }
    }
}
