// Spielsteuerung: State-Machine, Fixed-Timestep-Loop, Canvas-Skalierung, Start.
var Game = (function () {
  var STEP = 1 / 60;
  var canvas, ctx;
  var state = 'title'; // title | levels | playing | dead | complete | win
  var levelIndex = 0;
  var rt = null;
  var timer = 0;
  var lastTime = 0;
  var acc = 0;
  var portraitQuery = null;

  function boot() {
    canvas = document.getElementById('game');
    var rs = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = CONFIG.WIDTH * rs;
    canvas.height = CONFIG.HEIGHT * rs;
    ctx = canvas.getContext('2d');
    ctx.setTransform(rs, 0, 0, rs, 0, 0);

    Save.load();
    I18N.init();
    Renderer.init(ctx);
    Input.init();
    UI.init();
    portraitQuery = window.matchMedia('(orientation: portrait)');

    UI.showTitle();
    requestAnimationFrame(frame);
  }

  function inLevel() {
    return state === 'playing' || state === 'dead' || state === 'complete';
  }

  function loadLevel(i) {
    levelIndex = i;
    rt = Traps.buildRuntime(LEVELS[i]);
    Player.reset(rt);
    state = 'playing';
    acc = 0;
    UI.showGame(i);
  }

  function respawn() {
    rt = Traps.buildRuntime(LEVELS[levelIndex]);
    Player.reset(rt);
    state = 'playing';
  }

  // Spott bei Todes-Meilensteinen – Verzweiflung gehört zum Konzept.
  // Die Sprüche selbst stehen zweisprachig in js/i18n.js.
  function tauntMilestone(deaths) {
    return deaths === 5 || deaths === 10 || deaths === 20 || deaths === 35 ||
      (deaths >= 50 && deaths % 25 === 0);
  }

  function die() {
    var S = Player.state;
    Sfx.death();
    Renderer.deathBurst(S.x + S.w / 2, S.y + S.h / 2);
    Save.addDeath(levelIndex);
    UI.updateDeaths(levelIndex);
    var deaths = Save.get().deaths[levelIndex] | 0;
    if (tauntMilestone(deaths)) {
      var taunts = I18N.t('taunts');
      var msg = taunts[(deaths + levelIndex) % taunts.length].replace('{n}', deaths);
      UI.toast(msg, true);
    }
    state = 'dead';
    timer = CONFIG.DEATH_FREEZE;
  }

  function complete() {
    Sfx.door();
    Save.unlock(levelIndex + 2);
    state = 'complete';
    timer = 1.2;
    UI.toast(I18N.t('done'));
  }

  function step(dt) {
    if (state === 'playing') {
      if (Input.consumeEsc()) { gotoLevels(); return; }
      if (Input.consumeRestart()) { respawn(); return; }
      var input = Input.state();
      Traps.preUpdate(rt, dt);
      var ev = Player.update(rt, input, dt);
      var res = Traps.postUpdate(rt, Player.state, dt);
      if (res.kill || ev.fellOut) { die(); return; }
      if (ev.won) { complete(); return; }
    } else if (state === 'dead') {
      timer -= dt;
      if (timer <= 0) respawn();
    } else if (state === 'complete') {
      timer -= dt;
      if (timer <= 0) {
        if (levelIndex + 1 >= LEVELS.length) {
          state = 'win';
          UI.showWin();
        } else {
          var chapterChanged = LEVELS[levelIndex + 1].chapter !== LEVELS[levelIndex].chapter;
          loadLevel(levelIndex + 1);
          if (chapterChanged) {
            var m = I18N.t('chapterMsgs')[LEVELS[levelIndex].chapter];
            if (m) UI.toast(m, true);
          }
        }
      }
    }
  }

  function frame(t) {
    requestAnimationFrame(frame);
    var dt = Math.min((t - lastTime) / 1000, 0.1);
    lastTime = t;
    if (!(dt > 0)) return;

    // Im Hochformat: Spiel pausiert (Dreh-Overlay ist sichtbar)
    var paused = portraitQuery && portraitQuery.matches;

    if (inLevel() && !paused) {
      acc += dt;
      while (acc >= STEP) {
        step(STEP);
        acc -= STEP;
      }
    }

    Renderer.updateParticles(dt);
    if (rt && inLevel()) {
      Renderer.draw(rt, Player.state, state !== 'dead');
    } else if (rt) {
      Renderer.draw(rt, null, false); // Hintergrund hinter Menü-Overlays
    } else {
      // Noch kein Level geladen: leerer dunkler Hintergrund
      ctx.fillStyle = CONFIG.COLORS.bg;
      ctx.fillRect(0, 0, CONFIG.WIDTH, CONFIG.HEIGHT);
    }
  }

  function startFromSave() {
    var unlocked = Save.get().unlocked;
    loadLevel(Math.min(unlocked, LEVELS.length) - 1);
  }

  function gotoTitle() {
    state = 'title';
    UI.showTitle();
  }

  function gotoLevels() {
    state = 'levels';
    UI.showLevels();
  }

  function restart() {
    if (inLevel()) respawn();
  }

  document.addEventListener('DOMContentLoaded', boot);

  var api = {
    startFromSave: startFromSave,
    loadLevel: loadLevel,
    gotoTitle: gotoTitle,
    gotoLevels: gotoLevels,
    restart: restart
  };

  // Debug-/Test-Haken für automatisierte Tests
  window.__game = {
    get state() { return state; },
    get levelIndex() { return levelIndex; },
    get rt() { return rt; },
    get player() { return Player.state; },
    loadLevel: loadLevel,
    completeLevel: complete,
    respawn: respawn,
    save: Save,
    input: Input
  };

  return api;
})();
