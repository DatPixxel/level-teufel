// Alle 18 Level. Ein Level = ASCII-Grid (17 Zeilen x 30 Zeichen) + Fallen.
//   '#' = solide   '.' = leer      'P' = Start    'D' = Tür
//   '^' 'v' '<' '>' = Stacheln     '~' = Fake-Boden (sieht solide aus)
// Fallen-Zellen sind [Spalte, Zeile]. Trigger-Bereiche in Tile-Koordinaten.
var LEVELS = [];

// Häufige Zeilen als Kürzel
var W = '##############################'; // Vollwand
var E = '#............................#'; // leer mit Seitenwänden

// ================= KAPITEL 1: WILLKOMMEN =================

// 1: Ein harmloser Spaziergang ... bis kurz vor der Tür.
LEVELS.push({
  name: 'Spaziergang', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '########################~~####'],
  traps: []
});

// 2: Die Plattform in der Mitte hält ... kurz.
LEVELS.push({
  name: 'Sprungstunde', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#..........##................#',
    '#.P.......................D..#',
    '#########......####...########'],
  traps: [
    { type: 'vanish', cells: [[11, 14], [12, 14]], trigger: { kind: 'onLand', x: 10, y: 12, w: 4, h: 2 } }
  ]
});

// 3: Der bequeme Weg durch die Stacheln ist eine Lüge. Oben herum!
LEVELS.push({
  name: 'Spitzen', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E,
    '#............###.............#',
    '#........###.....###.........#',
    E,
    '#.P.....^^^^....^^^^^......D.#',
    W],
  traps: [
    { type: 'popSpikes', dir: 'up', cells: [[12, 15], [13, 15], [14, 15], [15, 15]],
      trigger: { kind: 'zone', x: 6, y: 12, w: 1, h: 4 } }
  ]
});

// 4: Die Tür hat Angst vor dir. Einmal.
LEVELS.push({
  name: 'Die Tür', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P...........D..............#',
    W],
  traps: [
    { type: 'doorMove', to: [26, 15], trigger: { kind: 'zone', x: 11, y: 12, w: 2, h: 4 } }
  ]
});

