// Alle 24 Level der Verzweiflungs-Edition. Ein Level = ASCII-Grid
// (17 Zeilen x 30 Zeichen) + Fallen.
//   '#' = solide   '.' = leer      'P' = Start    'D' = Tür
//   '^' 'v' '<' '>' = Stacheln     '~' = Fake-Boden (sieht solide aus)
// Fallen-Zellen sind [Spalte, Zeile]. Trigger-Bereiche in Tile-Koordinaten.
var LEVELS = [];

// Häufige Zeilen als Kürzel
var W = '##############################'; // Vollwand
var E = '#............................#'; // leer mit Seitenwänden

// ================= KAPITEL 1: WILLKOMMEN =================

// 1: Ein harmloser Spaziergang. Zwei Überraschungen.
LEVELS.push({
  name: 'Spaziergang', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '########################~~####'],
  traps: [
    { type: 'popSpikes', dir: 'up', cells: [[17, 15], [18, 15]],
      trigger: { kind: 'zone', x: 14, y: 12, w: 1, h: 4 } }
  ]
});

// 2: Nichts, worauf du landest, bleibt.
LEVELS.push({
  name: 'Sprungstunde', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#..........##................#',
    '#.P.......................D..#',
    '#########......####...########'],
  traps: [
    { type: 'vanish', id: 'v1', cells: [[11, 14], [12, 14]], trigger: { kind: 'onLand', x: 10, y: 12, w: 4, h: 2 } },
    { type: 'popSpikes', dir: 'up', cells: [[14, 15], [15, 15]], trigger: { kind: 'after', id: 'v1', delay: 0.35 } },
    { type: 'vanish', cells: [[22, 16], [23, 16]], trigger: { kind: 'onLand', x: 22, y: 12, w: 2, h: 4 } }
  ]
});

// 3: Der bequeme Weg ist eine Lüge. Der unbequeme auch, fast.
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
      trigger: { kind: 'zone', x: 6, y: 12, w: 1, h: 4 } },
    { type: 'vanish', cells: [[9, 13], [10, 13], [11, 13]],
      trigger: { kind: 'onLand', x: 8, y: 11, w: 4, h: 2 } },
    { type: 'popSpikes', dir: 'up', cells: [[24, 15], [25, 15]],
      trigger: { kind: 'zone', x: 21, y: 12, w: 1, h: 4 } }
  ]
});

// 4: Die Tür hat Angst vor dir. Zweimal. Und dann lügt der Boden.
LEVELS.push({
  name: 'Die Tür', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P...........D..............#',
    '########################~~####'],
  traps: [
    { type: 'doorMove', id: 'd1', to: [22, 15], trigger: { kind: 'zone', x: 11, y: 12, w: 2, h: 4 } },
    { type: 'doorMove', to: [27, 15], trigger: { kind: 'zone', x: 19, y: 12, w: 1, h: 4, afterId: 'd1' } }
  ]
});

