import { ref, onMounted, onUnmounted } from 'vue';

export interface SyncMessage {
  type: 'SYNC_STATUS' | 'SYNC_COMPLETE' | 'SYNC_ERROR';
  pendingCount: number;
  message?: string;
}

const pendingCount = ref(0);
const lastError = ref<string | null>(null);

function isSyncMessage(data: unknown): data is SyncMessage {
  return (
    typeof data === 'object' &&
    data !== null &&
    'type' in data &&
    typeof (data as SyncMessage).type === 'string' &&
    ['SYNC_STATUS', 'SYNC_COMPLETE', 'SYNC_ERROR'].includes((data as SyncMessage).type)
  );
}

let handler: ((event: MessageEvent) => void) | null = null;

export function usePendingSync() {
  onMounted(() => {
    if (handler || !('serviceWorker' in navigator)) return;

    handler = (event: MessageEvent) => {
      if (!isSyncMessage(event.data)) return;
      const msg = event.data;

      pendingCount.value = msg.pendingCount;

      if (msg.type === 'SYNC_ERROR') {
        lastError.value = msg.message ?? 'Ошибка синхронизации';
      } else if (msg.type === 'SYNC_COMPLETE') {
        lastError.value = null;
      }
    };

    navigator.serviceWorker.addEventListener('message', handler);
  });

  onUnmounted(() => {
    if (handler && 'serviceWorker' in navigator) {
      navigator.serviceWorker.removeEventListener('message', handler);
      handler = null;
    }
  });

  return { pendingCount, lastError };
}
