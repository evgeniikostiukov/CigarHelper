import { precacheAndRoute, cleanupOutdatedCaches } from 'workbox-precaching';
import { registerRoute } from 'workbox-routing';
import { NetworkFirst, CacheFirst, StaleWhileRevalidate, NetworkOnly } from 'workbox-strategies';
import { BackgroundSyncPlugin, Queue } from 'workbox-background-sync';
import { ExpirationPlugin } from 'workbox-expiration';
import { clientsClaim } from 'workbox-core';

declare let self: ServiceWorkerGlobalScope;

self.addEventListener('message', (event) => {
  if (event.data?.type === 'SKIP_WAITING') self.skipWaiting();
});
clientsClaim();

cleanupOutdatedCaches();
precacheAndRoute(self.__WB_MANIFEST);

// ── Утилита: оповестить всех клиентов ──────────────────────────────────────
function broadcastToClients(data: Record<string, unknown>) {
  self.clients.matchAll({ type: 'window' }).then((clients) => {
    clients.forEach((client) => client.postMessage(data));
  });
}

// ── BackgroundSync: очередь мутаций ────────────────────────────────────────
let pendingCount = 0;

const mutationQueue = new Queue('cigar-helper-mutations', {
  maxRetentionTime: 7 * 24 * 60, // 7 дней
  onSync: async ({ queue }) => {
    let entry;
    while ((entry = await queue.shiftRequest())) {
      try {
        await fetch(entry.request.clone());
        pendingCount = Math.max(0, pendingCount - 1);
        broadcastToClients({ type: 'SYNC_STATUS', pendingCount });
      } catch (error) {
        await queue.unshiftRequest(entry);
        broadcastToClients({
          type: 'SYNC_ERROR',
          pendingCount,
          message: 'Ошибка синхронизации. Повтор при следующем подключении.',
        });
        throw error;
      }
    }
    broadcastToClients({ type: 'SYNC_COMPLETE', pendingCount: 0 });
    pendingCount = 0;
  },
});

const bgSyncPlugin = new BackgroundSyncPlugin('cigar-helper-mutations', {
  maxRetentionTime: 7 * 24 * 60,
});

// ── GET /api/(humidors|cigars|dashboard|reviews|brands) → NetworkFirst ─────
registerRoute(
  ({ url, request }) =>
    request.method === 'GET' && /^\/api\/(humidors|cigars|dashboard|reviews|brands)(\/|$|\?)/.test(url.pathname),
  new NetworkFirst({
    cacheName: 'api-lists',
    plugins: [new ExpirationPlugin({ maxAgeSeconds: 60 * 60, maxEntries: 100 })],
    networkTimeoutSeconds: 5,
  }),
);

// ── GET /api/cigars/bases/* → StaleWhileRevalidate ─────────────────────────
registerRoute(
  ({ url, request }) => request.method === 'GET' && url.pathname.startsWith('/api/cigars/bases'),
  new StaleWhileRevalidate({
    cacheName: 'api-catalog',
    plugins: [new ExpirationPlugin({ maxAgeSeconds: 24 * 60 * 60, maxEntries: 200 })],
  }),
);

// ── GET /api/cigar-images/*/thumbnail|data → CacheFirst ────────────────────
registerRoute(
  ({ url, request }) =>
    request.method === 'GET' && /^\/api\/cigar-images\/\d+\/(thumbnail|data)$/.test(url.pathname),
  new CacheFirst({
    cacheName: 'api-images',
    plugins: [new ExpirationPlugin({ maxAgeSeconds: 30 * 24 * 60 * 60, maxEntries: 200 })],
  }),
);

// ── GET /api/search → NetworkOnly (поиск из кеша бессмысленен) ─────────────
registerRoute(
  ({ url, request }) => request.method === 'GET' && url.pathname.startsWith('/api/search'),
  new NetworkOnly(),
);

// ── POST/PUT/DELETE /api/* → NetworkOnly + BackgroundSync ──────────────────
registerRoute(
  ({ url, request }) => ['POST', 'PUT', 'DELETE'].includes(request.method) && url.pathname.startsWith('/api/'),
  new NetworkOnly({ plugins: [bgSyncPlugin] }),
);

// Перехват fetchDidFail не нужен — bgSyncPlugin сам кладёт в очередь.
// Дополнительно оповещаем клиент о постановке в очередь.
self.addEventListener('fetch', (event) => {
  const { request } = event;
  if (!['POST', 'PUT', 'DELETE'].includes(request.method)) return;
  if (!new URL(request.url).pathname.startsWith('/api/')) return;

  // Через микротаск после того, как bgSyncPlugin обработал ошибку —
  // оповестить клиента о новом элементе в очереди.
  const originalFetch = fetch(request.clone());
  originalFetch.catch(() => {
    pendingCount++;
    broadcastToClients({ type: 'SYNC_STATUS', pendingCount });
  });
});
