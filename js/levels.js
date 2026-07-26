// Alle 24 Level der Verzweiflungs-Edition II ("Noch viel gemeiner").
// Ein Level = ASCII-Grid (17 Zeilen x 30 Zeichen) + Fallen.
//   '#' = solide   '.' = leer      'P' = Start    'D' = Tür
//   '^' 'v' '<' '>' = Stacheln     '~' = Fake-Boden (sieht solide aus)
// Fallen-Zellen sind [Spalte, Zeile]. Trigger-Bereiche in Tile-Koordinaten.
//
// Design-Prinzip dieser Edition: Stehenbleiben wird bestraft, blindes
// Rennen wird bestraft, und die letzte Falle wartet immer direkt vor
// der Tür. Jedes Level ist per Headless-Simulation (tools/sim.js)
// nachweislich schaffbar – aber nur mit auswendig gelerntem Plan.
var LEVELS = [];

// Häufige Zeilen als Kürzel
var W = '##############################'; // Vollwand
var E = '#............................#'; // leer mit Seitenwänden

// ================= KAPITEL 1: WILLKOMMEN =================

// 1: Ein harmloser Spaziergang. Vier Gemeinheiten.
LEVELS.push({
  name: 'Spaziergang', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '########################~~####'],
  traps: [
    { type: 'fallBlock', cell: [6, 1], trigger: { kind: 'zone', x: 4, y: 12, w: 1, h: 4 } },
    { type: 'popSpikes', dir: 'up', cells: [[10, 15], [11, 15]], delay: 0.28,
      trigger: { kind: 'zone', x: 8, y: 12, w: 1, h: 4 } },
    { type: 'popSpikes', dir: 'up', cells: [[17, 15], [18, 15]], delay: 0.25,
      trigger: { kind: 'zone', x: 14, y: 12, w: 1, h: 4 } },
    { type: 'vanish', cells: [[20, 16], [21, 16]], trigger: { kind: 'onLand', x: 19, y: 12, w: 4, h: 4 } }
  ]
});

// 2: Nichts, worauf du landest, bleibt. Und die Decke wirft mit Steinen.
LEVELS.push({
  name: 'Sprungstunde', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#..........##................#',
    '#.P.......................D..#',
    '#########......####...########'],
  traps: [
    { type: 'fallBlock', cell: [5, 1], trigger: { kind: 'zone', x: 4, y: 12, w: 1, h: 4 } },
    { type: 'vanish', id: 'v1', cells: [[11, 14], [12, 14]], trigger: { kind: 'onLand', x: 10, y: 11, w: 4, h: 3 } },
    { type: 'popSpikes', dir: 'up', cells: [[16, 15], [17, 15]], trigger: { kind: 'after', id: 'v1', delay: 0.35 } },
    { type: 'vanish', cells: [[22, 16], [23, 16]], trigger: { kind: 'onLand', x: 22, y: 12, w: 2, h: 4 } },
    { type: 'popSpikes', dir: 'up', cells: [[25, 15]], delay: 0.2,
      trigger: { kind: 'zone', x: 23, y: 12, w: 1, h: 4 } }
  ]
});

// 3: Der Boden ist Lava, die Ledges sind Lügen, das Ende ist ein Limbo.
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
    { type: 'vanish', cells: [[13, 12], [14, 12], [15, 12]],
      trigger: { kind: 'onLand', x: 12, y: 10, w: 4, h: 2 } },
    { type: 'popSpikes', dir: 'up', cells: [[21, 15], [22, 15]], delay: 0.2,
      trigger: { kind: 'zone', x: 17, y: 5, w: 3, h: 8 } },
    { type: 'popSpikes', dir: 'up', cells: [[25, 15]], delay: 0.18,
      trigger: { kind: 'zone', x: 23, y: 12, w: 1, h: 4 } },
    { type: 'invisibleWall', cells: [[26, 11], [26, 12]] }
  ]
});

