// Eingabe: Tastatur (Desktop) + Touch-Buttons (Handy).
// Liefert pro Tick: left, right, jumpHeld, jumpPressed (Flanke, wird konsumiert).
var Input = (function () {
  var keys = {};
  var touch = { left: false, right: false, jump: false };
  var jumpPressed = false;
  var restartPressed = false;
  var lastWasTouch = false;

  // Test-Haken: automatisierte Tests können die Eingabe direkt setzen.
  var override = null;

  var KEY_LEFT = ['ArrowLeft', 'KeyA'];
  var KEY_RIGHT = ['ArrowRight', 'KeyD'];
  var KEY_JUMP = ['Space', 'ArrowUp', 'KeyW'];

  function any(list) {
    for (var i = 0; i < list.length; i++) if (keys[list[i]]) return true;
    return false;
  }

  window.addEventListener('keydown', function (e) {
    if (e.repeat) return;
    keys[e.code] = true;
    lastWasTouch = false;
    if (KEY_JUMP.indexOf(e.code) >= 0) { jumpPressed = true; Sfx.unlock(); }
    if (e.code === 'KeyR') restartPressed = true;
    if (e.code === 'Escape') Input.escPressed = true;
    if (['Space', 'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].indexOf(e.code) >= 0) {
      e.preventDefault();
    }
  });

  window.addEventListener('keyup', function (e) { keys[e.code] = false; });

  window.addEventListener('blur', function () {
    keys = {};
    touch.left = touch.right = touch.jump = false;
  });

  // Floating-Joypad: Die linke Bildschirmhälfte ist die Lauf-Zone.
  // Wo der Daumen landet, erscheint das Pad; Ziehen relativ zum
  // Auflagepunkt steuert links/rechts (kleine Totzone).
  function bindMoveZone(zone, pad, knob) {
    if (!zone) return;
    var pid = null;
    var originX = 0;

    function apply(clientX) {
      var dx = clientX - originX;
      var dead = 10;
      touch.left = dx < -dead;
      touch.right = dx > dead;
      if (pad && knob) {
        var max = pad.offsetWidth / 2 - pad.offsetHeight / 2;
        var off = Math.max(-max, Math.min(max, dx));
        knob.style.transform = 'translateX(' + off.toFixed(1) + 'px)';
      }
    }

    function reset() {
      pid = null;
      touch.left = touch.right = false;
      if (pad) pad.classList.add('off');
      if (knob) knob.style.transform = 'translateX(0)';
    }

    zone.addEventListener('pointerdown', function (e) {
      e.preventDefault();
      if (pid !== null) return; // nur ein Finger steuert das Laufen
      pid = e.pointerId;
      try { zone.setPointerCapture(e.pointerId); } catch (err) { /* synthetische Events */ }
      lastWasTouch = true;
      Sfx.unlock();
      originX = e.clientX;
      if (pad) {
        pad.style.left = e.clientX + 'px';
        pad.style.top = e.clientY + 'px';
        pad.classList.remove('off');
      }
      apply(e.clientX);
    });
    zone.addEventListener('pointermove', function (e) {
      if (e.pointerId !== pid) return;
      apply(e.clientX);
    });
    function release(e) {
      if (e.pointerId === pid) reset();
    }
    zone.addEventListener('pointerup', release);
    zone.addEventListener('pointercancel', release);
    zone.addEventListener('lostpointercapture', release);
    zone.addEventListener('contextmenu', function (e) { e.preventDefault(); });
    window.addEventListener('blur', reset);
  }

  // Sprung-Zone (rechte Bildschirmhälfte): tippen = springen,
  // halten = höher springen. Ein Ring zeigt die Tipp-Position.
  function bindJumpZone(zone, ring) {
    if (!zone) return;
    var pid = null;

    function reset() {
      pid = null;
      touch.jump = false;
      if (ring) ring.classList.add('off');
    }

    zone.addEventListener('pointerdown', function (e) {
      e.preventDefault();
      if (pid !== null) return;
      pid = e.pointerId;
      try { zone.setPointerCapture(e.pointerId); } catch (err) { /* synthetische Events */ }
      lastWasTouch = true;
      Sfx.unlock();
      touch.jump = true;
      jumpPressed = true;
      if (ring) {
        ring.style.left = e.clientX + 'px';
        ring.style.top = e.clientY + 'px';
        ring.classList.remove('off');
      }
    });
    function release(e) {
      if (e.pointerId === pid) reset();
    }
    zone.addEventListener('pointerup', release);
    zone.addEventListener('pointercancel', release);
    zone.addEventListener('lostpointercapture', release);
    zone.addEventListener('contextmenu', function (e) { e.preventDefault(); });
    window.addEventListener('blur', reset);
  }

  return {
    escPressed: false,
    init: function () {
      bindMoveZone(
        document.getElementById('tz-left'),
        document.getElementById('joypad'),
        document.getElementById('joypad-knob')
      );
      bindJumpZone(
        document.getElementById('tz-right'),
        document.getElementById('jump-ring')
      );
    },
    state: function () {
      if (override) {
        var o = {
          left: !!override.left,
          right: !!override.right,
          jumpHeld: !!override.jump,
          jumpPressed: !!override.jumpPressed
        };
        override.jumpPressed = false;
        return o;
      }
      var s = {
        left: any(KEY_LEFT) || touch.left,
        right: any(KEY_RIGHT) || touch.right,
        jumpHeld: any(KEY_JUMP) || touch.jump,
        jumpPressed: jumpPressed
      };
      jumpPressed = false;
      return s;
    },
    consumeRestart: function () {
      var r = restartPressed;
      restartPressed = false;
      return r;
    },
    consumeEsc: function () {
      var r = Input.escPressed;
      Input.escPressed = false;
      return r;
    },
    usingTouch: function () { return lastWasTouch; },
    setOverride: function (o) { override = o; }
  };
})();
