// Speicherstand im localStorage (übersteht Browser-Neustarts).
// Alles in try/catch: im Privatmodus kann localStorage fehlen.
var Save = (function () {
  var KEY = 'newgamestyle.leveldevil.v1';

  var data = {
    unlocked: 1,        // höchstes freigeschaltetes Level (1-basiert)
    deaths: [],         // Tode pro Level
    totalDeaths: 0,
    muted: false
  };

  function load() {
    try {
      var raw = localStorage.getItem(KEY);
      if (raw) {
        var parsed = JSON.parse(raw);
        if (parsed && typeof parsed === 'object') {
          data.unlocked = Math.max(1, parsed.unlocked | 0);
          data.deaths = Array.isArray(parsed.deaths) ? parsed.deaths : [];
          data.totalDeaths = parsed.totalDeaths | 0;
          data.muted = !!parsed.muted;
        }
      }
    } catch (e) { /* Speichern nicht verfügbar */ }
    return data;
  }

  function persist() {
    try {
      localStorage.setItem(KEY, JSON.stringify(data));
    } catch (e) { /* ignorieren */ }
  }

  function addDeath(levelIndex) {
    data.deaths[levelIndex] = (data.deaths[levelIndex] | 0) + 1;
    data.totalDeaths += 1;
    persist();
  }

  function unlock(levelNumber) {
    if (levelNumber > data.unlocked) {
      data.unlocked = levelNumber;
      persist();
    }
  }

  function setMuted(m) {
    data.muted = !!m;
    persist();
  }

  function reset() {
    data.unlocked = 1;
    data.deaths = [];
    data.totalDeaths = 0;
    persist();
  }

  return {
    load: load,
    persist: persist,
    addDeath: addDeath,
    unlock: unlock,
    setMuted: setMuted,
    reset: reset,
    get: function () { return data; }
  };
})();
