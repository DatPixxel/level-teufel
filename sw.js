// Service Worker: macht das Spiel offline spielbar und installierbar.
// Strategie: Netz zuerst (damit Updates sofort ankommen), Cache als
// Fallback (damit es ohne Verbindung weiterläuft).
var CACHE = 'level-teufel-v1';
var ASSETS = [
  './',
  'index.html',
  'style.css',
  'manifest.webmanifest',
  'js/config.js',
  'js/storage.js',
  'js/audio.js',
  'js/input.js',
  'js/renderer.js',
  'js/traps.js',
  'js/player.js',
  'js/levels.js',
  'js/ui.js',
  'js/main.js',
  'icons/icon-192.png',
  'icons/icon-512.png',
  'icons/icon-maskable-512.png'
];

self.addEventListener('install', function (e) {
  e.waitUntil(
    caches.open(CACHE).then(function (c) { return c.addAll(ASSETS); })
      .then(function () { return self.skipWaiting(); })
  );
});

self.addEventListener('activate', function (e) {
  e.waitUntil(
    caches.keys().then(function (keys) {
      return Promise.all(keys.map(function (k) {
        if (k !== CACHE) return caches.delete(k);
      }));
    }).then(function () { return self.clients.claim(); })
  );
});

self.addEventListener('fetch', function (e) {
  if (e.request.method !== 'GET') return;
  e.respondWith(
    fetch(e.request).then(function (res) {
      var copy = res.clone();
      caches.open(CACHE).then(function (c) { c.put(e.request, copy); });
      return res;
    }).catch(function () {
      return caches.match(e.request, { ignoreSearch: true });
    })
  );
});
