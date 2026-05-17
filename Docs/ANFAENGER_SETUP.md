# Schritt-für-Schritt Setup — Für Anfänger
## Android & iOS — beide Plattformen

Ziel: In 15 Minuten das Merge-Board spielbar sehen — zuerst am PC, dann aufs Handy.

---

## Teil 1 — Unity Projekt erstellen

### Schritt 1 — Unity Hub öffnen

1. **Unity Hub** öffnen
2. Links auf **"Installs"** klicken
3. Prüfen ob du diese **Modules** installiert hast:
   - ✅ **Android Build Support** (für Android-Handys)
   - ✅ **Android SDK & NDK Tools** (kommt automatisch mit Android Build Support)
   - ✅ **iOS Build Support** (für iPhone/iPad — nur auf Mac möglich!)

> Falls nicht: Klick auf das Zahnrad neben deiner Unity-Version → "Add Modules"

### Schritt 2 — Neues Projekt

1. Klick auf **"New project"**
2. Template wählen: **"2D (Core)"**
3. Name: `SherlockMerge`
4. Klick **"Create project"**

> Warte bis Unity fertig geladen hat (~1–2 Minuten)

---

## Teil 2 — Scripts ins Projekt kopieren

### Schritt 3

1. Im Windows Explorer / Finder: gehe in den Ordner wo dieses Repository liegt
2. Kopiere diese Ordner in deinen Unity-Projekt `Assets/` Ordner:
   ```
   Assets/Scripts/     → komplett kopieren
   Assets/Plugins/     → komplett kopieren (enthält AndroidManifest.xml)
   Assets/Editor/      → komplett kopieren
   ```
3. Zurück in Unity: Unity erkennt die neuen Scripts automatisch
   - Unten rechts siehst du einen Ladebalken — warte bis er fertig ist

### Schritt 4 — Fehler beheben (nur beim ersten Mal)

Unity zeigt vielleicht diesen Fehler:
```
The type 'IDetailedStoreListener' could not be found
```

**Lösung:** Die Datei `IAPManagerFull.cs` braucht ein Extra-Paket (Unity IAP).
Für den Anfang einfach löschen:

1. Im Project-Fenster: `Assets/Scripts/Meta/IAPManagerFull.cs` suchen
2. Rechtsklick → **Delete** → Bestätigen
3. Fehler verschwinden automatisch

---

## Teil 3 — Demo starten (am PC)

### Schritt 5 — Demo-Szene einrichten

1. Oben in Unity: Menü **File → New Scene → Basic (Built-in) → Create**
2. Im **Hierarchy-Fenster** (links): Rechtsklick → **"Create Empty"**
3. Umbenennen zu `DemoRunner`
4. Rechts im **Inspector**: Klick **"Add Component"** → tippe `QuickDemoRunner` → auswählen

### Schritt 6 — Play drücken!

Klick auf den **▶ Play-Button** oben.

Du siehst jetzt das Merge-Board! Klick auf farbige Felder um Items zu bewegen und zu kombinieren.

---

## Teil 4 — Aufs Android-Handy bringen

> Das ist der wichtigste Schritt — so testest du wie es sich auf dem echten Gerät anfühlt!

### Schritt 7 — Android am PC aktivieren

**Auf deinem Android-Handy:**
1. **Einstellungen** öffnen
2. Ganz unten: **"Über das Telefon"**
3. Auf **"Build-Nummer"** 7× tippen
4. Zurück → **"Entwickleroptionen"** → **"USB-Debugging"** einschalten

**Handy per USB mit dem PC verbinden:**
- Es erscheint ein Dialog auf dem Handy → **"Zulassen"** tippen

### Schritt 8 — In Unity auf Android umstellen

1. Menü **File → Build Settings**
2. Links auf **"Android"** klicken
3. Klick **"Switch Platform"** (dauert 1–2 Minuten)
4. Klick **"Player Settings..."** (unten links)

Im Inspector rechts:
- **Company Name**: Dein Name (z.B. `MeinStudio`)
- **Product Name**: `Sherlock Merge`
- **Package Name**: `com.meinstudio.sherlockmerge`
- **Minimum API Level**: `Android 7.0 'Nougat' (API level 24)`
- **Target API Level**: `Automatic (highest installed)`

