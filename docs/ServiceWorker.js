const cacheName = "fire company-BASSETBALL-0.1";
const contentToCache = [
    "Build/docs.loader.js",
    "Build/fc301f1c624c71529979ff3e8b70ccb8.js",
    "Build/29c893f9cbe38fd400ea2611fde53936.data",
    "Build/ef4a33a014d2662a01ddbc200fa21b0c.wasm",
    "TemplateData/style.css"

];

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');
    
    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
    })());
});

self.addEventListener('fetch', function (e) {
    e.respondWith((async function () {
      let response = await caches.match(e.request);
      console.log(`[Service Worker] Fetching resource: ${e.request.url}`);
      if (response) { return response; }

      response = await fetch(e.request);
      const cache = await caches.open(cacheName);
      console.log(`[Service Worker] Caching new resource: ${e.request.url}`);
      cache.put(e.request, response.clone());
      return response;
    })());
});
