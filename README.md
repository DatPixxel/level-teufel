# Level Teufel 😈

Ein gemeines kleines Handy-Spiel im Stil von **Level Devil**: Die Level sehen
harmlos aus – sind sie aber nicht. Böden verschwinden, Stacheln schnellen hoch,
Türen laufen weg. Sterben gehört dazu: Deine Tode werden gezählt und sind Teil
des Spaßes.

**18 Level in 3 Kapiteln** · komplett auf Deutsch · läuft direkt im Browser,
keine Installation nötig.

## 🎮 Sofort spielen

**Auf dem Handy:** Öffne einfach diese Adresse im Browser und dreh das Handy quer:

> **https://datpixxel.github.io/New-Game-Style/**

*(Falls der Link noch nicht funktioniert: siehe unten „Spiel online stellen".)*

**Auf dem PC:** Gleiche Adresse – oder das Repository herunterladen und die
Datei `index.html` doppelklicken. Es wird kein Server und keine Installation
benötigt.

## 🕹️ Steuerung

| | Handy | Tastatur |
|---|---|---|
| Laufen | ◀ ▶ Buttons unten links | Pfeiltasten oder A/D |
| Springen | ⬆ Button unten rechts | Leertaste, W oder Pfeil hoch |
| Level neu starten | ↻ Button oben rechts | R |
| Zur Levelauswahl | ☰ Button oben rechts | Esc |

Tipp: Kurz tippen = kleiner Hüpfer, gedrückt halten = hoher Sprung.

## 📖 Die Kapitel

1. **Willkommen** (Level 1–6) – lerne, niemandem zu vertrauen.
2. **Vertrauen ist gut** (Level 7–12) – … Kontrolle ist besser.
3. **Bosheit** (Level 13–18) – jede Falle bringt ihre beste Freundin mit.

Dein Fortschritt (freigeschaltete Level, Tode) wird automatisch im Browser
gespeichert.

## 🌐 Spiel online stellen (einmalig, 1 Klick)

Damit der Spiel-Link oben funktioniert, muss GitHub Pages einmal aktiviert
werden:

1. Auf GitHub in diesem Repository: **Settings → Pages** öffnen
2. Bei **Source** die Option **„GitHub Actions"** auswählen – fertig!

Sobald dieser Branch in den `main`-Branch übernommen (gemergt) wurde, baut
GitHub das Spiel automatisch und es ist unter dem Link oben erreichbar. Danach
gilt: Jede Änderung an `main` aktualisiert das Spiel von selbst.

## 🛠️ Technik (für Neugierige)

- Reines HTML5 + JavaScript + Canvas – **keine** Frameworks, **keine** Downloads,
  keine externen Server. Auch Sounds werden live im Browser erzeugt (WebAudio).
- Die Level stehen als einfache Text-Raster in `js/levels.js` – neue Level
  lassen sich dort mit etwas Ausprobieren selbst bauen:
  `#` = Wand, `.` = leer, `P` = Start, `D` = Tür, `^` = Stacheln, `~` = Fake-Boden.
- Fallen und Auslöser (Zonen, Timer, Ketten) sind in `js/traps.js` beschrieben –
  12 Fallen-Typen von `vanish` (Boden verschwindet) bis `doorMove` (Tür flieht).

| Datei | Aufgabe |
|---|---|
| `index.html` / `style.css` | Seite, Menüs, Touch-Buttons |
| `js/config.js` | alle Stellschrauben (Physik, Farben) |
| `js/levels.js` | die 18 Level |
| `js/traps.js` | Fallen-System |
| `js/player.js` | Spielerphysik (Coyote-Time, Jump-Buffer) |
| `js/renderer.js` | Grafik |
| `js/input.js` / `js/audio.js` / `js/storage.js` | Eingabe, Sound, Speicherstand |
| `js/ui.js` / `js/main.js` | Menüs und Spielschleife |

Viel Spaß – und nicht ärgern! 😈