### Schritt 9 — Direkt aufs Handy bauen

Zurück in **Build Settings**:
1. Dein Handy sollte unter **"Run Device"** erscheinen
2. Klick **"Build And Run"**
3. Speicherort wählen → **"Save"**

Unity baut die App (~2–5 Minuten) und installiert sie direkt auf deinem Handy!

---

## Teil 5 — Aufs iPhone bringen (nur auf Mac!)

> Für iOS brauchst du zwingend einen Mac mit Xcode. Auf Windows ist iOS-Build nicht möglich.

### Schritt 10 — Xcode installieren

1. Mac App Store öffnen → **Xcode** suchen → installieren (groß, ~15 GB)
2. Xcode einmal öffnen → Lizenz akzeptieren

### Schritt 11 — Apple Developer Account

1. Gehe zu [developer.apple.com](https://developer.apple.com)
2. Registriere dich (kostenlos für Testen auf eigenem Gerät)
3. Für App Store Veröffentlichung: **99 $/Jahr** Apple Developer Program

### Schritt 12 — iOS Build in Unity

1. **File → Build Settings → iOS → Switch Platform**
2. **Player Settings**:
   - **Bundle Identifier**: `com.meinstudio.sherlockmerge`
   - **Target minimum iOS Version**: `16.0`
3. **Build** → Ordner wählen → Unity erstellt ein Xcode-Projekt
4. Xcode-Projekt öffnen → iPhone anschließen → **▶ Run**

---

## Interaktions-Referenz (Demo)

| Aktion | PC | Handy |
|---|---|---|
| Item auswählen | Linksklick auf Feld | Einmal tippen |
| Item verschieben | Auswählen → Ziel klicken | Tippen → Ziel tippen |
| Merge | Zwei gleiche auswählen | Zwei gleiche antippen |
| Neues Item | "Objekt finden" Button | Button tippen |
| Verkaufen | Auswählen → "Verkaufen" | Tippen → Button |

### Merge-Kette:
```
Stufe 1: Schnipsel  +  Schnipsel  →  Stufe 2: Brief
Stufe 2: Brief      +  Brief      →  Stufe 3: Dokument
Stufe 3: Dokument   +  Dokument   →  Stufe 4: Nachricht
Stufe 4: Nachricht  +  Nachricht  →  Stufe 5: Forensik-Kit ★
```

---

## Was ist schon Mobile-ready in diesem Projekt?

| Feature | Status |
|---|---|
| Touch-Steuerung (Tippen, Ziehen) | ✅ Fertig |
| Pinch-to-Zoom (Hidden Object Szenen) | ✅ Fertig |
| Safe Area (Notch, Punch-Hole) | ✅ Fertig (`SafeAreaPanel.cs`) |
| Android Zurück-Button | ✅ Fertig (`BackButtonHandler.cs`) |
| Canvas-Skalierung für alle Bildschirme | ✅ Fertig (`MobileCanvasSetup.cs`) |
| Haptisches Feedback beim Merge | ✅ Fertig (Android + iOS) |
| Android Manifest | ✅ Fertig (`Plugins/Android/`) |
| Speicherung (Spielstand) | ✅ Fertig (funktioniert auf beiden Plattformen) |
| In-App-Käufe (Stub) | ⚠️ Stub — braucht Unity IAP Paket |
| App Store / Google Play Upload | ⬜ Nächster Schritt |

---

## Häufige Fehler

**"Android SDK not found"**
→ Unity Hub → Installs → Zahnrad → Add Modules → Android Build Support installieren

**"Handy wird nicht erkannt"**
→ USB-Debugging aktivieren (Schritt 7) + anderes USB-Kabel probieren

**"Build failed: Package name invalid"**
→ Package Name darf keine Leerzeichen und keine Großbuchstaben haben:
  ✅ `com.meinstudio.sherlockmerge`
  ❌ `com.Mein Studio.SherlockMerge`

**"LegacyRuntime.ttf not found"**
→ In `QuickDemoRunner.cs` alle `"LegacyRuntime.ttf"` durch `"Arial.ttf"` ersetzen
