// DOM-Overlays: Titel, Levelauswahl, HUD, Sieg-Screen.
// Alle sichtbaren Texte kommen aus js/i18n.js (Deutsch/Englisch).
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
    return Save.get().muted ? I18N.t('soundOff') : I18N.t('soundOn');
  }

  // Statische Texte (HTML-Grundgerüst) in der aktuellen Sprache setzen.
  function applyLanguage() {
    document.documentElement.lang = I18N.lang();
    document.title = I18N.t('docTitle');
    $('title-sub').textContent = I18N.t('titleSub');
    $('title-hint').textContent = I18N.t('hint');
    $('btn-select').textContent = I18N.t('levelSelect');
    $('btn-lang').textContent = I18N.t('langBtn');
    $('levels-title').textContent = I18N.t('levelSelect');
    $('btn-levels-back').textContent = I18N.t('back');
    $('win-title').textContent = I18N.t('winTitle');
    $('win-sub').textContent = I18N.t('winSub');
    $('btn-win-title').textContent = I18N.t('backToTitle');
    $('btn-win-reset').textContent = I18N.t('resetProgress');
    $('btn-hud-restart').title = I18N.t('restartTitle');
    $('btn-hud-back').title = I18N.t('backTitle');
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
    $('btn-lang').addEventListener('click', function () {
      Sfx.click();
      I18N.toggle();
      applyLanguage();
      showTitle();
    });
    $('btn-levels-back').addEventListener('click', function () { Sfx.click(); Game.gotoTitle(); });
    $('btn-hud-restart').addEventListener('click', function () { Sfx.click(); Game.restart(); });
    $('btn-hud-back').addEventListener('click', function () { Sfx.click(); Game.gotoLevels(); });
    $('btn-win-title').addEventListener('click', function () { Sfx.click(); Game.gotoTitle(); });
    $('btn-win-reset').addEventListener('click', function () {
      if (window.confirm(I18N.t('confirmReset'))) {
        Save.reset();
        Game.gotoTitle();
      }
    });

    applyLanguage();
  }

  function showTitle() {
    hideAll();
    $('btn-sound').textContent = soundLabel();
    var d = Save.get();
    $('btn-start').textContent = d.unlocked > 1 ? I18N.t('resume') : I18N.t('play');
    els.title.classList.remove('hidden');
  }

  function showLevels() {
    hideAll();
    els.grid.innerHTML = '';
    var d = Save.get();
    var chapters = I18N.t('chapters');
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
    els.hudName.textContent = (i + 1) + '. ' + I18N.levelName(lv);
    updateDeaths(i);
    els.hud.classList.remove('hidden');
    els.controls.classList.remove('hidden');
  }

  function updateDeaths(i) {
    var d = Save.get();
    els.hudDeaths.textContent = '💀 ' + (d.deaths[i] | 0);
  }

  function rankFor(deaths) {
    var ranks = I18N.t('ranks');
    if (deaths < 150) return ranks[0];
    if (deaths < 350) return ranks[1];
    if (deaths < 700) return ranks[2];
    return ranks[3];
  }

  function showWin() {
    hideAll();
    els.winDeaths.textContent = I18N.t('winDeaths') + Save.get().totalDeaths + ' 💀';
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