// 5: Bleib bloß nicht stehen.
LEVELS.push({
  name: 'Steinregen', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    W],
  traps: [
    { type: 'fallBlock', cell: [8, 1], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [11, 1], trigger: { kind: 'zone', x: 10, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [14, 1], trigger: { kind: 'zone', x: 13, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [17, 1], trigger: { kind: 'zone', x: 16, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [20, 1], trigger: { kind: 'zone', x: 19, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [23, 1], trigger: { kind: 'zone', x: 22, y: 12, w: 1, h: 4 } }
  ]
});

// 6: Kein Witz. Wirklich keiner. Versprochen.
LEVELS.push({
  name: 'Ehrlich!', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P.............^^^........D.#',
    '##########...#################'],
  traps: []
});

// ================= KAPITEL 2: VERTRAUEN IST GUT =================

// 7: Die Stacheln weichen zurück. Der letzte Sprung ... lass es einfach.
LEVELS.push({
  name: 'Vertrauen', chapter: 2,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P......^^^...^^^.........D.#',
    W],
  traps: [
    { type: 'hideSpikes', cells: [[9, 15], [10, 15], [11, 15]], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'hideSpikes', cells: [[15, 15], [16, 15], [17, 15]], trigger: { kind: 'zone', x: 13, y: 12, w: 1, h: 4 } },
    { type: 'invisibleWall', cells: [[22, 11], [22, 12], [22, 13]] }
  ]
});

// 8: Der Aufzug fährt dahin, wo ER will.
LEVELS.push({
  name: 'Aufzug', chapter: 2,
  grid: [W, E, E, E, E, E,
    '#.........................D..#',
    '#................#############',
    '#................#############',
    '#................#############',
    '#................#############',
    '#................#############',
    '#................#############',
    '#................#############',
    '#................#############',
    '#.P.....^^^^^^^^^#############',
    W],
  traps: [
    { type: 'movingPlatform', x1: 9, y1: 14, x2: 14, y2: 6, w: 3, speed: 110, reverseOnLand: true },
    { type: 'popSpikes', dir: 'up', cells: [[20, 6], [21, 6]], trigger: { kind: 'zone', x: 18, y: 4, w: 1, h: 3 } }
  ]
});

// 9: Vier Stampfer. Einer tanzt aus der Reihe.
LEVELS.push({
  name: 'Deckenfresser', chapter: 2,
  grid: [W, W, W, W, W, W, W, W, W, W, E, E, E, E, E,
    '#.P........................D.#',
    W],
  traps: [
    { type: 'crusher', c: 8, w: 2, fromR: 10, toR: 15, repeat: true, pause: 1.2, offset: 0 },
    { type: 'crusher', c: 13, w: 2, fromR: 10, toR: 15, repeat: true, pause: 1.2, offset: 0.85 },
    { type: 'crusher', c: 18, w: 2, fromR: 10, toR: 15, repeat: true, pause: 1.7, offset: 0.4 },
    { type: 'crusher', c: 23, w: 2, fromR: 10, toR: 15, repeat: true, pause: 1.2, offset: 1.25 }
  ]
});

// 10: Der Boden hat gekündigt. RENN!
LEVELS.push({
  name: 'Falltür', chapter: 2,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '####################..########'],
  traps: [
    { type: 'vanish', id: 'w1', cells: [[3, 16], [4, 16], [5, 16], [6, 16]], trigger: { kind: 'zone', x: 5, y: 12, w: 1, h: 4 } },
    { type: 'vanish', cells: [[7, 16], [8, 16], [9, 16], [10, 16]], trigger: { kind: 'after', id: 'w1', delay: 0.6 } },
    { type: 'vanish', cells: [[11, 16], [12, 16], [13, 16], [14, 16]], trigger: { kind: 'after', id: 'w1', delay: 1.2 } },
    { type: 'vanish', cells: [[15, 16], [16, 16], [17, 16], [18, 16], [19, 16]], trigger: { kind: 'after', id: 'w1', delay: 1.8 } },
    { type: 'vanish', cells: [[22, 16], [23, 16], [24, 16], [25, 16]], trigger: { kind: 'after', id: 'w1', delay: 2.7 } }
  ]
});

// 11: Vier Federn. Drei Lügen.
LEVELS.push({
  name: 'Federball', chapter: 2,
  grid: [W, E, E, E,
    '#....###..###.......###......#',
    '#....vvv..vvv.......vvv......#',
    '#.....................D......#',
    '#.................#######....#',
    E, E, E, E, E, E, E,
    '#.P..........................#',
    W],
  traps: [
    { type: 'spring', cells: [[6, 15], [11, 15], [16, 15], [21, 15]] }
  ]
});

// 12: Wer wartet, gewinnt. Wer rennt, fängt Pfeile.
LEVELS.push({
  name: 'Geduld', chapter: 2,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '##############........########'],
  marker: [11, 15],
  traps: [
    { type: 'appear', id: 'bridge', cells: [[14, 16], [15, 16], [16, 16], [17, 16], [18, 16], [19, 16], [20, 16], [21, 16]],
      trigger: { kind: 'stay', x: 10, y: 12, w: 2, h: 4, duration: 3 } },
    { type: 'projectile', from: [28, 15], dir: 'left',
      trigger: { kind: 'zone', x: 12, y: 12, w: 2, h: 4, unlessId: 'bridge' } }
  ]
});

// ================= KAPITEL 3: BOSHEIT =================