// 5: Bleib bloß nicht stehen. Und duck dich nicht in Sicherheit.
LEVELS.push({
  name: 'Steinregen', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    W],
  traps: [
    { type: 'fallBlock', cell: [5, 1], trigger: { kind: 'zone', x: 4, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [7, 1], trigger: { kind: 'zone', x: 6, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [9, 1], trigger: { kind: 'zone', x: 8, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [11, 1], trigger: { kind: 'zone', x: 10, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [13, 1], trigger: { kind: 'zone', x: 12, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [15, 1], trigger: { kind: 'zone', x: 14, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [17, 1], trigger: { kind: 'zone', x: 16, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [19, 1], trigger: { kind: 'zone', x: 18, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [21, 1], trigger: { kind: 'zone', x: 20, y: 12, w: 1, h: 4 } },
    { type: 'popSpikes', dir: 'up', cells: [[25, 15], [26, 15]],
      trigger: { kind: 'zone', x: 22, y: 12, w: 1, h: 4 } }
  ]
});

// 6: Kein Witz. Wirklich keiner. Versprochen. (Aber eng.)
LEVELS.push({
  name: 'Ehrlich!', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P.............^^^........D.#',
    '##########....################'],
  traps: []
});

// ================= KAPITEL 2: VERTRAUEN IST GUT =================

// 7: Die ersten Stacheln weichen zurück. Die letzten nicht.
LEVELS.push({
  name: 'Vertrauen', chapter: 2,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P......^^^...^^^.^^......D.#',
    W],
  traps: [
    { type: 'hideSpikes', cells: [[9, 15], [10, 15], [11, 15]], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'hideSpikes', cells: [[15, 15], [16, 15], [17, 15]], trigger: { kind: 'zone', x: 13, y: 12, w: 1, h: 4 } },
    { type: 'invisibleWall', cells: [[22, 11], [22, 12], [22, 13]] }
  ]
});

// 8: Der Aufzug kehrt bei JEDER Landung um. Hüpf schlau.
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
    { type: 'movingPlatform', x1: 9, y1: 14, x2: 14, y2: 6, w: 3, speed: 110, reverseOnLand: true, every: true },
    { type: 'popSpikes', dir: 'up', cells: [[20, 6], [21, 6]], trigger: { kind: 'zone', x: 18, y: 4, w: 1, h: 3 } },
    { type: 'popSpikes', dir: 'up', cells: [[24, 6], [25, 6]], trigger: { kind: 'zone', x: 22, y: 4, w: 1, h: 3 } }
  ]
});

// 9: Fünf Stampfer. Kein gemeinsamer Takt.
LEVELS.push({
  name: 'Deckenfresser', chapter: 2,
  grid: [W, W, W, W, W, W, W, W, W, W, E, E, E, E, E,
    '#.P........................D.#',
    W],
  traps: [
    { type: 'crusher', c: 6, w: 2, fromR: 10, toR: 15, repeat: true, pause: 1.0, offset: 0 },
    { type: 'crusher', c: 10, w: 2, fromR: 10, toR: 15, repeat: true, pause: 1.0, offset: 0.5 },
    { type: 'crusher', c: 14, w: 2, fromR: 10, toR: 15, repeat: true, pause: 1.4, offset: 0.2 },
    { type: 'crusher', c: 18, w: 2, fromR: 10, toR: 15, repeat: true, pause: 1.0, offset: 1.0 },
    { type: 'crusher', c: 22, w: 2, fromR: 10, toR: 15, repeat: true, pause: 0.8, offset: 0.65 }
  ]
});

// 10: Der Boden kündigt, Steine fallen, und vor der Tür wird es spitz. RENN!
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
    { type: 'vanish', cells: [[22, 16], [23, 16], [24, 16], [25, 16]], trigger: { kind: 'after', id: 'w1', delay: 2.7 } },
    { type: 'fallBlock', cell: [13, 1], trigger: { kind: 'after', id: 'w1', delay: 1.0 } },
    { type: 'fallBlock', cell: [17, 1], trigger: { kind: 'after', id: 'w1', delay: 1.35 } },
    { type: 'popSpikes', dir: 'up', cells: [[25, 15], [26, 15]], trigger: { kind: 'after', id: 'w1', delay: 2.2 } }
  ]
});

// 11: Vier Federn. Drei Lügen. Und die Wahrheit liegt weit hinten.
LEVELS.push({
  name: 'Federball', chapter: 2,
  grid: [W, E, E, E,
    '#....###..###..###...........#',
    '#....vvv..vvv..vvv...........#',
    '#.........................D..#',
    '#......................#######',
    E, E, E, E, E, E, E,
    '#.P..........................#',
    W],
  traps: [
    { type: 'spring', cells: [[6, 15], [11, 15], [16, 15], [21, 15]] }
  ]
});

// 12: Warte 5 Sekunden. Während auf dich geschossen wird.
LEVELS.push({
  name: 'Geduld', chapter: 2,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '##############........########'],
  marker: [11, 15],
  traps: [
    { type: 'appear', id: 'bridge', cells: [[14, 16], [15, 16], [16, 16], [17, 16], [18, 16], [19, 16], [20, 16], [21, 16]],
      trigger: { kind: 'stay', x: 10, y: 12, w: 2, h: 4, duration: 5 } },
    { type: 'crumble', cells: [[14, 16], [15, 16], [16, 16], [17, 16], [18, 16], [19, 16], [20, 16], [21, 16]] },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 2.5, stopId: 'bridge',
      trigger: { kind: 'timer', t: 0.5 } },
    { type: 'projectile', from: [28, 15], dir: 'left',
      trigger: { kind: 'zone', x: 12, y: 12, w: 2, h: 4, unlessId: 'bridge' } }
  ]
});

