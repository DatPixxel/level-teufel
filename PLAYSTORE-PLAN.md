# Level Teufel → Google Play Store: Der Plan von A bis Z

Stand: 30.07.2026 · Spiel: https://datpixxel.github.io/level-teufel/

Bei jedem Schritt steht, wer ihn macht: **Du** (Google-Konto, Geld, Identität
nötig) oder **Claude** (kann direkt in der Session vorbereitet werden).

---

## Phase 1: Konto und Grundlagen

### Schritt 1 – Play-Console-Konto anlegen (Du, ~30 Min + 1–3 Tage Wartezeit)
- https://play.google.com/console → als **Privatperson** registrieren
- Einmalig **25 US-Dollar** (Kreditkarte)
- Identitätsprüfung: Ausweis-Foto hochladen, Freigabe dauert meist 1–3 Tage
- E-Mail-Adresse und Telefonnummer verifizieren
- ➡️ **Zuerst starten – alles andere hängt daran!**

### Schritt 2 – Repo für die Domain-Wurzel (Claude, ~10 Min)
- Der Store braucht später `https://datpixxel.github.io/.well-known/assetlinks.json`
  – das liegt an der Domain-**Wurzel**, nicht im Spiel-Unterordner
- Dafür wird ein zweites Repo namens `datpixxel.github.io` angelegt

---

## Phase 2: Die Android-App bauen (parallel zur Kontowartezeit)

### Schritt 3 – TWA-App erzeugen (Claude)
- Das Spiel wird als **Trusted Web Activity** (TWA) verpackt: eine Mini-Android-App,
  die die Website in Vollbild lädt. Werkzeug: **Bubblewrap** (CLI)
- Ergebnis: eine **`.aab`-Datei** (Android App Bundle) – die wird hochgeladen
- Dabei entsteht der **Signatur-Schlüssel** (`.keystore` + Passwort)

> ⚠️ **WICHTIG:** Keystore-Datei und Passwort NIEMALS verlieren und NIEMALS
> ins öffentliche Repo! Ohne den Schlüssel kann nie wieder ein Update
> veröffentlicht werden. Sichern: Passwort-Manager + Kopie auf USB-Stick.

### Schritt 4 – assetlinks.json veröffentlichen (Claude, ~10 Min)
- Aus dem Schlüssel wird ein SHA-256-Fingerabdruck berechnet
- Der kommt in die `assetlinks.json` im Repo aus Schritt 2
- Beweist, dass App und Website zusammengehören – sonst zeigt die App
  eine Browser-Adressleiste

### Schritt 5 – Store-Material erstellen (Claude, ~1 Std)
- [ ] Mindestens 2 (besser 4–8) **Screenshots** im Querformat (per `shot.html`)
- [ ] **Feature-Grafik** 1024×500 px (Banner im Store-Eintrag)
- [x] App-Icon 512×512 (liegt schon in `icons/`)
- [ ] **Kurzbeschreibung** (max. 80 Zeichen)
- [ ] **Langbeschreibung** (max. 4000 Zeichen)

---

## Phase 3: App in der Play Console einrichten (sobald Konto freigegeben)

### Schritt 6 – App anlegen und Formulare ausfüllen (Du klickst, Claude diktiert; ~1–2 Std)
- „App erstellen" → Name **Level Teufel**, Sprache Deutsch, Typ „Spiel", kostenlos
- **Datenschutzerklärung-URL:**
  `https://datpixxel.github.io/level-teufel/datenschutz.html` ✅ (existiert schon)
- **Datensicherheits-Formular:** „keine Daten erhoben, keine geteilt"
  (alles läuft lokal im Browser)
- **Altersfreigabe-Fragebogen:** harmloser Plattformer → USK 0 / PEGI 3
- **Zielgruppe:** „nicht primär für Kinder" wählen (sonst strengere Familien-Regeln)
- Store-Eintrag: Texte, Screenshots, Feature-Grafik hochladen

---

## Phase 4: Der Pflicht-Test (der Wartezeit-Brocken)

> Hintergrund: Neue Privat-Konten (nach 13.11.2023) müssen vor der
> Veröffentlichung einen **geschlossenen Test mit mind. 12 Testern über
> 14 Tage am Stück** durchführen.
> **Wichtig:** Die 14 Tage sind KEINE Spielzeit! Tester müssen nur die
> Einladung annehmen, die App installieren und **installiert lassen**.

### Schritt 7 – Geschlossenen Test starten (Du, ~30 Min)
- Play Console → „Testen" → „Geschlossener Test" → `.aab` hochladen
- Tester-E-Mail-Liste anlegen
- Google prüft das Release (beim ersten Mal meist 1–3 Tage)

### Schritt 8 – 12 Tester einsammeln (Du, verteilt über ein paar Tage)
- Gmail-Adressen der Tester in die Liste eintragen, Teilnahme-Link verschicken
- Jeder Tester: Link öffnen → „Teilnehmen" → App installieren → installiert lassen
- Quellen: Familie, Freunde, Kollegen, Verein …
- Auffüllen über **r/AndroidClosedTesting** (Reddit) – Entwickler testen
  sich gegenseitig, kostenlos
- ❌ Keine Bezahldienste – Grauzone, riskiert das Entwicklerkonto

### Schritt 9 – 14 Tage warten (niemand, 0 Min Arbeit)
- Zähler startet automatisch, sobald 12 Tester angemeldet sind und das
  Release freigegeben wurde
- Zeit nutzen, um den Store-Eintrag zu polieren

---

## Phase 5: Veröffentlichung

### Schritt 10 – Produktionszugang beantragen (Du, ~15 Min)
- Nach den 14 Tagen erscheint „Produktionszugang beantragen"
- Google fragt: Wie getestet? Was gelernt? Zielgruppe? (Antworten diktiert Claude)
- Entscheidung meist innerhalb weniger Tage

### Schritt 11 – Produktions-Release (Du, ~15 Min)
- Dieselbe `.aab` unter „Produktion" veröffentlichen
- Länder auswählen (einfach: alle) → abschicken
- Letzte Prüfung: Stunden bis wenige Tage → **Level Teufel ist im Play Store** 🎉

---

## Realistischer Zeitplan

| Woche | Was passiert |
|---|---|
| 1 | Konto + Identitätsprüfung; parallel: App bauen, assetlinks, Screenshots, Texte |
| 2 | Formulare ausfüllen, Test-Release hochladen, Tester einsammeln |
| 3–4 | Die 14 Test-Tage laufen (reine Wartezeit) |
| 5 | Produktionszugang beantragen, veröffentlichen |

Gesamt: **4–6 Wochen**, aktive Arbeitszeit ca. 4–5 Stunden.

---

## Aktueller Status

- [ ] Schritt 1: Play-Console-Konto (Du – **nächster Schritt**)
- [ ] Schritt 2: Repo `datpixxel.github.io`
- [ ] Schritt 3: TWA-App (`.aab` + Keystore)
- [ ] Schritt 4: assetlinks.json
- [ ] Schritt 5: Screenshots, Feature-Grafik, Store-Texte
- [ ] Schritt 6: App in Play Console einrichten
- [ ] Schritt 7–9: Geschlossener Test (12 Tester, 14 Tage)
- [ ] Schritt 10: Produktionszugang
- [ ] Schritt 11: Release 🎉