// 4: Die Tür hat Angst. Der Boden lügt. Und wer trödelt, verliert die Landung.
LEVELS.push({
  name: 'Die Tür', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P...........D..............#',
    '########################~~####'],
  traps: [
    { type: 'doorMove', id: 'd1', to: [22, 15], trigger: { kind: 'zone', x: 11, y: 12, w: 2, h: 4 } },
    { type: 'fallBlock', cell: [18, 1], trigger: { kind: 'after', id: 'd1', delay: 0.4 } },
    { type: 'doorMove', id: 'd2', to: [27, 15], trigger: { kind: 'zone', x: 19, y: 12, w: 1, h: 4, afterId: 'd1' } },
    { type: 'popSpikes', dir: 'up', cells: [[21, 15]], delay: 0,
      trigger: { kind: 'after', id: 'd2', delay: 0.1 } },
    { type: 'vanish', cells: [[26, 16]], trigger: { kind: 'after', id: 'd2', delay: 1.2 } }
  ]
});

// 5: Steinregen in zwei Wellen. Bleib bloß nicht stehen. Nie.
LEVELS.push({
  name: 'Steinregen', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    W],
  traps: [
    { type: 'fallBlock', id: 'b4', cell: [4, 1], trigger: { kind: 'zone', x: 3, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', id: 'b6', cell: [6, 1], trigger: { kind: 'zone', x: 5, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', id: 'b8', cell: [8, 1], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', id: 'b10', cell: [10, 1], trigger: { kind: 'zone', x: 9, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', id: 'b12', cell: [12, 1], trigger: { kind: 'zone', x: 11, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', id: 'b14', cell: [14, 1], trigger: { kind: 'zone', x: 13, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', id: 'b16', cell: [16, 1], trigger: { kind: 'zone', x: 15, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', id: 'b18', cell: [18, 1], trigger: { kind: 'zone', x: 17, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', id: 'b20', cell: [20, 1], trigger: { kind: 'zone', x: 19, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', id: 'b22', cell: [22, 1], trigger: { kind: 'zone', x: 21, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [5, 1], trigger: { kind: 'after', id: 'b4', delay: 0.9 } },
    { type: 'fallBlock', cell: [9, 1], trigger: { kind: 'after', id: 'b8', delay: 0.9 } },
    { type: 'fallBlock', cell: [13, 1], trigger: { kind: 'after', id: 'b12', delay: 0.9 } },
    { type: 'fallBlock', cell: [17, 1], trigger: { kind: 'after', id: 'b16', delay: 0.9 } },
    { type: 'fallBlock', cell: [21, 1], trigger: { kind: 'after', id: 'b20', delay: 0.9 } },
    { type: 'popSpikes', dir: 'up', cells: [[25, 15], [26, 15]], delay: 0.2,
      trigger: { kind: 'zone', x: 23, y: 12, w: 1, h: 4 } }
  ]
});

// 6: Kein Witz. Wirklich keiner. Nur ein Maximalsprung und drei Stacheln.
LEVELS.push({
  name: 'Ehrlich!', chapter: 1,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P.............^^^........D.#',
    '##########.....###############'],
  traps: []
});

// ================= KAPITEL 2: VERTRAUEN IST GUT =================

// 7: Die ersten Stacheln weichen zurück. Die zweiten nur zu zwei Dritteln.
LEVELS.push({
  name: 'Vertrauen', chapter: 2,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P......^^^...^^^.^^......D.#',
    W],
  traps: [
    { type: 'hideSpikes', cells: [[9, 15], [10, 15], [11, 15]], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'hideSpikes', cells: [[15, 15], [16, 15], [17, 15]], trigger: { kind: 'zone', x: 13, y: 12, w: 1, h: 4 } },
    { type: 'invisibleWall', cells: [[22, 12], [22, 13]] },
    { type: 'popSpikes', dir: 'up', cells: [[24, 15], [25, 15]], delay: 0.15,
      trigger: { kind: 'zone', x: 23, y: 12, w: 1, h: 4 } }
  ]
});

// 8: Der Aufzug kehrt bei JEDER Landung um. Oben: Stachel-Treppe + Pfeile.
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
    { type: 'movingPlatform', x1: 9, y1: 14, x2: 14, y2: 6, w: 3, speed: 125, reverseOnLand: true, every: true },
    { type: 'popSpikes', dir: 'up', cells: [[19, 6], [20, 6]], delay: 0.2,
      trigger: { kind: 'zone', x: 17, y: 4, w: 1, h: 3 } },
    { type: 'popSpikes', dir: 'up', cells: [[23, 6], [24, 6]], delay: 0.15,
      trigger: { kind: 'zone', x: 21, y: 4, w: 1, h: 3 } },
    { type: 'projectile', from: [28, 6], dir: 'left', repeat: true, interval: 2.4,
      trigger: { kind: 'timer', t: 1.0 } }
  ]
});

