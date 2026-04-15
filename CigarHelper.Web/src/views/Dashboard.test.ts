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
  Avatar: {
    template: '<span data-stub="avatar" />',
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
      averageDaysToSmoke: 17,
      averageCigarRating: 8.3,
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
          username: 'dashboard_user',
          isAuthorProfilePublic: true,
          userAvatarUrl: null,
        },
      ],
      timeline: [{ period: '2026-03', purchasedCount: 3, smokedCount: 1 }],
      staleCigarReminders: [
        {
          cigarId: 4,
          cigarName: 'Robusto',
          brandName: 'X',
          daysUntouched: 52,
          lastTouchedAt: '2026-02-01T00:00:00.000Z',
        },
      ],
    });

    const w = mount(Dashboard, {
      global: {
        stubs: {
          ...PrimeStubs,
          PublicProfileAuthorBlock: {
            props: ['username'],
            template: '<span data-testid="dashboard-review-author">{{ username }}</span>',
          },
        },
      },
    });

    await flushPromises();

    expect(w.get('[data-testid="dashboard-content"]')).toBeTruthy();
    expect(w.get('[data-testid="dashboard-summary-total-cigars"]').text()).toContain('5');
    expect(w.get('[data-testid="dashboard-summary-average-rating"]').text()).toContain('8.3/10');
    expect(w.get('[data-testid="dashboard-summary-aging"]').text()).toContain('17');
    expect(w.get('[data-testid="dashboard-timeline"]').text()).toContain('купил: 3');
    expect(w.get('[data-testid="dashboard-reminder-4"]').text()).toContain('Не трогали 52');
    expect(w.get('[data-testid="dashboard-brands-item-10"]').text()).toContain('X');
    expect(w.get('[data-testid="dashboard-review-100"]').text()).toContain('Отличная сигара');
    expect(w.get('[data-testid="dashboard-review-author"]').text()).toContain('dashboard_user');
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
      averageDaysToSmoke: 0,
      averageCigarRating: null,
      brandBreakdown: [],
      recentReviews: [
        {
          id: 42,
          title: 'Test',
          cigarName: 'C',
          cigarBrand: 'B',
          rating: 7,
          createdAt: '2026-03-30T10:00:00.000Z',
          username: 'u',
          isAuthorProfilePublic: false,
          userAvatarUrl: null,
        },
      ],
      timeline: [],
      staleCigarReminders: [],
    });

    const w = mount(Dashboard, {
      global: {
        stubs: {
          ...PrimeStubs,
          PublicProfileAuthorBlock: {
            props: ['username'],
            template: '<span data-testid="dashboard-review-author">{{ username }}</span>',
          },
        },
      },
    });

    await flushPromises();

    await w.get('[data-testid="dashboard-review-42"] button').trigger('click');

    expect(pushMock).toHaveBeenCalledWith({ name: 'ReviewDetail', params: { id: 42 } });
  });
});