// ================= KAPITEL 3: BOSHEIT =================

// 13: Jede Falle bringt ihre beste Freundin mit. Und deren Freundin.
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
    { type: 'fallBlock', cell: [12, 1], trigger: { kind: 'after', id: 'v1', delay: 1.0 } },
    { type: 'crusher', id: 'c1', c: 15, w: 2, fromR: 9, toR: 15, repeat: false, trigger: { kind: 'zone', x: 13, y: 12, w: 1, h: 4 } },
    { type: 'popSpikes', dir: 'up', cells: [[18, 15], [19, 15]], trigger: { kind: 'after', id: 'c1', delay: 0.7 } },
    { type: 'projectile', from: [28, 15], dir: 'left', trigger: { kind: 'after', id: 'c1', delay: 1.2 } },
    { type: 'doorMove', id: 'd1', to: [27, 15], trigger: { kind: 'zone', x: 22, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [25, 1], trigger: { kind: 'after', id: 'd1', delay: 0.3 } },
    { type: 'popSpikes', dir: 'up', cells: [[26, 15]], trigger: { kind: 'after', id: 'd1', delay: 0.5 } }
  ]
});

// 14: Die Tür will nach Hause. Der Rückweg ist die Hölle, die Stufen bröseln.
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
    { type: 'vanish', cells: [[9, 16], [10, 16]], trigger: { kind: 'zone', x: 11, y: 12, w: 1, h: 4, afterId: 'd1' } },
    { type: 'crumble', cells: [[7, 13], [8, 13]] },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 3.0,
      trigger: { kind: 'after', id: 'd1', delay: 1.5 } }
  ]
});

// 15: Licht aus. Pfeile an.
LEVELS.push({
  name: 'Dunkelheit', chapter: 3, dark: true, lightRadius: 110,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P..........^^............D.#',
    '########..########...#########'],
  traps: [
    { type: 'vanish', cells: [[10, 16]], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'vanish', cells: [[23, 16], [24, 16]], trigger: { kind: 'zone', x: 22, y: 12, w: 1, h: 4 } },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 3.5,
      trigger: { kind: 'timer', t: 1.0 } }
  ]
});

// 16: Pfeile im Sekundentakt, der Boden bröselt.
LEVELS.push({
  name: 'Spießrutenlauf', chapter: 3,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P.........^........^.....D.#',
    W],
  traps: [
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 1.9, trigger: { kind: 'timer', t: 1.0 } },
    { type: 'vanish', cells: [[8, 16], [9, 16]], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'crumble', cells: [[13, 16], [14, 16], [15, 16], [16, 16], [17, 16], [18, 16], [19, 16]] }
  ]
});

// 17: Alles auf einmal. Und hinter dir kommt die Wand.
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
    { type: 'spikeWall', fromX: -2, speed: 120, trigger: { kind: 'zone', x: 13, y: 12, w: 1, h: 4 } },
    { type: 'doorMove', id: 'd1', to: [22, 7], trigger: { kind: 'zone', x: 25, y: 5, w: 1, h: 3 } },
    { type: 'doorMove', id: 'd2', to: [27, 7], trigger: { kind: 'zone', x: 22, y: 5, w: 2, h: 3, afterId: 'd1' } }
  ]
});

// ================= KAPITEL 4: HÖLLE =================

// 18: Der ganze Boden bröselt. Stehenbleiben ist keine Option.
LEVELS.push({
  name: 'Krümelmonster', chapter: 4,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '##############..##############'],
  traps: [
    { type: 'crumble', cells: [
      [3, 16], [4, 16], [5, 16], [6, 16], [7, 16], [8, 16], [9, 16], [10, 16], [11, 16], [12, 16], [13, 16],
      [16, 16], [17, 16], [18, 16], [19, 16], [20, 16], [21, 16], [22, 16], [23, 16], [24, 16], [25, 16], [26, 16]
    ] },
    { type: 'crusher', c: 10, w: 2, fromR: 1, toR: 15, repeat: true, pause: 1.5, offset: 0 },
    { type: 'crusher', c: 18, w: 2, fromR: 1, toR: 15, repeat: true, pause: 1.5, offset: 1.2 }
  ]
});

