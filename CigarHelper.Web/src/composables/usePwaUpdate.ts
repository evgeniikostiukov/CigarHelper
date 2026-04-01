import { ref } from 'vue';
import { useRegisterSW } from 'virtual:pwa-register/vue';

const needRefresh = ref(false);

let updateSW: ((reloadPage?: boolean) => Promise<void>) | undefined;

export function usePwaUpdate() {
  if (!updateSW) {
    const reg = useRegisterSW({
      onNeedRefresh() {
        needRefresh.value = true;
      },
      onOfflineReady() {
        // приложение доступно офлайн — ничего не делаем, статус покажет useOnlineStatus
      },
    });
    updateSW = reg.updateServiceWorker;
  }

  async function applyUpdate() {
    if (updateSW) {
      await updateSW(true);
      needRefresh.value = false;
    }
  }

  function dismiss() {
    needRefresh.value = false;
  }

  return { needRefresh, applyUpdate, dismiss };
}
