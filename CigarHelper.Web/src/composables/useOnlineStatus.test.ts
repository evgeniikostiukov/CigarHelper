import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { nextTick } from 'vue';
import { mount } from '@vue/test-utils';
import { defineComponent, ref } from 'vue';

vi.mock('vue', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vue')>();
  return { ...actual };
});

function createTestComponent(onChange?: (online: boolean) => void) {
  return defineComponent({
    setup() {
      const { useOnlineStatus } = require('./useOnlineStatus');
      const { isOnline } = useOnlineStatus(onChange);
      return { isOnline };
    },
    template: '<div>{{ isOnline }}</div>',
  });
}

describe('useOnlineStatus', () => {
  let originalNavigatorOnLine: boolean;

  beforeEach(() => {
    originalNavigatorOnLine = navigator.onLine;
    vi.resetModules();
  });

  afterEach(() => {
    Object.defineProperty(navigator, 'onLine', { value: originalNavigatorOnLine, writable: true });
  });

  it('returns current online status', async () => {
    Object.defineProperty(navigator, 'onLine', { value: true, writable: true });
    const { useOnlineStatus } = await import('./useOnlineStatus');

    const wrapper = mount(
      defineComponent({
        setup() {
          const { isOnline } = useOnlineStatus();
          return { isOnline };
        },
        template: '<div>{{ isOnline }}</div>',
      }),
    );

    expect(wrapper.vm.isOnline).toBe(true);
    wrapper.unmount();
  });

  it('reacts to offline/online events', async () => {
    Object.defineProperty(navigator, 'onLine', { value: true, writable: true });
    const onChange = vi.fn();
    const { useOnlineStatus } = await import('./useOnlineStatus');

    const wrapper = mount(
      defineComponent({
        setup() {
          const { isOnline } = useOnlineStatus(onChange);
          return { isOnline };
        },
        template: '<div>{{ isOnline }}</div>',
      }),
    );

    window.dispatchEvent(new Event('offline'));
    await nextTick();
    expect(wrapper.vm.isOnline).toBe(false);
    expect(onChange).toHaveBeenCalledWith(false);

    window.dispatchEvent(new Event('online'));
    await nextTick();
    expect(wrapper.vm.isOnline).toBe(true);
    expect(onChange).toHaveBeenCalledWith(true);

    wrapper.unmount();
  });
});
