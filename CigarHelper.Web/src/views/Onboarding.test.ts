import { mount, flushPromises } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const pushMock = vi.fn();
const replaceMock = vi.fn();

vi.mock('vue-router', async () => {
  const actual = await vi.importActual<typeof import('vue-router')>('vue-router');
  return {
    ...actual,
    useRouter: () => ({ push: pushMock, replace: replaceMock }),
  };
});

const toastAddMock = vi.fn();
vi.mock('primevue/usetoast', () => ({
  useToast: () => ({ add: toastAddMock }),
}));

vi.mock('@/services/humidorService', () => ({
  default: {
    createHumidor: vi.fn(),
  },
}));

vi.mock('@/services/cigarService', () => ({
  default: {
    getCigarBasesPaginated: vi.fn(),
    createCigar: vi.fn(),
  },
}));

import humidorService from '@/services/humidorService';
import cigarService from '@/services/cigarService';
import Onboarding from './Onboarding.vue';

const PrimeStubs = {
  Button: {
    props: ['label', 'loading', 'disabled'],
    template: '<button :disabled="disabled"><slot />{{ label }}</button>',
  },
  Message: {
    template: '<div><slot /></div>',
  },
  InputText: {
    props: ['modelValue'],
    emits: ['update:modelValue', 'input'],
    template:
      '<input :value="modelValue" @input="$emit(\'update:modelValue\', $event.target.value); $emit(\'input\')" />',
  },
  Textarea: {
    props: ['modelValue'],
    emits: ['update:modelValue'],
    template: '<textarea :value="modelValue" @input="$emit(\'update:modelValue\', $event.target.value)" />',
  },
  InputNumber: {
    props: ['modelValue'],
    emits: ['update:modelValue'],
    template: '<input :value="modelValue" @input="$emit(\'update:modelValue\', Number($event.target.value))" />',
  },
  Skeleton: {
    template: '<div data-stub="skeleton" />',
  },
  Divider: {
    template: '<hr />',
  },
  Tag: {
    props: ['value'],
    template: '<span data-stub="tag">{{ value }}</span>',
  },
};

describe('Onboarding.vue', () => {
  beforeEach(() => {
    pushMock.mockReset();
    replaceMock.mockReset();
    toastAddMock.mockReset();
    localStorage.clear();
    vi.mocked(humidorService.createHumidor).mockReset();
    vi.mocked(cigarService.getCigarBasesPaginated).mockReset();
    vi.mocked(cigarService.createCigar).mockReset();
  });

  it('redirects to Dashboard when onboarding flag missing', async () => {
    vi.mocked(cigarService.getCigarBasesPaginated).mockResolvedValueOnce({ items: [], totalCount: 0 });

    mount(Onboarding, {
      global: {
        stubs: PrimeStubs,
      },
    });

    await flushPromises();

    expect(replaceMock).toHaveBeenCalledWith({ name: 'Dashboard' });
  });

  it('creates humidor and switches to step 2', async () => {
    localStorage.setItem('needsOnboarding', '1');

    vi.mocked(humidorService.createHumidor).mockResolvedValueOnce({ id: 10, name: 'H', capacity: null });
    vi.mocked(cigarService.getCigarBasesPaginated).mockResolvedValue({ items: [], totalCount: 0 });

    const w = mount(Onboarding, {
      global: {
        stubs: PrimeStubs,
      },
    });

    await flushPromises();

    await w.get('form[data-testid="onboarding-step1"]').trigger('submit');
    await flushPromises();

    expect(humidorService.createHumidor).toHaveBeenCalled();
    expect(w.get('[data-testid="onboarding-step2"]')).toBeTruthy();
  });

  it('adds cigar from base and finishes onboarding', async () => {
    localStorage.setItem('needsOnboarding', '1');

    vi.mocked(humidorService.createHumidor).mockResolvedValueOnce({ id: 7, name: 'First', capacity: 50 });
    vi.mocked(cigarService.getCigarBasesPaginated).mockResolvedValue({
      items: [
        {
          id: 101,
          name: 'Robusto',
          brand: { id: 1, name: 'X', isModerated: true, createdAt: '2026-03-30T00:00:00.000Z' },
          country: 'Cuba',
          strength: 'medium',
          size: '5x50',
        },
      ],
      totalCount: 1,
    });
    vi.mocked(cigarService.createCigar).mockResolvedValueOnce({ id: 1 });

    const w = mount(Onboarding, {
      global: {
        stubs: PrimeStubs,
      },
    });

    await flushPromises();
    await w.get('form[data-testid="onboarding-step1"]').trigger('submit');
    await flushPromises();

    await w.get('[data-testid="onboarding-add-101"]').trigger('click');
    await flushPromises();

    expect(cigarService.createCigar).toHaveBeenCalled();

    await w.get('[data-testid="onboarding-finish"]').trigger('click');
    await flushPromises();

    expect(localStorage.getItem('needsOnboarding')).toBeNull();
    expect(pushMock).toHaveBeenCalledWith({ name: 'HumidorDetail', params: { id: '7' } });
  });
});
