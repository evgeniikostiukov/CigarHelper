import { mount, flushPromises } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const pushMock = vi.fn();

vi.mock('vue-router', async () => {
  const actual = await vi.importActual<typeof import('vue-router')>('vue-router');
  return {
    ...actual,
    useRouter: () => ({ push: pushMock }),
  };
});

vi.mock('@/services/dashboardService', () => {
  return {
    default: {
      getDashboardSummary: vi.fn(),
    },
  };
});

import dashboardService from '@/services/dashboardService';
import Dashboard from './Dashboard.vue';

const PrimeStubs = {
  Button: {
    props: ['label'],
    template: '<button type="button"><slot />{{ label }}</button>',
  },
  Message: {
    template: '<div><slot /></div>',
  },
  ProgressBar: {
    template: '<div data-stub="progressbar" />',
  },
  Skeleton: {
    template: '<div data-stub="skeleton" />',
  },
};

describe('Dashboard.vue', () => {
  beforeEach(() => {
    pushMock.mockReset();
    vi.mocked(dashboardService.getDashboardSummary).mockReset();
  });

  it('renders content after successful load', async () => {
    vi.mocked(dashboardService.getDashboardSummary).mockResolvedValueOnce({
      totalHumidors: 2,
      totalCigars: 5,
      totalCapacity: 30,
      averageFillPercent: 50.1,
      brandBreakdown: [
        { brandId: 10, brandName: 'X', cigarCount: 3, averageRating: 8.2 },
        { brandId: 11, brandName: 'Y', cigarCount: 2, averageRating: null },
      ],
      recentReviews: [
        {
          id: 100,
          title: 'Отличная сигара',
          cigarName: 'Robusto',
          cigarBrand: 'X',
          rating: 9,
          createdAt: '2026-03-30T10:00:00.000Z',
        },
      ],
    });

    const w = mount(Dashboard, {
      global: {
        stubs: PrimeStubs,
      },
    });

    await flushPromises();

    expect(w.get('[data-testid="dashboard-content"]')).toBeTruthy();
    expect(w.get('[data-testid="dashboard-summary-total-cigars"]').text()).toContain('5');
    expect(w.get('[data-testid="dashboard-brands-item-10"]').text()).toContain('X');
    expect(w.get('[data-testid="dashboard-review-100"]').text()).toContain('Отличная сигара');
  });

  it('renders error state when load fails', async () => {
    vi.mocked(dashboardService.getDashboardSummary).mockRejectedValueOnce(new Error('network'));

    const w = mount(Dashboard, {
      global: {
        stubs: PrimeStubs,
      },
    });

    await flushPromises();

    expect(w.get('[data-testid="dashboard-error"]')).toBeTruthy();
    expect(w.get('[data-testid="dashboard-retry"]')).toBeTruthy();
  });

  it('navigates to review detail when recent review clicked', async () => {
    vi.mocked(dashboardService.getDashboardSummary).mockResolvedValueOnce({
      totalHumidors: 0,
      totalCigars: 1,
      totalCapacity: 0,
      averageFillPercent: 0,
      brandBreakdown: [],
      recentReviews: [
        {
          id: 42,
          title: 'Test',
          cigarName: 'C',
          cigarBrand: 'B',
          rating: 7,
          createdAt: '2026-03-30T10:00:00.000Z',
        },
      ],
    });

    const w = mount(Dashboard, {
      global: {
        stubs: PrimeStubs,
      },
    });

    await flushPromises();

    await w.get('[data-testid="dashboard-review-42"] button').trigger('click');

    expect(pushMock).toHaveBeenCalledWith({ name: 'ReviewDetail', params: { id: 42 } });
  });
});