// 9: Sechs Stampfer, kein gemeinsamer Takt – und der Boden dazwischen bröselt.
// Es gibt genau eine Stelle zum Nachdenken: den Start.
LEVELS.push({
  name: 'Deckenfresser', chapter: 2,
  grid: [W, W, W, W, W, W, W, W, W, W, E, E, E, E, E,
    '#.P........................D.#',
    W],
  traps: [
    { type: 'crusher', c: 5, w: 2, fromR: 10, toR: 15, repeat: true, pause: 0.9, offset: 0 },
    { type: 'crusher', c: 9, w: 2, fromR: 10, toR: 15, repeat: true, pause: 0.8, offset: 0.45 },
    { type: 'crusher', c: 13, w: 2, fromR: 10, toR: 15, repeat: true, pause: 1.1, offset: 0.2 },
    { type: 'crusher', c: 17, w: 2, fromR: 10, toR: 15, repeat: true, pause: 0.75, offset: 0.9 },
    { type: 'crusher', c: 21, w: 2, fromR: 10, toR: 15, repeat: true, pause: 0.95, offset: 0.6 },
    { type: 'crusher', c: 24, w: 2, fromR: 10, toR: 15, repeat: true, pause: 0.85, offset: 0.3 },
    { type: 'crumble', cells: [[7, 16], [8, 16], [11, 16], [12, 16], [15, 16], [16, 16], [19, 16], [20, 16], [23, 16]] }
  ]
});

// 10: Der Boden kündigt hinter dir, Steine folgen dir, Stacheln warten vorn.
// Ein einziger durchgehender Sprint – jede Zehntelsekunde Zögern ist der Tod.
LEVELS.push({
  name: 'Falltür', chapter: 2,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '####################..########'],
  traps: [
    { type: 'vanish', id: 'w1', cells: [[3, 16], [4, 16], [5, 16]], trigger: { kind: 'zone', x: 5, y: 12, w: 1, h: 4 } },
    { type: 'vanish', cells: [[6, 16], [7, 16], [8, 16], [9, 16]], trigger: { kind: 'after', id: 'w1', delay: 0.8 } },
    { type: 'vanish', cells: [[10, 16], [11, 16], [12, 16], [13, 16]], trigger: { kind: 'after', id: 'w1', delay: 1.25 } },
    { type: 'vanish', cells: [[14, 16], [15, 16], [16, 16], [17, 16], [18, 16], [19, 16]], trigger: { kind: 'after', id: 'w1', delay: 1.75 } },
    { type: 'vanish', cells: [[22, 16], [23, 16], [24, 16], [25, 16]], trigger: { kind: 'after', id: 'w1', delay: 2.9 } },
    { type: 'fallBlock', cell: [9, 1], trigger: { kind: 'after', id: 'w1', delay: 0.8 } },
    { type: 'fallBlock', cell: [13, 1], trigger: { kind: 'after', id: 'w1', delay: 1.25 } },
    { type: 'fallBlock', cell: [17, 1], trigger: { kind: 'after', id: 'w1', delay: 1.7 } },
    { type: 'popSpikes', dir: 'up', cells: [[24, 15], [25, 15]], delay: 0,
      trigger: { kind: 'after', id: 'w1', delay: 2.45 } }
  ]
});

