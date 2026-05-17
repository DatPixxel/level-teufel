# Bilder & Assets für Sherlock Merge — Komplette Anleitung

## Wo du kostenlose Grafiken findest

### 1. Unity Asset Store (direkt in Unity integriert)
**unity.com/asset-store** → Suche nach: `hidden object`, `puzzle`, `detective`

Kostenlose Pakete die gut passen:
| Name | Inhalt | Preis |
|---|---|---|
| **2D Game Kit** | Sprites, Tiles, UI | Kostenlos |
| **Free Platform Game Assets** | Allgemeine 2D Sprites | Kostenlos |
| **GUI Pro Kit - Casual Game** | Buttons, Panels, Icons | Kostenlos |
| **2D Casual UI HD** | Münzen, Buttons, Rahmen | Kostenlos |

### 2. Kenney.nl — BESTE Quelle für Anfänger!
**kenney.nl/assets** → alles kostenlos, keine Lizenzprobleme

Herunterladen:
- **Game Icons** → 1000+ Symbole (Brief, Lupe, Buch…)
- **UI Pack: RPG Expansion** → Buttons, Rahmen, Inventar-Slots
- **Puzzle Pack** → Kacheln und Merge-Board-Elemente

### 3. OpenGameArt
**opengameart.org** → kostenlose Spielgrafiken (CC0 = vollständig frei)

Suche nach: `mystery`, `detective`, `vintage`, `sepia`

### 4. Itch.io — Asset Packs
**itch.io/game-assets** → viele kostenlose und günstige Packs

Empfohlen:
- Suche: `"puzzle game UI"` → viele kostenlose Merge-Game Assets
- Suche: `"vintage paper"` → passend für Sherlock-Atmosphäre

### 5. Freepik / Flaticon (Icons)
**freepik.com** oder **flaticon.com**
- Kostenlos mit Namensnennung
- Suche: `detective`, `mystery`, `magnifying glass`, `scroll`
- Als PNG oder SVG herunterladen

---

## Was du für dieses Spiel brauchst

### Item-Icons (128×128 px PNG)
Datei ablegen in: `Assets/Resources/Sprites/Items/`
Dateiname = itemId aus ItemData

| Dateiname | Was soll es zeigen |
|---|---|
| `letter_fragment.png` | Zerknittertes Papierstück |
| `sealed_letter.png` | Versiegelter Brief mit Wachssiegel |
| `encrypted_doc.png` | Pergament mit kryptischen Zeichen |
| `decoded_message.png` | Aufentrollter Brief mit Text |
| `forensics_kit.png` | Koffer / Detektiv-Ausrüstung |
| `tobacco_ash.png` | Graue Aschenhäufchen |
| `tobacco_pouch.png` | Kleiner Lederbeutel |
| `sherlock_pipe.png` | Gebogene Pfeife |
| `muddy_footprint.png` | Schlammiger Schuh-Abdruck |
| `plaster_cast.png` | Weißer Gipsabguss |
| `watson_report.png` | Aufgeschlagenes Notizbuch |
| `magnifying_glass.png` | Lupe |
| `notebook.png` | Kleines Notizbuch |

### HO-Szenen Hintergründe (2048×2048 px JPG/PNG)
Datei ablegen in: `Assets/Resources/Sprites/Backgrounds/`

| Dateiname | Beschreibung |
|---|---|
| `library_01.jpg` | Viktorianische Bibliothek (Kapitel 1) |
| `library_02.jpg` | Bibliothek Nacht-Version (Kapitel 2) |
| `crime_scene_01.jpg` | Tatort — Straße im Nebel |
| `library_03.jpg` | Sherlocks Arbeitszimmer |

**Tipp für Hintergründe:** Suche auf Pixabay oder Unsplash nach:
- `"victorian library"` (CC0, kostenlos)
- `"old english room"`
- `"fog london street"`

**pixabay.com** und **unsplash.com** → kostenlos für kommerzielle Nutzung!