// 19: Links ist rechts. Rechts ist links. Viel Spaß.
LEVELS.push({
  name: 'Spiegelverkehrt', chapter: 4,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........^^..........^^..D.#',
    '#################...##########'],
  traps: [
    { type: 'flipControls', duration: 8, trigger: { kind: 'zone', x: 5, y: 12, w: 1, h: 4 } }
  ]
});

// 20: Die Wand wartet nicht.
LEVELS.push({
  name: 'Die Mauer', chapter: 4,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '#######..###..#######..#######'],
  traps: [
    { type: 'spikeWall', fromX: -1, speed: 150, trigger: { kind: 'timer', t: 0.8 } },
    { type: 'popSpikes', dir: 'up', cells: [[17, 15], [18, 15]], trigger: { kind: 'zone', x: 10, y: 12, w: 1, h: 4 } }
  ]
});

// 21: Vier Türen. Drei sind Lügen mit Konsequenzen.
LEVELS.push({
  name: 'Türsteher', chapter: 4,
  grid: [W, E, E, E, E, E, E, E,
    '#.......................##...#',
    E, E, E, E, E, E,
    '#.P........................D.#',
    W],
  traps: [
    { type: 'fakeDoor', cell: [8, 15], id: 'f1' },
    { type: 'popSpikes', dir: 'up', cells: [[6, 15], [7, 15]], trigger: { kind: 'after', id: 'f1', delay: 0.3 } },
    { type: 'fakeDoor', cell: [14, 15], id: 'f2' },
    { type: 'fallBlock', cell: [16, 1], trigger: { kind: 'after', id: 'f2', delay: 0.1 } },
    { type: 'fakeDoor', cell: [20, 15], id: 'f3' },
    { type: 'projectile', from: [28, 15], dir: 'left', trigger: { kind: 'after', id: 'f3', delay: 0.1 } },
    { type: 'crusher', c: 24, w: 2, fromR: 9, toR: 15, repeat: true, pause: 1.3, offset: 0 }
  ]
});

// 22: Dunkel, bröselig, beschossen – und am Ende spiegelverkehrt.
LEVELS.push({
  name: 'Blindflug', chapter: 4, dark: true, lightRadius: 100,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P.................^......D.#',
    '#########..###################'],
  traps: [
    { type: 'crumble', cells: [[13, 16], [14, 16], [15, 16], [16, 16], [17, 16], [18, 16]] },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 3.0, trigger: { kind: 'timer', t: 1.5 } },
    { type: 'flipControls', duration: 3.5, trigger: { kind: 'zone', x: 22, y: 12, w: 1, h: 4 } }
  ]
});

// 23: Alles, was du gelernt hast. Auf einmal. Mit Wand.
LEVELS.push({
  name: 'Das wahre Finale', chapter: 4,
  grid: [W, E, E, E, E, E, E,
    '#..................##........#',
    E, E, E, E, E, E, E,
    '#.P........................D.#',
    '#####~~######..###############'],
  traps: [
    { type: 'popSpikes', dir: 'up', cells: [[10, 15], [11, 15]], trigger: { kind: 'zone', x: 8, y: 12, w: 1, h: 4 } },
    { type: 'crusher', c: 19, w: 2, fromR: 8, toR: 15, repeat: true, pause: 1.6, offset: 0.5 },
    { type: 'fakeDoor', cell: [22, 15], id: 'fd' },
    { type: 'popSpikes', dir: 'up', cells: [[23, 15], [24, 15]], trigger: { kind: 'after', id: 'fd', delay: 0.12 } },
    { type: 'spikeWall', fromX: -1, speed: 125, trigger: { kind: 'zone', x: 10, y: 12, w: 1, h: 4 } }
  ]
});

// 24: Geschafft! Also ... fast.
LEVELS.push({
  name: 'Danke fürs Spielen', chapter: 4,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P...D......................#',
    W],
  traps: [
    { type: 'doorMove', to: [26, 15], trigger: { kind: 'zone', x: 4, y: 12, w: 1, h: 4 } }
  ]
});
