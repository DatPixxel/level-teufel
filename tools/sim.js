// Headless-Simulation: lädt die ECHTE Spiellogik (config/traps/player/levels)
// in Node und beweist mit Experten-Bots, dass jedes Level schaffbar ist.
//
//   node tools/sim.js            – alle Level mit Experten-Bots
//   node tools/sim.js 10         – nur Level 10 (1-basiert)
//   node tools/sim.js 10 trace   – mit Frame-Trace zum Debuggen
//   node tools/sim.js naive      – Naiv-Bot (rennt nur + springt über Sichtbares)
'use strict';
const fs = require('fs');
const path = require('path');
const vm = require('vm');

const root = path.join(__dirname, '..');
const noop = function () {};
const ctx = {
  console: console,
  window: {},
  Renderer: { poof: noop, deathBurst: noop },
  Sfx: { trap: noop, poof: noop, jump: noop, spring: noop, death: noop, door: noop, unlock: noop }
};
vm.createContext(ctx);
['js/config.js', 'js/traps.js', 'js/player.js', 'js/levels.js'].forEach(function (f) {
  vm.runInContext(fs.readFileSync(path.join(root, f), 'utf8'), ctx, { filename: f });
});
const CONFIG = ctx.CONFIG, Traps = ctx.Traps, Player = ctx.Player, LEVELS = ctx.LEVELS;
const T = CONFIG.TILE, DT = 1 / 60;

// ---------------------------------------------------------------------------
// Simulation eines Levels mit einem Controller.
// Controller: fn(env) -> {left,right,jumpHeld,jumpPressed}
// env = { rt, pl, t } – Reihenfolge wie in main.js: pre, player, post.
// ---------------------------------------------------------------------------
function simulate(levelIndex, controller, opts) {
  opts = opts || {};
  const rt = Traps.buildRuntime(LEVELS[levelIndex]);
  const pl = Player.reset(rt);
  const trace = [];
  for (let frame = 0; frame * DT < (opts.maxTime || 45); frame++) {
    let io = controller({ rt: rt, pl: pl, t: rt.time });
    // Spiegel-Kompensation: der Bot denkt in echten Richtungen,
    // ein menschlicher Spieler lernt das Umdenken ebenfalls.
    if (rt.flipUntil > rt.time) {
      io = { left: io.right, right: io.left, jumpHeld: io.jumpHeld, jumpPressed: io.jumpPressed };
    }
    Traps.preUpdate(rt, DT);
    const ev = Player.update(rt, io, DT);
    const res = Traps.postUpdate(rt, pl, DT);
    if (opts.trace && frame % 6 === 0) {
      trace.push({ t: +rt.time.toFixed(2), x: +pl.x.toFixed(0), y: +pl.y.toFixed(0), g: pl.onGround ? 1 : 0 });
    }
    if (res.kill || ev.fellOut) {
      return { won: false, t: rt.time, x: pl.x, y: pl.y, cause: res.kill ? 'kill' : 'fell', trace: trace };
    }
    if (ev.won) return { won: true, t: rt.time, x: pl.x, y: pl.y, trace: trace };
  }
  return { won: false, t: opts.maxTime || 45, x: pl.x, y: pl.y, cause: 'timeout', trace: trace };
}

// ---------------------------------------------------------------------------
// Bot-Baukasten: Schrittfolgen + reaktives Pfeil-Ausweichen.
// Schritte:
//   { dir, until }           – laufen bis pl.x die Marke erreicht (px)
//   { dir, until, jumpAt, hold } – dabei bei x>=jumpAt (am Boden) springen
//   { wait }                 – Sekunden stehenbleiben
//   { waitUntil: fn }        – stehen bis Bedingung
//   { custom: fn }           – eigene Frame-Logik; fertig, wenn fn true liefert
// ---------------------------------------------------------------------------
function bot(steps, opts) {
  opts = opts || {};
  let i = 0;
  let jumpUntil = -1;
  let jumped = false;
  let waitEnd = null;
  return function (env) {
    const rt = env.rt, pl = env.pl;
    const out = { left: false, right: false, jumpHeld: false, jumpPressed: false };

    // Reaktiv: nahenden Pfeilen mit einem kurzen Hüpfer ausweichen
    if (opts.dodge && pl.onGround && rt.time > jumpUntil) {
      for (let k = 0; k < rt.projectiles.length; k++) {
        const pr = rt.projectiles[k];
        if (!(pr.y < pl.y + pl.h + 6 && pr.y + pr.h > pl.y - 6)) continue; // keine vertikale Überlappung möglich
        const gap = pr.vx > 0 ? pl.x - (pr.x + pr.w) : pr.x - (pl.x + pl.w);
        const coming = pr.vx > 0 ? pr.x < pl.x + pl.w : pr.x + pr.w > pl.x;
        if (coming && gap > -6 && gap < (opts.dodgeDist || 100)) {
          out.jumpPressed = true;
          jumpUntil = rt.time + (opts.dodgeHold || 0.18);
        }
      }
    }
    if (rt.time <= jumpUntil) out.jumpHeld = true;

    if (i >= steps.length) return out;
    const s = steps[i];

    function advance() { i++; jumped = false; waitEnd = null; }

    if (s.wait != null) {
      if (waitEnd === null) waitEnd = rt.time + s.wait;
      if (rt.time >= waitEnd) advance();
      return out;
    }
    if (s.waitUntil) {
      if (s.waitUntil(env)) advance();
      return out;
    }
    if (s.custom) {
      if (s.custom(env, out)) advance();
      return out;
    }

    // Laufschritt
    if (s.dir > 0) out.right = true;
    else if (s.dir < 0) out.left = true;

    if (s.jumpAt != null && !jumped && (pl.onGround || pl.coyote > 0)) {
      const past = s.dir >= 0 ? pl.x >= s.jumpAt : pl.x <= s.jumpAt;
      if (past) {
        out.jumpPressed = true;
        jumpUntil = rt.time + (s.hold != null ? s.hold : 0.34);
        out.jumpHeld = true;
        jumped = true;
      }
    }
    const arrived = s.dir >= 0 ? pl.x >= s.until : pl.x <= s.until;
    if (arrived && (s.air ? true : pl.onGround)) advance();
    return out;
  };
}