// 13: Jede Falle bringt ihre beste Freundin mit.
LEVELS.push({
  name: 'Doppelt gemoppelt', chapter: 3,
  grid: [W, E, E, E, E, E, E, E,
    '#..............##............#',
    E, E, E, E, E,
    '#.......###..................#',
    '#.P.....................D....#',
    '#######.....##################'],
  traps: [
    { type: 'vanish', id: 'v1', cells: [[8, 14], [9, 14], [10, 14]], trigger: { kind: 'onLand', x: 7, y: 12, w: 4, h: 2 } },
    { type: 'popSpikes', dir: 'up', cells: [[5, 15], [6, 15]], trigger: { kind: 'after', id: 'v1', delay: 0.5 } },
    { type: 'crusher', id: 'c1', c: 15, w: 2, fromR: 9, toR: 15, repeat: false, trigger: { kind: 'zone', x: 13, y: 12, w: 1, h: 4 } },
    { type: 'popSpikes', dir: 'up', cells: [[18, 15], [19, 15]], trigger: { kind: 'after', id: 'c1', delay: 0.7 } },
    { type: 'doorMove', id: 'd1', to: [27, 15], trigger: { kind: 'zone', x: 22, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [25, 1], trigger: { kind: 'after', id: 'd1', delay: 0.3 } }
  ]
});

// 14: Die Tür will nach Hause. Der Rückweg ist ... anders.
LEVELS.push({
  name: 'Rückweg', chapter: 3,
  grid: [W, E, E, E, E, E, E, E, E, E, E,
    '######.......................#',
    E,
    '#......##....................#',
    E,
    '#.P........................D.#',
    W],
  traps: [
    { type: 'doorMove', id: 'd1', to: [3, 10], trigger: { kind: 'zone', x: 25, y: 12, w: 1, h: 4 } },
    { type: 'popSpikes', dir: 'up', cells: [[17, 15], [18, 15]], trigger: { kind: 'zone', x: 20, y: 12, w: 1, h: 4, afterId: 'd1' } },
    { type: 'fallBlock', cell: [12, 1], trigger: { kind: 'zone', x: 13, y: 12, w: 1, h: 4, afterId: 'd1' } },
    { type: 'vanish', cells: [[9, 16], [10, 16]], trigger: { kind: 'zone', x: 11, y: 12, w: 1, h: 4, afterId: 'd1' } }
  ]
});

// 15: Licht aus. Augen auf.
LEVELS.push({
  name: 'Dunkelheit', chapter: 3, dark: true,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P..........^^............D.#',
    '########..########...#########'],
  traps: [
    { type: 'vanish', cells: [[23, 16], [24, 16]], trigger: { kind: 'zone', x: 22, y: 12, w: 1, h: 4 } }
  ]
});

// 16: Pfeile von rechts, Löcher von unten.
LEVELS.push({
  name: 'Spießrutenlauf', chapter: 3,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P.........^........^.....D.#',
    W],
  traps: [
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 2.4, trigger: { kind: 'timer', t: 1.0 } },
    { type: 'vanish', cells: [[8, 16], [9, 16]], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'vanish', cells: [[16, 16], [17, 16]], trigger: { kind: 'zone', x: 15, y: 12, w: 1, h: 4 } }
  ]
});

// 17: Alles auf einmal. Viel Glück.
LEVELS.push({
  name: 'Das Finale', chapter: 3,
  grid: [W, E, E, E, E, E, E,
    '#..............##..........D.#',
    '#.....................########',
    E, E, E, E, E, E,
    '#.P.......^^^................#',
    '######~~######################'],
  traps: [
    { type: 'crusher', c: 15, w: 2, fromR: 8, toR: 15, repeat: true, pause: 1.4, offset: 0 },
    { type: 'spring', cells: [[20, 15]] },
    { type: 'doorMove', id: 'd1', to: [22, 7], trigger: { kind: 'zone', x: 25, y: 5, w: 1, h: 3 } },
    { type: 'doorMove', id: 'd2', to: [27, 7], trigger: { kind: 'zone', x: 22, y: 5, w: 2, h: 3, afterId: 'd1' } }
  ]
});

// 18: Geschafft! Also ... fast.
LEVELS.push({
  name: 'Danke fürs Spielen', chapter: 3,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P...D......................#',
    W],
  traps: [
    { type: 'doorMove', to: [26, 15], trigger: { kind: 'zone', x: 4, y: 12, w: 1, h: 4 } }
  ]
});
