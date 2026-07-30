// Zeichnet das komplette Spiel auf das Canvas.
// Stil wie Level Devil: dunkler Raum, schlichte Formen, weißer Spieler mit Augen.
var Renderer = (function () {
  var T = CONFIG.TILE;
  var C = CONFIG.COLORS;
  var ctx = null;
  var particles = [];
  var flashTime = 0;
  var animTime = 0;

  function init(context) {
    ctx = context;
  }

  // --- Partikel ---------------------------------------------------
  function spawnParticles(x, y, color, count, speed) {
    for (var i = 0; i < count; i++) {
      var a = Math.random() * Math.PI * 2;
      var v = speed * (0.3 + Math.random() * 0.7);
      particles.push({
        x: x, y: y,
        vx: Math.cos(a) * v,
        vy: Math.sin(a) * v - 60,
        life: 0.4 + Math.random() * 0.3,
        color: color,
        size: 2 + Math.random() * 3
      });
    }
  }

  function poof(x, y, color) {
    spawnParticles(x, y, color || '#5a5a68', 10, 120);
  }

  function deathBurst(x, y) {
    spawnParticles(x, y, C.player, 26, 260);
    flashTime = 0.12;
  }

  function updateParticles(dt) {
    animTime += dt;
    for (var i = particles.length - 1; i >= 0; i--) {
      var p = particles[i];
      p.life -= dt;
      if (p.life <= 0) { particles.splice(i, 1); continue; }
      p.vy += 500 * dt;
      p.x += p.vx * dt;
      p.y += p.vy * dt;
    }
    flashTime = Math.max(0, flashTime - dt);
  }

  // --- Einzelteile ------------------------------------------------
  function drawTile(x, y) {
    ctx.fillStyle = C.tile;
    ctx.fillRect(x, y, T, T);
    ctx.fillStyle = C.tileEdge;
    ctx.fillRect(x, y, T, 2);
  }

  function drawSpike(s) {
    var x = s.c * T, y = s.r * T;
    ctx.fillStyle = C.spike;
    ctx.beginPath();
    // Zwei Zacken pro Tile
    if (s.dir === 'up') {
      ctx.moveTo(x, y + T); ctx.lineTo(x + T * 0.25, y + 4); ctx.lineTo(x + T * 0.5, y + T);
      ctx.lineTo(x + T * 0.75, y + 4); ctx.lineTo(x + T, y + T);
    } else if (s.dir === 'down') {
      ctx.moveTo(x, y); ctx.lineTo(x + T * 0.25, y + T - 4); ctx.lineTo(x + T * 0.5, y);
      ctx.lineTo(x + T * 0.75, y + T - 4); ctx.lineTo(x + T, y);
    } else if (s.dir === 'left') {
      ctx.moveTo(x + T, y); ctx.lineTo(x + 4, y + T * 0.25); ctx.lineTo(x + T, y + T * 0.5);
      ctx.lineTo(x + 4, y + T * 0.75); ctx.lineTo(x + T, y + T);
    } else {
      ctx.moveTo(x, y); ctx.lineTo(x + T - 4, y + T * 0.25); ctx.lineTo(x, y + T * 0.5);
      ctx.lineTo(x + T - 4, y + T * 0.75); ctx.lineTo(x, y + T);
    }
    ctx.closePath();
    ctx.fill();
  }

  function drawDoor(door) {
    var x = door.c * T, y = (door.r - 1) * T;
    ctx.fillStyle = C.door;
    roundRect(x + 4, y + 6, T - 8, 2 * T - 6, 6);
    ctx.fill();
    ctx.fillStyle = C.doorInner;
    roundRect(x + 8, y + 10, T - 16, 2 * T - 14, 4);
    ctx.fill();
    // Türknauf
    ctx.fillStyle = C.door;
    ctx.beginPath();
    ctx.arc(x + T - 12, y + T + 6, 2.5, 0, Math.PI * 2);
    ctx.fill();
  }

  function drawSpring(sp) {
    var x = sp.c * T, y = sp.r * T;
    var compress = sp.anim > 0 ? 6 : 0;
    ctx.strokeStyle = C.spring;
    ctx.lineWidth = 3;
    ctx.beginPath();
    var baseY = y + T - 4;
    var topY = y + 14 + compress;
    for (var i = 0; i < 3; i++) {
      var yy = topY + (baseY - topY) * (i / 3);
      ctx.moveTo(x + 7, yy + 3);
      ctx.lineTo(x + T - 7, yy);
    }
    ctx.stroke();
    ctx.fillStyle = C.spring;
    ctx.fillRect(x + 5, topY - 5, T - 10, 5);
    ctx.fillStyle = C.platform;
    ctx.fillRect(x + 3, baseY, T - 6, 4);
  }

  function roundRect(x, y, w, h, r) {
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.arcTo(x + w, y, x + w, y + h, r);
    ctx.arcTo(x + w, y + h, x, y + h, r);
    ctx.arcTo(x, y + h, x, y, r);
    ctx.arcTo(x, y, x + w, y, r);
    ctx.closePath();
  }

  function drawPlayer(S) {
    var squash = S.squash > 0 ? S.squash / 0.12 : 0;
    var stretch = S.stretch > 0 ? S.stretch / 0.14 : 0;
    var sx = 1 + squash * 0.25 - stretch * 0.15;
    var sy = 1 - squash * 0.25 + stretch * 0.2;
    var w = S.w * sx, h = S.h * sy;
    var x = S.x + (S.w - w) / 2;
    var y = S.y + (S.h - h); // am Boden verankert
    ctx.fillStyle = C.player;
    roundRect(x, y, w, h, 5);
    ctx.fill();
    // Augen schauen in Laufrichtung
    var eyeY = y + h * 0.32 + (S.vy > 200 ? 2 : S.vy < -200 ? -2 : 0);
    var off = S.face * 2.5;
    ctx.fillStyle = C.eye;
    ctx.beginPath();
    ctx.arc(x + w * 0.34 + off, eyeY, 2.6, 0, Math.PI * 2);
    ctx.arc(x + w * 0.66 + off, eyeY, 2.6, 0, Math.PI * 2);
    ctx.fill();
  }

  // --- Hauptzeichnung --------------------------------------------
  function draw(rt, playerState, playerVisible) {
    ctx.fillStyle = C.bg;
    ctx.fillRect(0, 0, CONFIG.WIDTH, CONFIG.HEIGHT);

    // Warte-Markierung (pulsierender Ring)
    if (rt.def.marker) {
      var mx = rt.def.marker[0] * T + T / 2;
      var my = rt.def.marker[1] * T + T / 2;
      var pulse = 6 + Math.sin(animTime * 4) * 3;
      ctx.strokeStyle = C.marker;
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(mx, my, pulse + 6, 0, Math.PI * 2);
      ctx.stroke();
    }

    // Fake-Türen zuerst – absichtlich identisch mit der echten Tür
    rt.fakeDoors.forEach(function (fd) {
      if (!fd.gone) drawDoor(fd);
    });
    drawDoor(rt.door);

    // Wackel-Zellen sammeln
    var shakeSet = {};
    rt.shakes.forEach(function (s) { shakeSet[s.c + ',' + s.r] = true; });

    // Tiles (solide + Fake-Böden sehen identisch aus – Absicht!)
    for (var r = 0; r < CONFIG.ROWS; r++) {
      for (var c = 0; c < CONFIG.COLS; c++) {
        var solid = rt.solid[r][c];
        var fake = rt.fake[r][c];
        if (solid === 1 || fake) {
          var ox = 0, oy = 0;
          if (shakeSet[c + ',' + r]) {
            ox = (Math.random() - 0.5) * 4;
            oy = (Math.random() - 0.5) * 4;
          }
          drawTile(c * T + ox, r * T + oy);
        } else if (solid === 2 && rt.revealed[c + ',' + r]) {
          ctx.strokeStyle = C.invisibleWall;
          ctx.lineWidth = 2;
          ctx.setLineDash([4, 4]);
          ctx.strokeRect(c * T + 2, r * T + 2, T - 4, T - 4);
          ctx.setLineDash([]);
        }
      }
    }

    rt.spikes.forEach(drawSpike);
    rt.springs.forEach(drawSpring);

    // Bewegliche Plattformen
    rt.platforms.forEach(function (p) {
      ctx.fillStyle = C.platform;
      roundRect(p.x, p.y, p.w, p.h, 4);
      ctx.fill();
      ctx.fillStyle = C.tileEdge;
      ctx.fillRect(p.x + 2, p.y, p.w - 4, 2);
    });

    // Crusher: Schaft + Kopf
    rt.crushers.forEach(function (cr) {
      var x = cr.def.c * T, w = cr.def.w * T;
      var baseY = cr.def.fromR * T;
      ctx.fillStyle = '#2a2a33';
      ctx.fillRect(x + w / 2 - 5, baseY - 4, 10, cr.y - baseY + 8);
      ctx.fillStyle = C.crusher;
      roundRect(x + 2, cr.y, w - 4, T - 4, 3);
      ctx.fill();
      ctx.fillStyle = C.spike;
      var teeth = Math.floor(w / 8);
      for (var i = 0; i < teeth; i++) {
        ctx.beginPath();
        ctx.moveTo(x + 3 + i * 8, cr.y + T - 4);
        ctx.lineTo(x + 7 + i * 8, cr.y + T + 3);
        ctx.lineTo(x + 11 + i * 8, cr.y + T - 4);
        ctx.closePath();
        ctx.fill();
      }
    });

    // Fallende Blöcke
    rt.blocks.forEach(function (b) {
      if (b.state === 'settled') return; // liegt jetzt als solides Tile im Grid
      ctx.fillStyle = C.block;
      roundRect(b.c * T + 2, b.y + 2, T - 4, T - 4, 3);
      ctx.fill();
      ctx.strokeStyle = C.tileEdge;
      ctx.lineWidth = 1.5;
      ctx.beginPath();
      ctx.moveTo(b.c * T + 9, b.y + 9);
      ctx.lineTo(b.c * T + 16, b.y + 18);
      ctx.lineTo(b.c * T + 12, b.y + 25);
      ctx.stroke();
    });

    // Projektile (Pfeile)
    rt.projectiles.forEach(function (pr) {
      var dir = pr.vx > 0 ? 1 : -1;
      var tipX = dir > 0 ? pr.x + pr.w : pr.x;
      var backX = dir > 0 ? pr.x : pr.x + pr.w;
      ctx.strokeStyle = C.projectile;
      ctx.lineWidth = 3;
      ctx.beginPath();
      ctx.moveTo(backX, pr.y + pr.h / 2);
      ctx.lineTo(tipX, pr.y + pr.h / 2);
      ctx.stroke();
      ctx.fillStyle = C.projectile;
      ctx.beginPath();
      ctx.moveTo(tipX + dir * 6, pr.y + pr.h / 2);
      ctx.lineTo(tipX - dir * 4, pr.y - 1);
      ctx.lineTo(tipX - dir * 4, pr.y + pr.h + 1);
      ctx.closePath();
      ctx.fill();
    });

    // Nachrückende Stachelwand
    rt.walls.forEach(function (w) {
      if (w.x <= 0) return;
      ctx.fillStyle = '#1b1b22';
      ctx.fillRect(0, 0, w.x - 6, CONFIG.HEIGHT);
      ctx.fillStyle = C.spike;
      for (var wy = 0; wy < CONFIG.HEIGHT; wy += 16) {
        ctx.beginPath();
        ctx.moveTo(w.x - 8, wy);
        ctx.lineTo(w.x + 2, wy + 8);
        ctx.lineTo(w.x - 8, wy + 16);
        ctx.closePath();
        ctx.fill();
      }
    });

    if (playerVisible && playerState) drawPlayer(playerState);

    // Partikel
    particles.forEach(function (p) {
      ctx.globalAlpha = Math.min(1, p.life * 3);
      ctx.fillStyle = p.color;
      ctx.fillRect(p.x - p.size / 2, p.y - p.size / 2, p.size, p.size);
    });
    ctx.globalAlpha = 1;

    // Dunkel-Level: nur ein Lichtkreis um den Spieler
    if (rt.def.dark && playerState) {
      var px = playerState.x + playerState.w / 2;
      var py = playerState.y + playerState.h / 2;
      var radius = rt.def.lightRadius || CONFIG.LIGHT_RADIUS;
      var grad = ctx.createRadialGradient(px, py, radius * 0.37, px, py, radius);
      grad.addColorStop(0, 'rgba(8,8,12,0)');
      grad.addColorStop(1, 'rgba(8,8,12,0.985)');
      ctx.fillStyle = grad;
      ctx.fillRect(0, 0, CONFIG.WIDTH, CONFIG.HEIGHT);
    }

    // Gespiegelte Steuerung: unübersehbarer Hinweis
    if (rt.flipUntil > rt.time) {
      ctx.fillStyle = 'rgba(150, 80, 220, 0.10)';
      ctx.fillRect(0, 0, CONFIG.WIDTH, CONFIG.HEIGHT);
      ctx.fillStyle = '#c9a0f0';
      ctx.font = 'bold 26px system-ui, sans-serif';
      ctx.textAlign = 'center';
      ctx.fillText(I18N.t('flipped'), CONFIG.WIDTH / 2, 46);
      ctx.textAlign = 'left';
    }

    // Todes-Blitz
    if (flashTime > 0) {
      ctx.fillStyle = 'rgba(255,255,255,' + (flashTime / 0.12 * 0.5) + ')';
      ctx.fillRect(0, 0, CONFIG.WIDTH, CONFIG.HEIGHT);
    }
  }

  return {
    init: init,
    draw: draw,
    poof: poof,
    deathBurst: deathBurst,
    updateParticles: updateParticles
  };
})();