// Tile-Koordinate -> Pixel (linke Kante)
function X(c) { return c * T; }

// ---------------------------------------------------------------------------
// Naiv-Bot: rennt nur nach rechts und springt vor SICHTBAREN Lücken/Stacheln.
// Er kennt keine Fallen. Wenn er ein Level schafft, ist es zu leicht.
// ---------------------------------------------------------------------------
function naiveBot() {
  let jumpUntil = -1;
  return function (env) {
    const rt = env.rt, pl = env.pl;
    const out = { left: false, right: true, jumpHeld: rt.time <= jumpUntil, jumpPressed: false };
    if (!pl.onGround) return out;
    const footR = Math.floor((pl.y + pl.h + 1) / T);
    const aheadC = Math.floor((pl.x + pl.w + 12) / T);
    let danger = false;
    if (footR >= 0 && footR < CONFIG.ROWS && !Traps.isSolid(rt, aheadC, footR)) danger = true;
    for (let k = 0; k < rt.spikes.length; k++) {
      const s = rt.spikes[k];
      if (s.r === footR - 1 && s.c >= aheadC && s.c <= aheadC + 1) danger = true;
    }
    if (danger) {
      out.jumpPressed = true;
      jumpUntil = rt.time + 0.3;
      out.jumpHeld = true;
    }
    return out;
  };
}

module.exports = { simulate: simulate, bot: bot, naiveBot: naiveBot, X: X, CONFIG: CONFIG, LEVELS: LEVELS, DT: DT };

// ---------------------------------------------------------------------------
// CLI
// ---------------------------------------------------------------------------
if (require.main === module) {
  const experts = require('./bots.js');
  const arg = process.argv[2];
  const doTrace = process.argv.indexOf('trace') >= 0;

  if (arg === 'naive') {
    let survived = 0;
    for (let i = 0; i < LEVELS.length; i++) {
      const r = simulate(i, naiveBot(), { maxTime: 30 });
      if (r.won) { survived++; console.log('⚠️  Level ' + (i + 1) + ' (' + LEVELS[i].name + '): Naiv-Bot GEWINNT in ' + r.t.toFixed(1) + 's'); }
      else console.log('☠️  Level ' + (i + 1) + ' (' + LEVELS[i].name + '): tot bei x=' + r.x.toFixed(0) + ' (' + r.cause + ', t=' + r.t.toFixed(1) + 's)');
    }
    console.log('\nNaiv-Bot überlebt ' + survived + '/' + LEVELS.length + ' Level.');
    process.exit(survived > 1 ? 1 : 0);
  }

  const only = arg ? [parseInt(arg, 10) - 1] : LEVELS.length ? Array.from({ length: LEVELS.length }, (_, i) => i) : [];
  let fails = 0;
  only.forEach(function (i) {
    const make = experts[i];
    if (!make) { console.log('—  Level ' + (i + 1) + ': kein Experten-Bot definiert'); fails++; return; }
    const r = make(simulate, i, { trace: doTrace });
    if (r.won) {
      console.log('✅ Level ' + (i + 1) + ' (' + LEVELS[i].name + '): geschafft in ' + r.t.toFixed(2) + 's');
    } else {
      fails++;
      console.log('❌ Level ' + (i + 1) + ' (' + LEVELS[i].name + '): ' + r.cause + ' bei x=' + r.x.toFixed(0) + ' y=' + r.y.toFixed(0) + ' t=' + r.t.toFixed(2) + 's');
      if (doTrace && r.trace) console.log(r.trace.map(p => p.t + 's x' + p.x + ' y' + p.y + (p.g ? 'G' : '')).join(' | '));
    }
  });
  process.exit(fails ? 1 : 0);
}
