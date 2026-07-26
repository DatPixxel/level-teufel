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

  // Joypad (links/rechts): breiter Schieber statt zweier Tasten.
  // Der Daumen kann irgendwo auf dem Pad landen und zieht dann in die
  // gewünschte Richtung – mit kleiner Totzone in der Mitte.
  function bindJoypad(pad, knob) {
    if (!pad) return;
    var pid = null;

    function setDir(clientX) {
      var rect = pad.getBoundingClientRect();
      var half = rect.width / 2;
      var dx = clientX - (rect.left + half);
      var dead = Math.max(6, half * 0.08);
      touch.left = dx < -dead;
      touch.right = dx > dead;
      var max = half - rect.height / 2;
      var off = Math.max(-max, Math.min(max, dx));
      if (knob) knob.style.transform = 'translateX(' + off.toFixed(1) + 'px)';
    }

    function reset() {
      pid = null;
      touch.left = touch.right = false;
      pad.classList.remove('pressed');
      if (knob) knob.style.transform = 'translateX(0)';
    }

    pad.addEventListener('pointerdown', function (e) {
      e.preventDefault();
      if (pid !== null) return; // nur ein Finger steuert das Pad
      pid = e.pointerId;
      try { pad.setPointerCapture(e.pointerId); } catch (err) { /* synthetische Events */ }
      lastWasTouch = true;
      Sfx.unlock();
      pad.classList.add('pressed');
      setDir(e.clientX);
    });
    pad.addEventListener('pointermove', function (e) {
      if (e.pointerId !== pid) return;
      setDir(e.clientX);
    });
    function release(e) {
      if (e.pointerId === pid) reset();
    }
    pad.addEventListener('pointerup', release);
    pad.addEventListener('pointercancel', release);
    pad.addEventListener('lostpointercapture', release);
    pad.addEventListener('contextmenu', function (e) { e.preventDefault(); });
    window.addEventListener('blur', reset);
  }

  // Touch-Buttons: Pointer Events mit Capture, damit Multi-Touch
  // (laufen + springen gleichzeitig) zuverlässig funktioniert.
  function bindButton(el, name) {
    if (!el) return;
    el.addEventListener('pointerdown', function (e) {
      e.preventDefault();
      try { el.setPointerCapture(e.pointerId); } catch (err) { /* synthetische Events */ }
      touch[name] = true;
      lastWasTouch = true;
      Sfx.unlock();
      if (name === 'jump') jumpPressed = true;
      el.classList.add('pressed');
    });
    function release(e) {
      touch[name] = false;
      el.classList.remove('pressed');
    }
    el.addEventListener('pointerup', release);
    el.addEventListener('pointercancel', release);
    el.addEventListener('lostpointercapture', release);
    el.addEventListener('contextmenu', function (e) { e.preventDefault(); });
  }

  return {
    escPressed: false,
    init: function () {
      bindJoypad(document.getElementById('joypad'), document.getElementById('joypad-knob'));
      bindButton(document.getElementById('btn-jump'), 'jump');
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