// 11: Vier Federn. Drei ehrliche Todesfallen – und die "sichere" lügt oben.
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
    { type: 'spring', cells: [[6, 15], [11, 15], [16, 15], [21, 15]] },
    { type: 'popSpikes', dir: 'down', cells: [[19, 5], [20, 5]], delay: 0.12,
      trigger: { kind: 'zone', x: 19, y: 8, w: 2, h: 4 } },
    { type: 'fallBlock', cell: [8, 1], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [13, 1], trigger: { kind: 'zone', x: 12, y: 12, w: 1, h: 4 } },
    { type: 'crumble', cells: [[23, 7], [24, 7]] }
  ]
});

// 12: Warte 6 Sekunden im Kreuzfeuer. Die Brücke bröselt, die Pfeile bleiben.
LEVELS.push({
  name: 'Geduld', chapter: 2,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '##############........########'],
  marker: [11, 15],
  traps: [
    { type: 'appear', id: 'bridge', cells: [[14, 16], [15, 16], [16, 16], [17, 16], [18, 16], [19, 16], [20, 16], [21, 16]],
      trigger: { kind: 'stay', x: 10, y: 12, w: 2, h: 4, duration: 6 } },
    { type: 'crumble', cells: [[14, 16], [15, 16], [16, 16], [17, 16], [18, 16], [19, 16], [20, 16], [21, 16]] },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 1.7,
      trigger: { kind: 'timer', t: 0.5 } },
    { type: 'projectile', from: [3, 15], dir: 'right', repeat: true, interval: 2.3,
      trigger: { kind: 'timer', t: 1.6 } },
    { type: 'popSpikes', dir: 'up', cells: [[24, 15], [25, 15]], delay: 0.2,
      trigger: { kind: 'zone', x: 23, y: 12, w: 1, h: 4 } }
  ]
});

// ================= KAPITEL 3: BOSHEIT =================

// 13: Jede Falle bringt Freunde mit. Und die Tür? Geht am Ende zurück.
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
    { type: 'popSpikes', dir: 'up', cells: [[5, 15], [6, 15]], trigger: { kind: 'after', id: 'v1', delay: 0.4 } },
    { type: 'fallBlock', cell: [12, 1], trigger: { kind: 'after', id: 'v1', delay: 0.8 } },
    { type: 'crusher', c: 15, w: 2, fromR: 9, toR: 15, repeat: true, pause: 1.1, offset: 0 },
    { type: 'fallBlock', cell: [20, 1], trigger: { kind: 'zone', x: 18, y: 12, w: 1, h: 4 } },
    { type: 'doorMove', id: 'd1', to: [27, 15], trigger: { kind: 'zone', x: 21, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [25, 1], trigger: { kind: 'after', id: 'd1', delay: 0.3 } },
    { type: 'popSpikes', dir: 'up', cells: [[26, 15]], delay: 0,
      trigger: { kind: 'after', id: 'd1', delay: 0.4 } },
    { type: 'doorMove', id: 'd2', to: [13, 15], trigger: { kind: 'zone', x: 26, y: 12, w: 1, h: 4, afterId: 'd1' } },
    { type: 'popSpikes', dir: 'up', cells: [[21, 15], [22, 15]], delay: 0.15,
      trigger: { kind: 'zone', x: 23, y: 12, w: 1, h: 4, afterId: 'd2' } }
  ]
});

// 14: Die Tür flieht nach Hause. Der Rückweg: Kreuzfeuer, Löcher, Bröselstufen.
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
    { type: 'crumble', cells: [[7, 13], [8, 13], [4, 11], [5, 11]] },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 2.1,
      trigger: { kind: 'after', id: 'd1', delay: 1.0 } },
    { type: 'projectile', from: [1, 15], dir: 'right', repeat: true, interval: 2.7,
      trigger: { kind: 'after', id: 'd1', delay: 2.0 } }
  ]
});

