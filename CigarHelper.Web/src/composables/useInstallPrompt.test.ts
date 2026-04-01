import { describe, it, expect, vi, beforeEach } from 'vitest';
import { nextTick } from 'vue';
import { mount } from '@vue/test-utils';
import { defineComponent } from 'vue';

describe('useInstallPrompt', () => {
  beforeEach(() => {
    vi.resetModules();
  });

  it('canInstall is false initially', async () => {
    const { useInstallPrompt } = await import('./useInstallPrompt');

    const wrapper = mount(
      defineComponent({
        setup() {
          const { canInstall, install } = useInstallPrompt();
          return { canInstall, install };
        },
        template: '<div>{{ canInstall }}</div>',
      }),
    );

    expect(wrapper.vm.canInstall).toBe(false);
    wrapper.unmount();
  });

  it('sets canInstall=true on beforeinstallprompt event', async () => {
    const { useInstallPrompt } = await import('./useInstallPrompt');

    const wrapper = mount(
      defineComponent({
        setup() {
          const { canInstall } = useInstallPrompt();
          return { canInstall };
        },
        template: '<div>{{ canInstall }}</div>',
      }),
    );

    const promptEvent = new Event('beforeinstallprompt', { cancelable: true });
    (promptEvent as any).prompt = vi.fn().mockResolvedValue(undefined);
    (promptEvent as any).userChoice = Promise.resolve({ outcome: 'dismissed' });

    window.dispatchEvent(promptEvent);
    await nextTick();

    expect(wrapper.vm.canInstall).toBe(true);
    wrapper.unmount();
  });
});