### UI-Elemente
Datei ablegen in: `Assets/Resources/Sprites/UI/`

| Dateiname | Verwendung |
|---|---|
| `coin_icon.png` | Münz-Anzeige oben |
| `hint_icon.png` | Hinweis-Button |
| `merge_arrow.png` | Pfeil zwischen zwei Items |
| `star_empty.png` | Leerer Stern (Quest) |
| `star_filled.png` | Gefüllter Stern (Quest erledigt) |

---

## Schritt für Schritt: Bild einbauen

### Item-Icon einbauen (5 Minuten)

**Option A: Im ScriptableObject zuweisen (einfachste Methode)**
1. Bild in Unity ziehen: `Assets/Resources/Sprites/Items/` Ordner
2. Bild anklicken → Inspector:
   - **Texture Type**: `Sprite (2D and UI)`
   - **Max Size**: `128`
   - Klick **Apply**
3. Im Project-Fenster: `Assets/Resources/Items/letter_fragment` öffnen
4. Im Inspector: Feld **Icon** → Sprite reinziehen
5. Fertig! Automatisch im Spiel sichtbar

**Option B: Namenskonvention (kein Inspector nötig)**
1. Bild benennen: `letter_fragment.png`
2. In Ordner legen: `Assets/Resources/Sprites/Items/`
3. Texture Type auf `Sprite` stellen → Apply
4. Code erkennt es automatisch über `SpriteManager.GetItemSprite("letter_fragment")`

### Hintergrundbild einbauen (10 Minuten)

1. Bild benennen: `library_01.jpg`
2. In Ordner: `Assets/Resources/Sprites/Backgrounds/`
3. Texture Type: `Sprite (2D and UI)`
4. Max Size: `2048`
5. Compression: `Normal Quality`
6. Klick **Apply**
7. In der HO-Szene: GameObject mit `HOSceneBackground` Script → `sceneId = "library_01"`

---

## Bildgrößen-Empfehlungen

| Asset-Typ | Größe | Format | Kompression |
|---|---|---|---|
| Item Icons | 128×128 | PNG | Komprimiert |
| UI Icons | 64×64 | PNG | Keine |
| HO Hintergründe | 2048×2048 | JPG | Normal |
| Charakter-Portraits | 512×512 | PNG | Normal |
| Splash Screen | 2048×2048 | JPG | Normal |
| App Icon | 1024×1024 | PNG | Keine |

---

## Was das Spiel OHNE echte Bilder macht

Der `PlaceholderSpriteGenerator` erzeugt automatisch farbige Symbole:

```
letter_fragment  → braunes Kreis-Icon mit Brief-Symbol
sealed_letter    → blaues Kreis-Icon mit Brief-Symbol
forensics_kit    → grünes Kreis-Icon mit Stern (Max-Tier)
sherlock_pipe    → lila Kreis-Icon mit Pfeife
```

Das Spiel ist also sofort spielbar — echte Bilder machen es nur hübscher.

---

## Empfohlene Reihenfolge

1. **Zuerst:** Spiel mit Placeholders testen bis alles funktioniert
2. **Dann:** Item-Icons von Kenney.nl herunterladen und einbauen
3. **Dann:** Hintergrundbild für Library_01 von Unsplash holen
4. **Zuletzt:** App-Icon und Splash Screen erstellen (für Store-Veröffentlichung)

---

## App-Icon erstellen

Für den Store brauchst du:

**Android:**
- `1024×1024 px` PNG (für Google Play)
- Unity erstellt alle anderen Größen automatisch

**iOS:**
- `1024×1024 px` PNG (für App Store)

**Tools:**
- Canva (canva.com) — kostenlos, einfach
- GIMP — kostenlos, mächtig
- Photoshop / Affinity Designer — kostenpflichtig

**Tipp:** Suche auf Canva nach `"game icon detective"` als Vorlage.
