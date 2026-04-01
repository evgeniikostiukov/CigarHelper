import { describe, it, expect, vi, beforeEach } from 'vitest';
import { nextTick } from 'vue';
import { mount } from '@vue/test-utils';
import { defineComponent } from 'vue';

describe('usePendingSync', () => {
  let messageHandler: ((event: MessageEvent) => void) | null = null;

  beforeEach(() => {
    vi.resetModules();
    messageHandler = null;

    if (!('serviceWorker' in navigator)) {
      Object.defineProperty(navigator, 'serviceWorker', {
        value: {
          addEventListener: vi.fn((event: string, handler: (event: MessageEvent) => void) => {
            if (event === 'message') messageHandler = handler;
          }),
          removeEventListener: vi.fn(),
        },
        writable: true,
        configurable: true,
      });
    } else {
      vi.spyOn(navigator.serviceWorker, 'addEventListener').mockImplementation(
        (event: string, handler: any) => {
          if (event === 'message') messageHandler = handler;
        },
      );
      vi.spyOn(navigator.serviceWorker, 'removeEventListener').mockImplementation(() => {});
    }
  });

  it('starts with pendingCount=0', async () => {
    const { usePendingSync } = await import('./usePendingSync');

    const wrapper = mount(
      defineComponent({
        setup() {
          const { pendingCount, lastError } = usePendingSync();
          return { pendingCount, lastError };
        },
        template: '<div>{{ pendingCount }}</div>',
      }),
    );

    expect(wrapper.vm.pendingCount).toBe(0);
    expect(wrapper.vm.lastError).toBeNull();
    wrapper.unmount();
  });

  it('updates pendingCount on SYNC_STATUS message', async () => {
    const { usePendingSync } = await import('./usePendingSync');

    const wrapper = mount(
      defineComponent({
        setup() {
          const { pendingCount } = usePendingSync();
          return { pendingCount };
        },
        template: '<div>{{ pendingCount }}</div>',
      }),
    );

    expect(messageHandler).toBeTruthy();
    messageHandler!(new MessageEvent('message', { data: { type: 'SYNC_STATUS', pendingCount: 3 } }));
    await nextTick();

    expect(wrapper.vm.pendingCount).toBe(3);
    wrapper.unmount();
  });

  it('sets lastError on SYNC_ERROR', async () => {
    const { usePendingSync } = await import('./usePendingSync');

    const wrapper = mount(
      defineComponent({
        setup() {
          const { pendingCount, lastError } = usePendingSync();
          return { pendingCount, lastError };
        },
        template: '<div>{{ lastError }}</div>',
      }),
    );

    messageHandler!(
      new MessageEvent('message', {
        data: { type: 'SYNC_ERROR', pendingCount: 1, message: 'Network error' },
      }),
    );
    await nextTick();

    expect(wrapper.vm.lastError).toBe('Network error');
    expect(wrapper.vm.pendingCount).toBe(1);
    wrapper.unmount();
  });

  it('clears lastError on SYNC_COMPLETE', async () => {
    const { usePendingSync } = await import('./usePendingSync');

    const wrapper = mount(
      defineComponent({
        setup() {
          const { pendingCount, lastError } = usePendingSync();
          return { pendingCount, lastError };
        },
        template: '<div>{{ lastError }}</div>',
      }),
    );

    messageHandler!(
      new MessageEvent('message', {
        data: { type: 'SYNC_ERROR', pendingCount: 2, message: 'fail' },
      }),
    );
    await nextTick();
    expect(wrapper.vm.lastError).toBe('fail');

    messageHandler!(
      new MessageEvent('message', {
        data: { type: 'SYNC_COMPLETE', pendingCount: 0 },
      }),
    );
    await nextTick();
    expect(wrapper.vm.lastError).toBeNull();
    expect(wrapper.vm.pendingCount).toBe(0);
    wrapper.unmount();
  });
});
