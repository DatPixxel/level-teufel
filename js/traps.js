// Fallen-System: baut aus einer Level-Definition den Laufzeitzustand
// und wertet Trigger aus. Beim Respawn wird alles neu aufgebaut,
// dadurch sind alle Fallen automatisch wieder scharf.
//
// Trigger-Arten:
//   zone   – Spieler betritt einen Bereich (Tile-Koordinaten x,y,w,h)
//   onLand – Spieler landet innerhalb eines Bereichs
//   onJump – Spieler springt innerhalb eines Bereichs ab
//   timer  – t Sekunden nach Levelstart
//   after  – delay Sekunden nachdem Falle mit id gefeuert hat
//   stay   – Spieler bleibt duration Sekunden im Bereich
// Zusätzlich für alle: afterId (nur scharf, nachdem id gefeuert hat)
// und unlessId (nie feuern, wenn id bereits gefeuert hat).
var Traps = (function () {
  var T = CONFIG.TILE;

  function cellRect(c, r) {
    return { x: c * T, y: r * T, w: T, h: T };
  }

  function intersects(a, b) {
    return a.x < b.x + b.w && a.x + a.w > b.x && a.y < b.y + b.h && a.y + a.h > b.y;
  }

  function zoneRect(z) {
    return { x: z.x * T, y: z.y * T, w: z.w * T, h: z.h * T };
  }

  // ---------------------------------------------------------------
  // Laufzeitzustand aus der Level-Definition bauen
  // ---------------------------------------------------------------
  function buildRuntime(def) {
    if (def.grid.length !== CONFIG.ROWS) {
      throw new Error('Level "' + def.name + '": Grid hat ' + def.grid.length + ' Zeilen statt ' + CONFIG.ROWS);
    }
    var rt = {
      def: def,
      time: 0,
      solid: [],      // 0 = frei, 1 = solide, 2 = unsichtbare Wand
      fake: [],       // sieht solide aus, ist aber keine Kollision
      spikes: [],     // {c, r, dir}
      door: null,
      spawn: null,
      traps: [],
      platforms: [],
      crushers: [],
      blocks: [],
      projectiles: [],
      emitters: [],
      springs: [],
      shakes: [],     // Wackel-Animationen vor dem Verschwinden
      pending: [],    // zeitversetzte Aktionen
      revealed: {},   // aufgedeckte unsichtbare Wände ("c,r" -> true)
      firedAt: {},
      trapSignal: false // fürs HUD/Sound: diese Runde hat eine Falle gefeuert
    };

    for (var r = 0; r < CONFIG.ROWS; r++) {
      var row = def.grid[r];
      if (row.length !== CONFIG.COLS) {
        throw new Error('Level "' + def.name + '": Zeile ' + r + ' hat ' + row.length + ' Zeichen statt ' + CONFIG.COLS);
      }
      rt.solid[r] = [];
      rt.fake[r] = [];
      for (var c = 0; c < CONFIG.COLS; c++) {
        var ch = row[c];
        rt.solid[r][c] = ch === '#' ? 1 : 0;
        rt.fake[r][c] = ch === '~';
        if (ch === '^') rt.spikes.push({ c: c, r: r, dir: 'up' });
        if (ch === 'v') rt.spikes.push({ c: c, r: r, dir: 'down' });
        if (ch === '<') rt.spikes.push({ c: c, r: r, dir: 'left' });
        if (ch === '>') rt.spikes.push({ c: c, r: r, dir: 'right' });
        if (ch === 'P') rt.spawn = { c: c, r: r };
        if (ch === 'D') rt.door = { c: c, r: r };
      }
    }
    if (!rt.spawn) throw new Error('Level "' + def.name + '": kein Startpunkt P');
    if (!rt.door) throw new Error('Level "' + def.name + '": keine Tür D');

    var defs = def.traps || [];
    for (var i = 0; i < defs.length; i++) {
      var td = defs[i];
      var inst = { def: td, state: 'armed', insideTime: 0 };
      rt.traps.push(inst);
      switch (td.type) {
        case 'invisibleWall':
          td.cells.forEach(function (cell) { rt.solid[cell[1]][cell[0]] = 2; });
          inst.state = 'done';
          break;
        case 'spring':
          td.cells.forEach(function (cell) { rt.springs.push({ c: cell[0], r: cell[1], anim: 0 }); });
          inst.state = 'done';
          break;
        case 'movingPlatform':
          rt.platforms.push({
            def: td,
            x: td.x1 * T, y: td.y1 * T,
            w: (td.w || 3) * T, h: 14,
            t: 0, dir: 1, dx: 0, dy: 0,
            reversed: false, hasPlayer: false
          });
          inst.state = 'done';
          break;
        case 'crusher':
          rt.crushers.push({
            def: td, inst: inst,
            y: td.fromR * T,
            phase: td.repeat ? 'wait' : 'idle',
            t: -(td.offset || 0)
          });
          break;
        case 'fallBlock':
          rt.blocks.push({ def: td, inst: inst, c: td.cell[0], r: td.cell[1], y: td.cell[1] * T, vy: 0, state: 'hang' });
          break;
      }
    }
    return rt;
  }

  function isSolid(rt, c, r) {
    if (c < 0 || c >= CONFIG.COLS) return true;
    if (r < 0) return true;              // Decke: oben kommt man nicht raus
    if (r >= CONFIG.ROWS) return false;  // unten: freier Fall = Tod
    return rt.solid[r][c] > 0;
  }

  // ---------------------------------------------------------------
  // Aktionen beim Feuern einer Falle
  // ---------------------------------------------------------------
  function fire(rt, inst) {
    var td = inst.def;
    inst.state = 'fired';
    inst.firedTime = rt.time;
    if (td.id) rt.firedAt[td.id] = rt.time;
    rt.trapSignal = true;

    switch (td.type) {
      case 'vanish':
        td.cells.forEach(function (cell) {
          rt.shakes.push({ c: cell[0], r: cell[1], until: rt.time + CONFIG.VANISH_SHAKE });
        });
        rt.pending.push({
          at: rt.time + CONFIG.VANISH_SHAKE,
          run: function () {
            td.cells.forEach(function (cell) {
              rt.solid[cell[1]][cell[0]] = 0;
              rt.fake[cell[1]][cell[0]] = false;
              Renderer.poof(cell[0] * T + T / 2, cell[1] * T + T / 2, '#5a5a68');
            });
            Sfx.trap();
          }
        });
        break;

      case 'appear':
        td.cells.forEach(function (cell) {
          rt.solid[cell[1]][cell[0]] = 1;
          Renderer.poof(cell[0] * T + T / 2, cell[1] * T + T / 2, '#5a5a68');
        });
        Sfx.trap();
        rt.appearCheck = td.cells; // Zerquetsch-Prüfung im selben Tick
        break;

      case 'popSpikes':
        rt.pending.push({
          at: rt.time + (td.delay != null ? td.delay : 0.12),
          run: function () {
            td.cells.forEach(function (cell) {
              rt.spikes.push({ c: cell[0], r: cell[1], dir: td.dir || 'up', popped: true });
            });
            Sfx.trap();
          }
        });
        break;

      case 'hideSpikes':
        rt.spikes = rt.spikes.filter(function (s) {
          var hit = td.cells.some(function (cell) { return cell[0] === s.c && cell[1] === s.r; });
          if (hit) Renderer.poof(s.c * T + T / 2, s.r * T + T / 2, '#8d8d99');
          return !hit;
        });
        Sfx.poof();
        break;

      case 'fallBlock':
        rt.blocks.forEach(function (b) {
          if (b.inst === inst) b.state = 'fall';
        });
        break;

      case 'projectile':
        spawnProjectile(rt, td);
        if (td.repeat) rt.emitters.push({ def: td, nextAt: rt.time + (td.interval || 2) });
        break;

      case 'doorMove':
        Renderer.poof(rt.door.c * T + T / 2, rt.door.r * T, CONFIG.COLORS.door);
        rt.door = { c: td.to[0], r: td.to[1] };
        Renderer.poof(rt.door.c * T + T / 2, rt.door.r * T, CONFIG.COLORS.door);
        Sfx.poof();
        break;

      case 'crusher':
        rt.crushers.forEach(function (cr) {
          if (cr.inst === inst && cr.phase === 'idle') { cr.phase = 'slam'; }
        });
        break;

      case 'movingPlatform':
        // Trigger auf einer Plattform: Richtung einmalig umkehren
        rt.platforms.forEach(function (p) {
          if (p.def === td) p.dir *= -1;
        });
        break;
    }
  }

  function spawnProjectile(rt, td) {
    var dir = td.dir === 'right' ? 1 : -1;
    rt.projectiles.push({
      x: td.from[0] * T + (dir > 0 ? 0 : T - 22),
      y: td.from[1] * T + 11,
      w: 22, h: 10,
      vx: dir * (td.speed || CONFIG.PROJECTILE_SPEED)
    });
    Sfx.trap();
  }

  // ---------------------------------------------------------------
  // Vor der Spieler-Physik: bewegliche Objekte updaten
  // ---------------------------------------------------------------
  function preUpdate(rt, dt) {
    rt.time += dt;
    rt.trapSignal = false;
    rt.appearCheck = null;

    // Zeitversetzte Aktionen
    for (var i = rt.pending.length - 1; i >= 0; i--) {
      if (rt.time >= rt.pending[i].at) {
        var p = rt.pending.splice(i, 1)[0];
        p.run();
      }
    }
    rt.shakes = rt.shakes.filter(function (s) { return rt.time < s.until; });

    // Plattformen
    rt.platforms.forEach(function (p) {
      var td = p.def;
      var x1 = td.x1 * T, y1 = td.y1 * T, x2 = td.x2 * T, y2 = td.y2 * T;
      var len = Math.hypot(x2 - x1, y2 - y1) || 1;
      var speed = (td.speed || CONFIG.PLATFORM_SPEED) / len;
      p.t += p.dir * speed * dt;
      if (p.t > 1) { p.t = 1; p.dir = -1; }
      if (p.t < 0) { p.t = 0; p.dir = 1; }
      var nx = x1 + (x2 - x1) * p.t;
      var ny = y1 + (y2 - y1) * p.t;
      p.dx = nx - p.x;
      p.dy = ny - p.y;
      p.x = nx;
      p.y = ny;
    });

    // Crusher
    rt.crushers.forEach(function (cr) {
      var td = cr.def;
      var fromY = td.fromR * T, toY = td.toR * T;
      cr.t += dt;
      switch (cr.phase) {
        case 'wait':
          if (cr.t >= 0 && cr.t >= (td.pause || 0.9)) { cr.phase = 'slam'; cr.t = 0; }
          break;
        case 'slam':
          cr.y += CONFIG.CRUSHER_SLAM * dt;
          if (cr.y >= toY) { cr.y = toY; cr.phase = 'hold'; cr.t = 0; }
          break;
        case 'hold':
          if (cr.t >= 0.25) { cr.phase = 'retract'; cr.t = 0; }
          break;
        case 'retract':
          cr.y -= CONFIG.CRUSHER_RETRACT * dt;
          if (cr.y <= fromY) {
            cr.y = fromY;
            cr.t = 0;
            cr.phase = td.repeat ? 'wait' : 'idle';
          }
          break;
      }
    });

    // Fallende Blöcke
    rt.blocks.forEach(function (b) {
      if (b.state !== 'fall') return;
      b.vy += CONFIG.FALLBLOCK_GRAVITY * dt;
      b.y += b.vy * dt;
      var rr = Math.floor((b.y + T) / T);
      if (isSolid(rt, b.c, rr)) {
        b.y = rr * T - T;
        b.r = rr - 1;
        b.state = 'settled';
        rt.solid[b.r][b.c] = 1;
        Renderer.poof(b.c * T + T / 2, b.y + T, '#5a5a68');
        Sfx.trap();
      }
    });

    // Projektile
    rt.projectiles = rt.projectiles.filter(function (pr) {
      pr.x += pr.vx * dt;
      return pr.x > -40 && pr.x < CONFIG.WIDTH + 40;
    });
    rt.emitters.forEach(function (em) {
      if (rt.time >= em.nextAt) {
        spawnProjectile(rt, em.def);
        em.nextAt = rt.time + (em.def.interval || 2);
      }
    });

    // Federn-Animation abklingen lassen
    rt.springs.forEach(function (s) { if (s.anim > 0) s.anim -= dt; });
  }

  // ---------------------------------------------------------------
  // Nach der Spieler-Physik: Trigger auswerten + tödliche Kontakte
  // Rückgabe: { kill: bool }
  // ---------------------------------------------------------------
  function postUpdate(rt, pl, dt) {
    var box = { x: pl.x, y: pl.y, w: pl.w, h: pl.h };

    // Trigger
    rt.traps.forEach(function (inst) {
      if (inst.state !== 'armed') return;
      var tg = inst.def.trigger;
      if (!tg) { fire(rt, inst); return; }
      if (tg.unlessId && rt.firedAt[tg.unlessId] != null) { inst.state = 'done'; return; }
      if (tg.afterId && rt.firedAt[tg.afterId] == null) return;

      var hit = false;
      switch (tg.kind) {
        case 'zone':
          hit = intersects(box, zoneRect(tg));
          break;
        case 'onLand':
          hit = pl.justLanded && intersects(box, zoneRect(tg));
          break;
        case 'onJump':
          hit = pl.justJumped && intersects(box, zoneRect(tg));
          break;
        case 'timer':
          hit = rt.time >= tg.t;
          break;
        case 'after':
          hit = rt.firedAt[tg.id] != null && rt.time >= rt.firedAt[tg.id] + (tg.delay || 0);
          break;
        case 'stay':
          if (intersects(box, zoneRect(tg))) {
            inst.insideTime += dt;
            if (inst.insideTime >= tg.duration) hit = true;
          } else {
            inst.insideTime = 0;
          }
          break;
      }
      if (hit) fire(rt, inst);
    });

    // Tödliche Kontakte
    var inset = CONFIG.SPIKE_INSET;
    var kill = false;

    rt.spikes.forEach(function (s) {
      var rect = { x: s.c * T + inset, y: s.r * T + inset, w: T - 2 * inset, h: T - 2 * inset };
      if (intersects(box, rect)) kill = true;
    });

    rt.crushers.forEach(function (cr) {
      var head = { x: cr.def.c * T + 2, y: cr.y, w: cr.def.w * T - 4, h: T - 4 };
      if (intersects(box, head)) kill = true;
    });

    rt.blocks.forEach(function (b) {
      if (b.state !== 'fall') return;
      var rect = { x: b.c * T + 2, y: b.y + 2, w: T - 4, h: T - 4 };
      if (intersects(box, rect)) kill = true;
    });

    rt.projectiles.forEach(function (pr) {
      if (intersects(box, pr)) kill = true;
    });

    if (rt.appearCheck) {
      rt.appearCheck.forEach(function (cell) {
        if (intersects(box, cellRect(cell[0], cell[1]))) kill = true;
      });
      rt.appearCheck = null;
    }

    return { kill: kill };
  }

  return {
    buildRuntime: buildRuntime,
    isSolid: isSolid,
    preUpdate: preUpdate,
    postUpdate: postUpdate,
    intersects: intersects
  };
})();
