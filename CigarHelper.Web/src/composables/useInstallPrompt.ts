import { ref, onMounted, onUnmounted } from 'vue';

interface BeforeInstallPromptEvent extends Event {
  prompt(): Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

const canInstall = ref(false);
let deferredPrompt: BeforeInstallPromptEvent | null = null;
let boundHandler: ((e: Event) => void) | null = null;

export function useInstallPrompt() {
  onMounted(() => {
    if (boundHandler) return;

    boundHandler = (e: Event) => {
      e.preventDefault();
      deferredPrompt = e as BeforeInstallPromptEvent;
      canInstall.value = true;
    };

    window.addEventListener('beforeinstallprompt', boundHandler);

    window.addEventListener('appinstalled', () => {
      canInstall.value = false;
      deferredPrompt = null;
    });
  });

  onUnmounted(() => {
    if (boundHandler) {
      window.removeEventListener('beforeinstallprompt', boundHandler);
      boundHandler = null;
    }
  });

  async function install() {
    if (!deferredPrompt) return;
    await deferredPrompt.prompt();
    const { outcome } = await deferredPrompt.userChoice;
    if (outcome === 'accepted') {
      canInstall.value = false;
    }
    deferredPrompt = null;
  }

  return { canInstall, install };
}
