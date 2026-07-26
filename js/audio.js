// Alle Sounds werden mit WebAudio synthetisiert – keine Audiodateien nötig.
// Der AudioContext darf erst nach der ersten Nutzer-Interaktion starten (Mobile).
var Sfx = (function () {
  var ctx = null;

  function ensureCtx() {
    if (Save.get().muted) return null;
    try {
      if (!ctx) {
        var AC = window.AudioContext || window.webkitAudioContext;
        if (!AC) return null;
        ctx = new AC();
      }
      if (ctx.state === 'suspended') ctx.resume();
      return ctx;
    } catch (e) {
      return null;
    }
  }

  // Kleiner Helfer: Oszillator mit Frequenzverlauf und Lautstärke-Hüllkurve.
  function tone(type, f0, f1, dur, vol) {
    var c = ensureCtx();
    if (!c) return;
    var t = c.currentTime;
    var osc = c.createOscillator();
    var gain = c.createGain();
    osc.type = type;
    osc.frequency.setValueAtTime(f0, t);
    osc.frequency.exponentialRampToValueAtTime(Math.max(1, f1), t + dur);
    gain.gain.setValueAtTime(vol, t);
    gain.gain.exponentialRampToValueAtTime(0.0001, t + dur);
    osc.connect(gain).connect(c.destination);
    osc.start(t);
    osc.stop(t + dur + 0.02);
  }

  function noise(dur, vol) {
    var c = ensureCtx();
    if (!c) return;
    var t = c.currentTime;
    var len = Math.floor(c.sampleRate * dur);
    var buf = c.createBuffer(1, len, c.sampleRate);
    var ch = buf.getChannelData(0);
    for (var i = 0; i < len; i++) ch[i] = (Math.random() * 2 - 1) * (1 - i / len);
    var src = c.createBufferSource();
    var gain = c.createGain();
    src.buffer = buf;
    gain.gain.setValueAtTime(vol, t);
    gain.gain.exponentialRampToValueAtTime(0.0001, t + dur);
    src.connect(gain).connect(c.destination);
    src.start(t);
  }

  return {
    unlock: function () { ensureCtx(); },
    jump: function () { tone('square', 320, 640, 0.12, 0.12); },
    death: function () { tone('sawtooth', 380, 60, 0.35, 0.18); noise(0.25, 0.12); },
    door: function () {
      tone('square', 440, 440, 0.09, 0.10);
      setTimeout(function () { tone('square', 554, 554, 0.09, 0.10); }, 90);
      setTimeout(function () { tone('square', 659, 659, 0.16, 0.10); }, 180);
    },
    trap: function () { tone('triangle', 160, 70, 0.15, 0.16); },
    spring: function () { tone('square', 200, 900, 0.2, 0.12); },
    poof: function () { noise(0.18, 0.10); },
    click: function () { tone('square', 700, 700, 0.05, 0.07); }
  };
})();
