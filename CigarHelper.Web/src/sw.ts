import { precacheAndRoute, cleanupOutdatedCaches, createHandlerBoundToURL } from 'workbox-precaching';
import { registerRoute, NavigationRoute } from 'workbox-routing';
import { NetworkFirst, CacheFirst, StaleWhileRevalidate } from 'workbox-strategies';
import { Queue } from 'workbox-background-sync';
import { ExpirationPlugin } from 'workbox-expiration';
import { clientsClaim } from 'workbox-core';

declare let self: ServiceWorkerGlobalScope;

self.addEventListener('message', (event) => {
  if (event.data?.type === 'SKIP_WAITING') self.skipWaiting();
  if (event.data?.type === 'REPLAY_QUEUE') replayMutations();
});
clientsClaim();

cleanupOutdatedCaches();
precacheAndRoute(self.__WB_MANIFEST);

registerRoute(new NavigationRoute(createHandlerBoundToURL('/index.html')));

// ── Утилита: оповестить всех клиентов ──────────────────────────────────────
function broadcastToClients(data: Record<string, unknown>) {
  self.clients.matchAll({ type: 'window' }).then((clients) => {
    clients.forEach((client) => client.postMessage(data));
  });
}

// ── Очередь мутаций (Queue напрямую, без BackgroundSyncPlugin) ─────────────
let pendingCount = 0;

const mutationQueue = new Queue('cigar-helper-mutations', {
  maxRetentionTime: 7 * 24 * 60,
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

async function replayMutations() {
  try {
    const allEntries = await mutationQueue.getAll();
    if (allEntries.length === 0) return;
    pendingCount = allEntries.length;
    broadcastToClients({ type: 'SYNC_STATUS', pendingCount });

    let entry;
    while ((entry = await mutationQueue.shiftRequest())) {
      try {
        await fetch(entry.request.clone());
        pendingCount = Math.max(0, pendingCount - 1);
        broadcastToClients({ type: 'SYNC_STATUS', pendingCount });
      } catch {
        await mutationQueue.unshiftRequest(entry);
        broadcastToClients({
          type: 'SYNC_ERROR',
          pendingCount,
          message: 'Ошибка синхронизации. Повтор позже.',
        });
        return;
      }
    }
    broadcastToClients({ type: 'SYNC_COMPLETE', pendingCount: 0 });
    pendingCount = 0;
  } catch {
    // Queue access failed — ignore silently
  }
}

// Fallback: при восстановлении сети SW тоже получает событие —
// вручную запускаем replay, не дожидаясь ненадёжного sync event.
self.addEventListener('fetch', (event) => {
  // Любой успешный GET-запрос — признак наличия сети, пробуем replay.
  if (event.request.method === 'GET' && pendingCount > 0) {
    event.waitUntil(replayMutations());
  }
});

function offlineQueuedResponse(): Response {
  return new Response(JSON.stringify({ offlineQueued: true }), {
    status: 202,
    statusText: 'Accepted',
    headers: {
      'Content-Type': 'application/json',
      'X-CigarHelper-Offline-Queued': '1',
    },
  });
}

// ── POST/PUT/DELETE /api/* → сеть; офлайн/сбой → очередь + 202 (без throw) ─
// Иначе страничный fetch падает → консоль net::ERR_*; при !onLine не зовём fetch.
registerRoute(
  ({ url, request }) => ['POST', 'PUT', 'DELETE'].includes(request.method) && url.pathname.startsWith('/api/'),
  async ({ request }) => {
    if (!self.navigator.onLine) {
      await mutationQueue.pushRequest({ request });
      pendingCount++;
      broadcastToClients({ type: 'SYNC_STATUS', pendingCount });
      return offlineQueuedResponse();
    }
    try {
      return await fetch(request.clone());
    } catch {
      await mutationQueue.pushRequest({ request });
      pendingCount++;
      broadcastToClients({ type: 'SYNC_STATUS', pendingCount });
      return offlineQueuedResponse();
    }
  },
);

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

// ── GET /api/search → сеть или ничего ──────────────────────────────────────
registerRoute(
  ({ url, request }) => request.method === 'GET' && url.pathname.startsWith('/api/search'),
  async ({ request }) => fetch(request),
);
