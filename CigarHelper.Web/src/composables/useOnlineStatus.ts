import { ref, onMounted, onUnmounted } from 'vue';

const isOnline = ref(typeof navigator !== 'undefined' ? navigator.onLine : true);

const listeners: Array<(online: boolean) => void> = [];

let initialized = false;

function handleOnline() {
  isOnline.value = true;
  listeners.forEach((fn) => fn(true));
}

function handleOffline() {
  isOnline.value = false;
  listeners.forEach((fn) => fn(false));
}

export function useOnlineStatus(onChange?: (online: boolean) => void) {
  if (onChange) listeners.push(onChange);

  onMounted(() => {
    if (!initialized) {
      window.addEventListener('online', handleOnline);
      window.addEventListener('offline', handleOffline);
      initialized = true;
    }
  });

  onUnmounted(() => {
    if (onChange) {
      const idx = listeners.indexOf(onChange);
      if (idx >= 0) listeners.splice(idx, 1);
    }
  });

  return { isOnline };
}
