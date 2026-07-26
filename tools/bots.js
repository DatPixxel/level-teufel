// Experten-Bots: pro Level ein auswendig gelernter Plan, der beweist,
// dass das Level schaffbar ist. Koordinaten in Pixeln (Tile = 32 px).
// Faustregel: jumpAt liegt mindestens ~35 px vor der nächsten Hitbox
// (Frame-Quantisierung), Landefenster werden mittig angepeilt.
'use strict';
const sim = require('./sim.js');
const bot = sim.bot;
const X = sim.X;

// Warten, bis ein Crusher sicher passierbar ist (fährt gerade hoch).
function crusherSafe(idx, maxY) {
  return function (env) {
    const cr = env.rt.crushers[idx];
    return cr.phase === 'retract' && cr.y < (maxY || 450);
  };
}

module.exports = [
  // 1: Spaziergang – Stopp vor den Pop-Stacheln, dann drei saubere Sprünge.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 262 },
    { wait: 0.5 },
    { dir: 1, until: 440, jumpAt: 290, hold: 0.35 },
    { dir: 1, until: 454 },
    { wait: 0.45 },
    { dir: 1, until: 620, jumpAt: 485, hold: 0.35 },
    { dir: 1, until: X(28), jumpAt: 700, hold: 0.4 }
  ]), o),

  // 2: Sprungstunde – Ledge, Ablaufen, Mini-Hop, Weitsprung, Tür-Hop.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 355, jumpAt: 230, hold: 0.34 },
    { dir: 1, until: 476 },
    { dir: 1, until: 590, jumpAt: 474, hold: 0.1 },
    { dir: 1, until: 736, jumpAt: 598, hold: 0.4 },
    { dir: 1, until: X(28), jumpAt: 762, hold: 0.3 },
    { dir: -1, until: 848 }
  ]), o),

  // 3: Spitzen – Ledge-Kette (alles bröckelt), Tap-Sprünge, Limbo unter der Wand.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 330, jumpAt: 222, hold: 0.4 },
    { dir: 1, until: 486, jumpAt: 336, hold: 0.4 },
    { dir: 1, until: 552 },
    { dir: 1, until: 750, jumpAt: 585, hold: 0.12 },
    { dir: 1, until: X(28), jumpAt: 753, hold: 0.05 }
  ]), o),

  // 4: Die Tür – Pause für den Steinblock, zwei Mini-Hops, Weitsprung zur Tür.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 500 },
    { wait: 0.75 },
    { dir: 1, until: 635, jumpAt: 522, hold: 0.1 },
    { dir: 1, until: 744, jumpAt: 640, hold: 0.05 },
    { dir: 1, until: X(28), jumpAt: 748, hold: 0.42 },
    { dir: -1, until: 882 }
  ]), o),

  // 5: Steinregen – niemals stehenbleiben, am Ende über die Stacheln.
  (s, i, o) => s(i, bot([
    { dir: 1, until: X(28), jumpAt: 768, hold: 0.35 },
    { dir: -1, until: 880 }
  ]), o),

  // 6: Ehrlich! – Maximalsprung + sofortiger Stachelsprung. Kein Trick.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 474, jumpAt: 314, hold: 0.45 },
    { dir: 1, until: X(28), jumpAt: 470, hold: 0.4 }
  ]), o),

  // 7: Vertrauen – durch die Lügen laufen, präzise über den ehrlichen Rest.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 250 },
    { dir: 1, until: 440 },
    { dir: 1, until: 700, jumpAt: 576, hold: 0.12 },
    { dir: 1, until: 738 },
    { dir: 1, until: X(28), jumpAt: 738, hold: 0.34 },
    { dir: -1, until: 880 }
  ]), o),

  // 8: Aufzug – Plattform unten abpassen, hochreiten, Stachel-Treppe hüpfen.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 210 },
    { waitUntil: (env) => {
        const p = env.rt.platforms[0];
        return p.dir < 0 && p.y > 424;
      } },
    { dir: 1, until: 330, jumpAt: 212, hold: 0.32 },
    { custom: (env, out) => {
        // Mitfahren; sobald die Plattform hoch genug ist, zur Kante wandern
        // und oben rechts auf den Sims treten.
        const p = env.rt.platforms[0];
        if (env.pl.platform) {
          const nearRight = env.pl.x > p.x + p.w - 45;
          if (p.y < 320 && !nearRight) out.right = true; // zur Kante vorarbeiten
          else if (p.y <= 214) out.right = true;         // ganz oben: abtreten
          return false;
        }
        if (!env.pl.onGround) { out.right = true; return false; }
        return env.pl.x > 540 && env.pl.y > 180;
      } },
    { dir: 1, until: 680, jumpAt: 576, hold: 0.05 },
    { dir: 1, until: X(28), jumpAt: 700, hold: 0.2 },
    { dir: -1, until: 848 }
  ], { dodge: true, dodgeDist: 80 }), o),

  // 9: Deckenfresser – ein einziger Sprint; der Startzeitpunkt entscheidet.
  (s, i, o) => {
    let best = null;
    for (let t0 = 0; t0 <= 4; t0 += 1 / 30) {
      const r = s(i, bot([{ wait: t0 }, { dir: 1, until: X(28) }]), o);
      if (r.won) return r;
      if (!best || r.x > best.x) best = r;
    }
    return best;
  },

  // 10: Falltür – Sprint auf kündigendem Boden, zwei getimte Sprünge.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 733, jumpAt: 612, hold: 0.14 },
    { dir: 1, until: X(28), jumpAt: 736, hold: 0.32 }
  ]), o),

  // 11: Federball – über drei Todesfedern hüpfen, die vierte mit vollem
  // Rechtsdrall nehmen (die Decken-Stachelfalle verfehlt einen dann).
  (s, i, o) => s(i, bot([
    { dir: 1, until: 290, jumpAt: 162, hold: 0.12 },
    { dir: 1, until: 450, jumpAt: 322, hold: 0.12 },
    { dir: 1, until: 610, jumpAt: 482, hold: 0.12 },
    { custom: (env, out) => {
        const p = env.pl;
        if (p.onGround) { out.right = true; return p.y < 300; }
        if (p.vy > -900) out.right = true; // erst steigen, dann driften
        return false;
      } },
    { custom: (env, out) => {
        if (env.pl.x < 842) out.right = true;
        else if (env.pl.x > 854) out.left = true;
        return false;
      } }
  ]), o),

  // 12: Geduld – 6 s im Kreuzfeuer ausharren, dann über die Bröselbrücke.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 340 },
    { waitUntil: (env) => env.rt.solid[16][14] === 1 },
    { dir: 1, until: 742 },
    { dir: 1, until: X(28), jumpAt: 746, hold: 0.32 }
  ], { dodge: true }), o),

  // 13: Doppelt gemoppelt – hin über alles, zurück durch alles.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 318, jumpAt: 195, hold: 0.3 },
    { dir: 1, until: 415 },
    { waitUntil: crusherSafe(0, 450) },
    { dir: 1, until: 800 },
    { dir: 1, until: 876, jumpAt: 800, hold: 0.12 },
    { dir: -1, until: 790, jumpAt: 895, hold: 0.1 },
    { dir: -1, until: 645, jumpAt: 770, hold: 0.15 },
    { dir: -1, until: 560 },
    { waitUntil: crusherSafe(0, 450) },
    { dir: -1, until: 425 }
  ]), o),

  // 14: Rückweg – hin ist geschenkt, zurück ist Krieg, oben bröselt es.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 806 },
    { dir: -1, until: 630 },
    { dir: -1, until: 508, jumpAt: 645, hold: 0.15 },
    { dir: -1, until: 360, jumpAt: 450, hold: 0.03 },
    { dir: -1, until: 240, jumpAt: 362, hold: 0.35 },
    { dir: -1, until: 150, jumpAt: 222, hold: 0.4 },
    { dir: 1, until: 142 }
  ]), o),

  // 15: Dunkelheit – zwei Maximalsprünge blind hintereinander.
  (s, i, o) => s(i, bot([
    { wait: 0.45 },
    { dir: 1, until: 368, jumpAt: 226, hold: 0.22 },
    { dir: 1, until: 522, jumpAt: 366, hold: 0.2 },
    { dir: 1, until: 700, jumpAt: 548, hold: 0.32 },
    { dir: 1, until: 745 },
    { dir: 1, until: X(28), jumpAt: 762, hold: 0.34 },
    { dir: -1, until: 885 }
  ]), o),

  // 16: Spießrutenlauf – Sprint mit Pfeil-Hopsern und Block-Hürde.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 352, jumpAt: 236, hold: 0.1 },
    { dir: 1, until: 484, jumpAt: 350, hold: 0.12 },
    { dir: 1, until: 565, jumpAt: 488, hold: 0.1 },
    { dir: 1, until: 722, jumpAt: 630, hold: 0.03 },
    { dir: 1, until: X(28), jumpAt: 728, hold: 0.3 },
    { dir: -1, until: 885 }
  ]), o),

  // 17: Das Finale – Feder rauf, Tür zweimal verscheuchen, Loch überleben.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 280, jumpAt: 160, hold: 0.1 },
    { dir: 1, until: 420, jumpAt: 283, hold: 0.2 },
    { waitUntil: crusherSafe(0, 440) },
    { dir: 1, until: 640 },
    { custom: (env, out) => {
        out.right = true;
        return env.pl.onGround && env.pl.y < 300;
      } },
    { dir: -1, until: 722, jumpAt: 800, hold: 0.06 },
    { dir: 1, until: X(28), jumpAt: 728, hold: 0.08 }
  ], { dodge: true, dodgeDist: 80 }), o),

  // 18: Krümelmonster – Sprint über bröselnden Boden durch drei Stampfer.
  (s, i, o) => {
    let best = null;
    for (let t0 = 0; t0 <= 4; t0 += 1 / 30) {
      const r = s(i, bot([
        { wait: t0 },
        { dir: 1, until: 570, jumpAt: 424, hold: 0.25 },
        { dir: 1, until: X(28), jumpAt: 800, hold: 0.3 },
        { dir: -1, until: 885 }
      ]), o);
      if (r.won) return r;
      if (!best || r.x > best.x) best = r;
    }
    return best;
  },

  // 19: Spiegelverkehrt – der Bot denkt in echten Richtungen, der Executor
  // spiegelt (wie ein Mensch, der umgelernt hat).
  (s, i, o) => s(i, bot([
    { dir: 1, until: 458, jumpAt: 315, hold: 0.2 },
    { dir: 1, until: 672, jumpAt: 520, hold: 0.3 },
    { dir: 1, until: X(28), jumpAt: 700, hold: 0.25 },
    { dir: -1, until: 885 }
  ]), o),

  // 20: Die Mauer – fünf Sprünge, Pfeile frontal, Wand im Nacken.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 338, jumpAt: 200, hold: 0.2 },
    { dir: 1, until: 498, jumpAt: 360, hold: 0.2 },
    { dir: 1, until: 645, jumpAt: 510, hold: 0.2 },
    { dir: 1, until: 753, jumpAt: 648, hold: 0.07 },
    { dir: 1, until: X(28), jumpAt: 756, hold: 0.3 },
    { dir: -1, until: 885 }
  ], { dodge: true, dodgeDist: 85 }), o),

  // 21: Türsteher – alle Lügentüren überhüpfen, hinten anklopfen, zurückrennen.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 340, jumpAt: 210, hold: 0.12 },
    { dir: 1, until: 530, jumpAt: 400, hold: 0.12 },
    { dir: 1, until: 720, jumpAt: 592, hold: 0.12 },
    { waitUntil: crusherSafe(0, 450) },
    { dir: 1, until: 850 },
    { waitUntil: crusherSafe(0, 450) },
    { dir: -1, until: 700 },
    { dir: -1, until: 520, jumpAt: 680, hold: 0.12 },
    { dir: -1, until: 330, jumpAt: 500, hold: 0.12 },
    { dir: -1, until: 170, jumpAt: 300, hold: 0.12 },
    { custom: (env, out) => {
        if (env.pl.x < 132) out.right = true;
        else if (env.pl.x > 152) out.left = true;
        return false;
      } }
  ], { dodge: true, dodgeDist: 85 }), o),

  // 22: Blindflug – Loch, Bröselstrecke, Stachel+Weltdrehung, Schlusssprung.
  // Pfeile werden über das Streckentiming abgehandelt, nicht reaktiv.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 412, jumpAt: 264, hold: 0.25 },
    { dir: 1, until: 700, jumpAt: 555, hold: 0.2 },
    { dir: 1, until: X(28), jumpAt: 733, hold: 0.3 },
    { dir: -1, until: 885 }
  ]), o),

  // 23: Das wahre Finale – alles auf einmal, in einer Linie.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 276, jumpAt: 136, hold: 0.22 },
    { dir: 1, until: 400, jumpAt: 280, hold: 0.1 },
    { waitUntil: crusherSafe(0, 260) },
    { dir: 1, until: 600, jumpAt: 404, hold: 0.28 },
    { dir: 1, until: 782, jumpAt: 652, hold: 0.14 },
    { dir: 1, until: X(28), jumpAt: 790, hold: 0.3 },
    { dir: -1, until: 885 }
  ], { dodge: true, dodgeDist: 80 }), o),

  // 24: Danke fürs Spielen – hin, kurz sammeln (Pfeil-Timing!), zurück,
  // die Pfeile unter den geplanten Sprüngen durchfliegen lassen.
  (s, i, o) => s(i, bot([
    { dir: 1, until: 473, jumpAt: 330, hold: 0.2 },
    { dir: 1, until: 745, jumpAt: 522, hold: 0.2 },
    { dir: -1, until: 540, jumpAt: 672, hold: 0.12 },
    { dir: -1, until: 342, jumpAt: 485, hold: 0.22 },
    { dir: -1, until: 220, jumpAt: 338, hold: 0.3 },
    { custom: (env, out) => {
        if (env.pl.x < 196) out.right = true;
        else if (env.pl.x > 216) out.left = true;
        return false;
      } }
  ]), o)
];
