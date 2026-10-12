// Development uses network-first requests so local changes are visible immediately.
// The versioned precache is still available as an offline fallback.
self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall()));
self.addEventListener('activate', event => event.waitUntil(onActivate()));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'daypulse-offline-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

// Replace with the application base path if the app is hosted in a subfolder.
const base = '/';
const baseUrl = new URL(base, self.location.origin);
const indexUrl = new URL('index.html', baseUrl).href;
const precacheUrls = new Set(
    self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href));
const serviceWorkerUrls = new Set([
    new URL('service-worker.js', baseUrl).href,
    new URL('service-worker.published.js', baseUrl).href,
    new URL('service-worker-assets.js', baseUrl).href
]);
const precacheRequests = self.assetsManifest.assets.map(asset => new Request(
    new URL(asset.url, baseUrl).href,
    {
        cache: 'no-cache',
        integrity: asset.hash
    }));

async function onInstall() {
    console.info('Service worker: Install');

    const cache = await caches.open(cacheName);
    await cache.addAll(precacheRequests);
    await self.skipWaiting();
}

async function onActivate() {
    console.info('Service worker: Activate');

    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
    await self.clients.claim();
}

async function onFetch(event) {
    if (event.request.method !== 'GET' || new URL(event.request.url).origin !== self.location.origin) {
        return fetch(event.request);
    }

    try {
        // Always prefer the development server so local changes are not hidden by
        // a previous install. The precache is used only when the network is down.
        return await fetch(event.request, { cache: 'no-store' });
    } catch (error) {
        console.warn('Service worker: Network-first request failed; trying the precache.', error);

        const cache = await caches.open(cacheName);
        const request = shouldServeIndex(event.request)
            ? indexUrl
            : event.request;
        const cachedResponse = await cache.match(request);

        if (cachedResponse) {
            return cachedResponse;
        }

        throw error;
    }
}

function shouldServeIndex(request) {
    return request.mode === 'navigate'
        && !precacheUrls.has(request.url)
        && !serviceWorkerUrls.has(request.url);
}
