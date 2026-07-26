// Spieler: Physik, Kollision mit dem Tile-Grid und Plattformen,
// Coyote-Time + Jump-Buffer für ein präzises, faires Sprunggefühl.
var Player = (function () {
  var T = CONFIG.TILE;
  var S = null;

  function reset(rt) {
    S = {
      x: rt.spawn.c * T + (T - CONFIG.PLAYER_W) / 2,
      y: (rt.spawn.r + 1) * T - CONFIG.PLAYER_H,
      w: CONFIG.PLAYER_W,
      h: CONFIG.PLAYER_H,
      vx: 0, vy: 0,
      face: 1,
      onGround: false,
      coyote: 0,
      buffer: 0,
      cutApplied: false,
      justLanded: false,
      justJumped: false,
      squash: 0,   // Landeanimation
      stretch: 0,  // Sprunganimation
      platform: null
    };
    return S;
  }

  function reveal(rt, c, r) {
    if (r >= 0 && r < CONFIG.ROWS && c >= 0 && c < CONFIG.COLS && rt.solid[r][c] === 2) {
      rt.revealed[c + ',' + r] = true;
    }
  }

  function update(rt, input, dt) {
    var wasOnGround = S.onGround;
    S.justLanded = false;
    S.justJumped = false;

    // Von der Plattform mitnehmen lassen
    if (S.platform) {
      var p = S.platform;
      S.x += p.dx;
      S.y += p.dy;
      var stillOn = S.x + S.w > p.x && S.x < p.x + p.w && Math.abs((S.y + S.h) - p.y) < 6;
      if (!stillOn) S.platform = null;
    }

    // Horizontal: sofortige Reaktion (Präzisions-Plattformer)
    // Gemeine Ausnahme: die flipControls-Falle spiegelt links/rechts
    var flipped = rt.flipUntil > rt.time;
    var inLeft = flipped ? input.right : input.left;
    var inRight = flipped ? input.left : input.right;
    var dir = (inRight ? 1 : 0) - (inLeft ? 1 : 0);
    S.vx = dir * CONFIG.RUN_SPEED;
    if (dir !== 0) S.face = dir;

    // Springen (mit Buffer + Coyote)
    if (input.jumpPressed) S.buffer = CONFIG.JUMP_BUFFER;
    if (S.buffer > 0 && (S.onGround || S.coyote > 0)) {
      S.vy = -CONFIG.JUMP_VEL;
      S.buffer = 0;
      S.coyote = 0;
      S.onGround = false;
      S.platform = null;
      S.cutApplied = false;
      S.justJumped = true;
      S.stretch = 0.14;
      Sfx.jump();
    }
    S.buffer = Math.max(0, S.buffer - dt);

    // Variable Sprunghöhe: Loslassen kappt den Sprung einmalig
    if (!input.jumpHeld && S.vy < 0 && !S.cutApplied) {
      S.vy *= CONFIG.JUMP_CUT;
      S.cutApplied = true;
    }

    // Gravitation
    S.vy = Math.min(S.vy + CONFIG.GRAVITY * dt, CONFIG.MAX_FALL);

    // --- X-Achse bewegen und auflösen ---
    S.x += S.vx * dt;
    var top = Math.floor(S.y / T);
    var bot = Math.floor((S.y + S.h - 0.01) / T);
    var r, c;
    if (S.vx > 0) {
      c = Math.floor((S.x + S.w - 0.01) / T);
      for (r = top; r <= bot; r++) {
        if (Traps.isSolid(rt, c, r)) {
          S.x = c * T - S.w;
          reveal(rt, c, r);
          break;
        }
      }
    } else if (S.vx < 0) {
      c = Math.floor(S.x / T);
      for (r = top; r <= bot; r++) {
        if (Traps.isSolid(rt, c, r)) {
          S.x = (c + 1) * T;
          reveal(rt, c, r);
          break;
        }
      }
    }
    if (S.x < 0) S.x = 0;
    if (S.x + S.w > CONFIG.WIDTH) S.x = CONFIG.WIDTH - S.w;

    // --- Y-Achse bewegen und auflösen ---
    var prevBottom = S.y + S.h;
    S.y += S.vy * dt;
    S.onGround = false;
    var left = Math.floor(S.x / T);
    var right = Math.floor((S.x + S.w - 0.01) / T);
    if (S.vy >= 0) {
      r = Math.floor((S.y + S.h - 0.01) / T);
      for (c = left; c <= right; c++) {
        if (Traps.isSolid(rt, c, r)) {
          S.y = r * T - S.h;
          S.vy = 0;
          S.onGround = true;
          reveal(rt, c, r);
          break;
        }
      }
    } else {
      r = Math.floor(S.y / T);
      for (c = left; c <= right; c++) {
        if (Traps.isSolid(rt, c, r)) {
          S.y = (r + 1) * T;
          S.vy = 0;
          reveal(rt, c, r);
          break;
        }
      }
    }

    // Auf bewegliche Plattformen landen (nur von oben)
    if (!S.onGround && S.vy >= 0) {
      for (var i = 0; i < rt.platforms.length; i++) {
        var pl = rt.platforms[i];
        var newBottom = S.y + S.h;
        var overlapX = S.x + S.w > pl.x + 2 && S.x < pl.x + pl.w - 2;
        if (overlapX && prevBottom <= pl.y + 6 && newBottom >= pl.y) {
          S.y = pl.y - S.h;
          S.vy = 0;
          S.onGround = true;
          S.platform = pl;
          if (!wasOnGround && pl.def.reverseOnLand && (pl.def.every || !pl.reversed)) {
            pl.dir *= -1;
            pl.reversed = true;
            Sfx.trap();
          }
          break;
        }
      }
    }

    if (S.onGround) {
      S.coyote = CONFIG.COYOTE_TIME;
      if (!wasOnGround) {
        S.justLanded = true;
        S.squash = 0.12;
      }
    } else {
      S.coyote = Math.max(0, S.coyote - dt);
    }
    S.squash = Math.max(0, S.squash - dt);
    S.stretch = Math.max(0, S.stretch - dt);

    // Federn
    for (var j = 0; j < rt.springs.length; j++) {
      var sp = rt.springs[j];
      var pad = { x: sp.c * T + 4, y: sp.r * T + 14, w: T - 8, h: T - 14 };
      var box = { x: S.x, y: S.y, w: S.w, h: S.h };
      if (Traps.intersects(box, pad) && S.vy >= -50) {
        S.vy = -CONFIG.SPRING_VEL;
        S.onGround = false;
        S.platform = null;
        S.cutApplied = true; // Federsprung wird nicht gekappt
        sp.anim = 0.3;
        S.stretch = 0.2;
        Sfx.spring();
      }
    }

    // Tür erreicht?
    var door = rt.door;
    var doorRect = { x: door.c * T + 6, y: (door.r - 1) * T + 8, w: T - 12, h: 2 * T - 8 };
    var won = Traps.intersects({ x: S.x, y: S.y, w: S.w, h: S.h }, doorRect);

    // Aus dem Bildschirm gefallen
    var fellOut = S.y > CONFIG.HEIGHT + 80;

    return { won: won, fellOut: fellOut };
  }

  return {
    reset: reset,
    update: update,
    get state() { return S; }
  };
})();