// 15: Licht aus, Maximalsprünge an. Zwei davon. Nacheinander. Im Dunkeln.
LEVELS.push({
  name: 'Dunkelheit', chapter: 3, dark: true, lightRadius: 90,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P..........^^............D.#',
    '########..########...#########'],
  traps: [
    { type: 'vanish', cells: [[10, 16]], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'popSpikes', dir: 'up', cells: [[15, 15]], delay: 0.25,
      trigger: { kind: 'zone', x: 13, y: 8, w: 1, h: 8 } },
    { type: 'vanish', cells: [[21, 16], [22, 16]], trigger: { kind: 'onLand', x: 21, y: 12, w: 2, h: 4 } },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 2.2,
      trigger: { kind: 'timer', t: 1.0 } },
    { type: 'popSpikes', dir: 'up', cells: [[25, 15], [26, 15]], delay: 0.15,
      trigger: { kind: 'zone', x: 24, y: 12, w: 1, h: 4 } }
  ]
});

// 16: Pfeile im 1,5-Sekunden-Takt. Dazwischen: Löcher, Steine, Stacheln.
LEVELS.push({
  name: 'Spießrutenlauf', chapter: 3,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P.........^........^.....D.#',
    W],
  traps: [
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 1.5, speed: 440,
      trigger: { kind: 'timer', t: 0.8 } },
    { type: 'vanish', cells: [[8, 16], [9, 16]], trigger: { kind: 'zone', x: 7, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [11, 1], trigger: { kind: 'zone', x: 9, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [16, 1], trigger: { kind: 'zone', x: 10, y: 12, w: 1, h: 4 } },
    { type: 'crumble', cells: [[13, 16], [14, 16], [15, 16], [16, 16], [17, 16], [18, 16], [19, 16]] },
    { type: 'popSpikes', dir: 'up', cells: [[24, 15], [25, 15]], delay: 0.25,
      trigger: { kind: 'zone', x: 22, y: 12, w: 1, h: 4 } }
  ]
});

// 17: Stachelwand hinter dir, Stampfer vor dir, oben flieht die Tür zweimal.
LEVELS.push({
  name: 'Das Finale', chapter: 3,
  grid: [W, E, E, E, E, E, E,
    '#..............##..........D.#',
    '#.....................########',
    E, E, E, E, E, E,
    '#.P.......^^^................#',
    '######~~######################'],
  traps: [
    { type: 'spikeWall', fromX: -2, speed: 130, trigger: { kind: 'zone', x: 9, y: 12, w: 1, h: 4 } },
    { type: 'crusher', c: 15, w: 2, fromR: 8, toR: 15, repeat: true, pause: 1.0, offset: 0.3 },
    { type: 'fallBlock', cell: [18, 1], trigger: { kind: 'zone', x: 16, y: 12, w: 1, h: 4 } },
    { type: 'spring', cells: [[20, 15]] },
    { type: 'crumble', cells: [[24, 8]] },
    { type: 'doorMove', id: 'd1', to: [22, 7], trigger: { kind: 'zone', x: 25, y: 5, w: 1, h: 3 } },
    { type: 'doorMove', id: 'd2', to: [27, 7], trigger: { kind: 'zone', x: 22, y: 5, w: 2, h: 3, afterId: 'd1' } },
    { type: 'projectile', from: [28, 7], dir: 'left', repeat: true, interval: 2.2,
      trigger: { kind: 'after', id: 'd1', delay: 0.5 } }
  ]
});

// ================= KAPITEL 4: HÖLLE =================

// 18: Der ganze Boden bröselt, drei Stampfer, Pfeile – und ein letzter Stachel.
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
    { type: 'crusher', c: 9, w: 2, fromR: 1, toR: 15, repeat: true, pause: 1.1, offset: 0 },
    { type: 'crusher', c: 17, w: 2, fromR: 1, toR: 15, repeat: true, pause: 1.3, offset: 0.8 },
    { type: 'crusher', c: 23, w: 2, fromR: 1, toR: 15, repeat: true, pause: 1.0, offset: 0.4 },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 2.7,
      trigger: { kind: 'timer', t: 1.2 } },
    { type: 'popSpikes', dir: 'up', cells: [[26, 15]], delay: 0.12,
      trigger: { kind: 'zone', x: 24, y: 12, w: 1, h: 4 } }
  ]
});

