// Zweisprachigkeit (Deutsch/Englisch): alle sichtbaren Texte an einem Ort.
// Sprachwahl: gespeicherte Wahl > Browser-Sprache > Englisch.
var I18N = (function () {
  var STR = {
    de: {
      docTitle: "Level Teufel – das gemeine Jump'n'Run",
      titleSub: 'Ein gemeines kleines Spiel 😈',
      play: 'Spielen',
      resume: 'Weiterspielen',
      levelSelect: 'Levelauswahl',
      soundOn: 'Ton: an 🔊',
      soundOff: 'Ton: aus 🔇',
      langBtn: '🌐 Deutsch',
      hint: 'Steuerung: Pfeiltasten / WASD + Leertaste · Handy: links Daumen auflegen & ziehen = laufen, rechts tippen = springen',
      back: 'Zurück',
      restartTitle: 'Neustart (R)',
      backTitle: 'Levelauswahl',
      winTitle: 'GESCHAFFT! 🎉',
      winSub: 'Du hast alle 24 Level überlebt.',
      winDeaths: 'Gesamte Tode: ',
      backToTitle: 'Zurück zum Titel',
      resetProgress: 'Fortschritt löschen',
      confirmReset: 'Wirklich den kompletten Fortschritt löschen?',
      rotate: 'Bitte dreh dein Handy quer!',
      done: 'Geschafft!',
      flipped: '↔ Steuerung vertauscht!',
      chapters: {
        1: 'Kapitel 1 · Willkommen',
        2: 'Kapitel 2 · Vertrauen ist gut',
        3: 'Kapitel 3 · Bosheit',
        4: 'Kapitel 4 · Hölle'
      },
      chapterMsgs: {
        2: 'Kapitel 2 — es wird schlimmer.',
        3: 'Kapitel 3 — es wird viel schlimmer.',
        4: 'Kapitel 4 — willkommen in der Hölle. 😈'
      },
      taunts: [
        '{n} Tode. Die Tür lacht dich aus.',
        'Schon {n} Tode? Das Level ist 20 Sekunden lang.',
        '{n}. Tod. Der Boden dankt für deine Treue.',
        'Tipp: Einfach nicht sterben. ({n} Tode)',
        '{n} Tode. Der Stachel kennt dich jetzt beim Namen.',
        '{n} Tode. Es wird nicht leichter. Versprochen.'
      ],
      ranks: [
        'Rang: Der Teufel persönlich 😈',
        'Rang: Schmerzresistent 🩹',
        'Rang: Sturkopf 🪨',
        'Rang: Unsterblich — aus Übung 💀'
      ]
    },
    en: {
      docTitle: 'Level Teufel – the mean platformer',
      titleSub: 'A mean little game 😈',
      play: 'Play',
      resume: 'Continue',
      levelSelect: 'Level Select',
      soundOn: 'Sound: on 🔊',
      soundOff: 'Sound: off 🔇',
      langBtn: '🌐 English',
      hint: 'Controls: arrow keys / WASD + space · Phone: rest your thumb on the left half & drag to run, tap the right half to jump',
      back: 'Back',
      restartTitle: 'Restart (R)',
      backTitle: 'Level select',
      winTitle: 'YOU MADE IT! 🎉',
      winSub: 'You survived all 24 levels.',
      winDeaths: 'Total deaths: ',
      backToTitle: 'Back to Title',
      resetProgress: 'Delete Progress',
      confirmReset: 'Really delete all your progress?',
      rotate: 'Please rotate your phone!',
      done: 'Level clear!',
      flipped: '↔ Controls flipped!',
      chapters: {
        1: 'Chapter 1 · Welcome',
        2: 'Chapter 2 · Trust Is Good',
        3: 'Chapter 3 · Malice',
        4: 'Chapter 4 · Hell'
      },
      chapterMsgs: {
        2: 'Chapter 2 — it gets worse.',
        3: 'Chapter 3 — it gets much worse.',
        4: 'Chapter 4 — welcome to hell. 😈'
      },
      taunts: [
        '{n} deaths. The door is laughing at you.',
        '{n} deaths already? This level is 20 seconds long.',
        'Death no. {n}. The floor thanks you for your loyalty.',
        'Pro tip: just stop dying. ({n} deaths)',
        '{n} deaths. The spikes know you by name now.',
        "{n} deaths. It doesn't get easier. Promise."
      ],
      ranks: [
        'Rank: The Devil Himself 😈',
        'Rank: Pain-Resistant 🩹',
        'Rank: Stubborn as a Rock 🪨',
        'Rank: Immortal — Through Practice 💀'
      ]
    }
  };

  // Level-Namen: Schlüssel = deutscher Name aus levels.js.
  // Fehlt eine Übersetzung, bleibt der deutsche Name stehen (kein Absturz).
  var LEVEL_EN = {
    'Spaziergang': 'A Walk in the Park',
    'Sprungstunde': 'Jumping Lessons',
    'Spitzen': 'Spikes',
    'Die Tür': 'The Door',
    'Steinregen': 'Rock Rain',
    'Ehrlich!': 'Honest!',
    'Vertrauen': 'Trust',
    'Aufzug': 'The Elevator',
    'Deckenfresser': 'Ceiling Muncher',
    'Falltür': 'Trapdoor',
    'Federball': 'Spring Fever',
    'Geduld': 'Patience',
    'Doppelt gemoppelt': 'Double Trouble',
    'Rückweg': 'The Way Back',
    'Dunkelheit': 'Darkness',
    'Spießrutenlauf': 'The Gauntlet',
    'Das Finale': 'The Finale',
    'Krümelmonster': 'Crumble Monster',
    'Spiegelverkehrt': 'Mirror Mode',
    'Die Mauer': 'The Wall',
    'Türsteher': 'The Bouncer',
    'Blindflug': 'Flying Blind',
    'Das wahre Finale': 'The True Finale',
    'Danke fürs Spielen': 'Thanks for Playing'
  };

  var lang = 'en';

  function init() {
    var saved = Save.get().lang;
    if (saved === 'de' || saved === 'en') {
      lang = saved;
    } else {
      var nav = (navigator.language || navigator.userLanguage || 'en').toLowerCase();
      lang = nav.indexOf('de') === 0 ? 'de' : 'en';
    }
  }

  function t(key) {
    var v = STR[lang][key];
    return v === undefined ? STR.de[key] : v;
  }

  function levelName(lv) {
    if (lang === 'en' && LEVEL_EN[lv.name]) return LEVEL_EN[lv.name];
    return lv.name;
  }

  function toggle() {
    lang = lang === 'de' ? 'en' : 'de';
    Save.setLang(lang);
  }

  return {
    init: init,
    t: t,
    levelName: levelName,
    toggle: toggle,
    lang: function () { return lang; }
  };
})();
