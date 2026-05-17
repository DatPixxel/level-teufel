# Schritt-für-Schritt Setup — Für Anfänger

Ziel: In 10 Minuten das Merge-Board spielbar sehen.

---

## Schritt 1 — Unity Projekt erstellen

1. **Unity Hub** öffnen
2. Klick auf **"New project"**
3. Template wählen: **"2D (Core)"**
4. Name: `SherlockMerge`
5. Klick **"Create project"**

> Warte bis Unity fertig geladen hat (~1–2 Minuten)

---

## Schritt 2 — Scripts ins Projekt kopieren

1. Im Windows Explorer / Finder: gehe in den Ordner wo dieses Repository liegt
2. Kopiere den ganzen Ordner **`Assets/`** in deinen Unity-Projekt-Ordner
   (ersetze den vorhandenen Assets-Ordner)
3. Zurück in Unity: Unity erkennt die neuen Scripts automatisch
   - Unten rechts siehst du einen Ladebalken — warte bis er fertig ist
   - Falls rote Fehlermeldungen erscheinen → **Schritt 3 lesen**

---

## Schritt 3 — Falls Fehler erscheinen (häufig bei Anfängern)

Unity zeigt vielleicht diesen Fehler:
```
The type or namespace name 'IDetailedStoreListener' could not be found
```

**Lösung:** Die Datei `IAPManagerFull.cs` braucht ein Extra-Paket.
Ignoriere sie vorerst einfach:

1. Im Project-Fenster: `Assets > Scripts > Meta > IAPManagerFull.cs` suchen
2. Rechtsklick → **Delete**
3. Bestätige mit "Delete"
4. Fehler sollten verschwinden

---

## Schritt 4 — Demo-Szene öffnen

1. Oben in Unity: Menü **File → New Scene**
2. Wähle **"Basic (Built-in)"** → Klick Create
3. Im **Hierarchy-Fenster** (links): Rechtsklick → **"Create Empty"**
4. Das neue Objekt heißt "GameObject" — umbennen zu `DemoRunner`
5. Das Objekt ist noch ausgewählt → rechts im **Inspector-Fenster**:
   - Klick auf **"Add Component"**
   - Tippe: `QuickDemoRunner`
   - Klick auf den Treffer in der Liste

---

## Schritt 5 — Play drücken!

1. Klick auf den **▶ Play-Button** oben in der Mitte
2. Du siehst jetzt das Merge-Board!

---

## Was du in der Demo machen kannst

| Aktion | Wie |
|---|---|
| Item auswählen | Auf ein gefülltes Feld klicken (wird gelb markiert) |
| Item verschieben | Danach auf ein leeres Feld klicken |
| Zwei Items kombinieren | Zwei gleiche Items wählen → klick auf das zweite |
| Neues Item finden | Klick auf **"Objekt finden"** Button |
| Item verkaufen | Item auswählen → Klick **"Ausgewähltes verkaufen"** |

### Die Merge-Kette (probiere es aus!):
```
Stufe 1: Schnipsel  +  Schnipsel  →  Stufe 2: Brief
Stufe 2: Brief      +  Brief      →  Stufe 3: Dokument
Stufe 3: Dokument   +  Dokument   →  Stufe 4: Nachricht
Stufe 4: Nachricht  +  Nachricht  →  Stufe 5: Forensik-Kit  ★
```

---

## Schritt 6 — Wenn du bereit bist (nächste Stufe)

Wenn das Demo läuft und du es verstanden hast, kannst du:

1. **Echte Szene aufbauen** → Menü `Sherlock → Seed Chapter 1 Data` ausführen
   *(erstellt alle Gegenstände als Assets)*
2. **Hintergrundbilder** für die Hidden-Object-Szenen hinzufügen
3. **Sounds** im AudioManager zuweisen

---

## Häufige Fragen

**F: Unity friert ein wenn ich Play drücke**
→ Warte einfach 10–30 Sekunden beim ersten Mal. Danach ist es schneller.

**F: Der Bildschirm ist schwarz**
→ Stelle sicher dass `DemoRunner` ausgewählt ist und `QuickDemoRunner` im Inspector steht

**F: Ich sehe nur einen blauen Hintergrund**
→ Klick oben auf **"Game"** (nicht "Scene") Tab um die Spielansicht zu sehen

**F: Fehlermeldung "LegacyRuntime.ttf not found"**
→ Du benutzt eine sehr neue Unity-Version. Gehe in `QuickDemoRunner.cs` und ersetze
  `"LegacyRuntime.ttf"` durch `"Arial.ttf"` an allen Stellen.