// 19: Links ist rechts. Das ganze Level lang. Und die Landezone wächst Stacheln.
LEVELS.push({
  name: 'Spiegelverkehrt', chapter: 4,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........^^..........^^..D.#',
    '#################...##########'],
  traps: [
    { type: 'flipControls', duration: 60, trigger: { kind: 'zone', x: 4, y: 12, w: 1, h: 4 } },
    { type: 'crumble', cells: [[14, 16], [15, 16], [16, 16]] },
    { type: 'popSpikes', dir: 'up', cells: [[20, 15]], delay: 0.1,
      trigger: { kind: 'zone', x: 17, y: 8, w: 1, h: 8 } }
  ]
});

// 20: Die Wand wartet nicht. Die Pfeile kommen dir entgegen. Viel Erfolg.
LEVELS.push({
  name: 'Die Mauer', chapter: 4,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P........................D.#',
    '#######..###..#######..#######'],
  traps: [
    { type: 'spikeWall', fromX: -1, speed: 160, trigger: { kind: 'timer', t: 0.6 } },
    { type: 'popSpikes', dir: 'up', cells: [[17, 15], [18, 15]], delay: 0.2,
      trigger: { kind: 'zone', x: 10, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [15, 1], trigger: { kind: 'zone', x: 12, y: 12, w: 1, h: 4 } },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 2.4, speed: 430,
      trigger: { kind: 'timer', t: 1.5 } },
    { type: 'popSpikes', dir: 'up', cells: [[25, 15], [26, 15]], delay: 0.15,
      trigger: { kind: 'zone', x: 23, y: 12, w: 1, h: 4 } }
  ]
});

// 21: Drei Türen lügen doppelt. Die echte rennt weg. Der Rückweg schießt.
LEVELS.push({
  name: 'Türsteher', chapter: 4,
  grid: [W, E, E, E, E, E, E, E,
    '#.......................##...#',
    E, E, E, E, E, E,
    '#.P........................D.#',
    W],
  traps: [
    { type: 'fakeDoor', cell: [8, 15], id: 'f1' },
    { type: 'popSpikes', dir: 'up', cells: [[6, 15], [7, 15]], trigger: { kind: 'after', id: 'f1', delay: 0.25 } },
    { type: 'popSpikes', dir: 'up', cells: [[9, 15], [10, 15]], trigger: { kind: 'after', id: 'f1', delay: 0.25 } },
    { type: 'fakeDoor', cell: [14, 15], id: 'f2' },
    { type: 'fallBlock', cell: [16, 1], trigger: { kind: 'after', id: 'f2', delay: 0.1 } },
    { type: 'fallBlock', cell: [12, 1], trigger: { kind: 'after', id: 'f2', delay: 0.3 } },
    { type: 'fakeDoor', cell: [20, 15], id: 'f3' },
    { type: 'projectile', from: [28, 15], dir: 'left', trigger: { kind: 'after', id: 'f3', delay: 0.1 } },
    { type: 'popSpikes', dir: 'up', cells: [[18, 15]], trigger: { kind: 'after', id: 'f3', delay: 0.4 } },
    { type: 'crusher', c: 24, w: 2, fromR: 9, toR: 15, repeat: true, pause: 1.0, offset: 0 },
    { type: 'doorMove', id: 'dm', to: [4, 15], trigger: { kind: 'zone', x: 25, y: 12, w: 1, h: 4 } },
    { type: 'projectile', from: [1, 15], dir: 'right', repeat: true, interval: 2.5,
      trigger: { kind: 'after', id: 'dm', delay: 0.5 } }
  ]
});

