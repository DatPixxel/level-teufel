# Level Teufel 😈

Ein absichtlich gemeines Handy-Spiel im Stil von **Level Devil**: Die Level
sehen harmlos aus – sind sie aber nicht. Böden verschwinden, Stacheln schnellen
hoch, Türen laufen weg, Wände rücken nach und manchmal ist links plötzlich
rechts. **Dieses Spiel will, dass du verzweifelst.** Sterben gehört dazu: Deine
Tode werden gezählt, verspottet – und am Ende gibt es dafür einen Rang.

**24 Level in 4 Kapiteln** · komplett auf Deutsch · läuft direkt im Browser,
keine Installation nötig.

## 🎮 Sofort spielen

**Auf dem Handy:** Öffne einfach diese Adresse im Browser und dreh das Handy quer:

> **https://datpixxel.github.io/level-teufel/**

*(Der Link wird beim ersten Workflow-Lauf automatisch aktiviert — siehe unten.)*

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

1. **Willkommen** (Level 1–6) – lerne, niemandem zu vertrauen. Auch hier
   stirbt man schon reihenweise – willkommen bedeutet nicht harmlos.
2. **Vertrauen ist gut** (Level 7–12) – … Kontrolle ist besser.
3. **Bosheit** (Level 13–17) – jede Falle bringt ihre beste Freundin mit,
   und manche Tür geht am Ende dahin zurück, wo du herkamst.
4. **Hölle** (Level 18–24) – bröselnde Böden, Stachelwände, Lügen-Türen und
   gespiegelte Steuerung. Viel Glück. Du wirst ihn brauchen.

**Fair bleibt es trotzdem:** Jedes Level ist mit einer Headless-Simulation
(`tools/sim.js`, Experten-Bots auf der echten Spiellogik) nachweislich
schaffbar – aber nur mit auswendig gelerntem Plan. Ein Bot, der einfach nur
rennt und über Sichtbares springt, stirbt in **allen 24 Leveln**.

Dein Fortschritt (freigeschaltete Level, Tode) wird automatisch im Browser
gespeichert. Bei Todes-Meilensteinen verspottet dich das Spiel, und auf dem
Sieg-Screen wartet dein Rang – von „Der Teufel persönlich" (unter 150 Tode)
bis „Unsterblich — aus Übung".

## 🌐 Spiel online stellen

Der Workflow in `.github/workflows/deploy-pages.yml` aktiviert GitHub Pages
beim ersten Lauf automatisch und veröffentlicht das Spiel bei jedem Push auf
`main` (und auf den Entwicklungs-Branch). Es ist kein manueller Schritt nötig.
Falls es doch einmal hakt: **Settings → Pages → Source: „GitHub Actions"**
prüfen und den Workflow unter **Actions** neu starten.

## 🛠️ Technik (für Neugierige)

- Reines HTML5 + JavaScript + Canvas – **keine** Frameworks, **keine** Downloads,
  keine externen Server. Auch Sounds werden live im Browser erzeugt (WebAudio).
- Die Level stehen als einfache Text-Raster in `js/levels.js` – neue Level
  lassen sich dort mit etwas Ausprobieren selbst bauen:
  `#` = Wand, `.` = leer, `P` = Start, `D` = Tür, `^` = Stacheln, `~` = Fake-Boden.
- Fallen und Auslöser (Zonen, Timer, Ketten) sind in `js/traps.js` beschrieben –
  16 Fallen-Typen von `vanish` (Boden verschwindet) über `crumble` (Boden
  bröselt), `fakeDoor` (Lügen-Tür) und `spikeWall` (nachrückende Stachelwand)
  bis `flipControls` (Steuerung gespiegelt).

| Datei | Aufgabe |
|---|---|
| `index.html` / `style.css` | Seite, Menüs, Touch-Buttons |
| `js/config.js` | alle Stellschrauben (Physik, Farben) |
| `js/levels.js` | die 24 Level |
| `js/traps.js` | Fallen-System |
| `js/player.js` | Spielerphysik (Coyote-Time, Jump-Buffer) |
| `js/renderer.js` | Grafik |
| `js/input.js` / `js/audio.js` / `js/storage.js` | Eingabe, Sound, Speicherstand |
| `js/ui.js` / `js/main.js` | Menüs und Spielschleife |
| `tools/sim.js` / `tools/bots.js` | Headless-Schaffbarkeits-Beweis (`node tools/sim.js`) |

Viel Spaß – und nicht ärgern! 😈
