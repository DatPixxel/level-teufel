// DOM-Overlays: Titel, Levelauswahl, HUD, Sieg-Screen. Alles auf Deutsch.
var UI = (function () {
  var els = {};

  function $(id) { return document.getElementById(id); }

  function hideAll() {
    ['title', 'levels', 'win', 'hud', 'controls'].forEach(function (k) {
      els[k].classList.add('hidden');
    });
    els.toast.classList.add('hidden');
  }

  function soundLabel() {
    return Save.get().muted ? 'Ton: aus 🔇' : 'Ton: an 🔊';
  }

  function init() {
    els.title = $('screen-title');
    els.levels = $('screen-levels');
    els.win = $('screen-win');
    els.hud = $('hud');
    els.toast = $('toast');
    els.controls = $('touch-controls');
    els.hudName = $('hud-name');
    els.hudDeaths = $('hud-deaths');
    els.grid = $('level-grid');
    els.winDeaths = $('win-deaths');

    $('btn-start').addEventListener('click', function () { Sfx.click(); Game.startFromSave(); });
    $('btn-select').addEventListener('click', function () { Sfx.click(); Game.gotoLevels(); });
    $('btn-sound').addEventListener('click', function () {
      Save.setMuted(!Save.get().muted);
      $('btn-sound').textContent = soundLabel();
      Sfx.click();
    });
    $('btn-levels-back').addEventListener('click', function () { Sfx.click(); Game.gotoTitle(); });
    $('btn-hud-restart').addEventListener('click', function () { Sfx.click(); Game.restart(); });
    $('btn-hud-back').addEventListener('click', function () { Sfx.click(); Game.gotoLevels(); });
    $('btn-win-title').addEventListener('click', function () { Sfx.click(); Game.gotoTitle(); });
    $('btn-win-reset').addEventListener('click', function () {
      if (window.confirm('Wirklich den kompletten Fortschritt löschen?')) {
        Save.reset();
        Game.gotoTitle();
      }
    });
  }

  function showTitle() {
    hideAll();
    $('btn-sound').textContent = soundLabel();
    var d = Save.get();
    $('btn-start').textContent = d.unlocked > 1 ? 'Weiterspielen' : 'Spielen';
    els.title.classList.remove('hidden');
  }

  function showLevels() {
    hideAll();
    els.grid.innerHTML = '';
    var d = Save.get();
    var chapters = { 1: 'Kapitel 1 · Willkommen', 2: 'Kapitel 2 · Vertrauen ist gut', 3: 'Kapitel 3 · Bosheit', 4: 'Kapitel 4 · Hölle' };
    var currentChapter = 0;
    LEVELS.forEach(function (lv, i) {
      if (lv.chapter !== currentChapter) {
        currentChapter = lv.chapter;
        var h = document.createElement('div');
        h.className = 'chapter-header';
        h.textContent = chapters[currentChapter];
        els.grid.appendChild(h);
      }
      var unlocked = i + 1 <= d.unlocked;
      var btn = document.createElement('button');
      btn.className = 'level-btn' + (unlocked ? '' : ' locked');
      var deaths = d.deaths[i] | 0;
      btn.innerHTML = unlocked
        ? '<span class="lv-num">' + (i + 1) + '</span><span class="lv-deaths">' + (deaths > 0 ? '💀' + deaths : '') + '</span>'
        : '<span class="lv-num">🔒</span>';
      if (unlocked) {
        btn.addEventListener('click', function () { Sfx.click(); Game.loadLevel(i); });
      }
      els.grid.appendChild(btn);
    });
    els.levels.classList.remove('hidden');
  }

  function showGame(i) {
    hideAll();
    var lv = LEVELS[i];
    els.hudName.textContent = (i + 1) + '. ' + lv.name;
    updateDeaths(i);
    els.hud.classList.remove('hidden');
    els.controls.classList.remove('hidden');
  }

  function updateDeaths(i) {
    var d = Save.get();
    els.hudDeaths.textContent = '💀 ' + (d.deaths[i] | 0);
  }

  function rankFor(deaths) {
    if (deaths < 150) return 'Rang: Der Teufel persönlich 😈';
    if (deaths < 350) return 'Rang: Schmerzresistent 🩹';
    if (deaths < 700) return 'Rang: Sturkopf 🪨';
    return 'Rang: Unsterblich — aus Übung 💀';
  }

  function showWin() {
    hideAll();
    els.winDeaths.textContent = 'Gesamte Tode: ' + Save.get().totalDeaths + ' 💀';
    var rankEl = document.getElementById('win-rank');
    if (rankEl) rankEl.textContent = rankFor(Save.get().totalDeaths);
    els.win.classList.remove('hidden');
  }

  var toastTimeout = null;
  function toast(msg, long) {
    els.toast.textContent = msg;
    els.toast.classList.remove('hidden');
    if (toastTimeout) clearTimeout(toastTimeout);
    toastTimeout = setTimeout(function () { els.toast.classList.add('hidden'); }, long ? 2200 : 1100);
  }

  return {
    init: init,
    showTitle: showTitle,
    showLevels: showLevels,
    showGame: showGame,
    showWin: showWin,
    updateDeaths: updateDeaths,
    toast: toast
  };
})();