// 22: Dunkel, bröselig, beschossen – und kurz vor der Tür dreht sich die Welt.
LEVELS.push({
  name: 'Blindflug', chapter: 4, dark: true, lightRadius: 80,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P.................^......D.#',
    '#########..###################'],
  traps: [
    { type: 'fallBlock', cell: [7, 1], trigger: { kind: 'zone', x: 5, y: 12, w: 1, h: 4 } },
    { type: 'crumble', cells: [[12, 16], [13, 16], [14, 16], [15, 16], [16, 16], [17, 16]] },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 1.9,
      trigger: { kind: 'timer', t: 1.2 } },
    { type: 'flipControls', duration: 8, trigger: { kind: 'zone', x: 19, y: 12, w: 1, h: 4 } },
    { type: 'vanish', cells: [[23, 16], [24, 16]], trigger: { kind: 'zone', x: 22, y: 12, w: 1, h: 4 } }
  ]
});

// 23: Alles auf einmal: Wand, Stampfer, Spiegel, Lügentür, Pfeile, Loch.
LEVELS.push({
  name: 'Das wahre Finale', chapter: 4,
  grid: [W, E, E, E, E, E, E,
    '#..................##........#',
    E, E, E, E, E, E, E,
    '#.P........................D.#',
    '#####~~######..###############'],
  traps: [
    { type: 'spikeWall', fromX: -1, speed: 135, trigger: { kind: 'zone', x: 8, y: 12, w: 1, h: 4 } },
    { type: 'popSpikes', dir: 'up', cells: [[10, 15], [11, 15]], delay: 0.2,
      trigger: { kind: 'zone', x: 8, y: 12, w: 1, h: 4 } },
    { type: 'crusher', c: 16, w: 2, fromR: 1, toR: 15, repeat: true, pause: 1.1, offset: 0.5 },
    { type: 'flipControls', duration: 4, trigger: { kind: 'zone', x: 19, y: 12, w: 1, h: 4 } },
    { type: 'fakeDoor', cell: [22, 15], id: 'fd' },
    { type: 'popSpikes', dir: 'up', cells: [[23, 15], [24, 15]], trigger: { kind: 'after', id: 'fd', delay: 0.12 } },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 2.3,
      trigger: { kind: 'timer', t: 2.0 } },
    { type: 'vanish', cells: [[25, 16], [26, 16]], trigger: { kind: 'zone', x: 24, y: 12, w: 1, h: 4 } }
  ]
});

// 24: Geschafft! Also ... zweimal fast. Die Tür wohnt jetzt woanders. Wieder.
LEVELS.push({
  name: 'Danke fürs Spielen', chapter: 4,
  grid: [W, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
    '#.P...D......................#',
    W],
  traps: [
    { type: 'doorMove', id: 'd1', to: [26, 15], trigger: { kind: 'zone', x: 4, y: 12, w: 1, h: 4 } },
    { type: 'fallBlock', cell: [8, 1], trigger: { kind: 'after', id: 'd1', delay: 0.2 } },
    { type: 'popSpikes', dir: 'up', cells: [[12, 15], [13, 15]], delay: 0.2,
      trigger: { kind: 'zone', x: 10, y: 12, w: 1, h: 4, afterId: 'd1' } },
    { type: 'popSpikes', dir: 'up', cells: [[18, 15], [19, 15]], delay: 0.2,
      trigger: { kind: 'zone', x: 16, y: 12, w: 1, h: 4, afterId: 'd1' } },
    { type: 'doorMove', id: 'd2', to: [6, 15], trigger: { kind: 'zone', x: 23, y: 12, w: 1, h: 4, afterId: 'd1' } },
    { type: 'projectile', from: [28, 15], dir: 'left', repeat: true, interval: 2.0,
      trigger: { kind: 'after', id: 'd2', delay: 0.5 } },
    { type: 'popSpikes', dir: 'up', cells: [[8, 15], [9, 15]], delay: 0.2,
      trigger: { kind: 'zone', x: 11, y: 12, w: 1, h: 4, afterId: 'd2' } }
  ]
});
