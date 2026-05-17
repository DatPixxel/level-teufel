using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sherlock.Core
{
    /// <summary>
    /// BackButtonHandler — verwaltet den Android-Zurück-Button und die iOS-Swipe-Geste.
    ///
    /// Funktioniert als Stack: Das zuletzt registrierte Panel/Overlay
    /// wird beim Zurück-Drücken zuerst geschlossen (z.B. Shop → Settings → Hauptmenü).
    ///
    /// Verwendung:
    ///   // Panel öffnen:
    ///   BackButtonHandler.Push(() => settingsPanel.SetActive(false));
    ///
    ///   // Panel schließen (aus Button-OnClick):
    ///   BackButtonHandler.Pop();
    ///
    ///   // Oder beim Öffnen den Handle speichern:
    ///   _handle = BackButtonHandler.Push(CloseShop);
    ///   // ...
    ///   BackButtonHandler.Remove(_handle);
    /// </summary>
    public class BackButtonHandler : MonoBehaviour
    {
        public static BackButtonHandler Instance { get; private set; }

        private readonly Stack<Action> _stack = new();

        // Sicherheitsdialog wenn Stack leer (App verlassen?)
        [SerializeField] private bool showQuitDialog = true;
        private bool _quitDialogOpen;
        private float _lastBackPress;
        private const float DoubleTapQuit = 2f;  // 2× Zurück innerhalb 2s → Beenden

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            // Escape = Android Zurück-Button + Tastatur-Escape im Editor
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            if (_stack.Count > 0)
            {
                _stack.Pop()?.Invoke();
                return;
            }

            // Stack leer = App verlassen
            HandleQuit();
        }

        // ── Öffentliche API ───────────────────────────────────────────────────

        /// <summary>
        /// Registriert eine Aktion für den nächsten Zurück-Druck.
        /// Gibt einen Handle zurück mit dem die Aktion wieder entfernt werden kann.
        /// </summary>
        public static Action Push(Action onBack)
        {
            Instance?._stack.Push(onBack);
            return onBack;
        }

        /// <summary>Führt die oberste Aktion aus und entfernt sie.</summary>
        public static void Pop() => Instance?.ManualPop();

        /// <summary>Entfernt alle registrierten Aktionen (z.B. beim Szenenwechsel).</summary>
        public static void Clear() => Instance?._stack.Clear();

        // ── Intern ────────────────────────────────────────────────────────────

        void ManualPop()
        {
            if (_stack.Count > 0) _stack.Pop()?.Invoke();
        }

        void HandleQuit()
        {
#if UNITY_ANDROID
            if (showQuitDialog)
            {
                float now = Time.unscaledTime;
                if (now - _lastBackPress < DoubleTapQuit)
                {
                    Application.Quit();
                    return;
                }
                _lastBackPress = now;
                // TODO: hier einen "Nochmals drücken zum Beenden" Toast anzeigen
                // z.B. UI.GameUIManager.Instance?.ShowToast("Nochmals drücken zum Beenden");
                Debug.Log("[BackButton] Nochmals drücken zum Beenden.");
            }
            else
            {
                Application.Quit();
            }
#endif
        }
    }
}
