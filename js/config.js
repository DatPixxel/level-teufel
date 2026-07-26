// Zentrale Konfiguration: alle Tuning-Werte an einem Ort.
// Einheiten: Pixel und Sekunden (Geschwindigkeiten in px/s).
var CONFIG = {
  COLS: 30,
  ROWS: 17,
  TILE: 32,
  WIDTH: 960,   // COLS * TILE
  HEIGHT: 544,  // ROWS * TILE

  // Physik
  GRAVITY: 2300,
  MAX_FALL: 900,
  RUN_SPEED: 260,
  JUMP_VEL: 720,       // Absprunggeschwindigkeit (nach oben)
  JUMP_CUT: 0.45,      // vy-Faktor beim Loslassen der Sprungtaste
  COYOTE_TIME: 0.08,   // Sprung kurz nach Kantenverlust erlaubt
  JUMP_BUFFER: 0.10,   // Sprungeingabe kurz vor Landung wird gepuffert
  SPRING_VEL: 1200,

  // Spieler
  PLAYER_W: 22,
  PLAYER_H: 28,

  // Fallen
  SPIKE_INSET: 6,        // Stachel-Hitbox pro Seite verkleinert (faire Tode)
  VANISH_SHAKE: 0.3,     // Wackelzeit, bevor ein Block verschwindet
  FALLBLOCK_GRAVITY: 2600,
  PROJECTILE_SPEED: 420,
  CRUSHER_SLAM: 1100,
  CRUSHER_RETRACT: 170,
  PLATFORM_SPEED: 100,

  CRUMBLE_DELAY: 0.3,    // Zeit bis ein betretener Bröckel-Block bricht
  LIGHT_RADIUS: 150,     // Standard-Lichtradius in Dunkel-Leveln

  DEATH_FREEZE: 0.45,    // Pause zwischen Tod und Respawn
  DOOR_POOF: 0.5,

  COLORS: {
    bg: '#141419',
    tile: '#23232d',
    tileEdge: '#34343f',
    fakeTile: '#23232d',   // absichtlich identisch mit tile
    spike: '#8d8d99',
    door: '#f2f2f2',
    doorInner: '#141419',
    player: '#ffffff',
    eye: '#141419',
    spring: '#e8c14a',
    platform: '#4a4a58',
    crusher: '#3c3c48',
    block: '#3c3c48',
    projectile: '#c9c9d4',
    marker: '#e8c14a',
    particle: '#ffffff',
    invisibleWall: '#5a5a68'
  }
};
