import { mount, flushPromises } from '@vue/test-utils';
import { describe, expect, it, vi } from 'vitest';

const pushMock = vi.fn();

vi.mock('vue-router', async () => {
  const actual = await vi.importActual<typeof import('vue-router')>('vue-router');
  return {
    ...actual,
    useRouter: () => ({ push: pushMock }),
    useRoute: () => ({ params: { id: '123' } }),
  };
});

vi.mock('../services/cigarService', () => {
  return {
    default: {
      getCigar: vi.fn(),
    },
  };
});

vi.mock('../services/humidorService', () => {
  return {
    default: {
      getHumidor: vi.fn(),
      getHumiditySeverity: vi.fn(() => 'info'),
    },
  };
});

vi.mock('../services/api', () => {
  return {
    default: {
      get: vi.fn(),
    },
  };
});

vi.mock('primevue/useconfirm', () => {
  return {
    useConfirm: () => ({
      require: vi.fn(),
    }),
  };
});

vi.mock('primevue/usetoast', () => {
  return {
    useToast: () => ({
      add: vi.fn(),
    }),
  };
});

import cigarService from '../services/cigarService';
import CigarDetail from './CigarDetail.vue';

const PrimeStubs = {
  Badge: { template: '<div data-stub="badge"><slot /></div>' },
  Button: { props: ['label'], template: '<button type="button"><slot />{{ label }}</button>' },
  Carousel: { template: '<div data-stub="carousel"><slot name="empty" /></div>' },
  ConfirmDialog: { template: '<div data-stub="confirm" />' },
  Message: { template: '<div data-stub="message"><slot /></div>' },
  Rating: { template: '<div data-stub="rating" />' },
  Skeleton: { template: '<div data-stub="skeleton" />' },
};

describe('CigarDetail.vue', () => {
  it('renders added-at field when createdAt present', async () => {
    vi.mocked(cigarService.getCigar).mockResolvedValueOnce({
      id: 123,
      cigarBaseId: 10,
      name: 'Test cigar',
      brand: { id: 1, name: 'Brand', isModerated: true, createdAt: '2026-01-01T00:00:00.000Z' },
      country: 'CU',
      lengthMm: null,
      diameter: null,
      strength: 'Medium',
      price: null,
      rating: null,
      description: null,
      wrapper: null,
      binder: null,
      filler: null,
      humidorId: null,
      quantity: 1,
      images: [],
      createdAt: '2026-04-01T00:00:00.000Z',
    });

    const w = mount(CigarDetail, {
      global: {
        stubs: PrimeStubs,
      },
    });

    await flushPromises();

    const row = w.get('[data-testid="cigar-detail-added-at"]');
    expect(row.text()).toContain('Дата добавления');
    expect(row.text()).not.toContain('—');
  });
});
